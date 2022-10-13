using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;

namespace Xarial.CadPlus.Plus.Modules.Drawing.FlatPatternExport
{
    public interface FlatPatternExportData 
    {
        IXSolidBody Body { get; }
        string OutFilePath { get; }
        FlatPatternOptions_e Options { get; }
        double? FontSize { get; }
        FontSizeType_e FontSizeType { get; }
        string NoteText { get; }
        NoteDock_e NoteDock { get; }
        NoteOrientation_e NoteOrientation { get; }
    }

    public interface IDrawingFlatPatternExportModule : IModule
    {
        IReadOnlyList<IBatchJobItem> BatchExportFlatPattern(IXDocument3D doc, IReadOnlyList<FlatPatternExportData> exportData);
    }
}
