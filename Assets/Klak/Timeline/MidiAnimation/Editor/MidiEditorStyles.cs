using UnityEngine;
using UnityEditor;

namespace Klak.Timeline
{
    static class MidiEditorStyles
    {
        static readonly Color splitterDark;
        static readonly Color splitterLight;
        public static Color splitter { get {
            return EditorGUIUtility.isProSkin ? splitterDark : splitterLight;
        } }

        static readonly Texture2D gearIconDark;
        static readonly Texture2D gearIconLight;
        public static Texture2D gearIcon { get {
            return EditorGUIUtility.isProSkin ? gearIconDark : gearIconLight;
        } }

        public static readonly GUIStyle headerLabel;

        static readonly Color headerBackgroundDark;
        static readonly Color headerBackgroundLight;
        public static Color headerBackground { get {
            return EditorGUIUtility.isProSkin ? headerBackgroundDark : headerBackgroundLight;
        } }

        public static readonly GUIStyle preLabel;

        static MidiEditorStyles()
        {
            splitterDark = new Color(0.12f, 0.12f, 0.12f, 1.333f);
            splitterLight = new Color(0.6f, 0.6f, 0.6f, 1.333f);

            headerBackgroundDark = new Color(0.1f, 0.1f, 0.1f, 0.2f);
            headerBackgroundLight = new Color(1f, 1f, 1f, 0.2f);

            gearIconDark = (Texture2D)EditorGUIUtility.Load("Builtin Skins/DarkSkin/Images/pane options.png");
            gearIconLight = (Texture2D)EditorGUIUtility.Load("Builtin Skins/LightSkin/Images/pane options.png");

            headerLabel = new GUIStyle(EditorStyles.miniLabel);

            preLabel = new GUIStyle("ShurikenLabel");
        }
    }
}
