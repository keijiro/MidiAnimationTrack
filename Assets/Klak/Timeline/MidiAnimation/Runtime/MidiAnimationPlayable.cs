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

        #region PlayableBehaviour overrides

        public override void OnPlayableCreate(Playable playable)
        {
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
        }

        #endregion
    }
}
