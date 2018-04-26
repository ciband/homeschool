using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using System.Threading.Tasks;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Lesson1
{
    public sealed class StartupTask : IBackgroundTask
    {
        private const int LED_PIN = 5;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                return;
            }

            var pin = gpio.OpenPin(LED_PIN);
            pin.SetDriveMode(GpioPinDriveMode.Output);

            bool LED_On = false;

            for (; ; )
            {
                if (LED_On)
                {
                    pin.Write(GpioPinValue.High);
                }
                else
                {
                    pin.Write(GpioPinValue.Low);
                }

                LED_On = !LED_On;

                Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }
    }
}
