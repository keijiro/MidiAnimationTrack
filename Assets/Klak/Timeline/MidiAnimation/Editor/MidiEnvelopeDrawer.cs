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
                return GraphHeight + EditorGUIUtility.singleLineHeight + 6;
            else
                return GraphHeight + EditorGUIUtility.singleLineHeight * 2 + 6;
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
            var graphRect = new Rect(
                rect.x + EditorGUIUtility.labelWidth,
                rect.y + EditorGUIUtility.singleLineHeight + 2,
                rect.width - EditorGUIUtility.labelWidth,
                GraphHeight
            );

            // Draw envelope graph
            GUI.BeginGroup(graphRect);
            DrawGraph(RetrieveEnvelope(prop), graphRect.width, graphRect.height, id0 + 4);
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

        static Vector3 [] _guideVerts = new Vector3 [10];
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

        // Draw envelope graph
        void DrawGraph(MidiEnvelope env, float width, float height, int controlID)
        {
            const float scale = 2;

            // Time parameters
            var t1 =      scale * env.AttackTime;
            var t2 = t1 + scale * env.DecayTime;
            var t3 = t2 + scale * 0.2f;
            var t4 = t3 + scale * env.ReleaseTime;

            // Position parameters
            var x1 = width * t1;
            var x2 = width * t2;
            var x3 = width * t3;
            var x4 = width * t4;
            var sus_y = (1 - env.SustainLevel) * (height - 1);

            // ADSR graph vertices
            _graphVerts[0] = new Vector3(0, height);
            _graphVerts[1] = new Vector3(x1, 0);
            _graphVerts[2] = new Vector3(x2, sus_y);
            _graphVerts[3] = new Vector3(x3, sus_y);
            _graphVerts[4] = new Vector3(x4, height - 1);
            _graphVerts[5] = new Vector3(width, height - 1);

            // Guide line vertices
            _guideVerts[0] = new Vector3(x1, 0);
            _guideVerts[1] = new Vector3(x1, height);
            _guideVerts[2] = new Vector3(x2, 0);
            _guideVerts[3] = new Vector3(x2, height);
            _guideVerts[4] = new Vector3(x3, 0);
            _guideVerts[5] = new Vector3(x3, height);
            _guideVerts[6] = new Vector3(x4, 0);
            _guideVerts[7] = new Vector3(x4, height);
            _guideVerts[8] = new Vector3(0, sus_y);
            _guideVerts[9] = new Vector3(width, sus_y);

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
            Handles.DrawLines(_guideVerts);

            // ADSR graph
            Handles.color = LineColor;
            Handles.DrawPolyLine(_graphVerts);
        }

        #endregion
    }
}
