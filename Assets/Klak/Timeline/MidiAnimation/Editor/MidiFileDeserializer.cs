using System;
using System.Collections.Generic;
using UnityEngine;

namespace Klak.Timeline.Midi
{
    // SMF file deserializer implementation
    static class MidiFileDeserializer
    {
        #region Public members

        public static MidiFileAsset Load(byte [] data)
        {
            var reader = new MidiDataStreamReader(data);

            // Chunk type
            if (reader.ReadChars(4) != "MThd")
                throw new FormatException("Can't find header chunk.");
            
            // Chunk length
            if (reader.ReadBEUInt32() != 6u)
                throw new FormatException("Length of header chunk must be 6.");
            
            // Format (unused)
            reader.Advance(2);
            
            // Number of tracks
            var trackCount = reader.ReadBEUInt16();

            // Ticks per quarter note
            var tpqn = reader.ReadBEUInt16();
            if ((tpqn & 0x8000u) != 0)
                throw new FormatException ("SMPTE time code is not supported.");

            // Tracks
            var tracks = new MidiAnimationAsset [trackCount];
            for (var i = 0; i < trackCount; i++)
                tracks[i] = ReadTrack(reader, tpqn);

            // Asset instantiation
            var asset = ScriptableObject.CreateInstance<MidiFileAsset>();
            asset.tracks = tracks;
            return asset;
        }

        #endregion

        #region Private members
        
        static MidiAnimationAsset ReadTrack(MidiDataStreamReader reader, uint tpqn)
        {
            // Chunk type
            if (reader.ReadChars(4) != "MTrk")
                throw new FormatException ("Can't find track chunk.");
            
            // Chunk length
            var chunkEnd = reader.ReadBEUInt32();
            chunkEnd += reader.Position;

            // MIDI event sequence
            var events = new List<MidiEvent>();
            var ticks = 0u;
            var stat = (byte)0;

            while (reader.Position < chunkEnd)
            {
                // Delta time
                ticks += reader.ReadMultiByteValue();

                // Status byte
                if ((reader.PeekByte() & 0x80u) != 0)
                    stat = reader.ReadByte();
                
                if (stat == 0xffu)
                {
                    // 0xff: Meta event (unused)
                    reader.Advance(1);
                    reader.Advance(reader.ReadMultiByteValue());
                }
                else if (stat == 0xf0u)
                {
                    // 0xf0: SysEx (unused)
                    while (reader.ReadByte() != 0xf7u) {}
                }
                else
                {
                    // MIDI event
                    var b1 = reader.ReadByte();
                    var b2 = (stat & 0xe0u) == 0xc0u ? (byte)0 : reader.ReadByte();
                    events.Add(new MidiEvent {
                        time = ticks, status = stat, data1 = b1, data2 = b2
                    });
                }
            }

            // Quantize duration with bars.
            var bars = (ticks + tpqn * 4 - 1) / (tpqn * 4);

            // Asset instantiation
            var asset = ScriptableObject.CreateInstance<MidiAnimationAsset>();
            asset.template.tempo = 120;
            asset.template.duration = bars * tpqn * 4;
            asset.template.ticksPerQuarterNote = tpqn;
            asset.template.events = events.ToArray();
            return asset;
        }

        #endregion
    }
}
