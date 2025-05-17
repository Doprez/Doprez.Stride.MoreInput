using DevDecoder.HIDDevices;
using DevDecoder.HIDDevices.Controllers;
using Microsoft.Extensions.Logging;
using Stride.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Doprez.Stride.MoreInput;

public class HIDDeviceInputSource : InputSourceBase
{
    private readonly Devices _devices = new();
    private readonly Dictionary<string, GameControllerHIDDevice> _gamePads = [];

    private InputManager _inputManager;

    public override void Initialize(InputManager inputManager)
    {
        _inputManager = inputManager;
        Scan();
    }

    public override void Update()
    {
        Scan();
    }

    public override void Scan()
    {
        using var subscription = _devices.Controllers<Gamepad>().Subscribe(g =>
        {
            //Dont register the same device twice.
            if (_gamePads.ContainsKey(g.Device.DevicePath))
            {
                return;
            }

            if (g.Name.ToLowerInvariant().Contains("xbox "))
            {
                Console.WriteLine(
                    $"{g.Name} found!  Unfortunately, it appears XInput-compatible HID device driver only transmits events from the HID device whilst the current process has a focussed window, so console applications/background services cannot detect button presses. Please try a different controller.");
                return;
            }

            // Assign this gamepad and connect to it.
            var gameController = new GameControllerHIDDevice(this, g);
            var gamepadLayout = new HIDDeviceLayout();
            var gamePad = new GamePadHIDDevice(this, _inputManager, gameController, gamepadLayout);
            RegisterDevice(gamePad);

            _gamePads.Add(g.Device.DevicePath, gameController);

            Console.WriteLine($"{g.Name} found!  Following controls were mapped:");
            foreach (var (control, infos) in g.Mapping)
            {
                Console.WriteLine(
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    $"  {Usage.GetName(control.Usages)} => {string.Join(", ", infos.Select(info => info.PropertyName))}");
            }
        });
    }

    public override void Dispose()
    {
        _devices.Dispose();
        base.Dispose();
    }
}
