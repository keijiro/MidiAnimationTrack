namespace Klak.Timeline.Midi
{
    // MIDI event raw data struct
    [System.Serializable]
    public struct MidiEvent
    {
        public uint time;
        public byte status;
        public byte data1;
        public byte data2;

        public bool IsCC      { get { return (status & 0xb0) == 0xb0; } }
        public bool IsNote    { get { return (status & 0xe0) == 0x80; } }
        public bool IsNoteOn  { get { return (status & 0xf0) == 0x90; } }
        public bool IsNoteOff { get { return (status & 0xf0) == 0x80; } }

        public override string ToString()
        {
            return string.Format("[{0}: {1:X}, {2}, {3}]", time, status, data1, data2);
        }
    }
}
