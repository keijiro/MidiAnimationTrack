using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

namespace Klak.Timeline
{
    [CustomEditor(typeof(MidiFileAssetImporter)), CanEditMultipleObjects]
    class MidiFileAssetImporterEditor : ScriptedImporterEditor
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
