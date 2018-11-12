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

        (int iOn, int iOff) GetNoteEventsBeforeTick(uint tick, int note, int octave)
        {
            var iOn = -1;
            var iOff = -1;
            for (var i = 0; i < events.Length; i++)
            {
                ref var e = ref events[i];
                if (e.time > tick) break;
                if ((e.status & 0xe0u) != 0x80u) continue;
                if (!IsNote(ref e, note, octave)) continue;
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

        float CalculateEnvelope(Vector4 envelope, float onTime, float offTime)
        {
            var attackRate = Mathf.Exp(envelope.x);
            var attackTime = 1 / attackRate;

            var decayRate = Mathf.Exp(envelope.y);
            var decayTime = 1 / decayRate;

            var level = -Mathf.Exp(envelope.w) * offTime;

            if (onTime < attackTime)
            {
                level += onTime * attackRate;
            }
            else if (onTime < attackTime + decayTime)
            {
                level += 1 - (onTime - attackTime) * decayRate * (1 - envelope.z);
            }
            else
            {
                level += envelope.z;
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
            var pair = GetNoteEventsBeforeTick(tick, control.noteNumber, control.octave);

            if (pair.iOn < 0) return 0;
            ref var eOn = ref events[pair.iOn]; // Note-on event

            // Note-on time
            var onTime = ConvertTicksToSecond(eOn.time);

            // Note-off time
            var offTime =
                pair.iOff < 0 || pair.iOff < pair.iOn ? time :
                    ConvertTicksToSecond(events[pair.iOff].time);

            var velocity = eOn.data2 / 127.0f;

            return CalculateEnvelope(control.envelope, Mathf.Max(0, offTime - onTime), Mathf.Max(0, time - offTime)) * velocity;
        }

        #endregion
    }
}
