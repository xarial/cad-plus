﻿//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Common.Utils
{
    public static class FileHelper
    {
        public static bool MatchesFilter(string file, string[] filters)
        {
            if (filters?.Any() == false)
            {
                return true;
            }
            else
            {
                const string ANY_FILTER = "*";

                return filters.Any(f =>
                {
                    var regex = (f.StartsWith(ANY_FILTER) ? "" : "^")
                    + Regex.Escape(f).Replace($"\\{ANY_FILTER}", ".*").Replace("\\?", ".")
                    + (f.EndsWith(ANY_FILTER) ? "" : "$");

                    return Regex.IsMatch(file, regex, RegexOptions.IgnoreCase);
                });
            }
        }
    }
}