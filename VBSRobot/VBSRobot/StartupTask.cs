using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace VBSRobot
{
    public sealed class StartupTask : IBackgroundTask
    {
        private const int LEFT_EYE_LED_PIN = 20;
        private const int RIGHT_EYE_LED_PIN = 21;

        private GpioPin pin;

        StartupTask()
        {
            InitGPIO();
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // 
            // TODO: Insert code to perform background work
            //
            // If you start any asynchronous methods here, prevent the task
            // from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            //
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                return;
            }

            led_on(gpio.OpenPin(LEFT_EYE_LED_PIN));
            led_on(gpio.OpenPin(RIGHT_EYE_LED_PIN));


        }

        private void led_on(GpioPin pin)
        {
            if (pin == null)
            {
                return;
            }

            pin.Write(GpioPinValue.High);
            pin.SetDriveMode(GpioPinDriveMode.Output);
        }
    }
}
