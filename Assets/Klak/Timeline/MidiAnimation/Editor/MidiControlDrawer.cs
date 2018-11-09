using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Klak.Timeline
{
    #region Property drawer for MidiControl

    class MidiControlDrawer
    {
        #region Public properties and methods

        public MidiControlDrawer(SerializedProperty property)
        {
            _controlNumber = property.FindPropertyRelative("controlNumber");

            _componentName = property.FindPropertyRelative("componentName");
            _propertyName  = property.FindPropertyRelative("propertyName");
            _fieldName     = property.FindPropertyRelative("fieldName");

            _baseVector    = property.FindPropertyRelative("baseVector");
            _rotationAxis  = property.FindPropertyRelative("rotationAxis");
            _colorAt0      = property.FindPropertyRelative("colorAt0");
            _colorAt1      = property.FindPropertyRelative("colorAt1");
        }

        public string ComponentName {
            get { return _componentName?.stringValue; }
        }

        public void SetRect(Rect rect)
        {
            _rect = rect;

            // We only use single-line height controls.
            _rect.height = EditorGUIUtility.singleLineHeight;
        }

        public float CalculateHeight()
        {
            return (EditorGUIUtility.singleLineHeight + 2) * 5 - 2;
        }

        #endregion

        #region Simple UI methods for offline editing

        public void DrawCommonSettings()
        {
            EditorGUI.PropertyField(_rect, _controlNumber);
            MoveRectToNextLine();
        }

        public void DrawComponentField()
        {
            EditorGUI.PropertyField(_rect, _componentName);
            MoveRectToNextLine();
        }

        public void DrawPropertyField()
        {
            EditorGUI.PropertyField(_rect, _propertyName);
            MoveRectToNextLine();
        }

        #endregion

        #region Detailed UI methods for online editing

        public void DrawComponentSelector(GameObject go)
        {
            CacheComponentsInGameObject(go);

            // Component selection drop-down
            var name = _componentName.stringValue;
            var index0 = System.Array.IndexOf(_componentNames, name);
            var index1 = EditorGUI.Popup
                (_rect, "Component", Mathf.Max(0, index0), _componentNames);
            MoveRectToNextLine();

            // Update the target on a selection change.
            if (index0 != index1)
            {
                _componentName.stringValue = _componentNames[index1];
                TimelineEditor.Refresh(RefreshReason.ContentsModified);
            }
        }

        public void DrawPropertySelector(Component component)
        {
            CachePropertiesInComponent(component);

            if (_propertyNames.Length == 0)
            {
                // There is no supported property in the component.
                // Clear the property selection.
                _propertyName.stringValue = "";
                _fieldName.stringValue = "";
            }
            else
            {
                // Property selection drop-down
                var name = _propertyName.stringValue;
                var index0 = System.Array.IndexOf(_propertyNames, name);
                var index1 = EditorGUI.Popup
                    (_rect, "Property", Mathf.Max(index0, 0), _propertyLabels);
                MoveRectToNextLine();

                // Update the target on selection changes.
                if (index0 != index1)
                {
                    _propertyName.stringValue = _propertyNames[index1];
                    _fieldName.stringValue = _fieldNames[index1];
                    TimelineEditor.Refresh(RefreshReason.ContentsModified);
                }
            }
        }

        public void DrawPropertyOptions()
        {
            if (_propertyTypes.Length == 0) return;

            var pidx = System.Array.IndexOf(_propertyNames, _propertyName.stringValue);
            var type = _propertyTypes[pidx];

            if (type == SerializedPropertyType.Vector3)
            {
                EditorGUI.PropertyField(_rect, _baseVector);
                MoveRectToNextLine();
            }
            else if (type == SerializedPropertyType.Quaternion)
            {
                EditorGUI.PropertyField(_rect, _rotationAxis);
                MoveRectToNextLine();
            }
            else if (type == SerializedPropertyType.Color)
            {
                EditorGUI.PropertyField(_rect, _colorAt0);
                MoveRectToNextLine();
                EditorGUI.PropertyField(_rect, _colorAt1);
                MoveRectToNextLine();
            }
        }

        #endregion

        #region Private members

        public SerializedProperty _controlNumber;

        public SerializedProperty _componentName;
        public SerializedProperty _propertyName;
        public SerializedProperty _fieldName;

        public SerializedProperty _baseVector;
        public SerializedProperty _rotationAxis;
        public SerializedProperty _colorAt0;
        public SerializedProperty _colorAt1;

        // Used in component selection drop-down
        public string [] _componentNames;
        public GameObject _cachedGameObject;

        // Used in property selection drop-down
        public string [] _propertyNames;
        public string [] _propertyLabels;
        public string [] _fieldNames;
        public SerializedPropertyType [] _propertyTypes;
        public System.Type _cachedComponentType;

        Rect _rect;

        void MoveRectToNextLine()
        {
            _rect.y += EditorGUIUtility.singleLineHeight + 2;
        }

        // Enumerate components attached to a given game object.
        void CacheComponentsInGameObject(GameObject go)
        {
            if (_cachedGameObject == go) return;

            _componentNames = go.GetComponents<Component>().
                Select(x => x.GetType().Name).ToArray();

            _cachedGameObject = go;
        }

        // Enumerate component properties that have corresponding serialized
        // fields.
        void CachePropertiesInComponent(Component component)
        {
            var componentType = component.GetType();

            if (_cachedComponentType == componentType) return;

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
                    if (MidiEditorUtility.IsPropertyTypeSupported(type))
                    {
                        // Check if the field has a corresponding property.
                        var pname = MidiEditorUtility.GuessPropertyNameFromFieldName(itr.name);
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

                _propertyNames = pnames.ToArray();
                _propertyLabels = labels.ToArray();
                _fieldNames = fnames.ToArray();
                _propertyTypes = types.ToArray();
            }
            else
            {
                // Failed to retrieve properties.
                _propertyNames = _fieldNames = _propertyLabels = new string [0];
                _propertyTypes = new SerializedPropertyType [0];
            }

            _cachedComponentType = componentType;
        }

        #endregion
    }

    #endregion

    #region Custom property drawer class (works as entry points)

    [CustomPropertyDrawer(typeof(MidiControl), true)]
    class MidiControlDrawerEntry : PropertyDrawer
    {
        Dictionary<string, MidiControlDrawer> _drawers = new Dictionary<string, MidiControlDrawer>();

        MidiControlDrawer GetCachedDrawer(SerializedProperty property)
        {
            MidiControlDrawer drawer;

            var path = property.propertyPath;
            _drawers.TryGetValue(path, out drawer);

            if (drawer == null)
            {
                // No instance was found witht the given path,
                // so create a new instance for it.
                drawer = new MidiControlDrawer(property);
                _drawers[path] = drawer;
            }

            return drawer;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var drawer = GetCachedDrawer(property);

            drawer.SetRect(rect);

            // Common settings
            drawer.DrawCommonSettings();

            // Track-bound game object
            var target = (TrackAsset)property.serializedObject.targetObject;
            var go = TimelineEditor.inspectedDirector?.GetGenericBinding(target) as GameObject;

            // Component selector
            if (go == null)
                drawer.DrawComponentField();
            else
                drawer.DrawComponentSelector(go);

            // Selected component
            var component = go?.GetComponent(drawer.ComponentName);

            // Property selector
            if (component == null)
                drawer.DrawPropertyField();
            else
                drawer.DrawPropertySelector(component);

            // Property options
            drawer.DrawPropertyOptions();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetCachedDrawer(property).CalculateHeight();
        }
    }

    #endregion
}
