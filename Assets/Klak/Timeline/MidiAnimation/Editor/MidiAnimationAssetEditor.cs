using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Klak.Timeline
{
    [CustomEditor(typeof(MidiAnimationAsset)), CanEditMultipleObjects]
    class MidiAnimationAssetEditor : Editor
    {
        SerializedProperty _tpqn;
        SerializedProperty _events;

        void OnEnable()
        {
            _tpqn = serializedObject.FindProperty("template.ticksPerQuarterNote");
            _events = serializedObject.FindProperty("template.events");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_tpqn);
            EditorGUILayout.PropertyField(_events, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
