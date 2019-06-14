using UnityEngine;

namespace Klak.Timeline
{
    // ScriptableObject class used for storing a MIDI file asset
    sealed public class MidiFileAsset : ScriptableObject
    {
        public MidiAnimationAsset [] tracks;
    }
}
