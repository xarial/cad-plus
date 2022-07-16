﻿//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Drawing.QrCode.Data;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XToolkit.Services.UserSettings;

namespace Xarial.CadPlus.Drawing.QrCode
{
    /// <summary>
    /// Sketch picture object does not provide persist references.
    /// This serializer provides a workaround and saves the name of the feature (invisible) as a serialized value
    /// </summary>
    public class PictureValueSerializer : IValueSerializer
    {
        public Type Type => typeof(IXObject);

        private readonly IXLogger m_Logger;
        private readonly IXDrawing m_Draw;

        public PictureValueSerializer(IXLogger logger, IXDrawing drw)
        {
            m_Logger = logger;
            m_Draw = drw;
        }

        public object DeserializeValue(string val)
        {
            try
            {
                if (!string.IsNullOrEmpty(val))
                {
                    var feat = (IFeature)((ISwDrawing)m_Draw).Drawing.FeatureByName(val);
                    if (feat != null)
                    {
                        var skPict = (ISketchPicture)feat.GetSpecificFeature2();
                        return ((ISwDrawing)m_Draw).CreateObjectFromDispatch<ISwObject>(skPict);
                    }
                    else
                    {
                        throw new Exception("Failed to find the picture");
                    }
                }
                else
                {
                    throw new Exception("Value is empty");
                }
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                return null;
            }
        }

        public string SerializeValue(object val)
        {
            try
            {
                var obj = (IXObject)val;

                var skPict = ((ISwObject)obj).Dispatch as ISketchPicture;
                return skPict.GetFeature().Name;
            }
            catch
            {
                return "";
            }
        }
    }

    public class QrCodeDrawingHandler : IDocumentHandler
    {
        private const string STREAM_NAME = "_Xarial_CadPlus_QRCodeData_";

        public ObservableCollection<QrCodeInfo> QrCodes => m_QrCodesLazy.Value;

        private bool m_HasChanges;

        private Lazy<ObservableCollection<QrCodeInfo>> m_QrCodesLazy;
        private IXDrawing m_Drawing;

        private readonly IXLogger m_Logger;
        private readonly UserSettingsService m_Serializer;

        public QrCodeDrawingHandler(IXLogger logger)
        {
            m_Serializer = new UserSettingsService();
            m_Logger = logger;
        }

        public void Init(IXApplication app, IXDocument model)
        {
            m_HasChanges = false;

            if (model is IXDrawing)
            {
                m_Drawing = (IXDrawing)model;
                m_Drawing.StreamWriteAvailable += OnStreamWriteAvailable;
                m_QrCodesLazy = new Lazy<ObservableCollection<QrCodeInfo>>(ReadQrCodeData);
            }
        }

        private ObservableCollection<QrCodeInfo> ReadQrCodeData()
        {
            ObservableCollection<QrCodeInfo> data = null;

            try
            {
                using (var stream = m_Drawing.TryOpenStream(STREAM_NAME, AccessType_e.Read))
                {
                    if (stream != null)
                    {
                        m_Logger.Log("Reading QR code data", LoggerMessageSeverity_e.Debug);

                        using (var reader = new StreamReader(stream))
                        {
                            data = m_Serializer.ReadSettings<ObservableCollection<QrCodeInfo>>(
                                reader, new PictureValueSerializer(m_Logger, m_Drawing));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
            }

            if (data != null)
            {
                var usedPictures = new List<IXObject>();

                for (int i = data.Count - 1; i >= 0; i--)
                {
                    if (data[i].Picture == null)
                    {
                        data.RemoveAt(i);
                        m_Logger.Log($"Removed dangling QR code data at index {i}", LoggerMessageSeverity_e.Debug);
                    }
                    //As we serialize names of the features (removing and readding QR code may cause duplication in the data which is not recognized as dangling)
                    else if (usedPictures.Find(p => p.Equals(data[i].Picture)) != null)
                    {
                        data.RemoveAt(i);
                        m_Logger.Log($"Removed duplicate QR code data at index {i}", LoggerMessageSeverity_e.Debug);
                    }
                    else
                    {
                        usedPictures.Add(data[i].Picture);
                    }
                }
            }
            else
            {
                data = new ObservableCollection<QrCodeInfo>();
            }

            data.CollectionChanged += OnDataCollectionChanged;

            Init(data);

            return data;
        }

        private void OnDataCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            m_HasChanges = true;
            Init((IEnumerable<QrCodeInfo>)sender);
        }

        private void Init(IEnumerable<QrCodeInfo> qrCodes)
        {
            foreach (var qrCode in qrCodes)
            {
                qrCode.Changed -= OnQrCodeChanged;
                qrCode.Changed += OnQrCodeChanged;
            }
        }

        private void OnStreamWriteAvailable(IXDocument doc)
        {
            if (m_HasChanges)
            {
                m_Logger.Log("Storing QR code data", LoggerMessageSeverity_e.Debug);

                try
                {
                    using (var stream = m_Drawing.OpenStream(STREAM_NAME, AccessType_e.Write))
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            m_Serializer.StoreSettings(QrCodes,
                                writer, new PictureValueSerializer(m_Logger, m_Drawing));
                        }
                    }

                    m_HasChanges = false;
                }
                catch (Exception ex)
                {
                    m_Logger.Log(ex);
                }
            }
        }

        private void OnQrCodeChanged(QrCodeInfo qrCode)
        {
            m_HasChanges = true;
        }

        public void Dispose()
        {
            if (m_Drawing != null)
            {
                m_Drawing.StreamWriteAvailable -= OnStreamWriteAvailable;
            }
        }
    }
}
