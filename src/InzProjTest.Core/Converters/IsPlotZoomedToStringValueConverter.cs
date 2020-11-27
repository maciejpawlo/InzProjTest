using System;
using System.Globalization;
using MvvmCross.Converters;

namespace InzProjTest.Core.Converters
{
    public class IsPlotZoomedToStringValueConverter : MvxValueConverter<bool, string>
    {
        protected override string Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value)
            {
                return "Resetuj przybliżenie";
            }
            return "Przybliż wykres";
        }
    }
}
