using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Klak.Timeline
{
    [TrackColor(0.4f, 0.4f, 0.4f)]
    [TrackClipType(typeof(MidiAnimationAsset))]
    [TrackBindingType(typeof(GameObject))]
    public class MidiAnimationTrack : TrackAsset
    {
        #region Serialized object

        public MidiAnimationMixer template = new MidiAnimationMixer();

        #endregion

        #region TrackAsset implementation

        public override Playable
            CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<MidiAnimationMixer>.
                Create(graph, template, inputCount);
        }

        protected override void OnCreateClip(TimelineClip clip)
        {
            ((MidiAnimationAsset)clip.asset).mixer = template;
        }

        #endregion

        #region ISerializationCallbackReceiver

        protected override void OnAfterTrackDeserialize()
        {
            foreach (var clip in GetClips())
            {
                var asset = clip.asset as MidiAnimationAsset;
                if (asset != null) asset.mixer = template;
            }
        }

        #endregion

        #region IPropertyPreview implementation

        public override void
            GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            if (string.IsNullOrEmpty(template.componentName)) return;
            if (string.IsNullOrEmpty(template.fieldName)) return;
                
            var go = director.GetGenericBinding(this) as GameObject;
            if (go == null) return;

            var component = go.GetComponent(template.componentName);
            if (component == null) return;

            driver.AddFromName(component.GetType(), go, template.fieldName);
        }

        #endregion
    }
}
