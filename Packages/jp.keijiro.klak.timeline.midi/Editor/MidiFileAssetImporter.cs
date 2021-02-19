using System.IO;
using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace Klak.Timeline.Midi
{
    // Custom importer for .mid files
    [ScriptedImporter(1, "mid")]
    sealed class MidiFileAssetImporter : ScriptedImporter
    {
        [SerializeField] float _tempo = 120;

        public override void OnImportAsset(AssetImportContext context)
        {
            var name = System.IO.Path.GetFileNameWithoutExtension(assetPath);

            // Main MIDI file asset
            var buffer = File.ReadAllBytes(context.assetPath);
            var asset = MidiFileDeserializer.Load(buffer);
            asset.name = name;
            context.AddObjectToAsset("MidiFileAsset", asset);
            context.SetMainObject(asset);

            // Contained tracks
            for (var i = 0; i < asset.tracks.Length; i++)
            {
                var track = asset.tracks[i];
                track.name = name + " Track " + (i + 1);
                track.template.tempo = _tempo;
                context.AddObjectToAsset(track.name, track);
            }
        }
    }
}
