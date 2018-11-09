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
            EditorGUILayout.PropertyField(_controls, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
