using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Modules.Toolbar;
using Xarial.XToolkit;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Plus.Samples
{
    [Module(typeof(IHostCadExtension))]
    public class ToolbarFileBrowserModule : IModule
    {
        private const string ARG_FILE_SAVE_BROWSE = "--filesave--";
        private const string ARG_FILE_OPEN_BROWSE = "--fileopen--";
        private const string ARG_FOLDER_BROWSE = "--folder--";

        private IToolbarModule m_Toolbar;

        public void Init(IHost host)
        {
            host.Initialized += OnHostInitialized;
        }

        private void OnHostInitialized(IApplication app, IServiceProvider svcProvider, IModuleCollection modules)
        {
            m_Toolbar = modules.Get<IToolbarModule>();

            m_Toolbar.MacroRunning += OnToolbarMacroRunning;
        }

        private void OnToolbarMacroRunning(EventType_e eventType, MacroRunningArguments args)
        {
            var macroArgs = new List<string>(args.MacroInfo.Arguments ?? Enumerable.Empty<string>());

            if (macroArgs.Any()) 
            {
                for (int i = 0; i < macroArgs.Count; i++) 
                {
                    if (string.Equals(macroArgs[i], ARG_FILE_SAVE_BROWSE, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (FileSystemBrowser.BrowseFileSave(out var path, $"Select file for the argument #{i + 1}", FileFilter.BuildFilterString(FileFilter.AllFiles)))
                        {
                            macroArgs[i] = path;
                        }
                        else
                        {
                            args.Cancel = true;
                            return;
                        }
                    }
                    else if (string.Equals(macroArgs[i], ARG_FILE_OPEN_BROWSE, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (FileSystemBrowser.BrowseFileOpen(out var path, $"Select file for the argument #{i + 1}", FileFilter.BuildFilterString(FileFilter.AllFiles)))
                        {
                            macroArgs[i] = path;
                        }
                        else
                        {
                            args.Cancel = true;
                            return;
                        }
                    }
                    else if (string.Equals(macroArgs[i], ARG_FOLDER_BROWSE, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (FileSystemBrowser.BrowseFolder(out var path, $"Select folder for the argument #{i + 1}"))
                        {
                            macroArgs[i] = path;
                        }
                        else
                        {
                            args.Cancel = true;
                            return;
                        }
                    }
                }

                args.MacroInfo = new MacroInfo(args.MacroInfo, macroArgs);
            }
        }

        public void Dispose()
        {
        }
    }
}
