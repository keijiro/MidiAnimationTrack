using UnityEditor;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace Klak.Timeline.Midi
{
    // Custom inspector for MIDI file assets
    [CustomEditor(typeof(MidiFileAssetImporter)), CanEditMultipleObjects]
    sealed class MidiFileAssetImporterEditor : ScriptedImporterEditor
    {
        SerializedProperty _tempo;

        public override bool showImportedObject { get { return false; } }

        public override void OnEnable()
        {
            base.OnEnable();
            _tempo = serializedObject.FindProperty("_tempo");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_tempo);
            serializedObject.ApplyModifiedProperties();
            ApplyRevertGUI();
        }
    }
}
