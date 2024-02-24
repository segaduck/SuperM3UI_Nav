using SharpDX.DirectInput;
using SharpDX.XInput;
using SuperM3UI_Nav.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using static SuperM3UI_Nav.ViewModel.HomeVM;
using Keyboard = SharpDX.DirectInput.Keyboard;
using Path = System.IO.Path;
using State = SharpDX.XInput.State;
using Xceed.Wpf.Toolkit;
using System.Security.Cryptography.X509Certificates;

namespace SuperM3UI_Nav.View
{
    public static class GamepadExtensions
    {
        public static string ConvertInput(this Gamepad returnedValue, bool fullAxis = false)
        {
            if (returnedValue.Buttons.ToString() != "None")
            {
                foreach (var type in from type in buttonList!
                                     where type.Value.Equals(returnedValue.Buttons.ToString())
                                     select type)
                {
                    if (type.Key != string.Empty)
                        return type.Key;
                    else
                        return type.Value;
                }
            }

            if (fullAxis)
            {
                //  InputSteering, InputAnalogJoyX, InputAnalogGunX, InputAnalogGunX2
                if (returnedValue.LeftThumbX > 12000 || returnedValue.LeftThumbX < -12000 || returnedValue.LeftThumbY > 12000 || returnedValue.LeftThumbY < -12000)
                {
                    return "XAXIS";
                    //return "_AXIS1";  //same as above?
                }

                //  InputAnalogJoyY, InputAnalogGunY, InputAnalogGunY2
                if (returnedValue.RightThumbX > 12000 || returnedValue.RightThumbX < -12000 || returnedValue.RightThumbY > 12000 || returnedValue.RightThumbY < -12000)
                {
                    return "YAXIS";
                    //return "_AXIS2";  //same as above?
                }
            }

            //Left Joy
            if (returnedValue.LeftThumbX > 12000)
            {
                //return "RIGHT";
                return "XAXIS_POS";  //same as above
                                     //return "JOY1_AXIS1_POS";  //same as above?
            }
            else if (returnedValue.LeftThumbX < -12000)
            {
                //return "LEFT";
                return "XAXIS_NEG";  //same as above
            }
            else if (returnedValue.LeftThumbY > 12000)
            {
                //return "UP";
                return "YAXIS_NEG";  //same as above
            }
            else if (returnedValue.LeftThumbY < -12000)
            {
                //return "DOWN";
                return "YAXIS_POS";  //same as above
            }

            //Right Joy
            if (returnedValue.RightThumbX > 12000)
            {
                return "RXAXIS_POS";
                //return $"Right Joy Right";
            }
            else if (returnedValue.RightThumbX < -12000)
            {
                return "RXAXIS_NEG";
                //return $"Right Joy Left";
            }
            else if (returnedValue.RightThumbY > 12000)
            {
                return "RYAXIS_NEG";
                //return $"Right Joy Up";
            }
            else if (returnedValue.RightThumbY < -12000)
            {
                return "RYAXIS_POS";
                //return $"Right Joy Down";
            }

            //Triggers
            if (returnedValue.LeftTrigger > 0)
                return "ZAXIS_POS";       // Left trigger

            if (returnedValue.RightTrigger > 0)
                return "RZAXIS_POS";      // Right trigger

            return string.Empty;
        }
        public static readonly Dictionary<string, string> buttonList = new()
        {
            { "BUTTON1", "A" },
            { "BUTTON2", "B" },
            { "BUTTON3", "X" },
            { "BUTTON4", "Y" },
            { "BUTTON5", "LeftShoulder" },
            { "BUTTON6", "RightShoulder" },
            { "BUTTON7", "Back" },
            { "BUTTON8", "Start" },
            { "BUTTON9", "LeftThumb" },
            { "BUTTON10", "RightThumb" },

            { "POV1_LEFT", "DPadLeft" },
            { "POV1_RIGHT", "DPadRight" },
            { "POV1_UP", "DPadUp" },
            { "POV1_DOWN", "DPadDown" },
        };

    }


