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

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<MidiAnimationMixer>.Create(graph, template, inputCount);
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

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            var go = director.GetGenericBinding(this) as GameObject;
            if (go == null) return;

            if (template.controls == null) return;

            foreach (var ctrl in template.controls)
            {
                if (string.IsNullOrEmpty(ctrl.componentName)) continue;
                if (string.IsNullOrEmpty(ctrl.fieldName)) continue;
                    
                var component = go.GetComponent(ctrl.componentName);
                if (component == null) continue;

                driver.AddFromName(component.GetType(), go, ctrl.fieldName);
            }
        }

        #endregion
    }
}
