//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xarial.CadPlus.Batch.Base.Extensions
{
    public class DataGridExtension
    {
        public static readonly DependencyProperty CellEditEndingCommandProperty = DependencyProperty.RegisterAttached(
              "CellEditEndingCommand",
              typeof(ICommand),
              typeof(DataGridExtension),
              new FrameworkPropertyMetadata(null, OnCellEditEndingCommandChanged));

        public static void SetCellEditEndingCommand(UIElement element, ICommand value)
        {
            element.SetValue(CellEditEndingCommandProperty, value);
        }

        public static ICommand GetCellEditEndingCommand(UIElement element)
        {
            return (ICommand)element.GetValue(CellEditEndingCommandProperty);
        }

        private static void OnCellEditEndingCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
        {
            var dataGrid = d as DataGrid;

            if (dataGrid != null && e.NewValue is ICommand) 
            {
                dataGrid.CellEditEnding += (o, ce) =>
                {
                    (e.NewValue as ICommand).Execute(ce);
                };
            }
        }
    }
}