    public partial class Control : UserControl
    {
        public string iniDir = Path.Combine(HomeVM.SharedDataStore.iniDirectory, "Supermodel.ini");
        private double countdownSeconds = 5.0; // countdown time in seconds
        private double remainingSeconds;
        private readonly Controller controller1;
        private readonly Controller controller2;
        private readonly Controller controller3;
        private readonly Controller controller4;
        public SharpDX.DirectInput.DirectInput dirInput;
        public SharpDX.DirectInput.Keyboard curKeyBoard;
        public bool isCountingDown = false;
        public string key = string.Empty;
        public static bool isIniMode = true;

        public Control()
        {
            InitializeComponent();
            cb_selectInput.ItemsSource = ControlVM.inputOptions;
            cb_selectInput.DisplayMemberPath = "name";
            cb_selectInput.SelectedValuePath = "name";
            cb_selectInput.IsReadOnly = true;
            cb_selectInput.SelectedIndex = HomeVM.SharedDataStore.selectedJoytickIndex;

            cbSpikeMode.ItemsSource = ControlVM.spikeoutOptions;
            cbSpikeMode.DisplayMemberPath = "name";
            cbSpikeMode.SelectedValuePath = "name";
            cbSpikeMode.IsReadOnly = true;

            string result = HomeVM.IniFileHelper.ReadValue("SuperM3UI", "SpikeoutMode", iniDir);
            if (result != "")
            {
                cbSpikeMode.SelectedIndex = Convert.ToInt32(result);
            }
            else
            {
                HomeVM.IniFileHelper.WriteValue("SuperM3UI", "SpikeoutMode", "0", iniDir);
                cbSpikeMode.SelectedIndex = 0;
            }


            //  Initialize All Xinput Controllers
            controller1 = new Controller(UserIndex.One);
            controller2 = new Controller(UserIndex.Two);
            controller3 = new Controller(UserIndex.Three);
            controller4 = new Controller(UserIndex.Four);

            if ((!controller1.IsConnected) && (!controller2.IsConnected) && (!controller3.IsConnected) && (!controller4.IsConnected))
            {
                outP.Text = "No controller connected";
            }

            //  Make sure at least one controller is connected
            string con = string.Empty;
            if (controller1.IsConnected) con=" #1 ";
            if (controller2.IsConnected) con = con + " #2 ";
            if (controller3.IsConnected) con = con + " #3 ";
            if (controller4.IsConnected) con = con + " #4 ";
            if (con.Length > 0) outP.Text = $"Controller {con}  connected";

            var directInput = new DirectInput();
            curKeyBoard = new Keyboard(directInput);
            curKeyBoard.Properties.BufferSize = 128;
            curKeyBoard.Acquire();

            // Common Buttons Config Initialize
            ReadCommonButtons();
            
            // Spikeout + 4-Ways Joysticks Config Initialize
            ReadSpikeoutButtons();

            // Fighting Game 1P Config Initialize
            ReadFightP1Buttons();

            // Fighting Game 2P Config Initialize
            ReadFightP2Buttons();

            // Visual Striker 2 - 1P Config Initialize
            ReadVs2P1Buttons();

            // Visual Striker 2 - 2P Config Initialize
            ReadVs2P2Buttons();

            isIniMode = false;
        }

