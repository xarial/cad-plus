using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;

namespace Xarial.CadPlus.Plus.Modules.Drawing.FlatPatternExport
{
    public interface IFlatPatternExportData
    {
        IXSolidBody Body { get; }
        string OutFilePath { get; }
        FlatPatternOptions_e Options { get; }
        Color? Color { get; }
        double? FontSize { get; }
        FontSizeType_e FontSizeType { get; }
        string NoteText { get; }
        NoteDock_e NoteDock { get; }
        NoteOrientation_e NoteOrientation { get; }
    }

    public class FlatPatternExportData : IFlatPatternExportData
    {
        public IXSolidBody Body { get; }
        public string OutFilePath { get; }
        public FlatPatternOptions_e Options { get; }
        public Color? Color { get; }
        public double? FontSize { get; }
        public FontSizeType_e FontSizeType { get; }
        public string NoteText { get; }
        public NoteDock_e NoteDock { get; }
        public NoteOrientation_e NoteOrientation { get; }

        public FlatPatternExportData(IXSolidBody body, string outFilePath, FlatPatternOptions_e options,
            string noteText, NoteDock_e noteDock, NoteOrientation_e noteOrientation,
            Color? color, double? fontSize, FontSizeType_e fontSizeType)
        {
            Body = body;
            OutFilePath = outFilePath;
            Options = options;

            NoteText = noteText;
            NoteDock = noteDock;
            NoteOrientation = noteOrientation;

            Color = color;
            FontSize = fontSize;
            FontSizeType = fontSizeType;
        }

        public FlatPatternExportData(IXSolidBody body, string outFilePath, FlatPatternOptions_e options)
            : this(body, outFilePath, options, "", default, default, null, null, default)
        {
        }

        public FlatPatternExportData(IXSolidBody body, string outFilePath, FlatPatternOptions_e options,
            string noteText, NoteDock_e noteDock, NoteOrientation_e noteOrientation)
            : this(body, outFilePath, options, noteText, noteDock, noteOrientation, null, null, default)
        {
        }

        public FlatPatternExportData(IXSolidBody body, string outFilePath, FlatPatternOptions_e options,
            string noteText, NoteDock_e noteDock, NoteOrientation_e noteOrientation, Color? color)
            : this(body, outFilePath, options, noteText, noteDock, noteOrientation, color, null, default)
        {
        }

    }

    public interface IDrawingFlatPatternExportModule : IModule
    {
        IReadOnlyList<IBatchJobItem> BatchExportFlatPatterns(IXDocument3D doc, IReadOnlyList<IFlatPatternExportData> exportData);
    }
}
