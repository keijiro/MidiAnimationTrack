using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

namespace Klak.Timeline
{
    [ScriptedImporter(1, "mid")]
    sealed class MidiAssetImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            var data = File.ReadAllBytes(context.assetPath);
            var tracks = MidiDeserializer.Load(data);
            if (tracks.Length == 0) return;

            var asset = ScriptableObject.CreateInstance<MidiAsset>();
            asset.clips = tracks;

            context.AddObjectToAsset("MidiAsset", asset);
            for (var i = 0; i < tracks.Length; i++)
                context.AddObjectToAsset("Track" + i, tracks[i]);

            context.SetMainObject(asset);
        }
    }
}
