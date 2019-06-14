using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Klak.Timeline.Midi
{
    // Receives MIDI signals (MIDI event notifications) from a timeline and
    // invokes assigned events.
    [ExecuteInEditMode]
    public sealed class MidiSignalReceiver : MonoBehaviour, INotificationReceiver
    {
        public MidiNoteFilter noteFilter = new MidiNoteFilter {
            note = MidiNote.All, octave = MidiOctave.All
        };

        public UnityEvent noteOnEvent = new UnityEvent();
        public UnityEvent noteOffEvent = new UnityEvent();

        public void OnNotify
            (Playable origin, INotification notification, object context)
        {
            var signal = (MidiSignal)notification;
            if (!noteFilter.Check(signal.Event)) return;
            (signal.Event.IsNoteOn ? noteOnEvent : noteOffEvent).Invoke();
        }
    }
}
