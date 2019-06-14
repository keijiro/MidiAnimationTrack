using UnityEngine;
using System.Collections.Generic;

namespace Klak.Timeline.Midi
{
    // Object pool class for MIDI signals
    sealed class MidiSignalPool
    {
        Stack<MidiSignal> _usedSignals = new Stack<MidiSignal>();
        Stack<MidiSignal> _freeSignals = new Stack<MidiSignal>();

        public MidiSignal Allocate(in MidiEvent data)
        {
            var signal = _freeSignals.Count > 0 ?  _freeSignals.Pop() : new MidiSignal();
            signal.Event = data;
            _usedSignals.Push(signal);
            return signal;
        }

        public void ResetFrame()
        {
            while (_usedSignals.Count > 0) _freeSignals.Push(_usedSignals.Pop());
        }
    }
}
