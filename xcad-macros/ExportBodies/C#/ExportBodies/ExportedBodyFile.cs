using Xarial.CadPlus.Plus.Job;
using Xarial.XCad.Geometry;

namespace Xarial.CadPlus.Examples
{
    public class ExportedBodyFile : IJobItemOperationResultFile
    {
        public string Path { get; }
        public JobItemOperationResultFileStatus_e Status { get; set; }

        public IXBody Body { get; }

        public ExportedBodyFile(string path, IXBody body) 
        {
            Path = path;
            Body = body;
            Status = JobItemOperationResultFileStatus_e.Initializing;
        }
    }
}
