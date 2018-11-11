using UnityEngine;
using UnityEngine.Playables;
using System.Reflection;

namespace Klak.Timeline
{
    [System.Serializable]
    public class MidiAnimationMixer : PlayableBehaviour
    {
        #region Serialized variables

        public MidiControlMode controlMode = MidiControlMode.ControlChange;
        public MidiControl [] controls = new MidiControl [0];

        #endregion

        #region PlayableBehaviour overrides

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var resolver = playable.GetGraph().GetResolver();

            foreach (var ctrl in controls)
            {
                // Target component
                var component = ctrl.targetComponent.Resolve(resolver);
                if (component == null) continue;

                // Target property
                var prop = component.GetType().GetProperty(ctrl.propertyName);
                if (prop == null) continue;

                // Controller value accumulation
                var acc = 0.0f;

                for (var i = 0; i < playable.GetInputCount(); i++)
                {
                    var clipPlayable = (ScriptPlayable<MidiAnimation>)playable.GetInput(i);
                    var midiAnim = clipPlayable.GetBehaviour();
                    var clipValue = midiAnim.GetValue(clipPlayable, ctrl, controlMode);
                    acc += playable.GetInputWeight(i) * clipValue;
                }

                var vec = Vector4.Lerp(ctrl.vector0, ctrl.vector1, acc);

                // Update the target property.
                if (prop.PropertyType == typeof(float))
                    prop.SetValue(component, vec.x, null);
                else if (prop.PropertyType == typeof(Vector3))
                    prop.SetValue(component, (Vector3)vec, null);
                else if (prop.PropertyType == typeof(Quaternion))
                    prop.SetValue(component, Quaternion.Euler(vec), null);
                else if (prop.PropertyType == typeof(Color))
                    prop.SetValue(component, (Color)vec, null);
            }
        }

        #endregion
    }
}
