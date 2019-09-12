namespace Klak.Timeline.Midi
{
    // MIDI binary data stream reader
    sealed class MidiDataStreamReader
    {
        #region Internal members

        readonly byte [] _data;
        readonly System.Text.StringBuilder _stringBuilder;

        #endregion

        #region Constructor

        public MidiDataStreamReader(byte [] data)
        {
            _data = data;
            _stringBuilder = new System.Text.StringBuilder();
        }

        #endregion

        #region Current reading position

        public uint Position { get; private set; }

        public void Advance(uint delta)
        {
            Position += delta;
        }

        #endregion

        #region Reader methods

        public byte PeekByte()
        {
            return _data[Position];
        }

        public byte ReadByte()
        {
            return _data[Position++];
        }

        public string ReadChars(int length)
        {
            _stringBuilder.Clear();
            for (var i = 0; i < length; i++)
                _stringBuilder.Append((char)ReadByte());
            return _stringBuilder.ToString();
        }

        public uint ReadBEUInt32()
        {
            uint b1 = ReadByte();
            uint b2 = ReadByte();
            uint b3 = ReadByte();
            uint b4 = ReadByte();
            return b4 + (b3 << 8) + (b2 << 16) + (b1 << 24);
        }
        
        public uint ReadBEUInt16()
        {
            uint b1 = ReadByte();
            uint b2 = ReadByte();
            return b2 + (b1 << 8);
        }

        public uint ReadMultiByteValue()
        {
            var v = 0u;
            while (true)
            {
                uint b = ReadByte();
                v += b & 0x7fu;
                if (b < 0x80u) break;
                v <<= 7;
            }
            return v;
        }

        #endregion
    }
}
