using UnityEngine;
using UnityEngine.Playables;
using System.Reflection;

namespace Klak.Timeline
{
    [System.Serializable]
    public class MidiControl
    {
        public int controlNumber = 1;

        public string componentName;
        public string propertyName;
        public string fieldName;

        public Vector4 vector0 = Vector3.zero;
        public Vector4 vector1 = Vector3.forward;
    }

    [System.Serializable]
    public class MidiAnimationMixer : PlayableBehaviour
    {
        #region Serialized variables

        public float bpm = 120;
        public MidiControl [] controls;

        #endregion

        #region PlayableBehaviour overrides

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var target = playerData as GameObject;
            if (target == null) return;

            foreach (var ctrl in controls)
            {
                var acc = 0.0f;

                for (var i = 0; i < playable.GetInputCount(); i++)
                {
                    var clip = (ScriptPlayable<MidiAnimation>)playable.GetInput(i);
                    var value = clip.GetBehaviour().GetCCValue(clip, ctrl.controlNumber);
                    acc += playable.GetInputWeight(i) * value;
                }

                var component = target.GetComponent(ctrl.componentName);
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
