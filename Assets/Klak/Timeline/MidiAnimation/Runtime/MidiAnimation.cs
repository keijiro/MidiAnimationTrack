using UnityEngine;
using UnityEngine.Playables;

namespace Klak.Timeline
{
    [System.Serializable]
    public class MidiAnimation : PlayableBehaviour
    {
        #region Serialized variables

        public float tempo;
        public uint duration;
        public uint ticksPerQuarterNote;
        public MidiEvent [] events;

        #endregion

        #region Public properties and methods

        public float DurationInSecond {
            get { return duration / tempo * 60 / ticksPerQuarterNote; }
        }

        public float GetValue(Playable playable, MidiControl control, MidiControlMode mode)
        {
            var t = (float)playable.GetTime() % DurationInSecond;
            if (mode == MidiControlMode.ControlChange)
                return GetCCValue(t, control.controlNumber);
            else // MonoNote
                return GetNoteValue(t, control.noteNumber, control.octave);
        }

        #endregion

        #region MIDI message operators

        static bool IsCC(ref MidiEvent e, int ccNumber)
        {
            return ((e.status & 0xb0) == 0xb0) && e.data1 == ccNumber;
        }

        static bool IsNote(ref MidiEvent e, int note, int octave)
        {
            if ((e.status & 0xe0) != 0x80) return false;

            var num = e.data1;

            // Octave test
            if (octave >= 0 && num / 12 != octave) return false;

            // Note (interval) test
            if (note >= 0 && num % 12 != note) return false;

            return true;
        }

        static bool IsNoteOn(ref MidiEvent e)
        {
            return (e.status & 0xf0) == 0x90;
        }

        #endregion

        #region Private variables and methods

        (int i0, int i1) GetCCEventIndexAroundTick(uint tick, int controlNumber)
        {
            var last = -1;
            for (var i = 0; i < events.Length; i++)
            {
                ref var e = ref events[i];
                if (!IsCC(ref e, controlNumber)) continue;
                if (e.time > tick) return (last, i);
                last = i;
            }
            return (last, last);
        }

        int GetNoteEventBeforeTick(uint tick, int note, int octave)
        {
            var last = -1;
            for (var i = 0; i < events.Length; i++)
            {
                ref var e = ref events[i];
                if (e.time > tick) break;
                if ((e.status & 0xe0u) != 0x80u) continue;
                if (IsNote(ref e, note, octave)) last = i;
            }
            return last;
        }

        float ConvertTicksToSecond(uint tick)
        {
            return tick * 60 / (tempo * ticksPerQuarterNote);
        }

        #endregion

        #region Value calculation methods

        public float GetCCValue(float time, int controlNumber)
        {
            var tick = (uint)(tempo * time / 60 * ticksPerQuarterNote);
            var pair = GetCCEventIndexAroundTick(tick, controlNumber);

            if (pair.i0 < 0) return 0;
            if (pair.i1 < 0) return events[pair.i0].data2 / 127.0f;

            ref var e0 = ref events[pair.i0];
            ref var e1 = ref events[pair.i1];

            var t0 = ConvertTicksToSecond(e0.time);
            var t1 = ConvertTicksToSecond(e1.time);

            var v0 = e0.data2 / 127.0f;
            var v1 = e1.data2 / 127.0f;

            return Mathf.Lerp(v0, v1, Mathf.Clamp01((time - t0) / (t1 - t0)));
        }

        public float GetNoteValue(float time, int note, int octave)
        {
            var tick = (uint)(tempo * time / 60 * ticksPerQuarterNote);
            var i = GetNoteEventBeforeTick(tick, note, octave);
            if (i < 0) return 0;
            ref var e = ref events[i];
            return IsNoteOn(ref e) ? e.data2 / 127.0f : 0;
        }

        #endregion
    }
}
