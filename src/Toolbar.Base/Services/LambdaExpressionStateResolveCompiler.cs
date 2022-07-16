using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.CustomToolbar.Base;
using Xarial.CadPlus.CustomToolbar.Services;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad;

namespace Xarial.CadPlus.Toolbar.Services
{
    public class LambdaExpressionStateResolveCompiler : IStateResolveCompiler
    {
        private const string APP_VAR_NAME = "Application";

        private readonly IXApplication m_App;

        public LambdaExpressionStateResolveCompiler(IXApplication app) 
        {
            m_App = app;
        }

        public IReadOnlyDictionary<CommandMacroInfo, IToggleButtonStateResolver> CreateResolvers(IEnumerable<CommandMacroInfo> macroInfos)
        {
            var res = new Dictionary<CommandMacroInfo, IToggleButtonStateResolver>();

            foreach (var grp in macroInfos.GroupBy(m => m.ToggleButtonStateExpression)) 
            {
                var paramApp = Expression.Parameter(m_App.GetType(), APP_VAR_NAME);

                var stateResolveLambda = DynamicExpressionParser.ParseLambda(new ParameterExpression[] { paramApp, }, typeof(bool), grp.Key);
                var invoker = stateResolveLambda.Compile();

                foreach (var macroInfo in macroInfos) 
                {
                    res.Add(macroInfo, new LambdaStateResolver(m_App, invoker));
                }
            }

            return res;
        }
    }
}
