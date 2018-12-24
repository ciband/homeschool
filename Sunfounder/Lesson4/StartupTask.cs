using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Windows.Devices.Pwm;
using System.Threading.Tasks;
using Microsoft.IoT.DeviceCore.Pwm;
using Microsoft.IoT.Devices.Pwm;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Lesson4
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();
            var pwmManager = new PwmProviderManager();
            pwmManager.Providers.Add(new SoftPwm());
            var pwmControllers = await pwmManager.GetControllersAsync();

            //use the first available PWM controller an set refresh rate (Hz)
           var pwmController = pwmControllers[0];
            pwmController.SetDesiredFrequency(50);

            var pin = pwmController.OpenPin(18);
            pin.Start();
            //slDutyCycle.Value = 100;

            for (; ; )
            {
                for (var i = 0; i <= 100; ++i)
                {
                    pin.SetActiveDutyCyclePercentage(i / 100.0);
                    await Task.Delay(TimeSpan.FromMilliseconds(20));
                }
                await Task.Delay(TimeSpan.FromSeconds(1));
                for (var i = 100; i >= 0; --i)
                {
                    pin.SetActiveDutyCyclePercentage(i / 100.0);
                    await Task.Delay(TimeSpan.FromMilliseconds(20));
                }
            }
        }
    }
}
