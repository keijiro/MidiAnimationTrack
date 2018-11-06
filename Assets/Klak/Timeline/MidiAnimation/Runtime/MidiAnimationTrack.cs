using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Klak.Timeline
{
    [TrackColor(0.4f, 0.4f, 0.4f)]
    [TrackClipType(typeof(MidiAnimationClip))]
    [TrackBindingType(typeof(GameObject))]
    public class MidiAnimationTrack : TrackAsset
    {
        public MidiAnimationMixer template = new MidiAnimationMixer();

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<MidiAnimationMixer>.Create(graph, template, inputCount);
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            if (string.IsNullOrEmpty(template.componentName)) return;
            if (string.IsNullOrEmpty(template.fieldName)) return;
                
            var go = director.GetGenericBinding(this) as GameObject;
            if (go == null) return;

            var component = go.GetComponent(template.componentName);
            if (component == null) return;

            driver.AddFromName(component.GetType(), go, template.fieldName);
        }
    }
}
