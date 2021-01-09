using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MvvmCross.Converters;

namespace InzProjTest.Core.Converters
{
    public class DoubleFrequencyToStringValueConverter : MvxValueConverter<double, string>
    {
        protected override string Convert(double value, Type targetType, object parameter, CultureInfo culture)
        {
            return value + " Hz";
        } 
    }
}
