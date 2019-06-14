using UnityEditor;
using UnityEngine;

namespace Klak.Timeline.Midi
{
    // Custom property drawer for ADSR envelope parameters
    [CustomPropertyDrawer(typeof(MidiEnvelope), true)]
    sealed class MidiEnvelopeDrawer : PropertyDrawer
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

        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            // Head control ID: Used to determine the control IDs.
            var id0 = GUIUtility.GetControlID(FocusType.Passive);

            // Envelope parameters (ADSR)
            rect = DrawEnvelopeParameterFields(rect, prop, label);

            // Envelope graph
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

        static Color backgroundColor { get {
            return EditorGUIUtility.isProSkin ?
                new Color(0.18f, 0.18f, 0.18f) : new Color(0.45f, 0.45f, 0.45f);
        } }
        static Color highlightColor { get {
            return EditorGUIUtility.isProSkin ?
                new Color(0.25f, 0.25f, 0.25f) : new Color(0.5f, 0.5f, 0.5f);
        } }

        static Color guideColor { get {
            return EditorGUIUtility.isProSkin ?
                new Color(0.3f, 0.3f, 0.3f) : new Color(0.56f, 0.56f, 0.56f);
        } }

        static Color LineColor { get {
            return EditorGUIUtility.isProSkin ?
                new Color(0.6f, 0.9f, 0.4f) : new Color(0.4f, 0.9f, 0.2f);
        } }

        static readonly GUIContent [] _adsrLabels = {
            new GUIContent("A"), new GUIContent("D"),
            new GUIContent("S"), new GUIContent("R")
        };

        static Vector3 [] _lineVerts = new Vector3 [2];
        static Vector3 [] _graphVerts = new Vector3 [6];

        Rect DrawEnvelopeParameterFields(Rect rect, SerializedProperty prop, GUIContent label)
        {
            // Make a copy of the SerializedProperty to iterate fields.
            var itr = prop.Copy();

            // Label (non clickable)
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(rect, label);

            if (EditorGUIUtility.wideMode)
            {
                // Wide mode: Add margin for the label.
                rect.x += EditorGUIUtility.labelWidth;
                rect.width -= EditorGUIUtility.labelWidth;
            }
            else
            {
                // Narrow mode: Move to the next line.
                rect.y += rect.height;

                // Indent the following controls.
                EditorGUI.indentLevel++;
                rect = EditorGUI.IndentedRect(rect);
                EditorGUI.indentLevel--;
            }

            // Field rect
            var r = rect;
            r.width = (r.width - 6) / 4;

            // Change the label width in the following fields.
            var originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 12;

            for (var i = 0; i < 4; i++)
            {
                itr.Next(true);

                // Element field
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(r, itr, _adsrLabels[i]);

                // Apply the value constraint.
                if (EditorGUI.EndChangeCheck())
                    if (i == 2)
                        itr.floatValue = Mathf.Clamp01(itr.floatValue); // S
                    else
                        itr.floatValue = Mathf.Max(0, itr.floatValue); // ADR

                // Move to the next field.
                r.x += r.width + 2;
            }

            // Recover the original label width.
            EditorGUIUtility.labelWidth = originalLabelWidth;

            // Calculate the graph position.
            rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            rect.height = GraphHeight;
            return rect;
        }

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
