//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Globalization;
using System.Windows.Data;
using Xarial.CadPlus.XToolbar.UI.ViewModels;

namespace Xarial.CadPlus.XToolbar.UI.Converters
{
    public enum CommandContextMenu_e
    {
        MoveUp,
        MoveDown,
        InsertBefore,
        InsertAfter,
        Remove
    }

    public class CommandContextMenuTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is CommandContextMenu_e)
            {
                var type = (CommandContextMenu_e)parameter;

                if (value != null)
                {
                    if (value.GetType() == typeof(CommandMacroVM))
                    {
                        switch (type)
                        {
                            case CommandContextMenu_e.MoveUp:
                                return "Move Left";

                            case CommandContextMenu_e.MoveDown:
                                return "Move Right";

                            case CommandContextMenu_e.InsertBefore:
                                return "Insert New Macro Button Before";

                            case CommandContextMenu_e.InsertAfter:
                                return "Insert New Macro Button After";

                            case CommandContextMenu_e.Remove:
                                return "Remove Macro Button";
                        }
                    }
                    else if (value.GetType() == typeof(CommandGroupVM))
                    {
                        switch (type)
                        {
                            case CommandContextMenu_e.MoveUp:
                                return "Move Up";

                            case CommandContextMenu_e.MoveDown:
                                return "Move Down";

                            case CommandContextMenu_e.InsertBefore:
                                return "Insert New Command Manager Before";

                            case CommandContextMenu_e.InsertAfter:
                                return "Insert New Command Manager After";

                            case CommandContextMenu_e.Remove:
                                return "Remove Command Manager";
                        }
                    }
                }
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}