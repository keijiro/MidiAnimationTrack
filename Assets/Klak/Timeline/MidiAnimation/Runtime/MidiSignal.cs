using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;

namespace Klak.Timeline
{
    // Notification payload class
    public sealed class MidiSignal : INotification
    {
        // Notification ID (not in use)
        PropertyName INotification.id { get { return default(PropertyName);} }

        // MIDI event
        public MidiEvent Event { get; set; }
    }

    // Object pool class for MIDI signals
    sealed class MidiSignalPool
    {
        Stack<MidiSignal> _usedSignals = new Stack<MidiSignal>();
        Stack<MidiSignal> _freeSignals = new Stack<MidiSignal>();

        public void ResetFrame()
        {
            while (_usedSignals.Count > 0) _freeSignals.Push(_usedSignals.Pop());
        }

        public void PushSignal(Playable playable, PlayableOutput output, in MidiEvent midiEvent)
        {
            var signal = _freeSignals.Count > 0 ? _freeSignals.Pop() : new MidiSignal();
            signal.Event = midiEvent;
            output.PushNotification(playable, signal);
        }
    }
}
