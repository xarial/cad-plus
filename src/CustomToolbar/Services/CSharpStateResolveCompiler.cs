//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using Xarial.XCad;

namespace Xarial.CadPlus.CustomToolbar.Services
{
    public partial class CommandsManager
    {
        public class CSharpStateResolveCompiler : RoslynStateResolveCompiler
        {
            public CSharpStateResolveCompiler(string codeTemplate, IXApplication app) : base(codeTemplate, app)
            {
            }

            protected override Compilation CreateCompilation(IEnumerable<SyntaxTree> code, string dllName, IEnumerable<MetadataReference> refs, CompilationOptions opts)
                => CSharpCompilation.Create(dllName, code, refs, (CSharpCompilationOptions)opts);

            protected override CompilationOptions CreateCompilationOptions()
                => new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            protected override SyntaxTree CreateSyntaxTree(SourceText src)
                => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseSyntaxTree(src,
                    CSharpParseOptions.Default.WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp6), "");

            protected override IEnumerable<MetadataReference> GetReferences()
            {
                yield return MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location);
                yield return MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.BinaryExpression).Assembly.Location);
            }
        }
    }
}
