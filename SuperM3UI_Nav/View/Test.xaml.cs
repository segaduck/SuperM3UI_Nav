using SharpDX.DirectInput;
using SharpDX.XInput;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Converters;
using System.Windows.Threading;
using Key = SharpDX.DirectInput.Key;

namespace SuperM3UI_Nav.View
{
    public partial class Test : UserControl
    {
        private DispatcherTimer timer = new();
        private Controller controller;
        int JStickPos = 5;
        int JStickPos_pre = 5;
        public SharpDX.DirectInput.DirectInput dirInput;
        public SharpDX.DirectInput.Keyboard curKeyBoard;

        public Test()
        {
            InitializeComponent();
            DataContext = this;
            // Init Timer
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.03); // every 1/30 seconds
            timer.Tick += Timer_Tick;
            timer.Start();

            // Init Xbox controller
            controller = new Controller(UserIndex.One);
            //if (!controller.IsConnected)
            //{
            //    MessageBox.Show("Xbox controller is not connected");
            //}

            dirInput = new SharpDX.DirectInput.DirectInput();
            var allDevices = dirInput.GetDevices();

            foreach (var item in allDevices)
            {
                if (SharpDX.DirectInput.DeviceType.Keyboard == item.Type)
                {
                    curKeyBoard = new SharpDX.DirectInput.Keyboard(dirInput);
                    //curKeyBoard.Acquire();
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            KeyboardState myKeyboardState;
            myKeyboardState = new KeyboardState();
            curKeyBoard.Acquire();
            curKeyBoard.GetCurrentState(ref myKeyboardState);

            Boolean btnA = myKeyboardState.PressedKeys.Contains(Key.A);
            Boolean btnB = myKeyboardState.PressedKeys.Contains(Key.S);
            Boolean btnX = myKeyboardState.PressedKeys.Contains(Key.D);
            Boolean btnY = myKeyboardState.PressedKeys.Contains(Key.F);

            Boolean btnUp = myKeyboardState.PressedKeys.Contains(Key.Up);
            Boolean btnDown = myKeyboardState.PressedKeys.Contains(Key.Down);
            Boolean btnLeft = myKeyboardState.PressedKeys.Contains(Key.Left);
            Boolean btnRight = myKeyboardState.PressedKeys.Contains(Key.Right);

            if (controller.IsConnected)
            {
                State st = controller.GetState();

                btnA = btnA ||  ((st.Gamepad.Buttons & GamepadButtonFlags.A) != 0);
                btnB = btnB || ((st.Gamepad.Buttons & GamepadButtonFlags.B) != 0);
                btnX = btnX || ((st.Gamepad.Buttons & GamepadButtonFlags.X) != 0);
                btnY = btnY || ((st.Gamepad.Buttons & GamepadButtonFlags.Y) != 0);

                btnUp = btnUp || ((st.Gamepad.Buttons & GamepadButtonFlags.DPadUp) != 0);
                btnDown = btnDown || ((st.Gamepad.Buttons & GamepadButtonFlags.DPadDown) != 0); 
                btnLeft = btnLeft || ((st.Gamepad.Buttons & GamepadButtonFlags.DPadLeft) != 0); 
                btnRight = btnRight || ((st.Gamepad.Buttons & GamepadButtonFlags.DPadRight) != 0);

                if (btnA) { greenButton.Visibility = Visibility.Visible; }
                else { greenButton.Visibility = Visibility.Hidden; }

                if (btnB) { blueButton.Visibility = Visibility.Visible; }
                else { blueButton.Visibility = Visibility.Hidden; }

                if (btnX) { pinkButton.Visibility = Visibility.Visible; }
                else { pinkButton.Visibility = Visibility.Hidden; }

                if (btnY) { redButton.Visibility = Visibility.Visible; }
                else { redButton.Visibility = Visibility.Hidden; }

                if ( btnUp )
                {
                    JStickPos = 8;
                    if (btnUp && btnRight) JStickPos = 9;
                    if (btnUp && btnLeft) JStickPos = 7;
                }

                if (btnDown)
                {
                    JStickPos = 2;
                    if (btnDown && btnLeft) JStickPos = 1;
                    if (btnDown && btnRight) JStickPos = 3;
                }

                if (btnRight)
                {
                    JStickPos = 6;
                    if (btnRight && btnUp) JStickPos = 9;
                    if (btnRight && btnDown) JStickPos = 3;
                }

                if (btnLeft)
                {
                    JStickPos = 4;
                    if (btnLeft && btnUp) JStickPos = 7;
                    if (btnLeft && btnDown) JStickPos = 1;
                }

                if ( (!btnUp)&&(!btnDown)&&(!btnLeft)&&(!btnRight) )
                {
                    JStickPos = 5;
                }

                if (JStickPos_pre != JStickPos) moveBall();
                JStickPos_pre = JStickPos;
            }
        }

        private void moveBall()
        {
            double moveDistance = 50;
            double move45X = 35;
            double move45Y = 35;
            // get current position
            double currentX = Canvas.GetLeft(stickBall);
            double currentY = Canvas.GetTop(stickBall);

            // create TranslateTransform
            TranslateTransform translateTransform = new TranslateTransform();

            if (JStickPos != JStickPos_pre)
            {
                // set target position
                double posX = 225;
                double posY = 275;

                double targetX = posX;
                double targetY = posY;

                // using number systems to setup animation direction and distance
                switch (JStickPos)
                {
                    case 8:
                        targetY -= moveDistance;
                        break;
                    case 2:
                        targetY += moveDistance;
                        break;
                    case 4:
                        targetX -= moveDistance;
                        break;
                    case 6:
                        targetX += moveDistance;
                        break;
                    case 9:
                        targetX = targetX + move45X;
                        targetY = targetY - move45Y;
                        break;
                    case 7:
                        targetX = targetX - move45X;
                        targetY = targetY - move45Y;
                        break;
                    case 1:
                        targetX = targetX - move45X;
                        targetY = targetY + move45Y;
                        break;
                    case 3:
                        targetX = targetX + move45X;
                        targetY = targetY + move45Y;
                        break;
                    case 5:
                        targetX = posX;
                        targetY = posY;
                        break;
                }
                // create and start animation
                DoubleAnimation xAnimation = new DoubleAnimation(posX, targetX, TimeSpan.FromSeconds(0.07));
                DoubleAnimation yAnimation = new DoubleAnimation(posY, targetY, TimeSpan.FromSeconds(0.07));
                stickBall.BeginAnimation(Canvas.LeftProperty, xAnimation);
                stickBall.BeginAnimation(Canvas.TopProperty, yAnimation);    
                
            }
            JStickPos_pre = JStickPos;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

    }

}
