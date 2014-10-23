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
        private OnRecognized recognized;
        private Notify notify;
        private bool running = false;
        private bool dictationMode = false;
        private List<Grammar> grammars = null;
        private int checkedDevice = 0;
        public Recognizer(OnRecognized recognized, Notify notify)
        {
            this.recognized = recognized;
            this.notify = notify;
        }

        public void SetDictationMode()
        {
            this.dictationMode = true;
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

        public void LoadGrammars(List<Grammar> grms)
        {
            grammars = grms;
        }

        public void Run()
        {
            PXCMSession session = PXCMSession.CreateInstance();
           
            PXCMSpeechRecognition sr;
            pxcmStatus status = session.CreateImpl<PXCMSpeechRecognition>(out sr);
            notify("STATUS : " + status);
            PXCMSpeechRecognition.ProfileInfo pinfo;
            sr.QueryProfile(0, out pinfo);
            sr.SetProfile(pinfo);

            if (dictationMode)
                sr.SetDictation();
            else
            {
                List<String> cmds = new List<string>();
                List<int> labels = new List<int>();
                foreach (Grammar g in grammars)
                {
                    cmds.Add(g.command);
                    labels.Add(g.label);
                }
                sr.BuildGrammarFromStringList(1, cmds.ToArray(), labels.ToArray());
                // Set the active grammar.
                sr.SetGrammar(1);
            }

            PXCMAudioSource source;
            source = session.CreateAudioSource();
            source.ScanDevices();
            if (devices == null)
                LoadAudioDevices();
            source.SetDevice(GetCheckedSource());

            PXCMSpeechRecognition.Handler handler = new PXCMSpeechRecognition.Handler();
            handler.onRecognition = OnRecognition;
            // sr is a PXCMSpeechRecognition instance
            status = sr.StartRec(source, handler);
            notify("AFTER start : " + status);
            running = true;
            while (running)
            {
                System.Threading.Thread.Sleep(5);
            }
            sr.StopRec();
      //      source.Dispose();
       //     session.Dispose();
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

     public class Grammar{
        public int label { set; get; }
        public string command {set; get;}
    }

}
