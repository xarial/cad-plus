//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using EPDM.Interop.epdm;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xarial.CadPlus.Drawing.QrCode.Data;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XToolkit.Services.Expressions;

namespace Xarial.CadPlus.Drawing.QrCode.Services
{
    public class QrDataProvider
    {
        private readonly IXApplication m_App;

        private readonly IDocumentMetadataAccessLayerProvider m_DocMalProvider;

        private readonly IXLogger m_Logger;

        private readonly QrCodeDataSourceExpressionSolver m_Solver;

        private readonly IExpressionParser m_ExpParser;

        public QrDataProvider(IXApplication app, IXLogger logger, IDocumentMetadataAccessLayerProvider docMalProvider, IExpressionParser expParser)
        {
            m_App = app;
            m_DocMalProvider = docMalProvider;
            m_Logger = logger;
            m_ExpParser = expParser;

            m_Solver = new QrCodeDataSourceExpressionSolver();
        }

        public string GetData(IXDrawing drw, string expression)
        {
            var token = m_ExpParser.Parse(expression);

            using (var scopedDoc = new DataSourceDocument(m_DocMalProvider, m_App, drw, m_Logger))
            {
                return m_Solver.Solve(token, scopedDoc);
            }
        }
    }
}
