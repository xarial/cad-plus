//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;

namespace Xarial.CadPlus.Xport.SwEDrawingsHost
{
    public enum EDrawingsVersion_e 
    {
        v2019,
        v2020,
        v2021,
        v2022,
        Default
    }

    public enum EDrawingsPrintType_e
    {
        WYSIWYG,
        ScaleToFit,
        OneToOne,
        PrintSelection,
        Scaled
    }

    public interface IEDrawingsControl 
    {
        event Action<string> OnFinishedLoadingDocument;
        event Action<string, int, string> OnFailedLoadingDocument;
        event Action OnFinishedSavingDocument;
        event Action<string, int, string> OnFailedSavingDocument;
        event Action<string> OnFinishedPrintingDocument;
        event Action<string> OnFailedPrintingDocument;

        EDrawingsVersion_e Version { get; }

        void OpenDoc(string fileName, bool isTemp, bool promptToSave, bool readOnly, string commandString);

        void Save(string saveName, bool saveAs, string commandString);

        void Print5(bool showDialog, string fileNameInPrintQueue, bool shaded, bool draftQuality, bool color,
            EDrawingsPrintType_e printType, double scale, int centerOffsetX, int centerOffsetY,
            bool printAll, int pageFirst, int pageLast, string printToFileName);

        int EnableFeatures { get; set; }
        string FileName { get; }
        void CloseActiveDoc(string commandString);

    }

    internal class EDrawingsControl : IEDrawingsControl
    {
        private enum EDrawingsEventDispId 
        {
            FinishedLoadingDocument = 3,
            FinishedSavingDocument = 4,
            FailedLoadingDocument = 5,
            FailedSavingDocument = 6,
            FinishedPrintingDocument = 7,
            FailedPrintingDocument = 8,
        }

        internal static string GetOcxGuid(EDrawingsVersion_e vers) 
        {
            switch (vers) 
            {
                case EDrawingsVersion_e.Default:
                    return "22945A69-1191-4DCF-9E6F-409BDE94D101";
                case EDrawingsVersion_e.v2019:
                    return "0DD2B893-45A4-473B-A464-82B578AAF383";
                case EDrawingsVersion_e.v2020:
                    return "0FEA599D-6369-4811-8D00-E52B8A59C901";
                case EDrawingsVersion_e.v2021:
                    return "DF78AAAB-45C8-420C-9DB3-CAD13762FE35";
                case EDrawingsVersion_e.v2022:
                    return "C59EEF21-0223-4C39-A708-A3BE9008C67E";
                default:
                    throw new NotSupportedException("This version of eDrawings is not supported");
            }
        }

        private static Guid GetEventsGuid(EDrawingsVersion_e vers)
        {
            switch (vers)
            {
                case EDrawingsVersion_e.v2019:
                    return new Guid("3BFB4A26-490D-4BBC-8C19-ED970CF4441D");
                case EDrawingsVersion_e.v2020:
                    return new Guid("18ADE509-EA30-4084-BF7A-6FA2C2D65A77");
                case EDrawingsVersion_e.v2021:
                    return new Guid("CEE0D6AD-C251-430C-BA88-FE237B138B91");
                case EDrawingsVersion_e.v2022:
                    return new Guid("2EA4AE0F-494C-4554-97AE-F02F3076A90D");
                default:
                    throw new NotSupportedException("This version of eDrawings is not supported");
            }
        }

        public EDrawingsVersion_e Version { get; }

        public string FileName => ((dynamic)m_Ocx).FileName;

        public int EnableFeatures 
        {
            get => ((dynamic)m_Ocx).EnableFeatures;
            set => ((dynamic)m_Ocx).EnableFeatures = value;
        }

        private readonly object m_Ocx;

        public event Action<string> OnFinishedLoadingDocument 
        {
            add => AttachEvent(value, EDrawingsEventDispId.FinishedLoadingDocument);
            remove => DetachEvent(value, EDrawingsEventDispId.FinishedLoadingDocument);
        }

        public event Action<string, int, string> OnFailedLoadingDocument
        {
            add => AttachEvent(value, EDrawingsEventDispId.FailedLoadingDocument);
            remove => DetachEvent(value, EDrawingsEventDispId.FailedLoadingDocument);
        }

        public event Action OnFinishedSavingDocument
        {
            add => AttachEvent(value, EDrawingsEventDispId.FinishedSavingDocument);
            remove => DetachEvent(value, EDrawingsEventDispId.FinishedSavingDocument);
        }
        
        public event Action<string, int, string> OnFailedSavingDocument
        {
            add => AttachEvent(value, EDrawingsEventDispId.FailedSavingDocument);
            remove => DetachEvent(value, EDrawingsEventDispId.FailedSavingDocument);
        }

        public event Action<string> OnFinishedPrintingDocument
        {
            add => AttachEvent(value, EDrawingsEventDispId.FinishedPrintingDocument);
            remove => DetachEvent(value, EDrawingsEventDispId.FinishedPrintingDocument);
        }

        public event Action<string> OnFailedPrintingDocument
        {
            add => AttachEvent(value, EDrawingsEventDispId.FailedPrintingDocument);
            remove => DetachEvent(value, EDrawingsEventDispId.FailedPrintingDocument);
        }

        public EDrawingsControl(object ocx) 
        {
            if (ocx == null) 
            {
                throw new NullReferenceException(nameof(ocx));
            }

            m_Ocx = ocx;

            Version = GetVersion();
        }

        private EDrawingsVersion_e GetVersion() 
        {
            string buildNumber = ((dynamic)m_Ocx).BuildNumber;

            var majorVer = int.Parse(buildNumber.Split('.').First());

            switch (majorVer) 
            {
                case 27:
                    return EDrawingsVersion_e.v2019;
                case 28:
                    return EDrawingsVersion_e.v2020;
                case 29:
                    return EDrawingsVersion_e.v2021;
                case 30:
                    return EDrawingsVersion_e.v2022;
                default:
                    throw new NotSupportedException($"Version of eDrawings '{buildNumber}' is not supported");
            }
        }

        public void OpenDoc(string fileName, bool isTemp, bool promptToSave, bool readOnly, string commandString)
        {
            ((dynamic)m_Ocx).OpenDoc(fileName, isTemp, promptToSave, readOnly, commandString);
        }

        public void Save(string saveName, bool saveAs, string commandString)
        {
            ((dynamic)m_Ocx).Save(saveName, saveAs, commandString);
        }

        public void Print5(bool showDialog, string fileNameInPrintQueue, bool shaded, bool draftQuality, bool color, EDrawingsPrintType_e printType, 
            double scale, int centerOffsetX, int centerOffsetY, bool printAll, int pageFirst, int pageLast, string printToFileName)
        {
            ((dynamic)m_Ocx).Print5(showDialog, fileNameInPrintQueue, shaded, draftQuality, color, printType,
                scale, centerOffsetX, centerOffsetY, printAll, pageFirst, pageLast, printToFileName);
        }

        public void CloseActiveDoc(string commandString)
        {
            ((dynamic)m_Ocx).CloseActiveDoc(commandString);
        }

        private void AttachEvent(Delegate value, EDrawingsEventDispId eventId)
        {
            System.Runtime.InteropServices.ComEventsHelper.Combine(
                m_Ocx,
                GetEventsGuid(Version), (int)eventId, value);
        }

        private void DetachEvent(Delegate value, EDrawingsEventDispId eventId)
        {
            System.Runtime.InteropServices.ComEventsHelper.Remove(
                m_Ocx, GetEventsGuid(Version), (int)eventId, value);
        }
    }
}
