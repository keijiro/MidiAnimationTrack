using UnityEditor;
using UnityEngine;

namespace Klak.Timeline
{
    [CustomPropertyDrawer(typeof(MidiEnvelope), true)]
    class MidiEnvelopeDrawer : PropertyDrawer
    {
        #region Public method

        public static float GetHeight()
        {
            if (EditorGUIUtility.wideMode)
                return EditorGUIUtility.singleLineHeight + 46;
            else
                return EditorGUIUtility.singleLineHeight * 2 + 46;
        }

        #endregion

        #region Internal variables

        static readonly GUIContent [] _adsrLabels = {
            new GUIContent("A"), new GUIContent("D"),
            new GUIContent("S"), new GUIContent("R")
        };

        static Vector3 [] _vertices = new Vector3 [5];

        #endregion

        #region PropertyDrawer implementation

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            // A-D-S-R float fields
            var itr = property.Copy();
            itr.Next(true);
            EditorGUI.MultiPropertyField(rect, _adsrLabels, itr, label);

            // Rect for graph area
            rect.y += EditorGUIUtility.singleLineHeight + 2;
            rect.height -= EditorGUIUtility.singleLineHeight + 6;
            rect.x += EditorGUIUtility.labelWidth;
            rect.width -= EditorGUIUtility.labelWidth;

            // Background fill
            EditorGUI.DrawRect(rect, MidiEditorStyles.headerBackground);

            // Retrieve serialized parameters #not_very_efficient
            var env = new MidiEnvelope {
                attack = property.FindPropertyRelative("attack").floatValue,
                decay = property.FindPropertyRelative("decay").floatValue,
                sustain = property.FindPropertyRelative("sustain").floatValue,
                release = property.FindPropertyRelative("release").floatValue
            };

            // Time parameters
            var t1 = env.AttackTime;
            var t2 = t1 + env.DecayTime;
            var t3 = t2 + 0.2f;
            var t4 = t3 + env.ReleaseTime;

            // Position parameters
            var w2 = rect.width * 2;
            var h = rect.height;
            var sus = (1 - env.SustainLevel) * h;

            // ADSR graph vertices
            _vertices[0] = new Vector3(0, h);
            _vertices[1] = new Vector3(w2 * t1, 0);
            _vertices[2] = new Vector3(w2 * t2, sus);
            _vertices[3] = new Vector3(w2 * t3, sus);
            _vertices[4] = new Vector3(w2 * t4, h);

            // Draw the graph with clipping
            GUI.BeginGroup(rect);
            Handles.DrawAAPolyLine(_vertices);
            GUI.EndGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetHeight();
        }

        #endregion
    }
}
