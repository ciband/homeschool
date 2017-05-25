using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Win10_LCD;
using Windows.Devices;
using Windows.Devices.Gpio;
using Windows.Devices.Pwm;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VBSRobot_GUI
{
    enum ArmDirection
    {
        Extend,
        Retract
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int LEFT_EYE_LED_PIN = 27;
        private const int RIGHT_EYE_LED_PIN = 5;
        private const int ELBOW_SERVO_PIN = 22;

        private const double MIN_ARM_POSITION = 0.05;
        private const double MAX_ARM_POSITION = 0.1;
        private const double ARM_POSITION_STEP = 0.0005;

        private const int LCD_DB4 = 12;
        private const int LCD_DB5 = 19;
        private const int LCD_DB6 = 6;
        private const int LCD_DB7 = 13;
        private const int LCD_E = 24;
        private const int LCD_RS = 23;

        // private SoftPwm elbow_servo;
        private PwmController _pwmController;
        private PwmPin _elbowServo;
        private ArmDirection elbowDirection = ArmDirection.Extend;
        private LCD _lcd;
        private SpeechSynthesizer _talk = new SpeechSynthesizer();
        private MediaElement _media = new MediaElement();

        private Timer _armTimer;
        private Timer _mouthTimer;

        public MainPage()
        {
            this.InitializeComponent();

            go();

            
           /* elbow_servo.StartAsync();
            elbow_servo.Value = 1.0;
            elbow_servo.Value = 1.5;
            elbow_servo.Value = 2.0;
            for (;;)
            {
                MoveArm();
                Task.Delay(TimeSpan.FromMilliseconds(1000)).Wait();
            }*/
        }

        private async void go()
        {
            if (LightningProvider.IsLightningEnabled)
            {
                LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();
            }

            await InitGPIO();

            // init LCD
            _lcd = new LCD(16, 2);
            await _lcd.InitAsync(LCD_RS, LCD_E, LCD_DB4, LCD_DB5, LCD_DB6, LCD_DB7);
           

            var c = await PwmController.GetControllersAsync(LightningPwmProvider.GetPwmProvider());
            _pwmController = c[1];
            _pwmController.SetDesiredFrequency(50);
            _elbowServo = _pwmController.OpenPin(ELBOW_SERVO_PIN);

            _elbowServo.Start();
            _elbowServo.SetActiveDutyCyclePercentage(MIN_ARM_POSITION);
            var speechStream = await _talk.SynthesizeTextToStreamAsync("kill all humans");
            _media.AutoPlay = true;
            _media.IsLooping = true;
            _media.SetSource(speechStream, speechStream.ContentType);
            _media.Play();
            
            _armTimer = new Timer(async (o) => 
            //await Task.Run(async () =>
            {
                //for (;;)
               // {
                    // move arm
                    await MoveArm();
                    //await Task.Delay(TimeSpan.FromMilliseconds(20));
              //  }
            }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(20));

            _mouthTimer = new Timer(async (o) => {
                await _lcd.clearAsync();

                _lcd.WriteLine("Beep Beep Beep");
                _lcd.WriteLine("Kill All Humans");
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
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
            /*elbow_servo = new SoftPwm(gpio.OpenPin(ELBOW_SERVO_PIN))
            {
                // fill in
                PulseFrequency = 50, // hz
                MaximumValue = 2.0,
                Value = 1.5
            };*/


            
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

        private async Task MoveArm()
        {
            var newArmPosition = _elbowServo.GetActiveDutyCyclePercentage();
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
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
            else if (newArmPosition < MIN_ARM_POSITION)
            {
                elbowDirection = ArmDirection.Extend;
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
            _elbowServo.SetActiveDutyCyclePercentage(newArmPosition);
        }
    }
}
