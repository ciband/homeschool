using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using System.Threading.Tasks;
using Windows.System.Threading;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Lesson2
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral deferral;
        private GpioPinValue ledPinValue = GpioPinValue.High;
        private const int LED_PIN = 5;
        private const int BUTTON_PIN = 6;
        private GpioPin ledPin;
        private GpioPin buttonPin;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();
            InitGPIO();
        }
        private void InitGPIO()
        {
            var controller = GpioController.GetDefault();

            ledPin = controller.OpenPin(LED_PIN);
            buttonPin = controller.OpenPin(BUTTON_PIN);

            ledPin.Write(GpioPinValue.High);
            ledPin.SetDriveMode(GpioPinDriveMode.Output);

            // Check if input pull-up resistors are supported
            if (buttonPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            {
                buttonPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            }
            else
            {
                buttonPin.SetDriveMode(GpioPinDriveMode.Input);
            }

            // Set a debounce timeout to filter out switch bounce noise from a button press
            buttonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);

            // Register for the ValueChanged event so our buttonPin_ValueChanged 
            // function is called when the button is pressed
            buttonPin.ValueChanged += buttonPin_ValueChanged;
        }

        private void buttonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            // toggle the state of the LED every time the button is pressed
            if (e.Edge == GpioPinEdge.FallingEdge)
            {
                ledPinValue = (ledPinValue == GpioPinValue.Low) ? GpioPinValue.High : GpioPinValue.Low;
                ledPin.Write(ledPinValue);
            }
        }
        }
}
