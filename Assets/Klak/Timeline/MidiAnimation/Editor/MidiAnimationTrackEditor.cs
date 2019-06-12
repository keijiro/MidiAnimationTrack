using UnityEngine;
using UnityEditor;
using UnityEditor.Timeline;

namespace Klak.Timeline
{
    [CustomEditor(typeof(MidiAnimationTrack))]
    class MidiAnimationTrackEditor : Editor
    {
        #region Editor implementation

        SerializedProperty _controls;

        void OnEnable()
        {
            _controls = serializedObject.FindProperty("template.controls");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            // Draw all the controls using the "header with a toggle" style.
            for (var i = 0; i < _controls.arraySize; i++)
            {
                CoreEditorUtils.DrawSplitter();

                var title = "Control Element " + (i + 1);
                var control = _controls.GetArrayElementAtIndex(i);
                var enabled = control.FindPropertyRelative("enabled");

                var toggle = CoreEditorUtils.DrawHeaderToggle
                    (title, control, enabled, pos => OnContextClick(pos, i));

                if (!toggle) continue;

                using (new EditorGUI.DisabledScope(!enabled.boolValue))
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(control);
                }
            }

            // We have to manually refresh the timeline.
            if (EditorGUI.EndChangeCheck())
                TimelineEditor.Refresh(RefreshReason.ContentsModified);

            if (_controls.arraySize > 0) CoreEditorUtils.DrawSplitter();

            EditorGUILayout.Space();

            // "Add" button
            if (GUILayout.Button("Add Control Element")) AppendDefaultMidiControl();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private property and method

        AnimationCurve defaultCurve {
            get {
                return new AnimationCurve(
                    new Keyframe(0, 0, 90, 90),
                    new Keyframe(0.02f, 1),
                    new Keyframe(0.5f, 0)
                );
            }
        }

        void AppendDefaultMidiControl()
        {
            var index = _controls.arraySize;
            _controls.InsertArrayElementAtIndex(index);

            var prop = _controls.GetArrayElementAtIndex(index);
            prop.isExpanded = true;

            ResetControl(prop);
        }

        void ResetControl(SerializedProperty prop)
        {
            prop.FindPropertyRelative("enabled").boolValue = true;
            prop.FindPropertyRelative("mode").enumValueIndex = 0;
            prop.FindPropertyRelative("noteFilter.note").enumValueIndex = 0;
            prop.FindPropertyRelative("noteFilter.octave").enumValueIndex = 0;
            prop.FindPropertyRelative("envelope.attack").floatValue = 0;
            prop.FindPropertyRelative("envelope.decay").floatValue = 1;
            prop.FindPropertyRelative("envelope.sustain").floatValue = 0.5f;
            prop.FindPropertyRelative("envelope.release").floatValue = 1;
            prop.FindPropertyRelative("curve").animationCurveValue = defaultCurve;
            prop.FindPropertyRelative("ccNumber").intValue = 1;
            prop.FindPropertyRelative("targetComponent.exposedName").stringValue = "";
            prop.FindPropertyRelative("propertyName").stringValue = "";
            prop.FindPropertyRelative("fieldName").stringValue = "";
            prop.FindPropertyRelative("vector0").vector4Value = Vector3.zero;
            prop.FindPropertyRelative("vector1").vector4Value = Vector3.forward;
        }

        #endregion

        #region Context menu

        static class Labels
        {
            public static readonly GUIContent MoveUp = new GUIContent("Move Up");
            public static readonly GUIContent MoveDown = new GUIContent("Move Down");
            public static readonly GUIContent Reset = new GUIContent("Reset");
            public static readonly GUIContent Remove = new GUIContent("Remove");
            public static readonly GUIContent Copy = new GUIContent("Copy");
            public static readonly GUIContent Paste = new GUIContent("Paste");
        }

        void OnContextClick(Vector2 pos, int index)
        {
            var menu = new GenericMenu();

            // "Move Up"
            if (index == 0)
                menu.AddDisabledItem(Labels.MoveUp);
            else
                menu.AddItem(Labels.MoveUp, false, () => OnMoveControl(index, index - 1));

            // "Move Down"
            if (index == _controls.arraySize - 1)
                menu.AddDisabledItem(Labels.MoveDown);
            else
                menu.AddItem(Labels.MoveDown, false, () => OnMoveControl(index, index + 1));

            // "Reset" / "Remove"
            menu.AddSeparator(string.Empty);
            menu.AddItem(Labels.Reset, false, () => OnResetControl(index));
            menu.AddItem(Labels.Remove, false, () => OnRemoveControl(index));

            // "Copy" / "Paste"
            menu.AddSeparator(string.Empty);
            menu.AddItem(Labels.Copy, false, () => OnCopyControl(index));
            menu.AddItem(Labels.Paste, false, () => OnPasteControl(index));

            // Show the drop down.
            menu.DropDown(new Rect(pos, Vector2.zero));
        }

        void OnMoveControl(int src, int dst)
        {
            serializedObject.Update();
            _controls.MoveArrayElement(src, dst);
            serializedObject.ApplyModifiedProperties();
        }

        void OnResetControl(int index)
        {
            serializedObject.Update();
            ResetControl(_controls.GetArrayElementAtIndex(index));
            serializedObject.ApplyModifiedProperties();
        }

        void OnRemoveControl(int index)
        {
            serializedObject.Update();
            _controls.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
        }

        void OnCopyControl(int index)
        {
            Debug.Log("Not implemented");
        }

        void OnPasteControl(int index)
        {
            Debug.Log("Not implemented");
        }

        #endregion
    }
}
