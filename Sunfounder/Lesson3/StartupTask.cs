using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using System.Threading.Tasks;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Lesson3
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;
        private GpioPin[] ledPins;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();
            InitGPIO();
            
            for(; ;)
            {
                foreach (var pin in ledPins)
                {
                    led_on(pin);
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    led_off(pin);
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                foreach (var pin in ledPins.Reverse())
                {
                    led_on(pin);
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    led_off(pin);
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }

        private void InitGPIO()
        {
            var controller = GpioController.GetDefault();

            // init pins
            ledPins = new GpioPin[]
            {
                controller.OpenPin(4),
                controller.OpenPin(5),
                controller.OpenPin(6),
                controller.OpenPin(13),
                controller.OpenPin(19),
                controller.OpenPin(26)
            };

            // set pins to output
            foreach (var pin in ledPins)
            {
                pin.SetDriveMode(GpioPinDriveMode.Output);
                led_off(pin);
            }
        }

        private void led_on(GpioPin pin)
        {
            pin.Write(GpioPinValue.Low);
        }

        private void led_off(GpioPin pin)
        {
            pin.Write(GpioPinValue.High);
        }
    }
}
