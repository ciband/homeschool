using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Windows.Devices.Pwm;
using System.Threading.Tasks;
using Microsoft.IoT.Lightning.Providers;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Lesson4
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();
            var controllers = await PwmController.GetControllersAsync(LightningPwmProvider.GetPwmProvider());
            var controller = controllers[1]; // use the on-device controller
            controller.SetDesiredFrequency(50); // try to match 50Hz
            var pin = controller.OpenPin(18);
            pin.SetActiveDutyCyclePercentage(0.0);
            pin.Start();
            for (; ; )
            {
                for (var i = 0; i <= 100; ++i)
                {
                    pin.SetActiveDutyCyclePercentage(i);
                    await Task.Delay(TimeSpan.FromMilliseconds(20));
                }
                await Task.Delay(TimeSpan.FromSeconds(1));
                for (var i = 100; i >= 0; --i)
                {
                    pin.SetActiveDutyCyclePercentage(i);
                    await Task.Delay(TimeSpan.FromMilliseconds(20));
                }
            }
        }
    }
}
