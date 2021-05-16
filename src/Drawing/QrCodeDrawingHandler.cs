using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Drawing.Data;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XToolkit.Services.UserSettings;

namespace Xarial.CadPlus.Drawing
{
    public class SelObjectValueSerializer : IValueSerializer
    {
        public Type Type => typeof(IXObject);

        private readonly IXLogger m_Logger;
        private readonly IXDrawing m_Draw;

        public SelObjectValueSerializer(IXLogger logger, IXDrawing drw) 
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
                        return SwObjectFactory.FromDispatch<ISwObject>(skPict, (ISwDrawing)m_Draw);
                    }
                    else 
                    {
                        throw new Exception("Failed to find the picture");
                    }
                    //TODO: uncomment when new version of xCAD is released

                    //var buffer = Convert.FromBase64String(val);

                    //using (var stream = new MemoryStream(buffer))
                    //{
                    //    return m_Doc.DeserializeObject(stream);
                    //}
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

                //TODO: uncomment when new version of xCAD is released

                //byte[] buffer;

                //using (var memStr = new MemoryStream())
                //{
                //    obj.Serialize(memStr);
                //    memStr.Seek(0, SeekOrigin.Begin);
                //    buffer = memStr.ToArray();
                //}

                //return Convert.ToBase64String(buffer);
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

        public ObservableCollection<QrCodeData> QrCodes => m_QrCodesLazy.Value;

        private bool m_HasChanges;

        private Lazy<ObservableCollection<QrCodeData>> m_QrCodesLazy;
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
                m_QrCodesLazy = new Lazy<ObservableCollection<QrCodeData>>(ReadQrCodeData);
            }
        }

        private ObservableCollection<QrCodeData> ReadQrCodeData()
        {
            ObservableCollection<QrCodeData> data = null;

            try
            {
                using (var stream = m_Drawing.TryOpenStream(STREAM_NAME, AccessType_e.Read))
                {
                    if (stream != null)
                    {
                        m_Logger.Log("Reading QR code data", LoggerMessageSeverity_e.Debug);

                        using (var reader = new StreamReader(stream))
                        {
                            data = m_Serializer.ReadSettings<ObservableCollection<QrCodeData>>(
                                reader, new SelObjectValueSerializer(m_Logger, m_Drawing));
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
                for (int i = data.Count - 1; i >= 0; i--) 
                {
                    if (data[i].Picture == null) 
                    {
                        data.RemoveAt(i);
                        m_Logger.Log($"Removed dangling QR code data at index {i}", LoggerMessageSeverity_e.Debug);
                    }
                }
            }
            else 
            {
                data = new ObservableCollection<QrCodeData>();
            }

            data.CollectionChanged += OnDataCollectionChanged;
            
            Init(data);

            return data;
        }

        private void OnDataCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            m_HasChanges = true;
            Init((IEnumerable<QrCodeData>)sender);
        }

        private void Init(IEnumerable<QrCodeData> qrCodes)
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
                                writer, new SelObjectValueSerializer(m_Logger, m_Drawing));
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

        private void OnQrCodeChanged(QrCodeData qrCode)
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
