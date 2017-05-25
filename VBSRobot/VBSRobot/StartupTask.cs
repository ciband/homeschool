using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Porrey.Uwp.IoT;
using System.Threading.Tasks;
using Win10_LCD;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Controls;
// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace VBSRobot
{
	enum ArmDirection
	{
		Extend,
		Retract
	}

    public sealed class StartupTask : IBackgroundTask
    {
        private const int LEFT_EYE_LED_PIN = 27;
        private const int RIGHT_EYE_LED_PIN = 5;
		private const int ELBOW_SERVO_PIN = 22;

		private const double MIN_ARM_POSITION = 0.8;
		private const double MAX_ARM_POSITION = 1.25;
		private const double ARM_POSITION_STEP = 0.1;

		private const int LCD_DB4 = 12;
		private const int LCD_DB5 = 19;
		private const int LCD_DB6 = 6;
		private const int LCD_DB7 = 13;
		private const int LCD_E = 24;
		private const int LCD_RS = 23;

		private SoftPwm elbow_servo;
		private ArmDirection elbowDirection = ArmDirection.Extend;
		private LCD _lcd = new LCD(16, 2);
        private SpeechSynthesizer _talk = new SpeechSynthesizer();
        private MediaElement _media = new MediaElement();

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            await InitGPIO();
            // 
            // TODO: Insert code to perform background work
            //
            // If you start any asynchronous methods here, prevent the task
            // from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            //
            var speechStream = await _talk.SynthesizeTextToStreamAsync("Kill All Humans");
            speechStream.p
            _media.AutoPlay = true;
            _media.SetSource(speechStream, speechStream.ContentType);
            _media.Play();
			elbow_servo.StartAsync();
            elbow_servo.Value = 1.0;
            elbow_servo.Value = 1.5;
            elbow_servo.Value = 2.0;
            for (;;)
			{
				MoveArm();
				Task.Delay(TimeSpan.FromMilliseconds(1000)).Wait();
			}
        }

        private async Task InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                return;
            }

			// turn on eyes
            LEDOn(gpio.OpenPin(LEFT_EYE_LED_PIN));
            LEDOn(gpio.OpenPin(RIGHT_EYE_LED_PIN));

			//init elbow servo
			elbow_servo = new SoftPwm(gpio.OpenPin(ELBOW_SERVO_PIN))
			{
				// fill in
				PulseFrequency = 50, // hz
				MaximumValue = 2.0,
				Value = 1.5
			};
            

            // init LCD
            /*await _lcd.InitAsync(LCD_RS, LCD_E, LCD_DB4, LCD_DB5, LCD_DB6, LCD_DB7);
			await _lcd.clearAsync();

			_lcd.WriteLine("Beep Beep Beep");
			_lcd.WriteLine("Kill All Humans");*/
        }

		private void LEDOn(GpioPin pin)
        {
            if (pin == null)
            {
                return;
            }

            pin.Write(GpioPinValue.Low);
            pin.SetDriveMode(GpioPinDriveMode.Output);
        }

		private void MoveArm()
		{
			var newArmPosition = elbow_servo.Value;
			if (elbowDirection == ArmDirection.Extend)
			{
				newArmPosition += ARM_POSITION_STEP;
			}
			else
			{
				newArmPosition -= ARM_POSITION_STEP;
			}
			if (newArmPosition > MAX_ARM_POSITION)
			{
				elbowDirection = ArmDirection.Retract;
			} else if (newArmPosition < MIN_ARM_POSITION)
			{
				elbowDirection = ArmDirection.Extend;
			}
            elbow_servo.Value = newArmPosition;
		}
    }
}
