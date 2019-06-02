using UnityEngine;
using UnityEngine.Playables;

namespace Klak.Timeline
{
    [System.Serializable]
    public class MidiAnimation : PlayableBehaviour
    {
        #region Serialized variables

        public float tempo = 120;
        public uint duration;
        public uint ticksPerQuarterNote = 96;
        public MidiEvent [] events;

        #endregion

        #region Public properties and methods

        public float DurationInSecond {
            get { return duration / tempo * 60 / ticksPerQuarterNote; }
        }

        public float GetValue(Playable playable, MidiControl control, MidiControlMode mode)
        {
            if (events == null) return 0;
            var t = (float)playable.GetTime() % DurationInSecond;
            if (mode == MidiControlMode.ControlChange)
                return GetCCValue(control, t);
            else // MonoNote
                return GetNoteValue(control, t);
        }

        #endregion

        #region MIDI message operators

        static bool IsCC(ref MidiEvent e, int ccNumber)
        {
            return ((e.status & 0xb0) == 0xb0) && e.data1 == ccNumber;
        }

        static bool IsNote(ref MidiEvent e, MidiNoteFilter note)
        {
            if ((e.status & 0xe0) != 0x80) return false;

            var num = e.data1;

            // Octave test
            if (note.octave != MidiOctave.All && num / 12 != (int)note.octave - 1) return false;

            // Note (interval) test
            if (note.note != MidiNote.All && num % 12 != (int)note.note - 1) return false;

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

        (int iOn, int iOff) GetNoteEventsBeforeTick(uint tick, MidiNoteFilter note)
        {
            var iOn = -1;
            var iOff = -1;
            for (var i = 0; i < events.Length; i++)
            {
                ref var e = ref events[i];
                if (e.time > tick) break;
                if ((e.status & 0xe0u) != 0x80u) continue;
                if (!IsNote(ref e, note)) continue;
                if (IsNoteOn(ref e)) iOn = i; else iOff = i;
            }
            return (iOn, iOff);
        }

        float ConvertTicksToSecond(uint tick)
        {
            return tick * 60 / (tempo * ticksPerQuarterNote);
        }

        #endregion

        #region Envelope generator

        float CalculateEnvelope(MidiEnvelope envelope, float onTime, float offTime)
        {
            var attackTime = envelope.AttackTime;
            var attackRate = 1 / attackTime;

            var decayTime = envelope.DecayTime;
            var decayRate = 1 / decayTime;

            var level = -offTime / envelope.ReleaseTime;

            if (onTime < attackTime)
            {
                level += onTime * attackRate;
            }
            else if (onTime < attackTime + decayTime)
            {
                level += 1 - (onTime - attackTime) * decayRate * (1 - envelope.SustainLevel);
            }
            else
            {
                level += envelope.SustainLevel;
            }

            return Mathf.Max(0, level);
        }

        #endregion

        #region Value calculation methods

        float GetCCValue(MidiControl control, float time)
        {
            var tick = (uint)(tempo * time / 60 * ticksPerQuarterNote);
            var pair = GetCCEventIndexAroundTick(tick, control.controlNumber);

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

        float GetNoteValue(MidiControl control, float time)
        {
            var tick = (uint)(tempo * time / 60 * ticksPerQuarterNote);
            var pair = GetNoteEventsBeforeTick(tick, control.noteFilter);

            if (pair.iOn < 0) return 0;
            ref var eOn = ref events[pair.iOn]; // Note-on event

            // Note-on time
            var onTime = ConvertTicksToSecond(eOn.time);

            // Note-off time
            var offTime = pair.iOff < 0 || pair.iOff < pair.iOn ?
                time : ConvertTicksToSecond(events[pair.iOff].time);

            var envelope = CalculateEnvelope(
                control.envelope,
                Mathf.Max(0, offTime - onTime),
                Mathf.Max(0, time - offTime)
            );

            var velocity = eOn.data2 / 127.0f;

            return envelope * velocity;
        }

        #endregion
    }
}
