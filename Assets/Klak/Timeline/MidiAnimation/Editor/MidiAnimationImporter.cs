using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

namespace Klak.Timeline
{
    [ScriptedImporter(0, "mid")]
    sealed class MidiAssetImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            var asset = ScriptableObject.CreateInstance<MidiAsset>();
            asset.clips = MidiDeserializer.Load(File.ReadAllBytes(context.assetPath));

            context.AddObjectToAsset("MidiAsset", asset);
            context.SetMainObject(asset);

            for (var i = 0; i < asset.clips.Length; i++)
            {
                var track = asset.clips[i];
                track.name = "Track " + i;
                context.AddObjectToAsset(track.name, track);
            }
        }
    }
}
