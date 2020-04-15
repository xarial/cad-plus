using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Xarial.XTools.Xport.UI
{
    public class FileFilter
    {
        public static FileFilter AllFiles { get; } = new FileFilter("All Files", "*.*");
        public static FileFilter ImageFiles { get; } = new FileFilter("Image Files", "*.bmp", "*.png", "*.jpg", "*.jpeg", "*.gif", "*.tif", "*.tiff");

        public string Name { get; }
        public string[] Extensions { get; }
        
        public FileFilter(string name, params string[] exts)
        {
            Name = name;
            Extensions = exts;
        }
    }

    public static class FsoBrowser
    {
        public static bool BrowseForFolder(out string path, string desc = "") 
        {
            var dlg = new FolderBrowserDialog();
            dlg.Description = desc;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                path = dlg.SelectedPath;
                return true;
            }
            else 
            {
                path = "";
                return false;
            }
        }

        public static string BuildFilterString(params FileFilter[] filters) 
        {
            return string.Join("|", filters.Select(f =>
            {
                var exts = string.Join(";", f.Extensions);
                return $"{f.Name} ({exts})|{exts}";
            }));
        }

        public static bool BrowseForFileOpen(out string path, string title = "", string filter = "")
        {
            return BrowseForFile(out path, new OpenFileDialog(), title, filter);
        }

        public static bool BrowseForFileSave(out string path, string title = "", string filter = "")
        {
            return BrowseForFile(out path, new SaveFileDialog(), title, filter);
        }

        private static bool BrowseForFile(out string path, FileDialog dlg, string title, string filter)
        {
            dlg.Filter = filter;
            dlg.Title = title;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                path = dlg.FileName;
                return true;
            }
            else
            {
                path = "";
                return false;
            }
        }
    }
}
