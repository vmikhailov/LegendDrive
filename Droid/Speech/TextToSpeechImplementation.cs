using Android.Speech.Tts;
using Xamarin.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

[assembly: Dependency(typeof(LegendDrive.Droid.Speech.TextToSpeechImplementation))]
namespace LegendDrive.Droid.Speech
{
    public class TextToSpeechImplementation : Java.Lang.Object, ITextToSpeech, TextToSpeech.IOnInitListener
    {
        TextToSpeech speaker;
        object lockObject = new object();

        public void Speak(string text)
        {
            lock (lockObject)
            {
                if (speaker == null)
                {
                    speaker = new TextToSpeech(Forms.Context, this);
                    var voices = speaker.Voices;
                    speaker.SetSpeechRate(1.5f);
                }
                speaker.Speak(text, QueueMode.Add, null, null);
            }
        }

        public void OnInit(OperationResult status)
        {
            //if (status.Equals(OperationResult.Success))
            //{
            //    speaker.Speak(toSpeak, QueueMode.Flush, null, null);
            //}
        }
    }
}