        private void cb_selectInput_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            gdCommon.Visibility = Visibility.Hidden;
            gdSpikeout.Visibility = Visibility.Hidden;
            gdFight.Visibility = Visibility.Hidden;
            gdFightP2.Visibility = Visibility.Hidden;
            gdVs2P1.Visibility = Visibility.Hidden;
            gdVs2P2.Visibility = Visibility.Hidden;
            cbSpikeMode.Width = 0;
            //cbSpikeMode.Visibility = Visibility.Hidden;
            switch (cb_selectInput.SelectedIndex)
            {
                case 0:
                    gdCommon.Visibility = Visibility.Visible;
                    ReadCommonButtons();
                    break;
                // Spikeout
                case 1:
                    gdSpikeout.Visibility = Visibility.Visible;
                    cbSpikeMode.Width = 300;
                    //cbSpikeMode.Visibility = Visibility.Visible;
                    ReadSpikeoutButtons();
                    break;
                // Fight 1P      
                case 2:
                    gdFight.Visibility = Visibility.Visible;
                    ReadFightP1Buttons();
                    break;
                // Fight 2P
                case 3:
                    gdFightP2.Visibility = Visibility.Visible;
                    ReadFightP2Buttons();
                    break;
                // Visual Striker 2 - 1P
                case 4:
                    gdVs2P1.Visibility = Visibility.Visible;
                    ReadVs2P1Buttons();
                    break;
                // Visual Striker 2 - 2P
                case 5:
                    gdVs2P2.Visibility = Visibility.Visible;
                    ReadVs2P2Buttons();
                    break;
                //default:
                //    break;
            }
        }

