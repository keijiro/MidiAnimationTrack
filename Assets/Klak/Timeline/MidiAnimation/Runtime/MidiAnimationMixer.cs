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
            var target = playerData as GameObject;
            if (target == null) return;

            var resolver = playable.GetGraph().GetResolver();

            foreach (var ctrl in controls)
            {
                var acc = 0.0f;

                for (var i = 0; i < playable.GetInputCount(); i++)
                {
                    var clip = (ScriptPlayable<MidiAnimation>)playable.GetInput(i);
                    var value = clip.GetBehaviour().GetCCValue(clip, ctrl.controlNumber);
                    acc += playable.GetInputWeight(i) * value;
                }

                var component = ctrl.targetComponent.Resolve(resolver);
                if (component == null) continue;

                var prop = component.GetType().GetProperty(ctrl.propertyName);
                if (prop == null) continue;

                var v = Vector4.Lerp(ctrl.vector0, ctrl.vector1, acc);

                if (prop.PropertyType == typeof(float))
                    prop.SetValue(component, v.x, null);
                else if (prop.PropertyType == typeof(Vector3))
                    prop.SetValue(component, (Vector3)v, null);
                else if (prop.PropertyType == typeof(Quaternion))
                    prop.SetValue(component, Quaternion.Euler(v), null);
                else if (prop.PropertyType == typeof(Color))
                    prop.SetValue(component, (Color)v, null);
            }
        }

        #endregion
    }
}
