using UnityEditor;
using UnityEngine;

namespace Klak.Timeline
{
    [CustomPropertyDrawer(typeof(MidiEnvelope), true)]
    class MidiEnvelopeDrawer : PropertyDrawer
    {
        static readonly GUIContent [] _adsrLabels = {
            new GUIContent("A"), new GUIContent("D"),
            new GUIContent("S"), new GUIContent("R")
        };

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var itr = property.Copy();
            itr.Next(true);
            EditorGUI.MultiPropertyField(rect, _adsrLabels, itr, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (EditorGUIUtility.wideMode)
                return EditorGUIUtility.singleLineHeight + 2;
            else
                return EditorGUIUtility.singleLineHeight * 2 + 2;
        }
    }
}
