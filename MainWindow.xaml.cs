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

        private void ScanAudioDevices(){
            List<PXCMAudioSource.DeviceInfo> devices = recognizer.LoadAudioDevices();
            if (devices != null && devices.Count != 0)
                for (int i = 0; i < devices.Count-1; i++)
                {
                    this.Dispatcher.BeginInvoke(new Action(() => devicesCombo.Items.Add(devices[i].name)));
                }
            else
                AddToHistory("DEVICES DOESN'T FOUND");
        }
        private void OnDisconnect()
        {

        }

        private void ConfigureRecognizer(){
            recognizer = new Recognizer(OnRecognized, AddToHistory);
        }

        private void ConfigureTransmission(){
            transmission = new TransmissionManager(OnReseivedMessage, OnSentMessage, AddToHistory);
            transmission.onConnectionStatusChanged += OnConnectionStatusChanged;
            transmission.onServerStatusChanged += OnServerStatusChanged;
        }

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
                    AddToHistory("Add to history commend");
                    OnCommand("ADD TO DICTIONARY");
                    break;
                case Requests.LOAD_DICTIONARY:
                    AddToHistory("Load history commend");
                    OnCommand("LOAD DICTIONARY");
                    break;
            }
        }

        private void OnRecognitionStart()
        {
            if (!recognitionRan)
            {
                new Thread(() =>
                {
                    recognizer.Run();
                }).Start();
                recognitionRan = true;
            }
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

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if(transmissionRan)
                transmission.Close();
            if (recognitionRan)
                recognizer.Close();

        }
    }
}
