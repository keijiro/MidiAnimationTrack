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

        public Vector3 baseVector = Vector3.forward;
        public Vector3 rotationAxis = Vector3.forward;
        public Color colorAt0 = Color.red;
        public Color colorAt1 = Color.blue;
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

                if (prop.PropertyType == typeof(float))
                    prop.SetValue(component, acc, null);
                else if (prop.PropertyType == typeof(Vector3))
                    prop.SetValue(component, ctrl.baseVector * acc, null);
                else if (prop.PropertyType == typeof(Quaternion))
                    prop.SetValue(component, Quaternion.AngleAxis(acc, ctrl.rotationAxis), null);
                else if (prop.PropertyType == typeof(Color))
                    prop.SetValue(component, Color.Lerp(ctrl.colorAt0, ctrl.colorAt1, acc), null);
            }
        }

        #endregion
    }
}
