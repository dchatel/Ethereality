using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Ethereality.Converters
{
    public class InternalVisibilityConverter : IValueConverter
    {
        private Visibility VisibilityForFalse = Visibility.Collapsed;
        private Visibility VisibilityForTrue = Visibility.Visible;

        public InternalVisibilityConverter(bool reversed)
        {
            if (reversed)
            {
                VisibilityForTrue = Visibility.Collapsed;
                VisibilityForFalse = Visibility.Visible;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return b ? VisibilityForTrue : VisibilityForFalse;
            if (value is string s)
            {
                if (parameter is string p)
                    return s == p ? VisibilityForTrue : VisibilityForFalse;
                else
                    return !string.IsNullOrEmpty(s) ? VisibilityForTrue : VisibilityForFalse;
            }
            return value is not null ? VisibilityForTrue : VisibilityForFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [MarkupExtensionReturnType(typeof(IValueConverter))]
    public class VisibilityConverter : MarkupExtension
    {
        public VisibilityConverter()
        { }

        public VisibilityConverter(bool reversed)
        {
            Reversed = reversed;
        }

        [ConstructorArgument("Reversed")]
        public bool Reversed { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new InternalVisibilityConverter(Reversed);
        }
    }
}