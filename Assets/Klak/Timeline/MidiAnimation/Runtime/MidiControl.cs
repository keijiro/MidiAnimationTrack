using UnityEngine;

namespace Klak.Timeline
{
    public enum MidiControlMode { ControlChange, MonoNote }

    public enum MidiNote {
        All, C, CSharp, D, DSharp, E, F, FSharp, G, GSharp, A, ASharp, B
    }

    public enum MidiOctave {
        All, Minus2, Minus1, Zero, Plus1, Plus2, Plus3, Plus4, Plus5, Plus6, Plus7, Plus8
    }

    [System.Serializable]
    public struct MidiNoteFilter
    {
        public MidiNote note;
        public MidiOctave octave;
    }

    [System.Serializable]
    public struct MidiEnvelope
    {
        public float attack;
        public float decay;
        public float sustain;
        public float release;
    }

    [System.Serializable]
    public class MidiControl
    {
        // CC mode
        public int controlNumber = 1;

        // Mono note mode
        public MidiNoteFilter noteFilter = new MidiNoteFilter {
            note = MidiNote.All, octave = MidiOctave.All
        };

        // Envelope
        public MidiEnvelope envelope = new MidiEnvelope {
            attack = 1, decay = 1, sustain = 1, release = 0
        };

        // Component/property
        public ExposedReference<Component> targetComponent;
        public string propertyName;
        public string fieldName;

        // Value options
        public Vector4 vector0 = Vector3.zero;
        public Vector4 vector1 = Vector3.forward;
    }
}
