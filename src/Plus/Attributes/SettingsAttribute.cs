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

namespace Xarial.CadPlus.Plus.Attributes
{
    /// <summary>
    /// Attribute marks the specific type as settings
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingsAttribute : Attribute
    {
        /// <summary>
        /// Name of the settings file
        /// </summary>
        /// <remarks>Sub-folder names are supported</remarks>
        public string SettingsFileName { get; }

        /// <summary>
        /// List of alternative names for setting files
        /// </summary>
        /// <remarks>Use this to load data from legacy setting files</remarks>
        public string[] AltSettsFileNames { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SettingsAttribute(string settsFileName, params string[] altSettsFileNames) 
        {
            SettingsFileName = settsFileName;
            AltSettsFileNames = altSettsFileNames;
        }
    }
}
