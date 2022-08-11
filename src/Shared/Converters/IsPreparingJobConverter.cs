using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Xarial.CadPlus.Plus.Services;
using Xarial.XToolkit.Wpf.Converters;

namespace Xarial.CadPlus.Plus.Shared.Converters
{
    public class IsPreparingJobConverter : BooleanUniversalConverter
    {
        public IsPreparingJobConverter() 
        {
            TrueValue = Visibility.Visible;
            FalseValue = Visibility.Collapsed;
        }

        protected override bool? ConvertValueToBool(object value)
        {
            if (value is JobStatus_e?)
            {
                var status = (JobStatus_e?)value;

                return !status.HasValue || status == JobStatus_e.Initializing;
            }
            else
            {
                return true;
            }
        }
        //public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    if (value is JobStatus_e?)
        //    {
        //        var status = (JobStatus_e?)value;

        //        return !status.HasValue || status == JobStatus_e.Initializing;
        //    }
        //    else 
        //    {
        //        return true;
        //    }
        //}

        //public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
