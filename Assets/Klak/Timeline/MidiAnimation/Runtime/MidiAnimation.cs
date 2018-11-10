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

        #region Public properties

        public float DurationInSecond {
            get { return duration / tempo * 60 / ticksPerQuarterNote; }
        }

        #endregion

        #region Private variables and methods

        (int i0, int i1) GetEventIndexAroundTick(uint tick, int controlNumber)
        {
            var last = -1;
            for (var i = 0; i < events.Length; i++)
            {
                ref var e = ref events[i];
                if (e.data1 != controlNumber) continue;
                if (e.time > tick) return (last, i);
                last = i;
            }
            return (last, last);
        }

        float ConvertTicksToSecond(uint tick)
        {
            return tick * 60 / (tempo * ticksPerQuarterNote);
        }

        #endregion

        #region PlayableBehaviour overrides

        public float GetCCValue(Playable playable, int controlNumber)
        {
            var t = (float)playable.GetTime() % DurationInSecond;

            var tick = (uint)(tempo * t / 60 * ticksPerQuarterNote);
            var pair = GetEventIndexAroundTick(tick, controlNumber);

            if (pair.i0 < 0) return 0;
            if (pair.i1 < 0) return events[pair.i0].data2 / 127.0f;

            ref var e0 = ref events[pair.i0];
            ref var e1 = ref events[pair.i1];

            var t0 = ConvertTicksToSecond(e0.time);
            var t1 = ConvertTicksToSecond(e1.time);

            var v0 = e0.data2 / 127.0f;
            var v1 = e1.data2 / 127.0f;

            return Mathf.Lerp(v0, v1, Mathf.Clamp01((t - t0) / (t1 - t0)));
        }

        #endregion
    }
}