        private void cbSpikeMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HomeVM.IniFileHelper.WriteValue("SuperM3UI", "SpikeoutMode", cbSpikeMode.SelectedIndex.ToString(), iniDir);
            setSpikeMode(cbSpikeMode.SelectedIndex);
        }

        private void setSpikeMode (int spikemode)
        {
            try
            {
                if (File.Exists(Path.Combine("NVRAM", "spikeofe.nv")))
                    File.Delete(Path.Combine("NVRAM", "spikeofe.nv"));

                switch (spikemode)
                {
                    case 0:
                        File.Copy(Path.Combine("resources", "spikeofe-single.nv"), Path.Combine("NVRAM", "spikeofe.nv"), true);
                        break;
                    case 1:
                        File.Copy(Path.Combine("resources", "spikeofe-master.nv"), Path.Combine("NVRAM", "spikeofe.nv"), true);
                        break;
                    case 2:
                        File.Copy(Path.Combine("resources", "spikeofe-slave.nv"), Path.Combine("NVRAM", "spikeofe.nv"), true);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public void ReadCommonButtons()
        {
            btnComP1Start.Content = IniFileHelper.ReadValue("Global", "InputStart1", iniDir);
            btnComP1Coin.Content = IniFileHelper.ReadValue("Global", "InputCoin1", iniDir);
            btnComP2Start.Content = IniFileHelper.ReadValue("Global", "InputStart2", iniDir);
            btnComP2Coin.Content = IniFileHelper.ReadValue("Global", "InputCoin2", iniDir);
            btnComServiceA.Content = IniFileHelper.ReadValue("Global", "InputServiceA", iniDir);
            btnComServiceB.Content = IniFileHelper.ReadValue("Global", "InputServiceB", iniDir);
            btnComTestA.Content = IniFileHelper.ReadValue("Global", "InputTestA", iniDir);
            btnComTestB.Content = IniFileHelper.ReadValue("Global", "InputTestB", iniDir);
            btnComUp.Content = IniFileHelper.ReadValue("Global", "InputJoyUp", iniDir);
            btnComDown.Content = IniFileHelper.ReadValue("Global", "InputJoyDown", iniDir);
            btnComLeft.Content = IniFileHelper.ReadValue("Global", "InputJoyLeft", iniDir);
            btnComRight.Content = IniFileHelper.ReadValue("Global", "InputJoyRight", iniDir);
        }
        public void ReadSpikeoutButtons()
        {
            // Spikeout + 4-Ways Joysticks Config to Buttons
            //cbSpikeMode.SelectedIndex = HomeVM.SharedDataStore.selectedSpikeoutIndex;

            btnStart.Content = IniFileHelper.ReadValue("Global", "InputStart1", iniDir);
            btnCoin.Content = IniFileHelper.ReadValue("Global", "InputCoin1", iniDir);

            btnUp.Content = IniFileHelper.ReadValue("Global", "InputJoyUp", iniDir);
            btnDown.Content = IniFileHelper.ReadValue("Global", "InputJoyDown", iniDir);
            btnLeft.Content = IniFileHelper.ReadValue("Global", "InputJoyLeft", iniDir);
            btnRight.Content = IniFileHelper.ReadValue("Global", "InputJoyRight", iniDir);

            btnShift.Content = IniFileHelper.ReadValue("Global", "InputShift", iniDir);
            btnBeat.Content = IniFileHelper.ReadValue("Global", "InputBeat", iniDir);
            btnCharge.Content = IniFileHelper.ReadValue("Global", "InputCharge", iniDir);
            btnJump.Content = IniFileHelper.ReadValue("Global", "InputJump", iniDir);           
        }

        public void ReadFightP1Buttons()
        {
            // Fighting Game 1P Config to Buttons
            btnStartFight.Content = IniFileHelper.ReadValue("Global", "InputStart1", iniDir);
            btnCoinFight.Content = IniFileHelper.ReadValue("Global", "InputCoin1", iniDir);

            btnUpFight.Content = IniFileHelper.ReadValue("Global", "InputJoyUp", iniDir);
            btnDownFight.Content = IniFileHelper.ReadValue("Global", "InputJoyDown", iniDir);
            btnLeftFight.Content = IniFileHelper.ReadValue("Global", "InputJoyLeft", iniDir);
            btnRightFight.Content = IniFileHelper.ReadValue("Global", "InputJoyRight", iniDir);

            btnGuard.Content = IniFileHelper.ReadValue("Global", "InputGuard", iniDir);
            btnPunch.Content = IniFileHelper.ReadValue("Global", "InputPunch", iniDir);
            btnCKick.Content = IniFileHelper.ReadValue("Global", "InputKick", iniDir);
            btnEvade.Content = IniFileHelper.ReadValue("Global", "InputEvade", iniDir);
        }

        public void ReadFightP2Buttons()
        {
            // Fighting Game 2P Config to Buttons
            btnStartFightP2.Content = IniFileHelper.ReadValue("Global", "InputStart2", iniDir);
            btnCoinFightP2.Content = IniFileHelper.ReadValue("Global", "InputCoin2", iniDir);

            btnUpFightP2.Content = IniFileHelper.ReadValue("Global", "InputJoyUp2", iniDir);
            btnDownFightP2.Content = IniFileHelper.ReadValue("Global", "InputJoyDown2", iniDir);
            btnLeftFightP2.Content = IniFileHelper.ReadValue("Global", "InputJoyLeft2", iniDir);
            btnRightFightP2.Content = IniFileHelper.ReadValue("Global", "InputJoyRight2", iniDir);

            btnGuardP2.Content = IniFileHelper.ReadValue("Global", "InputGuard2", iniDir);
            btnPunchP2.Content = IniFileHelper.ReadValue("Global", "InputPunch2", iniDir);
            btnCKickP2.Content = IniFileHelper.ReadValue("Global", "InputKick2", iniDir);
            btnEvadeP2.Content = IniFileHelper.ReadValue("Global", "InputEvade2", iniDir);
        }

        public void ReadVs2P1Buttons()
        {
            // Visual Striker 2 - P1 Config to Buttons
            btnStartVs2P1.Content = IniFileHelper.ReadValue("Global", "InputStart1", iniDir);
            btnCoinVs2P1.Content = IniFileHelper.ReadValue("Global", "InputCoin1", iniDir);

            btnUpVs2P1.Content = IniFileHelper.ReadValue("Global", "InputJoyUp", iniDir);
            btnDownVs2P1.Content = IniFileHelper.ReadValue("Global", "InputJoyDown", iniDir);
            btnLeftVs2P1.Content = IniFileHelper.ReadValue("Global", "InputJoyLeft", iniDir);
            btnRightVs2P1.Content = IniFileHelper.ReadValue("Global", "InputJoyRight", iniDir);

            btnShortPassP1.Content = IniFileHelper.ReadValue("Global", "InputShortPass", iniDir);
            btnLongPassP1.Content = IniFileHelper.ReadValue("Global", "InputLongPass", iniDir);
            btnShootP1.Content = IniFileHelper.ReadValue("Global", "InputShoot", iniDir);
        }
        public void ReadVs2P2Buttons()
        {
            // Visual Striker 2 - P2 Config to Buttons
            btnStartVs2P2.Content = IniFileHelper.ReadValue("Global", "InputStart2", iniDir);
            btnCoinVs2P2.Content = IniFileHelper.ReadValue("Global", "InputCoin2", iniDir);

            btnUpVs2P2.Content = IniFileHelper.ReadValue("Global", "InputJoyUp2", iniDir);
            btnDownVs2P2.Content = IniFileHelper.ReadValue("Global", "InputJoyDown2", iniDir);
            btnLeftVs2P2.Content = IniFileHelper.ReadValue("Global", "InputJoyLeft2", iniDir);
            btnRightVs2P2.Content = IniFileHelper.ReadValue("Global", "InputJoyRight2", iniDir);

            btnShortPassP2.Content = IniFileHelper.ReadValue("Global", "InputShortPass2", iniDir);
            btnLongPassP2.Content = IniFileHelper.ReadValue("Global", "InputLongPass2", iniDir);
            btnShootP2.Content = IniFileHelper.ReadValue("Global", "InputShoot2", iniDir);
        }

        public async void btnEvent(object sender, string inputName)
        {
            Button button = (Button)sender;

            remainingSeconds = countdownSeconds;
            string previousKey;
            if (button.Content != null)
                previousKey = button.Content.ToString();
            else previousKey = string.Empty;

            button.Background = Brushes.LightGreen;
            isCountingDown = true;

            Dictionary<int, Gamepad> returnedValue = await GetInput(button);

            //  If the returned controller # is 0 (zero)
            if (returnedValue.Keys.First().Equals(0))
            {
                //ADD THIS
                if (!string.IsNullOrEmpty(key))
                {
                    if (key.StartsWith("D") && key.Length == 2)
                        key = key.Replace('D', ' ').Trim();
                    button.Content = $"KEY_{key}";
                    IniFileHelper.WriteValue("Global", inputName, button.Content.ToString(), iniDir);
                    button.Background = Brushes.Transparent;
                    isCountingDown = false;
                    return;
                }

                button.Content = previousKey;
                button.Background = Brushes.Transparent;
                isCountingDown = false;
                return;
            }

            if (!string.IsNullOrEmpty(key))
            {
                if (key.Contains("Control"))
                    key = $"{key.Split("Control")[0]}CTRL";

                if (key.StartsWith("D") && key.Length == 2)
                    key = key.Replace('D', ' ').Trim();

                button.Content = $"KEY_{key}".ToUpper();
                IniFileHelper.WriteValue("Global", inputName, button.Content.ToString(), iniDir);
                button.Background = Brushes.Transparent;
                isCountingDown = false;
                return;
            }

            button.Content = $"JOY{returnedValue.Keys.First()}_{returnedValue.Values.First().ConvertInput()}";
            IniFileHelper.WriteValue("Global", inputName, button.Content.ToString(), iniDir);
            button.Background = Brushes.Transparent;
            isCountingDown = false;
        }


        private void btnSetControl_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            btnEvent(sender, "InputStart1");
        }

        private async Task<Dictionary<int, Gamepad>> GetInput(Button button)
        {
            State previousState1 = new State();
            State previousState2 = new State();
            State previousState3 = new State();
            State previousState4 = new State();
            if (controller1.IsConnected)
                previousState1 = controller1.GetState();
            if (controller2.IsConnected)
                previousState2 = controller2.GetState();
            if (controller3.IsConnected)
                previousState3 = controller3.GetState();
            if (controller4.IsConnected)
                previousState4 = controller4.GetState();

            TimeSpan timeout = TimeSpan.FromSeconds(countdownSeconds);
            var stopwatch = Stopwatch.StartNew();

            key = string.Empty;
            curKeyBoard.Poll();

            do
            {
                State state1 = new State();
                State state2 = new State();
                State state3 = new State();
                State state4 = new State();

                if (controller1.IsConnected)
                    state1 = controller1.GetState();
                if (controller2.IsConnected)
                    state2 = controller2.GetState();
                if (controller3.IsConnected)
                    state3 = controller3.GetState();
                if (controller4.IsConnected)
                    state4 = controller4.GetState();

                var datas = curKeyBoard.GetBufferedData();
                foreach (var state in datas)
                {
                    if (state.IsPressed)
                    {
                        Debug.WriteLine($"Key: {state.Key}");
                        key = state.Key.ToString();
                        return new() { { 0, new Gamepad() } };
                    }
                }

                //  Check Controller #1
                if (previousState1.PacketNumber != state1.PacketNumber)
                {
                    if (state1.Gamepad.LeftThumbX > 12000 || state1.Gamepad.LeftThumbX < -12000 ||
                        state1.Gamepad.LeftThumbY > 12000 || state1.Gamepad.LeftThumbY < -12000 ||
                        state1.Gamepad.RightThumbX > 12000 || state1.Gamepad.RightThumbX < -12000 ||
                        state1.Gamepad.RightThumbY > 12000 || state1.Gamepad.RightThumbY < -12000 ||
                        state1.Gamepad.Buttons.ToString() != "None" ||
                        state1.Gamepad.LeftTrigger > 50 || state1.Gamepad.RightTrigger > 50)
                    {
                        return (new() { { 1, state1.Gamepad } });
                    }
                }

                //  Check Controller #2
                if (controller2.IsConnected)
                {
                    state2 = controller2.GetState();

                    if (previousState2.PacketNumber != state2.PacketNumber)
                    {
                        if (state2.Gamepad.LeftThumbX > 12000 || state2.Gamepad.LeftThumbX < -12000 ||
                            state2.Gamepad.LeftThumbY > 12000 || state2.Gamepad.LeftThumbY < -12000 ||
                            state2.Gamepad.RightThumbX > 12000 || state2.Gamepad.RightThumbX < -12000 ||
                            state2.Gamepad.RightThumbY > 12000 || state2.Gamepad.RightThumbY < -12000 ||
                            state2.Gamepad.Buttons.ToString() != "None" ||
                            state2.Gamepad.LeftTrigger > 50 || state2.Gamepad.RightTrigger > 50)

                            return (new() { { 2, state2.Gamepad } });
                    }
                }

                //  Check Controller #3
                if (controller3.IsConnected)
                {
                    state3 = controller3.GetState();

                    if (previousState3.PacketNumber != state3.PacketNumber)
                    {
                        if (state3.Gamepad.LeftThumbX > 12000 || state3.Gamepad.LeftThumbX < -12000 ||
                            state3.Gamepad.LeftThumbY > 12000 || state3.Gamepad.LeftThumbY < -12000 ||
                            state3.Gamepad.RightThumbX > 12000 || state3.Gamepad.RightThumbX < -12000 ||
                            state3.Gamepad.RightThumbY > 12000 || state3.Gamepad.RightThumbY < -12000 ||
                            state3.Gamepad.Buttons.ToString() != "None" ||
                            state3.Gamepad.LeftTrigger > 50 || state3.Gamepad.RightTrigger > 50)

                            return (new() { { 3, state3.Gamepad } });
                    }
                }

                //  Check Controller #4
                if (controller4.IsConnected)
                {
                    state4 = controller4.GetState();

                    if (previousState4.PacketNumber != state4.PacketNumber)
                    {
                        if (state4.Gamepad.LeftThumbX > 12000 || state4.Gamepad.LeftThumbX < -12000 ||
                            state4.Gamepad.LeftThumbY > 12000 || state4.Gamepad.LeftThumbY < -12000 ||
                            state4.Gamepad.RightThumbX > 12000 || state4.Gamepad.RightThumbX < -12000 ||
                            state4.Gamepad.RightThumbY > 12000 || state4.Gamepad.RightThumbY < -12000 ||
                            state4.Gamepad.Buttons.ToString() != "None" ||
                            state4.Gamepad.LeftTrigger > 50 || state4.Gamepad.RightTrigger > 50)

                            return (new() { { 4, state4.Gamepad } });
                    }
                }

                await Task.Delay(100);
                remainingSeconds -= 0.1;
                button.Content = $"Wait for {remainingSeconds:F1} s...";
                previousState1 = state1;
                previousState2 = state2;
                previousState3 = state3;
                previousState4 = state4;
                continue;
            }
            while (stopwatch.Elapsed < timeout);

            //There was no input from any controller
            return (new() { { 0, new Gamepad() } });
        }

        // Spikeout Buttons' Event
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputStart1");
        }
        private void btnCoin_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputCoin1");
        }
        private void btnUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputJoyUp");
        }
        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputJoyDown");
        }
        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputJoyLeft");
        }
        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputJoyRight");
        }
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputShift");
        }
        private void btnBeat_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputBeat");
        }
        private void btnCharge_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputCharge");
        }
        private void btnJump_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputJump");
        }


        // Fighting Games - P1 Buttons' Event
        private void btnGuard_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputGuard");
        }
        private void btnPunch_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputPunch");
        }
        private void btnCKick_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputKick");
        }
        private void btnEvade_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputEvade");
        }

        // Fighting Games - P2 Buttons' Event
        private void btnStartFightP2_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputStart2");
        }
        private void btnCoinFightP2_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputCoin2");
        }
        private void btnUpFightP2_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputJoyUp2");
        }
        private void btnDownFightP2_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputJoyDown2");
        }
        private void btnLeftFightP2_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputJoyLeft2");
        }
        private void btnRightFightP2_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputJoyRight2");
        }
        private void btnGuardP2_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputGuard2");
        }
        private void btnPunchP2_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputPunch2");
        }
        private void btnCKickP2_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputKick2");
        }
        private void btnEvadeP2_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputEvade2");
        }

        // Virtua Striker 2 - P1 Buttons' Event
        private void btnStartVs2P2_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputStart2");
        }
        private void btnCoinVs2P2_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputCoin2");
        }
        private void btnShortPassP1_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputShortPass");
        }
        private void btnLongPassP1_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputLongPass");
        }
        private void btnShootP1_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputShoot");
        }
        // Virtua Striker 2 - P1 Buttons' Event
        private void btnShortPassP2_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputShortPass2");
        }
        private void btnLongPassP2_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputLongPass2");
        }
        private void btnShootP2_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputShoot2");
        }

        private void btnComP1Start_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputStart1");
        }

        private void btnComP1Coin_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputCoin1");
        }

        private void btnComP2Start_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputStart2");
        }

        private void btnComP2Coin_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputCoin2");
        }

        private void btnComServiceA_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputServiceA");
        }

        private void btnComServiceB_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputServiceB");
        }

        private void btnComTestA_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputTestA");
        }

        private void btnComTestB_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputTestB");
        }

        private void btnComUp_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputJoyUp");
        }

        private void btnComDown_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputJoyDown");
        }

        private void btnComLeft_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputJoyLeft");
        }

        private void btnComRight_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown == true) { return; }
            btnEvent(sender, "InputJoyRight");
        }
    }
}