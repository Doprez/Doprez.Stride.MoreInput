using System.Threading.Tasks;
using SharpHook;
using Stride.Input;

namespace Doprez.Stride.MoreInput;
public class SharpHookInputSource : InputSourceBase
{
    private readonly TaskPoolGlobalHook _hook;

    private KeyboardSharpHook _keyboard;
    private MouseSharpHook _mouse;

    private Task? _hookTask;

    public SharpHookInputSource()
	{
		_hook = new TaskPoolGlobalHook();
    }

	public override void Initialize(InputManager inputManager)
    {

        // Create the keyboard device
        _keyboard = new KeyboardSharpHook(this, _hook);
        RegisterDevice(_keyboard);

        // Create the mouse device
        _mouse = new MouseSharpHook(this, _hook);
        RegisterDevice(_mouse);

        _hookTask = _hook.RunAsync();
    }

    public override void Update()
    {
        UpdateKeyboardKeys();
        UpdateMouse();
    }

    public override void Dispose()
    {
        _hookTask?.Dispose();
        base.Dispose();
    }

    private void UpdateKeyboardKeys()
    {
        var pressedKeys = _keyboard.GetPressedKeys();
        var releasedKeys = _keyboard.GetReleasedKeys();

        while (pressedKeys.Count > 0)
        {
            var key = pressedKeys.Dequeue();
            _keyboard.HandleKeyDown(key);
        }

        while (releasedKeys.Count > 0)
        {
            var key = releasedKeys.Dequeue();
            _keyboard.HandleKeyUp(key);
        }
    }

    private void UpdateMouse()
    {
        var mouseMovements = _mouse.GetMouseMovements();
        var mouseDeltas = _mouse.GetMouseDeltas();
        var pressedButtons = _mouse.GetPressedButtons();
        var releasedButtons = _mouse.GetReleasedButtons();
        var wheelDeltas = _mouse.GetMouseWheelDeltas();

        while (mouseMovements.Count > 0)
        {
            var movement = mouseMovements.Dequeue();
            _mouse.MouseDeviceState.HandleMove(movement);
        }

        while (mouseDeltas.Count > 0)
        {
            var delta = mouseDeltas.Dequeue();
            _mouse.MouseDeviceState.HandleMouseDelta(delta);
        }

        while (pressedButtons.Count > 0)
        {
            var button = pressedButtons.Dequeue();
            _mouse.MouseDeviceState.HandleButtonDown(button);
        }

        while (releasedButtons.Count > 0)
        {
            var button = releasedButtons.Dequeue();
            _mouse.MouseDeviceState.HandleButtonUp(button);
        }

        while (wheelDeltas.Count > 0)
        {
            var delta = wheelDeltas.Dequeue();
            _mouse.MouseDeviceState.HandleMouseWheel(delta);
        }
    }
}
