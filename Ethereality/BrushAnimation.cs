using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Ethereality
{
    public class BrushAnimation : AnimationTimeline
    {
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(Brush), typeof(BrushAnimation));

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(Brush), typeof(BrushAnimation));

        private VisualBrush _brush = null!;
        private Border _primary = null!;
        private Border _secondary = null!;

        //we must define From and To, AnimationTimeline does not have this properties
        public Brush From
        {
            get { return (Brush)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        public override Type TargetPropertyType
        {
            get
            {
                return typeof(Brush);
            }
        }

        public Brush To
        {
            get { return (Brush)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        public override object GetCurrentValue(object defaultOriginValue,
                                                       object defaultDestinationValue,
                                               AnimationClock animationClock)
        {
            if (defaultOriginValue is Brush originBrush)
            {
                if (defaultDestinationValue is Brush destinationBrush)
                {
                    return GetCurrentValue(originBrush,
                                           destinationBrush,
                                           animationClock);
                }
            }
            return null!; // Should never happen
        }

        public object GetCurrentValue(Brush defaultOriginValue,
                                      Brush defaultDestinationValue,
                                      AnimationClock animationClock)
        {
            if (!animationClock.CurrentProgress.HasValue)
                return Brushes.Transparent;

            //use the standard values if From and To are not set
            //(it is the value of the given property)
            defaultOriginValue = this.From ?? defaultOriginValue;
            defaultDestinationValue = this.To ?? defaultDestinationValue;

            if (animationClock.CurrentProgress.Value == 0)
                return defaultOriginValue;
            if (animationClock.CurrentProgress.Value == 1)
                return defaultDestinationValue;

            if (_brush is null)
            {
                _primary = new Border()
                {
                    Width = 1,
                    Height = 1,
                    Background = defaultOriginValue,
                    Opacity = 1 - animationClock.CurrentProgress.Value,
                };
                _secondary = new Border()
                {
                    Width = 1,
                    Height = 1,
                    Background = defaultDestinationValue,
                    Opacity = animationClock.CurrentProgress.Value,
                };
                var grid = new Grid();
                grid.Children.Add(_primary);
                grid.Children.Add(_secondary);
                _brush = new VisualBrush(grid);
            }
            else
            {
                _primary.Opacity = 1 - animationClock.CurrentProgress.Value;
                _secondary.Opacity = animationClock.CurrentProgress.Value;
            }
            return _brush;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BrushAnimation();
        }
    }
}