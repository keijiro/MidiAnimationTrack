using UnityEngine;

namespace Klak.Timeline
{
    [System.Serializable]
    public class MidiControl
    {
        // CC mode
        public int controlNumber = 1;

        // Mono note mode
        public int octave = -1;
        public int noteNumber = -1;

        // Envelope
        public float attack = 1;
        public float decay = 1;
        public float sustain = 1;
        public float release = 0;

        // Component/property
        public ExposedReference<Component> targetComponent;
        public string propertyName;
        public string fieldName;

        // Value options
        public Vector4 vector0 = Vector3.zero;
        public Vector4 vector1 = Vector3.forward;
    }

    public enum MidiControlMode { ControlChange, MonoNote }
}
