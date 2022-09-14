//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Xarial.CadPlus.Batch.StandAlone.Properties;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.StandAlone.Converters
{
    public class FileFolderIconConverter : IValueConverter
    {
        private static readonly BitmapImage m_FileIcon;
        private static readonly BitmapImage m_FolderIcon;
        private static readonly BitmapImage m_UnknownIcon;

        static FileFolderIconConverter() 
        {
            m_FileIcon = Resources.file_icon.ToBitmapImage();
            m_FolderIcon = Resources.folder_icon.ToBitmapImage();
            m_UnknownIcon = Resources.missing_fso.ToBitmapImage();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!string.IsNullOrEmpty(value as string))
            {
                try
                {
                    if (File.Exists(value as string))
                    {
                        return m_FileIcon;
                    }
                    else if (Directory.Exists(value as string))
                    {
                        return m_FolderIcon;
                    }
                    else 
                    {
                        return m_UnknownIcon;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
