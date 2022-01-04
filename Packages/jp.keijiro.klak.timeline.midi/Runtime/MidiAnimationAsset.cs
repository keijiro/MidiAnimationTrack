using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Klak.Timeline.Midi
{
    // Playable asset class that contains a MIDI animation clip
    [System.Serializable]
    public sealed class MidiAnimationAsset : PlayableAsset, ITimelineClipAsset
    {
        #region Serialized variables

        public MidiAnimation template = new MidiAnimation();

        #endregion

        #region PlayableAsset implementation

        public override double duration {
            get { return template.DurationInSecond; }
        }

        #endregion

        #region ITimelineClipAsset implementation

        public ClipCaps clipCaps { get {
            return ClipCaps.Blending |
                   ClipCaps.ClipIn |
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
