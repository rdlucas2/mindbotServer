using Microsoft.AspNet.SignalR.Client;
using MindBotConnector.Models;
using NeuroSky.ThinkGear;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MindBotConnector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Connector _connector;
        private static byte _poorSig;
        private static BrainData _brainData = new BrainData();
        private static HubConnection _hubConnection;
        private static IHubProxy _dataHubProxy;
        private static IHubProxy _modeHubProxy;
        private static int? _serverProcessId;

        private static readonly object _object = new object();

        #region button states

        private enum ButtonState
        {
            On,
            Off
        }

        private ButtonState _serverBtnState = ButtonState.Off;
        private ButtonState _clientBtnState = ButtonState.Off;
        private ButtonState _headsetBtnState = ButtonState.Off;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
        }

        #region events

        #region window
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            StopSignalRServer();
        }

        private void btn_instructions_Click(object sender, RoutedEventArgs e)
        {
            var instructionsWindow = new instructions();
            instructionsWindow.Show();
        }

        private void botMode_comboBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();
            data.Add("Circle Game");
            data.Add("Meditation");
            data.Add("Remote");
            data.Add("StopRobot");

            // ... Get the ComboBox reference.
            var comboBox = sender as ComboBox;

            // ... Assign the ItemsSource to the List.
            comboBox.ItemsSource = data;

            // ... Make the first item selected.
            comboBox.SelectedIndex = 0;
        }

        private void botMode_comboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            BroadcastMode();
        }

        private void BroadcastMode()
        {
            var mode = botMode_comboBox.SelectedItem as string;
            if (_modeHubProxy != null)
                _modeHubProxy.Invoke("ModeBroadcast", mode);
        }

        private void btn_server_Click(object sender, RoutedEventArgs e)
        {
            ToggleSignalRServer();
        }

        private void btn_client_Click(object sender, RoutedEventArgs e)
        {
            ToggleSignalRClient();
        }

        private void btn_headset_Click(object sender, RoutedEventArgs e)
        {
            ToggleMindWave();
        }

        private void ToggleSignalRServer()
        {
            if (_serverBtnState == ButtonState.Off)
            {
                if (StartSignalRServer())
                {
                    _serverBtnState = ButtonState.On;
                    btn_server.Content = "Stop SignalR Server";
                }
            }
            else
            {
                StopSignalRServer();
                _serverBtnState = ButtonState.Off;
                btn_server.Content = "Start SignalR Server";
            }
        }

        private void ToggleSignalRClient()
        {
            if (_clientBtnState == ButtonState.Off)
            {
                if(ConnectSignalRClient())
                {
                    BroadcastMode();
                    _clientBtnState = ButtonState.On;
                    btn_client.Content = "Disconnect SignalR Client";
                }
            }
            else
            {
                DisconnectSignalRClient();
                _clientBtnState = ButtonState.Off;
                btn_client.Content = "Connect SignalR Client";
            }
        }

        private void ToggleMindWave()
        {
            if (_headsetBtnState == ButtonState.Off)
            {
                if(ConnectMindWave())
                {
                    _headsetBtnState = ButtonState.On;
                    btn_headset.Content = "Disconnect Headset";
                }
            }
            else
            {
                DisconnectMindWave();
                _headsetBtnState = ButtonState.Off;
                btn_headset.Content = "Connect Headset";
            }
        }

        #endregion

        #region mindwave mobile

        // Called when a device is connected
        private void OnDeviceConnected(object sender, EventArgs e)
        {
            Connector.DeviceEventArgs de = (Connector.DeviceEventArgs)e;
            this.Dispatcher.Invoke((Action)(() =>
            {
                listBox_log.Items.Insert(0, "Device found on: " + de.Device.PortName);
            }));
            de.Device.DataReceived += new EventHandler(OnDataReceived);
        }

        // Called when scanning fails
        private void OnDeviceFail(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                ToggleMindWave();
                listBox_log.Items.Insert(0, "No devices found! :(");
            }));
        }

        // Called when each port is being validated
        private void OnDeviceValidating(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                listBox_log.Items.Insert(0, "validating...");
            }));
        }

        // Called when data is received from a device
        private void OnDataReceived(object sender, EventArgs e)
        {
            Device.DataEventArgs de = (Device.DataEventArgs)e;
            DataRow[] tempDataRowArray = de.DataRowArray;

            TGParser tgParser = new TGParser();
            tgParser.Read(de.DataRowArray);

            /* Loops through the newly parsed data of the connected headset*/
            // The comments below indicate and can be used to print out the different data outputs.

            var brainData = new BrainData();

            for (int i = 0; i < tgParser.ParsedData.Length; i++)
            {
                //if (tgParser.ParsedData[i].ContainsKey("Raw")){
                //Console.WriteLine("Raw Value:" + tgParser.ParsedData[i]["Raw"]);
                //}

                brainData.TimeStamp = DateTime.Now; //tgParser.ParsedData[i]["Time"]; //this looks like a javascript date - replace it with DateTime.Now

                //if (tgParser.ParsedData[i].ContainsKey("Time"))
                //{
                //Console.WriteLine("TimeStamp:" + tgParser.ParsedData[i]["Time"]);
                //}

                if (tgParser.ParsedData[i].ContainsKey("PoorSignal"))
                {
                    //The following line prints the Time associated with the parsed data
                    //Console.WriteLine("Time:" + tgParser.ParsedData[i]["Time"]);

                    //A Poor Signal value of 0 indicates that your headset is fitting properly
                    //Console.WriteLine("Poor Signal:" + tgParser.ParsedData[i]["PoorSignal"]);
                    _poorSig = (byte)tgParser.ParsedData[i]["PoorSignal"];
                    brainData.DevicePoorSignal = _poorSig;
                }

                if (tgParser.ParsedData[i].ContainsKey("Attention"))
                {
                    //Console.WriteLine("Att Value:" + tgParser.ParsedData[i]["Attention"]);
                    brainData.Attention = tgParser.ParsedData[i]["Attention"];
                }

                if (tgParser.ParsedData[i].ContainsKey("Meditation"))
                {
                    //Console.WriteLine("Med Value:" + tgParser.ParsedData[i]["Meditation"]);
                    brainData.Meditation = tgParser.ParsedData[i]["Meditation"];
                }

                if (tgParser.ParsedData[i].ContainsKey("BlinkStrength"))
                {
                    //Console.WriteLine("Eyeblink: " + tgParser.ParsedData[i]["BlinkStrength"]);
                    brainData.BlinkStrength = tgParser.ParsedData[i]["BlinkStrength"];
                    if (brainData.BlinkStrength > 0)
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            listBox_log.Items.Insert(0, string.Format("Blink Detected! Strength: {0}", brainData.BlinkStrength));
                        }));
                        if (_dataHubProxy != null)
                            _dataHubProxy.Invoke("BlinkBroadcast", brainData.BlinkStrength);
                    }
                }

                if (tgParser.ParsedData[i].ContainsKey("EegPowerDelta"))
                {
                    //Console.WriteLine("EegPowerDelta: " + tgParser.ParsedData[i]["EegPowerDelta"]);
                    brainData.EegPowerDelta = tgParser.ParsedData[i]["EegPowerDelta"];
                }

                if (tgParser.ParsedData[i].ContainsKey("EegPowerDelta"))
                {
                    //Console.WriteLine("EegPowerTheta: " + tgParser.ParsedData[i]["EegPowerTheta"]);
                    brainData.EegPowerTheta = tgParser.ParsedData[i]["EegPowerTheta"];
                }

                if (tgParser.ParsedData[i].ContainsKey("EegPowerDelta"))
                {
                    //Console.WriteLine("EegPowerAlpha1: " + tgParser.ParsedData[i]["EegPowerAlpha1"]);
                    brainData.EegPowerAlpha1 = tgParser.ParsedData[i]["EegPowerAlpha1"];
                }

                if (tgParser.ParsedData[i].ContainsKey("EegPowerAlpha2"))
                {
                    //Console.WriteLine("EegPowerAlpha2: " + tgParser.ParsedData[i]["EegPowerAlpha2"]);
                    brainData.EegPowerAlpha2 = tgParser.ParsedData[i]["EegPowerAlpha2"];
                }

                if (tgParser.ParsedData[i].ContainsKey("EegPowerBeta1"))
                {
                    //Console.WriteLine("EegPowerBeta1: " + tgParser.ParsedData[i]["EegPowerBeta1"]);
                    brainData.EegPowerBeta1 = tgParser.ParsedData[i]["EegPowerBeta1"];
                }

                if (tgParser.ParsedData[i].ContainsKey("EegPowerBeta2"))
                {
                    //Console.WriteLine("EegPowerBeta2: " + tgParser.ParsedData[i]["EegPowerBeta2"]);
                    brainData.EegPowerBeta2 = tgParser.ParsedData[i]["EegPowerBeta2"];
                }

                if (tgParser.ParsedData[i].ContainsKey("EegPowerGamma1"))
                {
                    //Console.WriteLine("EegPowerGamma1: " + tgParser.ParsedData[i]["EegPowerGamma1"]);
                    brainData.EegPowerGamma1 = tgParser.ParsedData[i]["EegPowerGamma1"];
                }

                if (tgParser.ParsedData[i].ContainsKey("EegPowerGamma2"))
                {
                    //Console.WriteLine("EegPowerGamma1: " + tgParser.ParsedData[i]["EegPowerGamma2"]);
                    brainData.EegPowerGamma2 = tgParser.ParsedData[i]["EegPowerGamma2"];
                }
            }

            if (newDataCheck(brainData))
            {
                _brainData = brainData;
                DisplayBrainData(_brainData);
                if(_dataHubProxy != null)
                    _dataHubProxy.Invoke("ClientBroadcast", _brainData);
            }
        }

        private void DisplayBrainData(BrainData brainData)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                dataGrid_brain.Items.Add(brainData);
                dataGrid_brain.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("TimeStamp", System.ComponentModel.ListSortDirection.Descending));
            }));
        }

        private bool newDataCheck(BrainData brainData)
        {
            if (
                brainData.Attention == 0 &&
                brainData.Meditation == 0 &&
                brainData.EegPowerAlpha1 == 0 &&
                brainData.EegPowerAlpha2 == 0 &&
                brainData.EegPowerBeta1 == 0 &&
                brainData.EegPowerBeta2 == 0 &&
                brainData.EegPowerDelta == 0 &&
                brainData.EegPowerGamma1 == 0 &&
                brainData.EegPowerGamma2 == 0 &&
                brainData.EegPowerTheta == 0
                )
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool ConnectMindWave()
        {
            listBox_log.Items.Insert(0, "Attempting to connect the mindwave mobile...");

            try
            {
                lock(_object)
                {
                    // Initialize a new Connector and add event handlers
                    _connector = new Connector();
                    _connector.DeviceConnected += new EventHandler(OnDeviceConnected);
                    _connector.DeviceConnectFail += new EventHandler(OnDeviceFail);
                    _connector.DeviceValidating += new EventHandler(OnDeviceValidating);

                    // Scan for devices across COM ports
                    // The COM port named will be the first COM port that is checked if provided.
                    _connector.ConnectScan();

                    // Blink detection needs to be manually turned on
                    _connector.setBlinkDetectionEnabled(true);
                    //_connector.setAppreciationEnabled(true);
                    //_connector.setMentalEffortEnable(true);
                    //_connector.setPositivityEnable(true);
                    //connector.setRespirationRateEnable(true);
                    //_connector.setTaskFamiliarityEnable(true);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void DisconnectMindWave()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                listBox_log.Items.Insert(0, "Disconnecting the mindwave mobile...");
            }));

            try
            {
                lock(_object)
                {
                    _connector.Close();
                    _connector = null;
                    listBox_log.Items.Insert(0, "Disconnected!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region signalR

        private bool StartSignalRServer()
        {
            listBox_log.Items.Insert(0, "Starting SignalR Server...");
            const int ERROR_CANCELLED = 1223; //The operation was canceled by the user.
            try
            {
                var currentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                ProcessStartInfo info = new ProcessStartInfo(currentDirectory + @"\signalRServer\signalRServer.exe");
                info.UseShellExecute = true;
                info.Verb = "runas";

                var serverProcess = Process.Start(info);
                _serverProcessId = serverProcess.Id;
                return true;
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == ERROR_CANCELLED)
                    MessageBox.Show("You must be an admin to run this application!");
                else
                    MessageBox.Show(ex.ToString(), "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void StopSignalRServer()
        {
            listBox_log.Items.Insert(0, "Stopping SignalR Server!");
            try
            {
                if (_serverProcessId.HasValue)
                {
                    Process.GetProcessById(_serverProcessId.Value).Kill();
                    _serverProcessId = null;
                }
                else
                    listBox_log.Items.Insert(0, "No Process Id saved for SignalR Server");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ConnectSignalRClient()
        {
            try
            {
                lock (_object)
                {
                    _hubConnection = new HubConnection("http://localhost:8080");
                    _dataHubProxy = _hubConnection.CreateHubProxy("DataHub");
                    _modeHubProxy = _hubConnection.CreateHubProxy("ModeHub");
                    _modeHubProxy.On("reportMode", new Action(() => {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            BroadcastMode();
                        }));
                    }));
                    _hubConnection.Start().Wait();
                    listBox_log.Items.Insert(0, "Client Connected To SignalR Server!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void DisconnectSignalRClient()
        {
            listBox_log.Items.Insert(0, "Disconnecting Client from SignalR Server...");
            try
            {
                lock (_object)
                {
                    _dataHubProxy = null;
                    _modeHubProxy = null;
                    _hubConnection.Stop();
                    _hubConnection = null;
                    listBox_log.Items.Insert(0, "Disconnected!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #endregion

    }
}
