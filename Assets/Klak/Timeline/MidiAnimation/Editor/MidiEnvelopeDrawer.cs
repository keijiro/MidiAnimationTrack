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
            var line = EditorGUIUtility.singleLineHeight;
            var space = EditorGUIUtility.standardVerticalSpacing;
            if (!EditorGUIUtility.wideMode) line *= 2;
            return line + GraphHeight + space * 3;
        }

        #endregion

        #region PropertyDrawer implementation

        public override void
            OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            // Used to determine the control IDs
            var id0 = GUIUtility.GetControlID(FocusType.Passive);

            // A-D-S-R float fields
            var itr = prop.Copy();
            itr.Next(true);
            EditorGUI.MultiPropertyField(rect, _adsrLabels, itr, label);

            // Rect for graph area
            rect.y += EditorGUIUtility.standardVerticalSpacing;
            rect.height = GraphHeight;

            if (EditorGUIUtility.wideMode)
            {
                rect.x += EditorGUIUtility.labelWidth;
                rect.y += EditorGUIUtility.singleLineHeight;
                rect.width -= EditorGUIUtility.labelWidth;
            }
            else
            {
                rect.y += EditorGUIUtility.singleLineHeight * 2;
                EditorGUI.indentLevel++;
                rect = EditorGUI.IndentedRect(rect);
                EditorGUI.indentLevel--;
            }

            // Draw envelope graph
            GUI.BeginGroup(rect);
            DrawGraph(RetrieveEnvelope(prop), rect.width, rect.height, id0 + 2);
            GUI.EndGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetHeight();
        }

        #endregion

        #region Internal constants and variables

        const float GraphHeight = 40;

        static readonly Color backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        static readonly Color highlightColor = new Color(0.23f, 0.22f, 0.22f);
        static readonly Color guideColor = new Color(0.3f, 0.3f, 0.3f);
        static readonly Color LineColor = new Color(0.6f, 0.9f, 0.4f);

        static readonly GUIContent [] _adsrLabels = {
            new GUIContent("A"), new GUIContent("D"),
            new GUIContent("S"), new GUIContent("R")
        };

        static Vector3 [] _lineVerts = new Vector3 [2];
        static Vector3 [] _graphVerts = new Vector3 [6];

        // Retrieve serialized envelope parameters #not_very_efficient
        MidiEnvelope RetrieveEnvelope(SerializedProperty prop)
        {
            return new MidiEnvelope {
                attack  = prop.FindPropertyRelative("attack" ).floatValue,
                decay   = prop.FindPropertyRelative("decay"  ).floatValue,
                sustain = prop.FindPropertyRelative("sustain").floatValue,
                release = prop.FindPropertyRelative("release").floatValue
            };
        }

        void DrawAALine(float x0, float y0, float x1, float y1)
        {
            _lineVerts[0].x = x0;
            _lineVerts[0].y = y0;
            _lineVerts[1].x = x1;
            _lineVerts[1].y = y1;
            Handles.DrawAAPolyLine(_lineVerts);
        }

        void DrawGraph(MidiEnvelope env, float width, float height, int controlID)
        {
            const float scale = 2;

            // Time parameters
            var t1 =      scale * env.AttackTime;
            var t2 = t1 + scale * env.DecayTime;
            var t3 = t2 + scale * 0.2f;
            var t4 = t3 + scale * env.ReleaseTime;

            // Position parameters
            var x1 = 1 + width * t1;
            var x2 = 1 + width * t2;
            var x3 = 1 + width * t3;
            var x4 = 1 + width * t4;
            var sus_y = (1 - env.SustainLevel) * (height - 2) + 1;

            // ADSR graph vertices
            _graphVerts[0] = new Vector3(1, height);
            _graphVerts[1] = new Vector3(x1, 1);
            _graphVerts[2] = new Vector3(x2, sus_y);
            _graphVerts[3] = new Vector3(x3, sus_y);
            _graphVerts[4] = new Vector3(x4, height - 1);
            _graphVerts[5] = new Vector3(width, height - 1);

            // Background
            EditorGUI.DrawRect(new Rect(0, 0, width, height), backgroundColor);

            // Guide elements
            var focus = GUIUtility.keyboardControl;

            if (focus == controlID)
                EditorGUI.DrawRect(new Rect(0, 0, x1, height), highlightColor);
            else if (focus == controlID + 1)
                EditorGUI.DrawRect(new Rect(x1, 0, x2 - x1, height), highlightColor);
            else if (focus == controlID + 2)
                EditorGUI.DrawRect(new Rect(0, sus_y, width, height), highlightColor);
            else if (focus == controlID + 3)
                EditorGUI.DrawRect(new Rect(x3, 0, x4 - x3, height), highlightColor);

            Handles.color = guideColor;
            DrawAALine(x1, 0, x1, height);
            DrawAALine(x2, 0, x2, height);
            DrawAALine(x3, 0, x3, height);
            DrawAALine(x4, 0, x4, height);
            DrawAALine(0, sus_y, width, sus_y);

            // ADSR graph
            Handles.color = LineColor;
            Handles.DrawAAPolyLine(_graphVerts);
        }

        #endregion
    }
}
