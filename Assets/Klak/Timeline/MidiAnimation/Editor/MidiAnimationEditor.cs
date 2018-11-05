using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Klak.Timeline
{
    [CustomEditor(typeof(MidiAnimation)), CanEditMultipleObjects]
    class MidiAnimationEditor : Editor
    {
        SerializedProperty _sequence;

        void OnEnable()
        {
            _sequence = serializedObject.FindProperty("template.sequence");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_sequence);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
