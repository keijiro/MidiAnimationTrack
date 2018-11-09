using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Klak.Timeline
{
    [CustomEditor(typeof(MidiAnimationAsset))]
    class MidiAnimationAssetEditor : Editor
    {
        string _text;

        void OnEnable()
        {
            var events = ((MidiAnimationAsset)target).template.events;

            var note = new HashSet<int>();
            var cc = new HashSet<int>();

            foreach (var e in events)
            {
                switch (e.status & 0xf0u)
                {
                    case 0x80u:
                    case 0x81u: note.Add(e.data1); break;
                    case 0xb0u: cc.Add(e.data1); break;
                }
            }

            if (note.Count > 0 || cc.Count > 0)
            {
                _text = "This MIDI sequence contains the following types of events.";
                if (note.Count > 0) _text += "\nNote: " + string.Join(", ", note.OrderBy(x => x));
                if (cc.Count > 0) _text += "\nCC: " + string.Join(", ", cc.OrderBy(x => x));
            }
            else
            {
                _text = "This MIDI sequence doesn't contain any supported event.";
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(_text, MessageType.None);
        }
    }
}
