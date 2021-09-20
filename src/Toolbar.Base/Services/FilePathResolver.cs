using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit;

namespace Xarial.CadPlus.Toolbar.Services
{
    public interface IFilePathResolver 
    {
        string Resolve(string path, string workDir);
    }

    public class FilePathResolver : IFilePathResolver
    {
        public string Resolve(string path, string workDir)
        {
            try
            {
                if (!Path.IsPathRooted(path))
                {
                    return FileSystemUtils.CombinePaths(workDir, path);
                }
                else
                {
                    return path;
                }
            }
            catch 
            {
                return path;
            }
        }
    }
}
