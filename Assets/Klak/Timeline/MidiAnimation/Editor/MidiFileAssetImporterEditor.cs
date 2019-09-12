using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

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
