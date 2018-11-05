using UnityEngine;
using UnityEngine.Playables;

namespace Klak.Timeline
{
    [System.Serializable]
    public class MidiAnimationMixer : PlayableBehaviour
    {
        #region Serialized variables

        public float bpm = 120;

        #endregion

        #region PlayableBehaviour overrides

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
        }

        #endregion
    }
}
