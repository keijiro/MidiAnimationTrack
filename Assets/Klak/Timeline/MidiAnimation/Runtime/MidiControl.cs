using UnityEngine;

namespace Klak.Timeline
{
    [System.Serializable]
    public class MidiControl
    {
        public int controlNumber = 1;

        public string componentName;
        public string propertyName;
        public string fieldName;

        public Vector4 vector0 = Vector3.zero;
        public Vector4 vector1 = Vector3.forward;
    }
}
