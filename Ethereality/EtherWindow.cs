using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shell;

namespace Ethereality
{
    [TemplatePart(Name = PART_TitleBar, Type = typeof(Border))]
    [TemplatePart(Name = PART_Title, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_SystemButtons, Type = typeof(StackPanel))]
    [TemplatePart(Name = PART_BtnMinimize, Type = typeof(Button))]
    [TemplatePart(Name = PART_BtnRestore, Type = typeof(Button))]
    [TemplatePart(Name = PART_BtnClose, Type = typeof(Button))]
    [TemplatePart(Name = PART_Content, Type = typeof(ContentPresenter))]
    public class EtherWindow : Window
    {
        public static readonly DependencyProperty HasBlurProperty =
            DependencyProperty.Register("HasBlur", typeof(bool), typeof(EtherWindow),
                new PropertyMetadata(false, OnHasBlurChanged));

        public static readonly DependencyProperty TitleBarContentProperty =
            DependencyProperty.Register("TitleBarContent", typeof(object), typeof(EtherWindow), new PropertyMetadata(null));

        private const string PART_BtnClose = "PART_BtnClose";
        private const string PART_BtnMinimize = "PART_BtnMinimize";
        private const string PART_BtnRestore = "PART_BtnRestore";
        private const string PART_SystemButtons = "PART_SystemButtons";
        private const string PART_Title = "PART_Title";
        private const string PART_TitleBar = "PART_TitleBar";
        private const string PART_Content = "PART_Content";

        readonly Storyboard closeStoryboard = new();
        bool closeStoryboardCompleted = false;

        static EtherWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EtherWindow),
                new FrameworkPropertyMetadata(typeof(EtherWindow)));
        }

        public EtherWindow()
        {
            FadeOutOnClose();
        }

        private void FadeOutOnClose()
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 250)),
            };
            Storyboard.SetTarget(fadeOut, this);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(EtherWindow.OpacityProperty));
            closeStoryboard.Children.Add(fadeOut);
            closeStoryboard.Completed += (s, e) =>
            {
                closeStoryboardCompleted = true;
                Close();
            };
            Closing += (s, e) =>
            {
                if (!closeStoryboardCompleted)
                {
                    closeStoryboard.Begin();
                    e.Cancel = true;
                }
            };
        }

        public bool HasBlur { get => (bool)GetValue(HasBlurProperty); set => SetValue(HasBlurProperty, value); }

        public object TitleBarContent
        {
            get { return (object)GetValue(TitleBarContentProperty); }
            set { SetValue(TitleBarContentProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            SetChrome();
            SetSystemButtons();
            ClearFocusOnMouseDown();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            WinAPI.FixWindowStyleNone(this);
            WinAPI.BlurBackground(this, HasBlur);
        }

        private static void OnHasBlurChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EtherWindow window)
            {
                WinAPI.BlurBackground(window, window.HasBlur);
            }
        }

        private void ClearFocusOnMouseDown()
        {
            MouseDown += (s, e) =>
            {
                FocusManager.SetFocusedElement(this, null);
                Keyboard.ClearFocus();
            };
        }

        private void SetChrome()
        {
            var chrome = new WindowChrome()
            {
                GlassFrameThickness = new(1),
                NonClientFrameEdges = NonClientFrameEdges.None,
                ResizeBorderThickness = new(5),
                UseAeroCaptionButtons = false,
            };
            var titleBar = GetTemplateChild(PART_TitleBar) as Border;
            BindingOperations.SetBinding(
                chrome,
                WindowChrome.CaptionHeightProperty,
                new Binding("ActualHeight") { Source = titleBar });
            WindowChrome.SetWindowChrome(this, chrome);
        }

        private void SetSystemButtons()
        {
            if (GetTemplateChild(PART_BtnMinimize) is Button btnMinimize)
                btnMinimize.Click += (s, e) => WindowState = WindowState.Minimized;

            if (GetTemplateChild(PART_BtnRestore) is Button btnRestore)
                btnRestore.Click += (s, e) => WindowState = WindowState switch
                {
                    WindowState.Maximized => WindowState.Normal,
                    WindowState.Normal => WindowState.Maximized,
                    _ => throw new NotImplementedException(),
                };

            if (GetTemplateChild(PART_BtnClose) is Button btnClose)
                btnClose.Click += (s, e) => Close();
        }
    }
}