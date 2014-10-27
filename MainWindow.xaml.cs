using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;

namespace recognizer
{
    public delegate void OnMessage(Request request);
    public delegate void OnDisconnect();
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Recognizer recognizer;
        private TransmissionManager transmission;
        private bool transmissionRan = false;
        private bool recognitionRan = false;
        private Thread recognitionThread;
        private WinForms.NotifyIcon notifier = new WinForms.NotifyIcon();

        public MainWindow()
        {
            InitializeComponent();
            ConfigureTransmission();
            ConfigureRecognizer();
            ScanAudioDevices();
            // InitTray();
            RunTransmission();
        }

        private void ScanAudioDevices()
        {
            List<PXCMAudioSource.DeviceInfo> devices = recognizer.LoadAudioDevices();
            if (devices != null && devices.Count != 0)
            {
                foreach (PXCMAudioSource.DeviceInfo d in devices)
                {
                    this.Dispatcher.BeginInvoke(new Action(() => devicesCombo.Items.Add((string)d.name.Clone())));
                }
                this.Dispatcher.BeginInvoke(new Action(() => devicesCombo.SelectedIndex = 0));
            }
            else
                AddToHistory("DEVICES DOESN'T FOUND");
        }
        private void OnDisconnect()
        {
            try
            {
                if (recognitionRan)
                {
                    recognizer.Close();
                    recognitionRan = false;
                    recognizer = null;
                    ChandeRecognitionStatus("not started");
                    ConfigureRecognizer();
                }
            }
            catch (Exception e)
            {
                AddToHistory("ERROR :" + e.Message);
            }
        }



        private void ConfigureRecognizer()
        {
            recognizer = new Recognizer(OnRecognized, AddToHistory);
        }

        private void ConfigureTransmission()
        {
            transmission = new TransmissionManager(OnReseivedMessage, OnSentMessage, AddToHistory);
            transmission.onConnectionStatusChanged += OnConnectionStatusChanged;
            transmission.onServerStatusChanged += OnServerStatusChanged;
            transmission.onDisconnect += this.OnDisconnect;
        }

        private void RunTransmission()
        {
            if (!transmissionRan)
            {
                new Thread(() =>
              {
                  transmission.Run();

              }).Start();
                transmissionRan = true;
            }
        }

        private void AddToHistory(String message)
        {
            this.Dispatcher.BeginInvoke(new Action(() => historyListBox.Items.Add(message)));
        }

        private void OnSentMessage(String message)
        {
            AddToHistory("SENT : " + message);
        }

        private void OnReseivedMessage(Request message)
        {
            switch (message.requestType)
            {
                case Requests.START_RECOGNITION:
                    AddToHistory("Start recognition");
                    OnRecognitionStart();
                    OnCommand("START RECOGNITION");
                    break;
                case Requests.ADD_TO_DICTIONARY:
                    AddToHistory("Add to history command ignored");
                    OnCommand("ADD TO DICTIONARY");
                    break;
                case Requests.LOAD_DICTIONARY:
                    AddToHistory("Load history command");
                    OnCommand("LOAD DICTIONARY");
                    OnLoadDictionary(message.data);
                    break;
                case Requests.SYNTHESIZE:
                    AddToHistory("Synhesize command");
                    OnCommand("SYNTHESIZE");
                    Synthesizer.Speak(message.data.ToString());
                    break;
            }
        }

        class Dictionary
        {
            public Grammar[] grammars { set; get; }
        }

        private void OnLoadDictionary(object dict)
        {
            Dictionary dictionary = JsonConvert.DeserializeObject<Dictionary>(dict.ToString());
            if (recognizer != null)
                recognizer.LoadGrammars(dictionary.grammars.ToList());
        }

        private void OnRecognitionStart()
        {
            if (!recognitionRan)
            {
                recognitionThread = new Thread(() =>
                 {
                     recognizer.Run();
                 });
                recognitionThread.Start();
                recognitionRan = true;
                ChandeRecognitionStatus("running");
            }
        }

        private void ChandeRecognitionStatus(string content)
        {
            this.Dispatcher.BeginInvoke(new Action(() => recStatusLbl.Content = content));
        }

        private void OnCommand(string content)
        {
            this.Dispatcher.BeginInvoke(new Action(() => lastCommandLbl.Content = content));
        }
        private void OnServerStatusChanged(string status)
        {
            this.Dispatcher.BeginInvoke(new Action(() => serverStatusLbl.Content = status));
        }

        private void OnConnectionStatusChanged(string status)
        {
            this.Dispatcher.BeginInvoke(new Action(() => connectionLbl.Content = status));
        }

        private void OnRecognized(PXCMSpeechRecognition.RecognitionData data)
        {
            transmission.SendMessage(new Request(Requests.RECOGNITION_RESULT, data));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (transmissionRan)
                transmission.Close();
            // OnDisconnect();
            if (recognitionRan)
            {
                recognizer.Close();
                recognizer = null;
            }

        }

        private void devicesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddToHistory("SELECTED : " + devicesCombo.SelectedIndex);
            if (recognizer != null)
                recognizer.CheckDevice(devicesCombo.SelectedIndex);
        }
    }
}


/*
        private void InitTray()
        {
            this.notifier.MouseDown += new WinForms.MouseEventHandler(notifier_MouseDown);
            this.notifier.Icon = Properties.Resources.icon;
            this.notifier.Visible = true;
        }

        void notifier_MouseDown(object sender, WinForms.MouseEventArgs e)
        {
            if (e.Button == WinForms.MouseButtons.Right)
            {
                ContextMenu menu = new ContextMenu();
                MenuItem item1 = new MenuItem();
                MenuItem item2 = new MenuItem();

                item1.Header = "&Open";
                item1.Click += new RoutedEventHandler(Menu_Open);
                menu.Items.Add(item1);

                item2.Header = "&Close";
                item2.Click += new RoutedEventHandler(Menu_Close);
                menu.Items.Add(item1);
                menu.Items.Add(item2);
                menu.IsOpen = true;
            }
        }


        private void Menu_Open(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Open");
        }

        private void Menu_Close(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Close");
        }

        */
