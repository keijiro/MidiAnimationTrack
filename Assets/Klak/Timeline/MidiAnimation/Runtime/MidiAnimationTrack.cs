using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Klak.Timeline.Midi
{
    // Track asset class that contains a MIDI animation track (clips and its
    // assigned controls)
    [TrackColor(0.4f, 0.4f, 0.4f)]
    [TrackClipType(typeof(MidiAnimationAsset))]
    [TrackBindingType(typeof(GameObject))]
    public sealed class MidiAnimationTrack : TrackAsset
    {
        #region Serialized object

        public MidiAnimationMixer template = new MidiAnimationMixer();

        #endregion

        #region TrackAsset implementation

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<MidiAnimationMixer>.Create(graph, template, inputCount);
        }

        #endregion

        #region IPropertyPreview implementation

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            if (template.controls == null) return;

            foreach (var ctrl in template.controls)
            {
                if (!ctrl.enabled) continue;

                var component = ctrl.targetComponent.Resolve(director);
                if (component == null) continue;

                if (string.IsNullOrEmpty(ctrl.fieldName)) continue;

                // This extension method is implemented in IPropertyCollectorExtension.cs
                driver.AddFromName(component.GetType(), component.gameObject, ctrl.fieldName);
            }
        }

        #endregion
    }
}
