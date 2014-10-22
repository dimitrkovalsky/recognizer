using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace recognizer
{
    static class Synthesizer
    {
        private static string module = "";
        private static int language = 0;
        private static int curr_volume = 80;
        private static int curr_pitch = 100;
        private static int curr_speech_rate = 100;

        public static string Speak(string sentence)
        {
            PXCMSession session = PXCMSession.CreateInstance();
            if (session == null)
                return "Failed to create an SDK session";

            PXCMSession.ImplDesc desc = new PXCMSession.ImplDesc();
            desc.friendlyName = module;
            desc.cuids[0] = PXCMSpeechSynthesis.CUID;
            PXCMSpeechSynthesis vsynth;
            pxcmStatus sts = session.CreateImpl<PXCMSpeechSynthesis>(desc, out vsynth);
            if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                session.Dispose();
                return "Failed to create the synthesis module";
            }

            PXCMSpeechSynthesis.ProfileInfo pinfo;
            vsynth.QueryProfile(language, out pinfo);
            pinfo.volume = curr_volume;
            pinfo.rate = curr_speech_rate;
            pinfo.pitch = curr_pitch;
            sts = vsynth.SetProfile(pinfo);
            if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                vsynth.Dispose();
                session.Dispose();
                return "Failed to initialize the synthesis module";
            }

            sts = vsynth.BuildSentence(1, sentence);
            if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                VoiceOut vo = new VoiceOut(pinfo.outputs);
                for (int i = 0; ; i++)
                {
                    PXCMAudio sample = vsynth.QueryBuffer(1, i);
                    if (sample == null) break;
                    vo.RenderAudio(sample);
                }
                vo.Close();
            }

            vsynth.Dispose();
            session.Dispose();
            return null;
        }
    }
}
