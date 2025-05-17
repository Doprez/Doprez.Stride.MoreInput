using Stride.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doprez.Stride.MoreInput;

public class HIDDeviceLayout : GamePadLayout
{

    public HIDDeviceLayout()
    {
        AddButtonToButton(0, GamePadButton.A);
        AddButtonToButton(1, GamePadButton.B);
        AddButtonToButton(2, GamePadButton.X);
        AddButtonToButton(3, GamePadButton.Y);
        AddButtonToButton(4, GamePadButton.LeftShoulder);
        AddButtonToButton(5, GamePadButton.RightShoulder);
        AddButtonToButton(6, GamePadButton.LeftThumb);
        AddButtonToButton(7, GamePadButton.RightThumb);
        AddButtonToButton(8, GamePadButton.Start);
        AddButtonToButton(9, GamePadButton.Back);
        AddAxisToAxis(0, GamePadAxis.LeftThumbX);
        AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
        AddAxisToAxis(2, GamePadAxis.RightThumbX);
        AddAxisToAxis(3, GamePadAxis.RightThumbY, true);

        // Do not Remap triggers for HIDDevices since it breaks initial values.
        AddAxisToAxis(4, GamePadAxis.LeftTrigger);
        AddAxisToAxis(5, GamePadAxis.RightTrigger);
    }

    public override bool MatchDevice(IInputSource source, IGameControllerDevice device)
    {
        if(source is HIDDeviceInputSource)
        {
            return true;
        }
        return false;
    }
}
