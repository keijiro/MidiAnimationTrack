using UnityEngine;
using UnityEngine.Playables;

namespace Klak.Timeline.Midi
{
    // Runtime playable class that manages a MIDI animation track (clips and
    // its assigned controls)
    [System.Serializable]
    public sealed class MidiAnimationMixer : PlayableBehaviour
    {
        #region Serialized variables

        public MidiControl [] controls = new MidiControl [0];

        #endregion

        #region PlayableBehaviour overrides

        ControlAction[] _actions;

        public override void OnPlayableCreate(Playable playable)
        {
            var resolver = playable.GetGraph().GetResolver();

            // Populate actions for the each controller.
            _actions = new ControlAction[controls.Length];

            for (var i = 0; i < controls.Length; i++)
                _actions[i] = ControlAction.CreateAction(
                    controls[i].targetComponent.Resolve(resolver),
                    controls[i].propertyName
                );
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            for (var ci = 0; ci < controls.Length; ci++)
            {
                var ctrl = controls[ci];
                if (!ctrl.enabled) continue;

                // Controller value accumulation
                var acc = 0.0f;

                for (var i = 0; i < playable.GetInputCount(); i++)
                {
                    var clip = (ScriptPlayable<MidiAnimation>)playable.GetInput(i);
                    acc += playable.GetInputWeight(i) *
                        clip.GetBehaviour().GetValue(clip, ctrl);
                }

                // Controller action invocation
                _actions[ci]?.Invoke(Vector4.Lerp(ctrl.vector0, ctrl.vector1, acc));
            }
        }

        #endregion
    }
}
