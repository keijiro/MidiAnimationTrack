using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Klak.Timeline
{
    [System.Serializable]
    public class MidiAnimationAsset : PlayableAsset, ITimelineClipAsset
    {
        #region Serialized variables

        public MidiAnimation template = new MidiAnimation();

        #endregion

        #region Assembly internal-use variables

        internal MidiAnimationMixer mixer { get; set; }

        #endregion

        #region PlayableAsset implementation

        public override double duration {
            get { return template.CalculateLastEventTime(mixer.bpm); }
        }

        #endregion

        #region ITimelineClipAsset implementation

        public ClipCaps clipCaps { get {
            return ClipCaps.Blending |
                   ClipCaps.Extrapolation |
                   ClipCaps.Looping |
                   ClipCaps.SpeedMultiplier;
        } }

        #endregion

        #region PlayableAsset overrides

        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            return ScriptPlayable<MidiAnimation>.Create(graph, template);
        }

        #endregion
    }
}
