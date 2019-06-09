using UnityEngine;
using UnityEngine.Playables;

namespace Klak.Timeline
{
    public sealed class MidiSignalReceiver : MonoBehaviour, INotificationReceiver
    {
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            var signal = (MidiSignal)notification;
            Debug.Log("NOTE " + signal.NoteNumber);
        }
    }
}
