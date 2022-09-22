using System;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Ethereality
{
    public class X : DependencyObject
    {
        private static DependencyObject? GetParentFromVisualTree(object source)
        {
            DependencyObject? parent = source as UIElement;
            while (parent != null && parent is not TextBox)
                parent = VisualTreeHelper.GetParent(parent);
            return parent;
        }

        #region Icon

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached(
                "Icon",
                typeof(Icon),
                typeof(X),
                new PropertyMetadata(null));

        public static Icon GetIcon(TextBox obj) => (Icon)obj.GetValue(IconProperty);

        public static void SetIcon(TextBox obj, Icon value) => obj.SetValue(IconProperty, value);

        #endregion Icon

        #region Hint

        public static readonly DependencyProperty HintProperty =
            DependencyProperty.RegisterAttached(
                "Hint",
                typeof(string),
                typeof(X),
                new PropertyMetadata(null));

        public static string GetHint(TextBox obj) => (string)obj.GetValue(HintProperty);

        public static void SetHint(TextBox obj, string value) => obj.SetValue(HintProperty, value);

        #endregion Hint

        #region PostContent
        public static object GetPostContent(TextBox obj)
            => (object)obj.GetValue(PostContentProperty);
        public static void SetPostContent(TextBox obj, object value)
            => obj.SetValue(PostContentProperty, value);
        public static readonly DependencyProperty PostContentProperty =
            DependencyProperty.RegisterAttached("PostContent", typeof(object), typeof(X), new PropertyMetadata(default));
        #endregion

        #region Select Text On Focus

        public static readonly DependencyProperty SelectTextOnFocusProperty =
            DependencyProperty.RegisterAttached(
                "SelectTextOnFocus",
                typeof(bool),
                typeof(X),
                new PropertyMetadata(false, SelectTextOnFocusChanged));

        public static bool GetSelectTextOnFocus(TextBox obj) => (bool)obj.GetValue(SelectTextOnFocusProperty);

        public static void SetSelectTextOnFocus(TextBox obj, bool value) => obj.SetValue(SelectTextOnFocusProperty, value);

        private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.OriginalSource is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dependencyObject = GetParentFromVisualTree(e.OriginalSource);

            if (dependencyObject is TextBox textBox
                && !textBox.IsKeyboardFocusWithin)
            {
                textBox.Focus();
                e.Handled = true;
            }
        }

        private static void SelectTextOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((e.NewValue as bool?).GetValueOrDefault(false))
                {
                    textBox.GotKeyboardFocus += OnGotKeyboardFocus;
                    textBox.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
                }
                else
                {
                    textBox.GotKeyboardFocus -= OnGotKeyboardFocus;
                    textBox.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
                }
            }
        }

        #endregion Select Text On Focus

        #region Highlight Text
        public static string GetSelection(TextBlock obj) => (string)obj.GetValue(SelectionProperty);
        public static void SetSelection(TextBlock obj, string value) => obj.SetValue(SelectionProperty, value);
        public static readonly DependencyProperty SelectionProperty =
            DependencyProperty.RegisterAttached(
                "Selection",
                typeof(string),
                typeof(X),
                new PropertyMetadata(new PropertyChangedCallback(SelectText)));

        public static string GetReplacement(TextBlock obj) => (string)obj.GetValue(ReplacementProperty);
        public static void SetReplacement(TextBlock obj, string value) => obj.SetValue(ReplacementProperty, value);
        public static readonly DependencyProperty ReplacementProperty =
            DependencyProperty.RegisterAttached(
                "Replacement",
                typeof(string),
                typeof(X),
                new PropertyMetadata(new(SelectText)));

        private static void SelectText(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (d == null) return;
                if (d is not TextBlock txtBlock) throw new InvalidOperationException("Only valid for TextBlock");

                string text = txtBlock.Text;
                if (string.IsNullOrEmpty(text)) return;

                string highlightText = (string)d.GetValue(SelectionProperty);
                if (string.IsNullOrEmpty(highlightText)) return;


                var matches = Regex.Matches(text, highlightText, RegexOptions.IgnoreCase);
                if (!matches.Any()) return;
                if (matches.Count >= text.Length) return;

                Brush highlightBackground = (Brush)d.GetValue(HighlightBackgroundProperty);
                Brush highlightForeground = (Brush)d.GetValue(HighlightForegroundProperty);

                string? replacement = (string)d.GetValue(ReplacementProperty);
                Brush replacementBackground = (Brush)d.GetValue(ReplacementBackgroundProperty);
                Brush replacementForeground = (Brush)d.GetValue(ReplacementForegroundProperty);

                txtBlock.Inlines.Clear();
                int index = 0;
                foreach (Match match in matches)
                {
                    txtBlock.Inlines.Add(new Run(text[index..match.Index]));
                    txtBlock.Inlines.Add(new Run(text[match.Index..(match.Index + match.Length)])
                    {
                        Background = highlightBackground,
                        Foreground = highlightForeground,
                    });
                    if (!string.IsNullOrEmpty(replacement))
                    {
                        var repl = Regex.Replace(
                            text[match.Index..(match.Index + match.Length)],
                            highlightText,
                            replacement,
                            RegexOptions.IgnoreCase);
                        txtBlock.Inlines.Add(new Run(repl)
                        {
                            Background = replacementBackground,
                            Foreground = replacementForeground,
                        });
                    }
                    index = match.Index + match.Length;
                }
                txtBlock.Inlines.Add(new Run(text[index..]));
            }
            catch
            {

            }
        }

        public static Brush GetHighlightBackground(DependencyObject obj)
            => (Brush)obj.GetValue(HighlightBackgroundProperty);
        public static void SetHighlightBackground(DependencyObject obj, Brush value)
            => obj.SetValue(HighlightBackgroundProperty, value);
        public static readonly DependencyProperty HighlightBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "HighlightBackground",
                typeof(Brush),
                typeof(X),
                new PropertyMetadata(Brushes.Transparent, new PropertyChangedCallback(SelectText)));

        public static Brush GetHighlightForeground(DependencyObject obj)
            => (Brush)obj.GetValue(HighlightForegroundProperty);
        public static void SetHighlightForeground(DependencyObject obj, Brush value)
            => obj.SetValue(HighlightForegroundProperty, value);
        public static readonly DependencyProperty HighlightForegroundProperty =
            DependencyProperty.RegisterAttached(
                "HighlightForeground",
                typeof(Brush),
                typeof(X),
                new PropertyMetadata(Brushes.Red, new PropertyChangedCallback(SelectText)));

        public static Brush GetReplacementBackground(DependencyObject obj)
            => (Brush)obj.GetValue(ReplacementBackgroundProperty);
        public static void SetReplacementBackground(DependencyObject obj, Brush value)
            => obj.SetValue(ReplacementBackgroundProperty, value);
        public static readonly DependencyProperty ReplacementBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "ReplacementBackground",
                typeof(Brush),
                typeof(X),
                new PropertyMetadata(Brushes.Transparent, new(SelectText)));

        public static Brush GetReplacementForeground(DependencyObject obj)
        => (Brush)obj.GetValue(ReplacementForegroundProperty);
        public static void SetReplacementForeground(DependencyObject obj, Brush value)
        => obj.SetValue(ReplacementForegroundProperty, value);
        public static readonly DependencyProperty ReplacementForegroundProperty =
            DependencyProperty.RegisterAttached(
                "ReplacementForeground",
                typeof(Brush),
                typeof(X),
                new PropertyMetadata(Brushes.Yellow, new(SelectText)));
        #endregion
    }
}