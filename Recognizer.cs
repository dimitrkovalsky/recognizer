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

        public Recognizer(OnRecognized recognized, Notify notify)
        {
            this.recognized = recognized;
            this.notify = notify;
        }

        public void Run()
        {
            PXCMSession session = PXCMSession.CreateInstance();
            notify(String.Format("SDK Version {0}.{1}", session.QueryVersion().major, session.QueryVersion().minor));
            PXCMSpeechRecognition sr;
            pxcmStatus status = session.CreateImpl<PXCMSpeechRecognition>(out sr);
            notify("STATUS : " + status);
            PXCMSpeechRecognition.ProfileInfo pinfo;
            sr.QueryProfile(0, out pinfo);
            sr.SetProfile(pinfo);

            String[] cmds = new String[3] { "One", "Two", "Three" };
            // Build the grammar.
            sr.BuildGrammarFromStringList(1, cmds, null);
            // Set the active grammar.
            sr.SetGrammar(1);

            //sr.SetDictation();
            PXCMAudioSource source;

            source = session.CreateAudioSource();
            source.ScanDevices();

            for (int i = 0; ; i++)
            {
                PXCMAudioSource.DeviceInfo dinfo;
                if (source.QueryDeviceInfo(i, out dinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                devices.Add(dinfo);
                notify("Device : " + dinfo.name);
            }


            source.SetDevice(GetCheckedSource());

            PXCMSpeechRecognition.Handler handler = new PXCMSpeechRecognition.Handler();
            handler.onRecognition = OnRecognition;
            // sr is a PXCMSpeechRecognition instance
            status = sr.StartRec(source, handler);
            notify("AFTER start : " + status);
            while (true)
            {
                System.Threading.Thread.Sleep(5);
            }

            session.Dispose();

        }

        public PXCMAudioSource.DeviceInfo GetCheckedSource()
        {
            notify("SELECTED : " + devices[1].name);
            return devices[1];
        }


        public void OnRecognition(PXCMSpeechRecognition.RecognitionData data)
        {
            notify("RECOGNIZED sentence : " + data.scores[0].sentence);
            notify("RECOGNIZED tags : " + data.scores[0].tags);
            recognized(data);
        }
    }

}
