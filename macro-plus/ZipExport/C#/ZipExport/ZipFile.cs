using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Job;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;

namespace Xarial.CadPlus.Examples
{
    public class ZipFile : IJobItemOperationResultFile, IDisposable
    {
        public string Path { get; }
        public JobItemOperationResultFileStatus_e Status { get; set; }

        public IZipStream ZipStream => m_ZipStreamLazy.Value;

        public bool Succeeded { get; set; }
        public string TempFolder { get; }

        private readonly IXLogger m_Logger;

        private readonly Lazy<IZipStream> m_ZipStreamLazy;

        public ZipFile(string path, Lazy<IZipStream> zipStreamLazy, IXLogger logger)
        {
            Path = path;
            m_ZipStreamLazy = zipStreamLazy;

            m_Logger = logger;

            Succeeded = true;
            Status = JobItemOperationResultFileStatus_e.Initializing;

            TempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
        }

        public void Dispose()
        {
            if (m_ZipStreamLazy.IsValueCreated)
            {
                ZipStream.Dispose();
            }
            
            try
            {
                Directory.Delete(TempFolder);
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
            }
        }
    }
}
