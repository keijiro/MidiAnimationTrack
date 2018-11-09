using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Klak.Timeline
{
    [CustomPropertyDrawer(typeof(MidiControl), true)] class MidiControlDrawer : PropertyDrawer
    {
        class DrawerState
        {
            public SerializedProperty controlNumber;

            public SerializedProperty componentName;
            public SerializedProperty propertyName;
            public SerializedProperty fieldName;

            public SerializedProperty baseVector;
            public SerializedProperty rotationAxis;
            public SerializedProperty colorAt0;
            public SerializedProperty colorAt1;

            // Used in component selection drop-down
            public string [] componentNames;
            public GameObject cachedGameObject;

            // Used in property selection drop-down
            public string [] propertyNames;
            public string [] propertyLabels;
            public string [] fieldNames;
            public SerializedPropertyType [] propertyTypes;
            public System.Type cachedComponentType;

            public DrawerState(SerializedProperty property)
            {
                controlNumber = property.FindPropertyRelative("controlNumber");

                componentName = property.FindPropertyRelative("componentName");
                propertyName  = property.FindPropertyRelative("propertyName");
                fieldName     = property.FindPropertyRelative("fieldName");

                baseVector    = property.FindPropertyRelative("baseVector");
                rotationAxis  = property.FindPropertyRelative("rotationAxis");
                colorAt0      = property.FindPropertyRelative("colorAt0");
                colorAt1      = property.FindPropertyRelative("colorAt1");
            }

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
            static string FieldToPropertyName(string name)
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
            static bool IsPropertyTypeSupported(SerializedPropertyType type)
            {
                return type == SerializedPropertyType.Float ||
                       type == SerializedPropertyType.Vector3 ||
                       type == SerializedPropertyType.Quaternion ||
                       type == SerializedPropertyType.Color;
            }

            // Enumerate components attached to a given game object.
            public void CacheComponentsInGameObject(GameObject go)
            {
                if (cachedGameObject == go) return;

                componentNames = go.GetComponents<Component>().
                    Select(x => x.GetType().Name).ToArray();

                cachedGameObject = go;
            }

            // Enumerate component properties that have corresponding serialized
            // fields.
            public void CachePropertiesInComponent(Component component)
            {
                var componentType = component.GetType();

                if (cachedComponentType == componentType) return;

                var itr = (new SerializedObject(component)).GetIterator();

                var pnames = new List<string>();
                var labels = new List<string>();
                var fnames = new List<string>();
                var types = new List<SerializedPropertyType>();

                if (itr.NextVisible(true))
                {
                    while (true)
                    {
                        var type = itr.propertyType;
                        if (IsPropertyTypeSupported(type))
                        {
                            // Check if the field has a corresponding property.
                            var pname = FieldToPropertyName(itr.name);
                            if (componentType.GetProperty(pname) != null)
                            {
                                // Append this field.
                                pnames.Add(pname);
                                labels.Add(ObjectNames.NicifyVariableName(pname));
                                fnames.Add(itr.name);
                                types.Add(type);
                            }
                        }

                        if (!itr.NextVisible(false)) break;
                    }

                    propertyNames = pnames.ToArray();
                    propertyLabels = labels.ToArray();
                    fieldNames = fnames.ToArray();
                    propertyTypes = types.ToArray();
                }
                else
                {
                    // Failed to retrieve properties.
                    propertyNames = fieldNames = propertyLabels = new string [0];
                    propertyTypes = new SerializedPropertyType [0];
                }

                cachedComponentType = componentType;
            }
        }

        Dictionary<string, DrawerState> _drawerStates = new Dictionary<string, DrawerState>();

        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            // Retrieve drawer state.
            DrawerState state;
            _drawerStates.TryGetValue(prop.propertyPath, out state);
            if (state == null) state = new DrawerState(prop);

            // We only use single-line height controls.
            rect.height = EditorGUIUtility.singleLineHeight;

            // Control number edit
            EditorGUI.PropertyField(rect, state.controlNumber);
            rect.y += EditorGUIUtility.singleLineHeight + 2;

            // Retrieve the track-bound game object.
            var target = (TrackAsset)prop.serializedObject.targetObject;
            var go = TimelineEditor.inspectedDirector?.GetGenericBinding(target) as GameObject;

            if (go == null)
            {
                // No game object: Simply present a normal text field.
                EditorGUI.PropertyField(rect, state.componentName);
            }
            else
            {
                // Retrieve and cache components in the game object.
                state.CacheComponentsInGameObject(go);

                // Component selection drop-down
                var name = state.componentName.stringValue;
                var index0 = System.Array.IndexOf(state.componentNames, name);
                var index1 = EditorGUI.Popup(
                    rect, "Component",
                    Mathf.Max(0, index0),
                    state.componentNames
                );

                // Update the target on selection changes.
                if (index0 != index1)
                {
                    state.componentName.stringValue = state.componentNames[index1];
                    TimelineEditor.Refresh(RefreshReason.ContentsModified);
                }
            }

            rect.y += EditorGUIUtility.singleLineHeight + 2;

            var component = go?.GetComponent(state.componentName.stringValue);

            if (component == null)
            {
                // No component selection: Simple present a normal text field.
                EditorGUI.PropertyField(rect, state.propertyName);
            }
            else
            {
                // Retrieve and cache properties in the component.
                state.CachePropertiesInComponent(component);

                if (state.propertyNames.Length == 0)
                {
                    // There is no supported property in the component.
                    // Clear the property selection.
                    state.propertyName.stringValue = "";
                    state.fieldName.stringValue = "";
                }
                else
                {
                    // Property selection drop-down
                    var name = state.propertyName.stringValue;
                    var index0 = System.Array.IndexOf(state.propertyNames, name);
                    var index1 = EditorGUI.Popup
                        (rect, "Property", Mathf.Max(index0, 0), state.propertyLabels);

                    // Update the target on selection changes.
                    if (index0 != index1)
                    {
                        state.propertyName.stringValue = state.propertyNames[index1];
                        state.fieldName.stringValue = state.fieldNames[index1];
                        TimelineEditor.Refresh(RefreshReason.ContentsModified);
                    }

                    /*
                    // Show additional options for non-float types.
                    var type = _propertyTypes[index1];
                    if (type == SerializedPropertyType.Vector3)
                        EditorGUILayout.PropertyField(_baseVector);
                    else if (type == SerializedPropertyType.Quaternion)
                        EditorGUILayout.PropertyField(_rotationAxis);
                    else if (type == SerializedPropertyType.Color)
                    {
                        EditorGUILayout.PropertyField(_colorAt0);
                        EditorGUILayout.PropertyField(_colorAt1);
                    }
                    */
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3 + 2 * 2;
        }
    }
}
