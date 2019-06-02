using UnityEngine;
using UnityEngine.Events;
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

        #region UnityEvent array

        class FloatEvent : UnityEvent<float> {}
        class Vector3Event : UnityEvent<Vector3> {}
        class QuaternionEvent : UnityEvent<Quaternion> {}
        class ColorEvent : UnityEvent<Color> {}

        UnityEventBase[] _events;

        #endregion

        #region PlayableBehaviour overrides

        static UnityAction<T> GetPropertyGetter<T>(Component component, string propertyName)
        {
            return (UnityAction<T>)System.Delegate.CreateDelegate(
                typeof(UnityAction<T>), component, "set_" + propertyName);
        }

        public override void OnPlayableCreate(Playable playable)
        {
            _events = new UnityEventBase[controls.Length];

            var resolver = playable.GetGraph().GetResolver();

            // Populate UnityEvents for the each controller.
            for (var i = 0; i < controls.Length; i++)
            {
                var ctrl = controls[i];

                // Target component, property name and type.
                var target = ctrl.targetComponent.Resolve(resolver);
                var name = ctrl.propertyName;
                var type = target?.GetType().GetProperty(name)?.PropertyType;

                if (type == typeof(float))
                {
                    var e = new FloatEvent();
                    e.AddListener(GetPropertyGetter<float>(target, name));
                    _events[i] = e;
                }
                else if (type == typeof(Vector3))
                {
                    var e = new Vector3Event();
                    e.AddListener(GetPropertyGetter<Vector3>(target, name));
                    _events[i] = e;
                }
                else if (type == typeof(Quaternion))
                {
                    var e = new QuaternionEvent();
                    e.AddListener(GetPropertyGetter<Quaternion>(target, name));
                    _events[i] = e;
                }
                else if (type == typeof(Color))
                {
                    var e = new ColorEvent();
                    e.AddListener(GetPropertyGetter<Color>(target, name));
                    _events[i] = e;
                }
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            for (var ci = 0; ci < controls.Length; ci++)
            {
                var ctrl = controls[ci];
                var ev = _events[ci];

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

                // Controller event invocation
                if (ev is FloatEvent)
                    ((FloatEvent)ev).Invoke(vec.x);
                else if (ev is Vector3Event)
                    ((Vector3Event)ev).Invoke((Vector3)vec);
                else if (ev is QuaternionEvent)
                    ((QuaternionEvent)ev).Invoke(Quaternion.Euler(vec));
                else if (ev is ColorEvent)
                    ((ColorEvent)ev).Invoke((Color)vec);
            }
        }

        #endregion
    }
}
