using System;
using System.Globalization;
using MvvmCross.Converters;

namespace InzProjTest.Core.Converters
{
    public class DoubleRpmToStringValueConverter : MvxValueConverter<double, string>
    {
        protected override string Convert(double value, Type targetType, object parameter, CultureInfo culture)
        {
            return value + " RPM";
        }
    }
}
