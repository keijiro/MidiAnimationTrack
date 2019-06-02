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

        #region UnityAction array

        struct ControlAction
        {
            public UnityAction<float> floatAction;
            public UnityAction<Vector3> vector3Action;
            public UnityAction<Quaternion> quaternionAction;
            public UnityAction<Color> colorAction;
        }

        ControlAction[] _actions;

        #endregion

        #region PlayableBehaviour overrides

        static UnityAction<T> GetPropertyGetter<T>(Component component, string propertyName)
        {
            return (UnityAction<T>)System.Delegate.CreateDelegate(
                typeof(UnityAction<T>), component, "set_" + propertyName);
        }

        public override void OnPlayableCreate(Playable playable)
        {
            _actions = new ControlAction[controls.Length];

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
                    _actions[i].floatAction = GetPropertyGetter<float>(target, name);
                else if (type == typeof(Vector3))
                    _actions[i].vector3Action = GetPropertyGetter<Vector3>(target, name);
                else if (type == typeof(Quaternion))
                    _actions[i].quaternionAction = GetPropertyGetter<Quaternion>(target, name);
                else if (type == typeof(Color))
                    _actions[i].colorAction = GetPropertyGetter<Color>(target, name);
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            for (var ci = 0; ci < controls.Length; ci++)
            {
                var ctrl = controls[ci];
                var act = _actions[ci];

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
                if (act.floatAction != null)
                    act.floatAction(vec.x);
                else if (act.vector3Action != null)
                    act.vector3Action((Vector3)vec);
                else if (act.quaternionAction != null)
                    act.quaternionAction(Quaternion.Euler(vec));
                else if (act.colorAction != null)
                    act.colorAction((Color)vec);
            }
        }

        #endregion
    }
}
