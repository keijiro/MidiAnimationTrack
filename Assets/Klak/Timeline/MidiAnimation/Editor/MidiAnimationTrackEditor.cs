using UnityEngine;
using UnityEditor;

namespace Klak.Timeline
{
    [CustomEditor(typeof(MidiAnimationTrack))]
    class MidiAnimationTrackEditor : Editor
    {
        #region Editor implementation

        SerializedProperty _bpm;
        SerializedProperty _controls;

        void OnEnable()
        {
            _bpm = serializedObject.FindProperty("template.bpm");
            _controls = serializedObject.FindProperty("template.controls");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_bpm);
            EditorGUILayout.Space();

            for (var i = 0; i < _controls.arraySize; i++)
            {
                DrawSplitter();
                DrawHeader(i);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_controls.GetArrayElementAtIndex(i));
                EditorGUILayout.Space();
            }

            DrawSplitter();

            EditorGUILayout.Space();
            if (GUILayout.Button("Add Control Element"))
                _controls.InsertArrayElementAtIndex(_controls.arraySize);

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Custom UI elements

        void DrawSplitter()
        {
            var rect = GUILayoutUtility.GetRect(1, 1);
            rect.xMin = 0;
            rect.width += 4;
            EditorGUI.DrawRect(rect, MidiEditorStyles.splitter);
        }

        void DrawHeader(int index)
        {
            var bgRect = GUILayoutUtility.GetRect(1, 17);

            var labelRect = bgRect;
            labelRect.xMin -= 4;
            labelRect.xMax -= 20;

            var menuIcon = MidiEditorStyles.gearIcon;
            var menuRect = new Rect(
                labelRect.xMax + 4, labelRect.y + 4,
                menuIcon.width, menuIcon.height
            );

            // Background rect should be full-width
            bgRect.xMin = 0;
            bgRect.width += 4;

            // Background
            EditorGUI.DrawRect(bgRect, MidiEditorStyles.headerBackground);

            // Title
            var title = "Control Element " + (index + 1);
            EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);

            // Dropdown menu icon
            GUI.DrawTexture(menuRect, menuIcon);

            // Handle events
            var e = Event.current;

            if (e.type == EventType.MouseDown && menuRect.Contains(e.mousePosition))
            {
                // Header context menu
                var menu = new GenericMenu();
                menu.AddItem(
                    new GUIContent("Remove"), false,
                    () => OnRemoveControl(index)
                );
                menu.DropDown(new Rect(menuRect.x, menuRect.yMax, 0, 0));
                e.Use();
            }
        }

        void OnRemoveControl(int index)
        {
            serializedObject.Update();
            _controls.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}
