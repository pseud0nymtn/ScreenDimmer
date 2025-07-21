using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace ScreenDimmer;

public class MouseWheelSliderBehavior : Behavior<Slider>
{
    public double Amount
    {
        get => (double)GetValue(AmountProperty);
        set => SetValue(AmountProperty, value);
    }
    public static readonly DependencyProperty AmountProperty =
        DependencyProperty.Register(
            nameof(Amount),
            typeof(double),
            typeof(MouseWheelSliderBehavior),
            new UIPropertyMetadata(0.0));

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewMouseWheel += OnPreviewMouseWheel;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PreviewMouseWheel -= OnPreviewMouseWheel;
        base.OnDetaching();
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Amount == 0.0) return;

        var slider = (Slider)sender;
        var deltaSteps = e.Delta > 0 ? Amount : -Amount;
        slider.Value = Math.Max(slider.Minimum,
                         Math.Min(slider.Maximum,
                                  slider.Value + deltaSteps));
        e.Handled = true;
    }
}
