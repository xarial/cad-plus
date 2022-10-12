using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Geometry;

namespace Xarial.CadPlus.Plus.Modules.Drawing.FlatPatternExport
{
    public interface IDrawingFlatPatternExportModule : IModule
    {
        FlatPatternBodyType_e GetType(IXSolidBody body);
        void ExportFlatPattern(IXSolidBody body, string outFilePath, FlatPatternOptions_e options, double? fontSize = null,
            FontSizeType_e fontSizeType = FontSizeType_e.Points, string noteText = "", NoteDock_e noteDock = NoteDock_e.Center, NoteOrientation_e noteOrientation = NoteOrientation_e.Auto);
    }
}
