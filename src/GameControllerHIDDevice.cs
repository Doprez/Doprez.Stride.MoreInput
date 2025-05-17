using DevDecoder.HIDDevices.Controllers;
using DevDecoder.HIDDevices.Converters;
using Stride.Input;
using System.Runtime.CompilerServices;
using System.Text;
using Direction = Stride.Input.Direction;

namespace Doprez.Stride.MoreInput;

public class GameControllerHIDDevice : GameControllerDeviceBase, IDisposable
{
    public override IInputSource Source { get; }

    public override string Name { get; }
    public override Guid Id { get; }
    public override Guid ProductId { get; }

    public override IReadOnlyList<GameControllerButtonInfo> ButtonInfos => buttonInfos;
    public override IReadOnlyList<GameControllerAxisInfo> AxisInfos => axisInfos;
    public override IReadOnlyList<GameControllerDirectionInfo> DirectionInfos => povControllerInfos;

    private readonly List<GameControllerButtonInfo> buttonInfos = [];
    private readonly List<GameControllerAxisInfo> axisInfos = [];
    private readonly List<GameControllerDirectionInfo> povControllerInfos = [];
    private readonly Gamepad _gamepad;

    private readonly Dictionary<string, int> _controlInfoButtons = [];
    private readonly Dictionary<string, int> _controlInfoAxis = [];
    private readonly Dictionary<string, int> _controlInfoHats = [];

    private long _timeStamp;

    public GameControllerHIDDevice(HIDDeviceInputSource source, Gamepad gamePad)
    {
        Source = source;
        _gamepad = gamePad;

        ProductId = ToGuid(gamePad.Device.ProductId);
        Id = Guid.NewGuid();
        Name = gamePad.Name;

        gamePad.Connect();

        buttonInfos.Add(new GameControllerButtonInfo { Name = "A" });
        buttonInfos.Add(new GameControllerButtonInfo { Name = "B" });
        buttonInfos.Add(new GameControllerButtonInfo { Name = "X" });
        buttonInfos.Add(new GameControllerButtonInfo { Name = "Y" });
        buttonInfos.Add(new GameControllerButtonInfo { Name = "LB" });
        buttonInfos.Add(new GameControllerButtonInfo { Name = "RB" });
        buttonInfos.Add(new GameControllerButtonInfo { Name = "L" });
        buttonInfos.Add(new GameControllerButtonInfo { Name = "R" });
        buttonInfos.Add(new GameControllerButtonInfo { Name = "Start" });
        buttonInfos.Add(new GameControllerButtonInfo { Name = "Select" });

        axisInfos.Add(new GameControllerAxisInfo { Name = "LX" });
        axisInfos.Add(new GameControllerAxisInfo { Name = "LY" });
        axisInfos.Add(new GameControllerAxisInfo { Name = "RX" });
        axisInfos.Add(new GameControllerAxisInfo { Name = "RY" });
        axisInfos.Add(new GameControllerAxisInfo { Name = "LT" });
        axisInfos.Add(new GameControllerAxisInfo { Name = "RT" });

        povControllerInfos.Add(new GameControllerDirectionInfo { Name = "Hat" });

        InitializeButtonStates();
    }

    public override void Update(List<InputEvent> inputEvents)
    {
        if (_gamepad.IsConnected != true) { return; }

        HandleButton(0, _gamepad.AButton);
        HandleButton(1, _gamepad.BButton);
        HandleButton(2, _gamepad.XButton);
        HandleButton(3, _gamepad.YButton);
        HandleButton(4, _gamepad.LeftBumper);
        HandleButton(5, _gamepad.RightBumper);
        HandleButton(6, _gamepad.LeftStick);
        HandleButton(7, _gamepad.RightStick);
        HandleButton(8, _gamepad.Start);
        HandleButton(9, _gamepad.Select);
        
        HandleAxis(0, (float)_gamepad.X);
        HandleAxis(1, (float)_gamepad.Y);
        HandleAxis(2, (float)_gamepad.Rx);
        HandleAxis(3, (float)_gamepad.Ry);
        HandleAxis(4, (float)_gamepad.LeftTrigger);
        HandleAxis(5, (float)_gamepad.RightTrigger);

        HandleDirection(0, ConvertHat(_gamepad.Hat));

        base.Update(inputEvents);
    }

    public void Dispose()
    {
        _gamepad.Dispose();
    }

    public static Guid ToGuid(int value)
    {
        byte[] bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    private Direction ConvertHat(DevDecoder.HIDDevices.Converters.Direction direction)
    {
        switch (direction)
        {
            case DevDecoder.HIDDevices.Converters.Direction.East:
                return Direction.Right;
            case DevDecoder.HIDDevices.Converters.Direction.West:
                return Direction.Left;
            case DevDecoder.HIDDevices.Converters.Direction.North:
                return Direction.Up;
            case DevDecoder.HIDDevices.Converters.Direction.South:
                return Direction.Down;
            case DevDecoder.HIDDevices.Converters.Direction.SouthEast:
                return Direction.RightDown;
            case DevDecoder.HIDDevices.Converters.Direction.SouthWest:
                return Direction.LeftDown;
            case DevDecoder.HIDDevices.Converters.Direction.NorthEast:
                return Direction.RightUp;
            case DevDecoder.HIDDevices.Converters.Direction.NorthWest:
                return Direction.LeftUp;
            default:
                return Direction.None;
        }
    }
}
