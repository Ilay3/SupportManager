// Converters/NullableBoolToRadioConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;

namespace SupportManager.Converters
{
    public class NullableBoolToRadioConverter : IValueConverter
    {
        // ConverterParameter ожидает "True", "False" или "None"
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            string? param = parameter as string;
            if (param == "None")
            {
                return value == null;
            }
            else if (bool.TryParse(param, out bool boolParam))
            {
                if (value is bool boolValue)
                {
                    return boolValue == boolParam;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? param = parameter as string;
            if (value is bool isChecked)
            {
                if (isChecked)
                {
                    if (param == "None")
                        return null;
                    else if (bool.TryParse(param, out bool boolParam))
                        return boolParam;
                }
            }
            return Binding.DoNothing;
        }
    }
}
