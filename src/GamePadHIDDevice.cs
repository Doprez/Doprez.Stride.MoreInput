using Stride.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doprez.Stride.MoreInput;

public class GamePadHIDDevice : GamePadFromLayout, IDisposable, IGamePadDevice
{
    public GamePadHIDDevice(HIDDeviceInputSource source, InputManager inputManager, IGameControllerDevice controller, GamePadLayout layout) : base(inputManager, controller, layout)
    {
        Source = source;
        Name = controller.Name;
        Id = controller.Id;
        ProductId = controller.ProductId;
    }

    public override string Name { get; }
    public override Guid Id { get; }
    public override Guid ProductId { get; }
    public override IInputSource Source { get; }

    public new int Index
    {
        get { return base.Index; }
        set { SetIndexInternal(value, false); }
    }

    public void Dispose()
    {

    }

    public override void SetVibration(float smallLeft, float smallRight, float largeLeft, float largeRight)
    {
        // No vibration support in SDL gamepads
    }
}
