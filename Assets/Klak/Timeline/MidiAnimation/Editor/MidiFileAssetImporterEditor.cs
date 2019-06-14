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
            _tempo = serializedObject.FindProperty("_tempo");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_tempo);
            ApplyRevertGUI();
        }
    }
}
