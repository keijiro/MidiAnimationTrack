using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

namespace Klak.Midi
{
    [ScriptedImporter(1, "mid")]
    sealed class MidiAssetImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            var asset = ScriptableObject.CreateInstance<MidiAsset>();
            MidiDeserializer.Load(File.ReadAllBytes(context.assetPath), asset);
            context.AddObjectToAsset("MIDI", asset);
            context.SetMainObject(asset);
        }
    }
}
