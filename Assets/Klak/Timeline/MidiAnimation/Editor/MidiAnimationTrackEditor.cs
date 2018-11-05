using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Klak.Timeline
{
    [CustomEditor(typeof(MidiAnimationTrack))]
    class MidiAnimationTrackEditor : Editor
    {
        #region Inspector implementation

        SerializedProperty _bpm;

        void OnEnable()
        {
            _bpm = serializedObject.FindProperty("template.bpm");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_bpm);

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}
