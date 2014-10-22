using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace recognizer
{
    class TransmissionManager
    {
        public const int SERVER_PORT = 5555;
        public const string IP_ADDRESS = "127.0.0.1";

        private TcpListener listener;
        private ClientHandler handler;
        private bool active;
        private OnMessage onMessageReseived;
        private Notify onMessageSent;
        private Notify notify;
        private Thread clientThread;
        public event Notify onServerStatusChanged;
        public event Notify onConnectionStatusChanged;

        public TransmissionManager(OnMessage onMessageReseived, Notify onMessageSent, Notify notify)
        {
            listener = new TcpListener(IPAddress.Parse(IP_ADDRESS), SERVER_PORT);
            this.onMessageReseived = onMessageReseived;
            this.onMessageSent = onMessageSent;
            this.notify = notify;
        }

        public void Run()
        {
            try
            {
                Listen();
            }
            catch (Exception)
            {
                onServerStatusChanged("Fail");
                onConnectionStatusChanged("Fail");
            }
        }

        private void Listen()
        {
            listener.Start();
            active = true;
            onServerStatusChanged("Running");
            notify("Waiting connection to : " + IP_ADDRESS + ":" + SERVER_PORT);
            onConnectionStatusChanged("Waiting");
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                notify("Connection established");
                onConnectionStatusChanged("Established");
                clientThread = new Thread(new ParameterizedThreadStart(ClientThread));
                clientThread.Start(client);
            }
        }

        public void Close()
        {
            if (handler != null)
            {
                handler.Close();
                // if(clientThread.IsAlive)

            }
            if (active)
                listener.Stop();
        }
        ~TransmissionManager()
        {
            if (listener != null)
            {
                listener.Stop();
                onConnectionStatusChanged("Stopped");
                onServerStatusChanged("Stopped");
            }
        }

        void ClientThread(Object StateInfo)
        {
            handler = new ClientHandler((TcpClient)StateInfo, onMessageReseived, onMessageSent, notify, this);
        }

        public void ChangeConnationStatus(String status)
        {
            onConnectionStatusChanged(status);
        }
        public void SendMessage(Request response)
        {
            if (handler != null)
                handler.SendMessage(response);
        }
    }

    class ClientHandler
    {
        private TcpClient tcpClient;
        private StreamWriter streamSender;
        private NetworkStream srReceiver;
        private Thread threadMessaging;
        private bool connected;
        private OnMessage onMessageReseived;
        private Notify onMessageSent;
        private Notify notify;
        private TransmissionManager manager;

        public ClientHandler(TcpClient client, OnMessage onMessageReseived, Notify onMessageSent, Notify notify, TransmissionManager manager)
        {
            this.tcpClient = client;
            this.onMessageReseived = onMessageReseived;
            this.onMessageSent = onMessageSent;
            this.notify = notify;
            InitTranmission();
            this.manager = manager;
        }

        private void InitTranmission()
        {
            try
            {
                streamSender = new StreamWriter(tcpClient.GetStream());
                srReceiver = tcpClient.GetStream();
                // Start the thread for receiving messages and further communication
                threadMessaging = new Thread(new ThreadStart(ReceiveMessage));
                threadMessaging.Start();
                connected = true;
                SendMessage(new Request(Requests.CLIENT_CONNECTED, "Connected"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }

        public void Close()
        {
            try
            {
                connected = false;
                if (tcpClient != null)
                    tcpClient.Close();
            }
            catch (Exception ex)
            {
            }
        }

        private void ReceiveMessage()
        {
            // Receive the response from the server
            try
            {
                while (connected)
                {
                    if (srReceiver.CanRead)
                    {
                        byte[] myReadBuffer = new byte[1024];
                        StringBuilder message = new StringBuilder();
                        int numberOfBytesRead = 0;
                        do
                        {
                            numberOfBytesRead = srReceiver.Read(myReadBuffer, 0, myReadBuffer.Length);
                            message.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
                        }
                        while (srReceiver.DataAvailable);

                        processMessage(message.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                notify("Error " + e.Message);
                manager.ChangeConnationStatus("Failed");
            }
        }

        private void processMessage(String message)
        {
            notify("Received : " + message);
            Request request = JsonConvert.DeserializeObject<Request>(message);
            onMessageReseived(request);
        }


        public void SendMessage(Request response)
        {
            if (response != null)
            {
                string output = JsonConvert.SerializeObject(response);
                streamSender.WriteLine(output);
                onMessageSent(output);
                streamSender.Flush();
            }
        }
    }

}


