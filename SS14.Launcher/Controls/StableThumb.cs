using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace SS14.Launcher.Controls;

public sealed class StableThumb : Thumb
{
    private Point? _lastRootPoint;

    private Visual GetReferenceVisual()
    {
        return this.GetVisualRoot() as Visual ?? this;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        e.Handled = true;
        e.PreventGestureRecognition();
        e.Pointer.Capture(this);

        _lastRootPoint = e.GetPosition(GetReferenceVisual());
        PseudoClasses.Add(":pressed");

        RaiseEvent(new VectorEventArgs
        {
            RoutedEvent = DragStartedEvent,
            Vector = default
        });
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (_lastRootPoint is not { } lastRootPoint)
            return;

        var currentRootPoint = e.GetPosition(GetReferenceVisual());
        _lastRootPoint = currentRootPoint;

        e.Handled = true;

        RaiseEvent(new VectorEventArgs
        {
            RoutedEvent = DragDeltaEvent,
            Vector = currentRootPoint - lastRootPoint
        });
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (_lastRootPoint == null)
            return;

        e.Handled = true;
        _lastRootPoint = null;
        e.Pointer.Capture(null);
        PseudoClasses.Remove(":pressed");

        RaiseEvent(new VectorEventArgs
        {
            RoutedEvent = DragCompletedEvent,
            Vector = default
        });
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (_lastRootPoint != null)
        {
            _lastRootPoint = null;

            RaiseEvent(new VectorEventArgs
            {
                RoutedEvent = DragCompletedEvent,
                Vector = default
            });
        }

        PseudoClasses.Remove(":pressed");

        base.OnPointerCaptureLost(e);
    }
}
