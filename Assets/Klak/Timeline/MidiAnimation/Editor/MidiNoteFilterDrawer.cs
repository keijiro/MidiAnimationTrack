using UnityEditor;
using UnityEngine;

namespace Klak.Timeline.Midi
{
    // Custom property drawer for MidiNoteFilter struct
    [CustomPropertyDrawer(typeof(MidiNoteFilter), true)]
    sealed class MidiNoteFilterDrawer : PropertyDrawer
    {
        static readonly int [] _noteValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        static readonly GUIContent [] _noteLabels = {
            new GUIContent("All"),
            new GUIContent("C" ), new GUIContent("C#"), new GUIContent("D" ),
            new GUIContent("D#"), new GUIContent("E" ), new GUIContent("F" ),
            new GUIContent("F#"), new GUIContent("G" ), new GUIContent("G#"),
            new GUIContent("A" ), new GUIContent("A#"), new GUIContent("B" )
        };

        static readonly int [] _octaveValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

        static readonly GUIContent [] _octaveLabels = {
            new GUIContent("All"),
            new GUIContent("-2"), new GUIContent("-1"), new GUIContent("0"),
            new GUIContent( "1"), new GUIContent( "2"), new GUIContent("3"),
            new GUIContent( "4"), new GUIContent( "5"), new GUIContent("6"),
            new GUIContent( "7"), new GUIContent( "8")
        };

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            // Label (focusable but not functional)
            EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Keyboard), label);

            // Half width control rect
            rect.x += EditorGUIUtility.labelWidth;
            rect.width = (rect.width - EditorGUIUtility.labelWidth - 4) / 2;

            // Note name drop down
            var note = property.FindPropertyRelative("note");
            EditorGUI.BeginChangeCheck();
            var index = EditorGUI.IntPopup(rect, note.enumValueIndex, _noteLabels, _noteValues);
            if (EditorGUI.EndChangeCheck()) note.enumValueIndex = index;

            rect.x += rect.width + 4;

            // Octave drop down
            var octave = property.FindPropertyRelative("octave"); 
            EditorGUI.BeginChangeCheck();
            index = EditorGUI.IntPopup(rect, octave.enumValueIndex, _octaveLabels, _octaveValues);
            if (EditorGUI.EndChangeCheck()) octave.enumValueIndex = index;
        }
    }
}
