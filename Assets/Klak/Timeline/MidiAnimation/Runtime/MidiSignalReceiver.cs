using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Klak.Timeline
{
    public sealed class MidiSignalReceiver : MonoBehaviour, INotificationReceiver
    {
        public MidiNoteFilter noteFilter = new MidiNoteFilter {
            note = MidiNote.All, octave = MidiOctave.All
        };

        public UnityEvent noteOnEvent = new UnityEvent();

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            var signal = (MidiSignal)notification;
            if (!noteFilter.Check(signal.Event)) return;
            noteOnEvent.Invoke();
        }
    }
}
