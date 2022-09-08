//*********************************************************************
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
using Xarial.XCad.Features;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Services.UserSettings;

namespace Xarial.CadPlus.Drawing.QrCode
{
    /// <summary>
    /// Sketch picture object does not provide persist references.
    /// This serializer provides a workaround and saves the name of the feature (invisible) as a serialized value
    /// </summary>
    public class PictureValueSerializer : IValueSerializer
    {
        public Type Type => typeof(IXSketchPicture);

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
                    return (IXSketchPicture)m_Draw.Features[val];
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
                return ((IXSketchPicture)val).Name;
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

        public QrCodeInfoCollection QrCodes => m_QrCodesLazy.Value;

        private bool m_HasChanges;

        private Lazy<QrCodeInfoCollection> m_QrCodesLazy;
        private IXDrawing m_Drawing;

        private readonly IXLogger m_Logger;
        private readonly UserSettingsService m_Serializer;

        private readonly IExpressionParser m_ExprParser;

        public QrCodeDrawingHandler(IExpressionParser exprParser, IXLogger logger)
        {   
            m_ExprParser = exprParser;
            m_Logger = logger;

            m_Serializer = new UserSettingsService();
        }

        public void Init(IXApplication app, IXDocument model)
        {
            m_HasChanges = false;

            if (model is IXDrawing)
            {
                m_Drawing = (IXDrawing)model;
                m_Drawing.StreamWriteAvailable += OnStreamWriteAvailable;
                m_QrCodesLazy = new Lazy<QrCodeInfoCollection>(ReadQrCodeData);
            }
        }

        private QrCodeInfoCollection ReadQrCodeData()
        {
            QrCodeInfoCollection data = null;

            try
            {
                using (var stream = m_Drawing.TryOpenStream(STREAM_NAME, AccessType_e.Read))
                {
                    if (stream != null)
                    {
                        m_Logger.Log("Reading QR code data", LoggerMessageSeverity_e.Debug);

                        using (var reader = new StreamReader(stream))
                        {
                            data = m_Serializer.ReadSettings<QrCodeInfoCollection>(
                                reader, t => 
                                {
                                    ((QrCodeInfoVersionTransformer)t).SetExpressonParser(m_ExprParser);
                                    return t;
                                }, new PictureValueSerializer(m_Logger, m_Drawing));
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
                data = new QrCodeInfoCollection();
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
