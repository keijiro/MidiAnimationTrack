using UnityEngine;
using UnityEngine.Playables;

namespace Klak.Timeline
{
    [System.Serializable]
    public class MidiAnimationPlayable : PlayableBehaviour
    {
        #region Serialized variables

        public uint ticksPerQuarterNote;
        public MidiEvent [] events;

        #endregion

        #region Public properties and methods

        public float CurrentValue { get; private set; }

        public float CalculateLastEventTime(float bpm)
        {
            return ConvertTicksToSecond(events[events.Length - 1].time, bpm);
        }

        #endregion

        #region Private variables and methods

        float _bpm;

        int GetLastEventIndexBeforeTick(uint tick)
        {
            for (var i = 0; i < events.Length - 1; i++)
                if (events[i].time > tick) return Mathf.Max(0, i - 1);
            return events.Length - 2;
        }

        float ConvertTicksToSecond(uint tick)
        {
            return ConvertTicksToSecond(tick, _bpm);
        }

        float ConvertTicksToSecond(uint tick, float bpm)
        {
            return tick * 60 / (bpm * ticksPerQuarterNote);
        }

        #endregion

        #region PlayableBehaviour overrides

        public override void OnGraphStart(Playable playable)
        {
            var mixer = (ScriptPlayable<MidiAnimationMixer>)playable.GetOutput(0);
            _bpm = mixer.GetBehaviour().bpm;
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            var t = (float)playable.GetTime() % CalculateLastEventTime(_bpm);

            var tick = (uint)(_bpm * t / 60 * ticksPerQuarterNote);
            var i = GetLastEventIndexBeforeTick(tick);

            ref var e0 = ref events[i];
            ref var e1 = ref events[i + 1];

            var t0 = ConvertTicksToSecond(e0.time);
            var t1 = ConvertTicksToSecond(e1.time);

            var v0 = e0.data2 / 127.0f;
            var v1 = e1.data2 / 127.0f;

            CurrentValue = Mathf.Lerp(v0, v1, Mathf.Clamp01((t - t0) / (t1 - t0)));
        }

        #endregion
    }
}
