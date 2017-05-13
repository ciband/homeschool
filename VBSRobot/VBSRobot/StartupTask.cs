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
        private const int LEFT_EYE_LED_PIN = 17;
        private const int RIGHT_EYE_LED_PIN = 27;
		private const int ELBOW_SERVO_PIN = 18;

		private const double MIN_ARM_POSITION = 0.8;
		private const double MAX_ARM_POSITION = 1.25;
		private const double ARM_POSITION_STEP = 0.01;

		private const int LCD_DB4 = 24;
		private const int LCD_DB5 = 5;
		private const int LCD_DB6 = 6;
		private const int LCD_DB7 = 13;
		private const int LCD_E = 23;
		private const int LCD_RS = 18;

        private GpioPin pin;
		private SoftPwm elbow_servo;
		private ArmDirection elbowDirection = ArmDirection.Extend;
		private LCD _lcd = new LCD(16, 2);

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
			elbow_servo.StartAsync();

			for(;;)
			{
				MoveArm();
				Task.Delay(TimeSpan.FromMilliseconds(10)).Wait();
			}
        }

        private async void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                return;
            }

			// turn on eyes
            led_on(gpio.OpenPin(LEFT_EYE_LED_PIN));
            led_on(gpio.OpenPin(RIGHT_EYE_LED_PIN));

			//init elbow servo
			elbow_servo = new SoftPwm(gpio.OpenPin(ELBOW_SERVO_PIN))
			{
				// fill in
				PulseFrequency = 50, // hz
				MaximumValue = 2.0,
				Value = 1.5
			};

			// init LCD
			await _lcd.InitAsync(LCD_RS, LCD_E, LCD_DB4, LCD_DB5, LCD_DB6, LCD_DB7);
			await _lcd.clearAsync();

			_lcd.WriteLine("Beep Beep Beep");
			_lcd.WriteLine("Kill All Humans");
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
		}
    }
}
