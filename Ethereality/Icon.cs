using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace Ethereality
{
    public class Icon : Control
    {
        static Icon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(typeof(Icon)));
            ForegroundProperty.OverrideMetadata(typeof(Icon),
                new FrameworkPropertyMetadata(
                    defaultValue: default(Brush),
                    flags: FrameworkPropertyMetadataOptions.Inherits
                    ));
        }

        public static readonly DependencyProperty CodeProperty =
            DependencyProperty.Register("Code", typeof(IconCode), typeof(Icon),
                new PropertyMetadata(default(IconCode), IconCodeChanged));

        private static void IconCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var icon = (Icon)d;
            icon.UpdateData();
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(string), typeof(Icon), new PropertyMetadata(""));


        public IconCode Code
        {
            get => (IconCode)GetValue(CodeProperty);
            set => SetValue(CodeProperty, value);
        }

        [TypeConverter(typeof(GeometryConverter))]
        public string Data
        {
            get => (string)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateData();
        }

        private void UpdateData()
        {
            if (IconData.store.TryGetValue(Code, out string? data))
                Data = data;
        }
    }

    [MarkupExtensionReturnType(typeof(Icon))]
    public class IconExtension : MarkupExtension
    {
        public IconExtension()
        { }

        public IconExtension(IconCode code)
        {
            Code = code;
        }

        [ConstructorArgument("code")]
        public IconCode Code { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var result = new Icon { Code = Code };

            return result;
        }
    }
}