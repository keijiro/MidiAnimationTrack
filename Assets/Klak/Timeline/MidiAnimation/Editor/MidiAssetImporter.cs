using System.IO;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;

namespace Klak.Timeline
{
    [ScriptedImporter(1, "mid")]
    public class MidiAssetImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            // Main MIDI asset
            var asset = MidiDeserializer.Load(File.ReadAllBytes(context.assetPath));
            context.AddObjectToAsset("MidiAsset", asset);
            context.SetMainObject(asset);

            // Contained clips (= MIDI tracks)
            for (var i = 0; i < asset.clips.Length; i++)
            {
                var track = asset.clips[i];
                track.name = "Track " + (i + 1);
                context.AddObjectToAsset(track.name, track);
            }
        }
    }
}
