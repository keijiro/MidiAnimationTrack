using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Klak.Timeline
{
    [CustomEditor(typeof(MidiAnimationClip)), CanEditMultipleObjects]
    class MidiAnimationClipEditor : Editor
    {
        SerializedProperty _events;

        void OnEnable()
        {
            _events = serializedObject.FindProperty("template.events");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_events);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
