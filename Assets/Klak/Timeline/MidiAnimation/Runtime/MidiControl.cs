using UnityEngine;

namespace Klak.Timeline
{
    #region Control parameter types

    public enum MidiControlMode { Note, CC }

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

        public bool Check(in MidiEvent e)
        {
            return e.IsNote &&
                (octave == MidiOctave.All || e.data1 / 12 == (int)octave - 1) &&
                (note   == MidiNote  .All || e.data1 % 12 == (int)note   - 1);
        }
    }

    [System.Serializable]
    public struct MidiEnvelope
    {
        // ADSR parameters
        public float attack;
        public float decay;
        public float sustain;
        public float release;

        // Times in seconds
        public float AttackTime  { get { return Mathf.Max(1e-5f, attack  / 10); } }
        public float DecayTime   { get { return Mathf.Max(1e-5f, decay   / 10); } }
        public float ReleaseTime { get { return Mathf.Max(1e-5f, release / 10); } }

        // Normalized sustain level value
        public float SustainLevel { get { return Mathf.Clamp01(sustain); } }
    }

    #endregion

    #region Serializable MIDI control class

    [System.Serializable]
    public class MidiControl
    {
        // CC mode parameter
        public int controlNumber = 1;

        // Mono note mode parameters
        public MidiNoteFilter noteFilter = new MidiNoteFilter {
            note = MidiNote.All, octave = MidiOctave.All
        };

        // Envelope parameters
        public MidiEnvelope envelope = new MidiEnvelope {
            attack = 0, decay = 0.2f, sustain = 1, release = 0
        };

        // Component/property options
        public ExposedReference<Component> targetComponent;
        public string propertyName;
        public string fieldName;

        // Value options
        public Vector4 vector0 = Vector3.zero;
        public Vector4 vector1 = Vector3.forward;
    }

    #endregion
}
