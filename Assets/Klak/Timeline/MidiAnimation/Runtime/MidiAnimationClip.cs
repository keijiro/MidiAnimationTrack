using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Klak.Timeline
{
    [System.Serializable]
    public class MidiAnimationClip : PlayableAsset, ITimelineClipAsset
    {
        #region Serialized variables

        public MidiAnimationPlayable template = new MidiAnimationPlayable();

        #endregion

        #region ITimelineClipAsset implementation

        public ClipCaps clipCaps { get { return ClipCaps.Blending | ClipCaps.Extrapolation; } }

        #endregion

        #region PlayableAsset overrides

        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            return ScriptPlayable<MidiAnimationPlayable>.Create(graph, template);
        }

        #endregion
    }
}
