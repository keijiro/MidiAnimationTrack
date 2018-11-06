using UnityEngine;
using UnityEngine.Playables;
using System.Reflection;

namespace Klak.Timeline
{
    [System.Serializable]
    public class MidiAnimationMixer : PlayableBehaviour
    {
        #region Serialized variables

        public float bpm = 120;

        public string componentName;
        public string propertyName;
        public string fieldName;

        public Vector3 baseVector = Vector3.forward;
        public Vector3 rotationAxis = Vector3.forward;
        public Color colorAt0 = Color.red;
        public Color colorAt1 = Color.blue;

        #endregion

        #region Private variables

        PropertyInfo _targetProperty;

        #endregion

        #region PlayableBehaviour overrides

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var acc = 0.0f;

            for (var i = 0; i < playable.GetInputCount(); i++)
            {
                var p = playable.GetInput(i);
                var b = (ScriptPlayable<MidiAnimationPlayable>)p;
                acc += playable.GetInputWeight(i) * b.GetBehaviour().CurrentValue;
            }

            var target = playerData as GameObject;
            if (target == null) return;

            var component = target.GetComponent(componentName);
            if (component == null) return;

            if (_targetProperty == null)
                _targetProperty = component.GetType().GetProperty(propertyName);

            if (_targetProperty != null)
            {
                if (_targetProperty.PropertyType == typeof(float))
                    _targetProperty.SetValue(component, acc, null);
                else if (_targetProperty.PropertyType == typeof(Vector3))
                    _targetProperty.SetValue(component, baseVector * acc, null);
                else if (_targetProperty.PropertyType == typeof(Quaternion))
                    _targetProperty.SetValue
                        (component, Quaternion.AngleAxis(acc, rotationAxis), null);
                else if (_targetProperty.PropertyType == typeof(Color))
                    _targetProperty.SetValue
                        (component, Color.Lerp(colorAt0, colorAt1, acc), null);
            }
        }

        #endregion
    }
}
