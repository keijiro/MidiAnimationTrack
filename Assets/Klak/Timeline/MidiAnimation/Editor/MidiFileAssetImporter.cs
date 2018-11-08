using System.IO;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;

namespace Klak.Timeline
{
    [ScriptedImporter(1, "mid")]
    public class MidiFileAssetImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            // Main MIDI file asset
            var buffer = File.ReadAllBytes(context.assetPath);
            var asset = MidiFileDeserializer.Load(buffer);
            context.AddObjectToAsset("MidiFileAsset", asset);
            context.SetMainObject(asset);

            // Contained tracks
            for (var i = 0; i < asset.tracks.Length; i++)
            {
                var track = asset.tracks[i];
                track.name = "Track" + (i + 1);
                context.AddObjectToAsset(track.name, track);
            }
        }
    }
}
