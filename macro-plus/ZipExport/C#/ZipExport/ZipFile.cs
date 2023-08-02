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
        
        public IZipStream ZipStream { get; }

        public bool Succeeded { get; set; }
        public string TempFolder { get; }

        private readonly IXLogger m_Logger;

        public ZipFile(string path, IZipStream zipStream, IXLogger logger)
        {
            Path = path;
            ZipStream = zipStream;

            m_Logger = logger;

            Succeeded = true;
            Status = JobItemOperationResultFileStatus_e.Initializing;

            TempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
        }

        public void Dispose()
        {
            ZipStream.Dispose();
            
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
