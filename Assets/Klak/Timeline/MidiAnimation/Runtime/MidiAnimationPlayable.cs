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

        #region Internal state variables

        float _bpm;

        #endregion

        #region PlayableBehaviour overrides

        public override void OnGraphStart(Playable playable)
        {
            var mixer = (ScriptPlayable<MidiAnimationMixer>)playable.GetOutput(0);
            _bpm = mixer.GetBehaviour().bpm;
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            var tick = _bpm * playable.GetTime() / 60 * ticksPerQuarterNote;
            foreach (var e in events)
            {
                if (e.time > tick)
                {
                    Debug.Log(e);
                    break;
                }
            }
        }

        #endregion
    }
}
