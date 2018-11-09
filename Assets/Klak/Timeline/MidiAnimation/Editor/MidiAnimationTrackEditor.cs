using UnityEngine;
using UnityEditor;

namespace Klak.Timeline
{
    [CustomEditor(typeof(MidiAnimationTrack))]
    class MidiAnimationTrackEditor : Editor
    {
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

            for (var i = 0; i < _controls.arraySize; i++)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_controls.GetArrayElementAtIndex(i));
                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                    _controls.DeleteArrayElementAtIndex(i);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Add Control"))
                _controls.InsertArrayElementAtIndex(_controls.arraySize);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
