//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Plus.Services
{
    /// <summary>
    /// Dual document for acessing metadata
    /// </summary>
    public interface IMetadataDocumentWrapper : IDisposable
    {
        /// <summary>
        /// Actual document - may be an original document if it is accessible and loaded or fallback version of the document
        /// </summary>
        IXDocument Document { get; }

        /// <summary>
        /// Indicates if the document is replaced document or an original document
        /// </summary>
        bool IsFallbackDocument { get; }

        /// <summary>
        /// Apply changes to the document
        /// </summary>
        void ApplyChanges();
    }

    /// <summary>
    /// Services allows to automatically create fallback version of the document to access lightweight metadata document
    /// </summary>
    /// <remarks>For example if SOLIDWORKS document is not loaded (lightweight, view only) or disconnected (closed) this methdo will create Document Manager version of the document</remarks>
    public interface IMetadataDocumentWrapperProvider
    {
        IMetadataDocumentWrapper Create(IXDocument doc, bool allowReadOnly);
    }
}
