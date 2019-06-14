using UnityEditor;

namespace Klak.Timeline.Midi
{
    static class MidiEditorUtility
    {
        //
        // Guess a property name from a given field name.
        //
        // As far as we know, there are four types of naming conventions for
        // a serialized field name.
        //
        // - Simple camelCase: "fooBar"
        // - Simple PascalCase: "FooBar"
        // - Space separated: "foo bar"
        // - Hangarion fashioned: "m_fooBar", "_fooBar", "fooBar_"
        //
        // This function converts them into a simple camelCased name.
        //
        public static string GuessPropertyNameFromFieldName(string name)
        {
            // Remove Hangarian-fashioned pre/post-fixes.
            if (name.StartsWith("m_"))
                name = name.Substring(2);
            else if (name.StartsWith("_"))
                name = name.Substring(1);
            else if (name.EndsWith("_"))
                name = name.Substring(0, name.Length - 1);

            // Split the name into words and normalize the head characters.
            var words = name.Split();
            for (var i = 0; i < words.Length; i++)
            {
                var w = words[i];
                words[i] = (i == 0 ? System.Char.ToLower(w[0]) :
                                     System.Char.ToUpper(w[0])) + w.Substring(1);
            }
            name = string.Join("", words);

            // We know Unity has some spelling inconsistencies. Let us solve it.
            if (name == "backGroundColor") name = "backgroundColor"; // Camera.backgroundColor

            return name;
        }

        // Check if the property type is supported one.
        public static bool IsPropertyTypeSupported(SerializedPropertyType type)
        {
            return type == SerializedPropertyType.Float ||
                   type == SerializedPropertyType.Vector3 ||
                   type == SerializedPropertyType.Quaternion ||
                   type == SerializedPropertyType.Color;
        }
    }
}
