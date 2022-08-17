﻿//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
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
using Xarial.CadPlus.Plus.Shared.Controls;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;

namespace Xarial.CadPlus.Plus.Shared.Converters
{
    public class CadObjectTooltipTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                switch (value)
                {
                    case IXDocument doc:
                        var path = doc.Path;
                        if (!string.IsNullOrEmpty(path))
                        {
                            return path;
                        }
                        else 
                        {
                            return doc.Title;
                        }

                    case IXConfiguration conf:
                        return conf.Name;

                    case IXSheet sheet:
                        return sheet.Name;

                    case IXCutListItem cutList:
                        return cutList.Name;
                }
            }
            catch 
            {
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
