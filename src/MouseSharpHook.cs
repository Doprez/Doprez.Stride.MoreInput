using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Doprez.Stride.MoreInput.MouseHelpers;
using SharpHook;
using Stride.Core.Mathematics;
using Stride.Input;

namespace Doprez.Stride.MoreInput;

public class MouseSharpHook : MouseDeviceBase, IDisposable
{
    public override bool IsPositionLocked => _isMousePositionLocked;

    public override string Name => "SharpHook Mouse";

    public override Guid Id { get; }

    public override IInputSource Source { get; }

    private readonly TaskPoolGlobalHook _hook;
    private bool _isMousePositionLocked;
    private Point _relativeCapturedPosition;
    private Point _previousPosition;

    private ConcurrentQueue<Vector2> _mouseMovements = [];
    private ConcurrentQueue<Vector2> _mouseDeltas = [];
    private ConcurrentQueue<MouseButton> _mouseButtonsPressed = [];
    private ConcurrentQueue<MouseButton> _mouseButtonsReleased = [];
    private ConcurrentQueue<float> _mouseWheelDeltas = [];
    private EventSimulator _sim;

    public MouseSharpHook(SharpHookInputSource source, TaskPoolGlobalHook hook)
    {
        Source = source;
        _sim = new EventSimulator();
        _hook = hook;

        hook.MouseMoved += OnMouseMoveEvent;
        hook.MouseDragged += OnMouseMoveEvent;
        hook.MousePressed += OnMouseInputDownEvent;
        hook.MouseReleased += OnMouseInputReleaseEvent;
        hook.MouseWheel += OnMouseWheelEvent;

        Id = InputDeviceUtils.DeviceNameToGuid(Name);

        SetSurfaceSize(new Vector2(1920, 1080));
    }

    public MouseDeviceState MouseDeviceState => MouseState;

    public Queue<Vector2> GetMouseMovements()
    {
        Queue<Vector2> movements = new Queue<Vector2>();
        while (_mouseMovements.TryDequeue(out var movement))
        {
            movements.Enqueue(movement);
        }
        _mouseMovements.Clear();
        return movements;
    }

    public Queue<Vector2> GetMouseDeltas()
    {
        Queue<Vector2> deltas = new Queue<Vector2>();
        while (_mouseDeltas.TryDequeue(out var delta))
        {
            deltas.Enqueue(delta);
        }
        _mouseDeltas.Clear();
        return deltas;
    }

    public Queue<MouseButton> GetPressedButtons()
    {
        Queue<MouseButton> buttons = new Queue<MouseButton>();
        while (_mouseButtonsPressed.TryDequeue(out var button))
        {
            buttons.Enqueue(button);
        }
        _mouseButtonsPressed.Clear();
        return buttons;
    }

    public Queue<MouseButton> GetReleasedButtons()
    {
        Queue<MouseButton> buttons = new Queue<MouseButton>();
        while (_mouseButtonsReleased.TryDequeue(out var button))
        {
            buttons.Enqueue(button);
        }
        _mouseButtonsReleased.Clear();
        return buttons;
    }

    public Queue<float> GetMouseWheelDeltas()
    {
        Queue<float> deltas = new Queue<float>();
        while (_mouseWheelDeltas.TryDequeue(out var delta))
        {
            deltas.Enqueue(delta);
        }
        _mouseWheelDeltas.Clear();
        return deltas;
    }

    private void OnMouseWheelEvent(object sender, MouseWheelHookEventArgs e)
    {
        _mouseWheelDeltas.Enqueue(e.Data.Delta);
    }

    private void OnMouseInputDownEvent(object sender, MouseHookEventArgs e)
    {
        MouseButton button = ConvertMouseButton(e.Data.Button);
        _mouseButtonsPressed.Enqueue(button);
    }

    private void OnMouseInputReleaseEvent(object sender, MouseHookEventArgs e)
    {
        MouseButton button = ConvertMouseButton(e.Data.Button);
        _mouseButtonsReleased.Enqueue(button);
    }

    private MouseButton ConvertMouseButton(SharpHook.Native.MouseButton button)
    {
        switch (button)
        {
            case SharpHook.Native.MouseButton.Button1:
                return MouseButton.Left;
            case SharpHook.Native.MouseButton.Button2:
                return MouseButton.Right;
            case SharpHook.Native.MouseButton.Button3:
                return MouseButton.Middle;
            case SharpHook.Native.MouseButton.Button4:
                return MouseButton.Extended1;
            case SharpHook.Native.MouseButton.Button5:
                return MouseButton.Extended2;
            default:
                throw new ArgumentOutOfRangeException(nameof(button), button, null);
        }
    }

    private void OnMouseMoveEvent(object sender, MouseHookEventArgs e)
    {
        var position = e.Data;

        if (IsPositionLocked)
        {
            var delta = new Vector2(position.X - _relativeCapturedPosition.X, position.Y - _relativeCapturedPosition.Y);
            _mouseDeltas.Enqueue(delta);
            _sim.SimulateMouseMovement((short)_relativeCapturedPosition.X, (short)_relativeCapturedPosition.Y);
        }
        else
        {
            // Update mouse position
            _mouseMovements.Enqueue(new Vector2(position.X, position.Y));
        }

        _previousPosition = new Point(position.X, position.Y);
    }

    public void Dispose()
    {
        _mouseMovements.Clear();
        _mouseDeltas.Clear();
        _mouseButtonsPressed.Clear();
        _mouseButtonsReleased.Clear();
        _mouseWheelDeltas.Clear();
    }

    public override void LockPosition(bool forceCenter = false)
    {
        if (!IsPositionLocked)
        {
            _relativeCapturedPosition = MouseHelper.GetCursorPosition();
            _isMousePositionLocked = true;
        }
    }

    public override void SetPosition(Vector2 normalizedPosition)
    {
        _sim.SimulateMouseMovement((short)normalizedPosition.X, (short)normalizedPosition.Y);
    }

    public override void UnlockPosition()
    {
        if (IsPositionLocked)
        {
            MouseHelper.SetCursorPosition(_relativeCapturedPosition.X, _relativeCapturedPosition.Y);
            _isMousePositionLocked = false;
            _relativeCapturedPosition = Point.Zero;
        }
    }
}
