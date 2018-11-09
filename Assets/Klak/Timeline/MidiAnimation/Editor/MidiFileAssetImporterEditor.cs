using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

namespace Klak.Timeline
{
    [CustomEditor(typeof(MidiFileAssetImporter))]
    class MidiFileAssetImporterEditor : ScriptedImporterEditor
    {
        public override void OnInspectorGUI() { }
    }
}
