using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Ethereality;

public class Rating : Control
{
    static Rating()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Rating), new FrameworkPropertyMetadata(typeof(Rating)));
    }

    readonly ObservableCollection<RatingButton> _internalButtons = new();
    public ReadOnlyObservableCollection<RatingButton> Buttons { get; }

    public Rating()
    {
        CommandBindings.Add(new(SelectRatingCommand, SelectRating));
        Buttons = new(_internalButtons);
    }

    public static readonly RoutedCommand SelectRatingCommand = new();
    void SelectRating(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is int value)// TODO && !IsReadOnly ?
            Value = value;
    }



    public IconCode SelectedIconCode
    {
        get => (IconCode)GetValue(SelectedIconCodeProperty);
        set => SetValue(SelectedIconCodeProperty, value);
    }
    public static readonly DependencyProperty SelectedIconCodeProperty =
        DependencyProperty.Register("SelectedIconCode", typeof(IconCode), typeof(Rating), new PropertyMetadata(IconCode.StarFilled));



    public IconCode UnSelectedIconCode
    {
        get => (IconCode)GetValue(UnSelectedIconCodeProperty);
        set => SetValue(UnSelectedIconCodeProperty, value);
    }
    public static readonly DependencyProperty UnSelectedIconCodeProperty =
        DependencyProperty.Register(
            "UnSelectedIconCode",
            typeof(IconCode),
            typeof(Rating),
            new PropertyMetadata(IconCode.StarOutline));

    public Brush SelectedColor
    {
        get => (Brush)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }
    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register("SelectedColor", typeof(Brush), typeof(Rating), new PropertyMetadata(Brushes.Orange));

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            "Value",
            typeof(int),
            typeof(Rating),
            new PropertyMetadata(default(int), ValuePropertyChanged));

    private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var rating = (Rating)d;
        if (e.NewValue is int value)
        {
            for (var i = 0; i < rating.Maximum; i++)
            {
                rating.Buttons[i].IsInValueRange = i + 1 <= value;
            }
        }
    }

    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            "Maximum",
            typeof(int),
            typeof(Rating),
            new PropertyMetadata(5, MaximumPropertyChanged));

    private static void MaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((Rating)d).RebuildButtons();
    }

    void RebuildButtons()
    {
        _internalButtons.Clear();
        int value = Value;
        for (var i = 0; i < Maximum; i++)
        {
            var button = new RatingButton
            {
                Value = i + 1,
                IsInValueRange = i + 1 <= value,
            };
            button.MouseEnter += (s, e) =>
            {
                for (var k = 0; k < Maximum; k++)
                    Buttons[k].PreviewIsInValueRange = k + 1 <= button.Value;
            };
            button.MouseLeave += (s, e) =>
            {
                foreach (var button in Buttons)
                    button.PreviewIsInValueRange = null;
            };
            _internalButtons.Add(button);
        }
    }

    public override void OnApplyTemplate()
    {
        RebuildButtons();
        base.OnApplyTemplate();
    }
}

public class RatingButton : ButtonBase
{
    static RatingButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RatingButton), new FrameworkPropertyMetadata(typeof(RatingButton)));
    }

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        internal set => SetValue(ValuePropertyKey, value);
    }
    public static readonly DependencyPropertyKey ValuePropertyKey =
        DependencyProperty.RegisterReadOnly("Value", typeof(int), typeof(RatingButton), new PropertyMetadata(default(int)));
    public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

    public bool IsInValueRange
    {
        get => (bool)GetValue(IsInValueRangeProperty);
        internal set => SetValue(IsInValueRangePropertyKey, value);
    }
    public static readonly DependencyPropertyKey IsInValueRangePropertyKey =
        DependencyProperty.RegisterReadOnly("IsInValueRange", typeof(bool), typeof(RatingButton), new PropertyMetadata(default(bool)));
    public static readonly DependencyProperty IsInValueRangeProperty = IsInValueRangePropertyKey.DependencyProperty;

    public bool? PreviewIsInValueRange
    {
        get => (bool?)GetValue(PreviewIsInValueRangeProperty);
        internal set => SetValue(PreviewIsInValueRangePropertyKey, value);
    }
    public static readonly DependencyPropertyKey PreviewIsInValueRangePropertyKey =
        DependencyProperty.RegisterReadOnly(
            "PreviewIsInValueRange",
            typeof(bool?),
            typeof(RatingButton),
            new PropertyMetadata(null));
    public static readonly DependencyProperty PreviewIsInValueRangeProperty = PreviewIsInValueRangePropertyKey.DependencyProperty;
}