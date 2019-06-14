using UnityEngine;
using UnityEditor;
using UnityEditor.Timeline;

namespace Klak.Timeline.Midi
{
    // Custom inspector for MIDI animation tracks
    // It provides a UI for editing track controls.
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
                    // All modifications should be applied at this moment
                    // because DrawHeaderToggle implicitly discards them.
                    serializedObject.ApplyModifiedProperties();
                }
            }

            // We have to manually refresh the timeline.
            if (EditorGUI.EndChangeCheck())
                TimelineEditor.Refresh(RefreshReason.ContentsModified);

            if (_controls.arraySize > 0) CoreEditorUtils.DrawSplitter();
            EditorGUILayout.Space();

            // "Add" button
            if (GUILayout.Button("Add Control Element")) AppendDefaultMidiControl();
        }

        #endregion

        #region Editor helper methods

        void AppendDefaultMidiControl()
        {
            // Expand the array via SerializedProperty.
            var index = _controls.arraySize;
            _controls.InsertArrayElementAtIndex(index);

            var prop = _controls.GetArrayElementAtIndex(index);
            prop.isExpanded = true;

            serializedObject.ApplyModifiedProperties();

            // Set a new control instance.
            var track = (MidiAnimationTrack)target;
            var controls = track.template.controls;
            Undo.RecordObject(track, "Add MIDI Control");
            controls[controls.Length - 1] = new MidiControl();
        }

        void CopyControl(MidiControl src, MidiControl dst, bool updateGuid)
        {
            // Copy MidiControl members.
            // Is there any smarter way to do this?
            dst.enabled = src.enabled;
            dst.mode = src.mode;
            dst.noteFilter = src.noteFilter;
            dst.envelope = src.envelope;
            dst.curve = new AnimationCurve(src.curve.keys);
            dst.propertyName = src.propertyName;
            dst.fieldName = src.fieldName;
            dst.ccNumber = src.ccNumber;
            dst.vector0 = src.vector0;
            dst.vector1 = src.vector1;

            if (updateGuid)
            {
                // Copy targetComponent as a new reference.
                var guid = GUID.Generate().ToString();
                dst.targetComponent.exposedName = guid;
                var resolver = serializedObject.context as IExposedPropertyTable;
                resolver?.SetReferenceValue(guid, src.targetComponent.Resolve(resolver));
            }
            else
                // Simply copy targetComponent.
                dst.targetComponent = src.targetComponent;
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

        static MidiControl _clipboard = new MidiControl();

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
            // We don't need to refresh the timeline in this case.
            // TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }

        void OnResetControl(int index)
        {
            var track = (MidiAnimationTrack)target;
            Undo.RecordObject(track, "Reset MIDI Control");
            track.template.controls[index] = new MidiControl();
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }

        void OnRemoveControl(int index)
        {
            serializedObject.Update();
            _controls.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }

        void OnCopyControl(int index)
        {
            var track = (MidiAnimationTrack)target;
            Undo.RecordObject(track, "Copy MIDI Control");
            CopyControl(track.template.controls[index], _clipboard, false);
        }

        void OnPasteControl(int index)
        {
            var track = (MidiAnimationTrack)target;
            Undo.RecordObject(track, "Paste MIDI Control");
            CopyControl(_clipboard, track.template.controls[index], true);
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }

        #endregion
    }
}
