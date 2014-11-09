using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace recognizer
{

    public delegate void OnRecognized(PXCMSpeechRecognition.RecognitionData data);
    public delegate void Notify(String notify);


    class Recognizer
    {
        private static List<PXCMAudioSource.DeviceInfo> devices = new List<PXCMAudioSource.DeviceInfo>();
        private const int DICTATION_MODE = 0;
        private OnRecognized recognized;
        private Notify notify;
        private bool running = false;
        private Dictionary<int, List<Grammar>> grammars = null;
        private int checkedDevice = 0;
        private int activeGrammar = DICTATION_MODE; // Default dictation
        private bool grammarChanged = false;
        private MainWindow window;

        public Recognizer(OnRecognized recognized, Notify notify, MainWindow window)
        {
            this.recognized = recognized;
            this.notify = notify;
            this.window = window;
        }

 
        public List<PXCMAudioSource.DeviceInfo> LoadAudioDevices()
        {
            PXCMSession session = PXCMSession.CreateInstance();
            notify(String.Format("SDK Version {0}.{1}", session.QueryVersion().major, session.QueryVersion().minor));
            PXCMAudioSource source;
            source = session.CreateAudioSource();
            source.ScanDevices();
            for (int i = 0; ; i++)
            {
                PXCMAudioSource.DeviceInfo dinfo;
                if (source.QueryDeviceInfo(i, out dinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                devices.Add(dinfo);
                notify("Found device : " + dinfo.name);
            }
            source.Dispose();
            session.Dispose();
            return devices;
        }

        public void LoadGrammars(Dictionary<int, List<Grammar>> grms)
        {
            grammars = grms;
        }

        private int GetActiveGrammar()
        {
            return activeGrammar;
        }

        public void SetActiveGrammar(int grammarId)
        {
            this.activeGrammar = grammarId;
            grammarChanged = true;
        }



        public void Run()
        {
            using (PXCMSession session = PXCMSession.CreateInstance())
            {
                PXCMSpeechRecognition sr;
                pxcmStatus status = session.CreateImpl<PXCMSpeechRecognition>(out sr);
                notify("STATUS : " + status);
                PXCMSpeechRecognition.ProfileInfo pinfo;
                sr.QueryProfile(0, out pinfo);
                sr.SetProfile(pinfo);


                // Load grammar
                foreach (KeyValuePair<int, List<Grammar>> pair in grammars)
                {
                    List<String> cmds = new List<string>();
                    List<int> labels = new List<int>();
                    foreach (Grammar g in pair.Value)
                    {
                        cmds.Add(g.command);
                        labels.Add(g.label);
                    }
                    sr.BuildGrammarFromStringList(pair.Key, cmds.ToArray(), labels.ToArray());
                }

                int active = GetActiveGrammar();
                if (active == DICTATION_MODE)
                {
                    sr.SetDictation();
                }
                else
                {
                    // Set the active grammar.
                    sr.SetGrammar(active);
                }

                using (PXCMAudioSource source = session.CreateAudioSource())
                {
                    source.ScanDevices();
                    if (devices == null)
                        LoadAudioDevices();
                    source.SetDevice(GetCheckedSource());
                 //   source.SetVolume(0.2f);
                    PXCMSpeechRecognition.Handler handler = new PXCMSpeechRecognition.Handler();
                    handler.onRecognition = OnRecognition;
                    // sr is a PXCMSpeechRecognition instance
                    status = sr.StartRec(source, handler);
                    notify("AFTER start : " + status);
                    notify("Ran with active grammarId : " + activeGrammar);
                    running = true;
                    window.SendRecognitionStarted();
                    while (running)
                    {
                        System.Threading.Thread.Sleep(1);
                        if (grammarChanged)
                        {
                            int activeGrammarId = GetActiveGrammar();
                            if (activeGrammarId == DICTATION_MODE)
                            {
                                sr.SetDictation();
                            }
                            else
                            {
                                // Set the active grammar.
                                sr.SetGrammar(activeGrammarId);
                            }
                            notify("Grammar changed into grammarId : " + activeGrammarId);
                            grammarChanged = false;
                        }
                    }
                    sr.StopRec();
                }
                //source.Dispose();
            }
            //session.Dispose();
        }

        public void Close()
        {
            running = false;
        }

        public PXCMAudioSource.DeviceInfo GetCheckedSource()
        {
            notify("SELECTED : " + devices[checkedDevice].name);
            return devices[checkedDevice];
        }

        public void CheckDevice(int id)
        {
            checkedDevice = id;
        }

        public void OnRecognition(PXCMSpeechRecognition.RecognitionData data)
        {
            notify("RECOGNIZED sentence : " + data.scores[0].sentence);
            notify("RECOGNIZED tags : " + data.scores[0].label);
            recognized(data);
        }
    }

    public class Grammar
    {
        public int label { set; get; }
        public string command { set; get; }
    }

}
