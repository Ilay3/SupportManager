using System;
using System.Globalization;
using System.Windows.Data;

namespace SupportManager.Converters
{
    public class NullableBoolToRadioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return false;

            if (value == null && parameter.ToString() == "None")
                return true;

            if (value is bool boolValue && bool.TryParse(parameter.ToString(), out var paramValue))
                return boolValue == paramValue;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == false) return Binding.DoNothing;

            if (parameter.ToString() == "None") return null;

            if (bool.TryParse(parameter.ToString(), out var paramValue))
                return paramValue;

            return null;
        }
    }
}
