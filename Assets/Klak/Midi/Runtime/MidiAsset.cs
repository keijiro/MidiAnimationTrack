using UnityEngine;

namespace Klak.Midi
{
    [System.Serializable]
    public struct MidiEvent
    {
        public uint time;
        public byte status;
        public byte data1;
        public byte data2;

        public override string ToString()
        {
            return string.Format
                ("[{0}: {1:X}, {2}, {3}]", time, status, data1, data2);
        }
    }

    [System.Serializable]
    public sealed class MidiTrack
    {
        public MidiEvent [] events;
    }

    public sealed class MidiAsset : ScriptableObject
    {
        public uint ticksPerQuarterNote;
        public MidiTrack [] tracks;
    }
}
