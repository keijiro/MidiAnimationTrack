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

            _targetComponent = property.FindPropertyRelative("targetComponent");
            _propertyName    = property.FindPropertyRelative("propertyName");
            _fieldName       = property.FindPropertyRelative("fieldName");

            _vector0 = property.FindPropertyRelative("vector0");
            _vector1 = property.FindPropertyRelative("vector1");
        }

        public Component TargetComponent {
            get { return (Component)(_targetComponent.exposedReferenceValue); }
        }

        public void SetRect(Rect rect)
        {
            _rect = rect;

            // We only use single-line height controls.
            _rect.height = EditorGUIUtility.singleLineHeight;
        }

        public float CalculateHeight()
        {
            if (EditorGUIUtility.wideMode)
                return EditorGUIUtility.singleLineHeight * 6 + 2 * 5;
            else
                return EditorGUIUtility.singleLineHeight * 8 + 2 * 5;
        }

        #endregion

        #region Simple UI methods for offline editing

        public void DrawCommonSettings()
        {
            EditorGUI.PropertyField(_rect, _controlNumber);
            MoveRectToNextLine();

            EditorGUI.PropertyField(_rect, _targetComponent, _labelTarget);
            MoveRectToNextLine();
        }

        public void DrawPropertyField()
        {
            EditorGUI.PropertyField(_rect, _propertyName);
            MoveRectToNextLine();
        }

        public void DrawPropertyVectorFields()
        {
            EditorGUI.BeginChangeCheck();
            var v0 = EditorGUI.Vector4Field(_rect, "Values at 0", _vector0.vector4Value);
            if (EditorGUI.EndChangeCheck()) _vector0.vector4Value = v0;

            MoveRectToNextLine();
            MoveRectToNextLineInNarrowMode();

            EditorGUI.BeginChangeCheck();
            var v1 = EditorGUI.Vector4Field(_rect, "Values at 1", _vector1.vector4Value);
            if (EditorGUI.EndChangeCheck()) _vector1.vector4Value = v1;

            MoveRectToNextLine();
            MoveRectToNextLineInNarrowMode();
        }

        #endregion

        #region Detailed UI methods for online editing

        public void DrawComponentSelector()
        {
            CacheSiblingComponents();

            EditorGUI.indentLevel++;

            // Component selection drop-down
            var i0 = System.Array.IndexOf(_componentNames, TargetComponent.GetType().Name);
            var i1 = EditorGUI.Popup(_rect, "Component", Mathf.Max(0, i0), _componentNames);
            MoveRectToNextLine();

            // Update the target on a selection change.
            if (i0 != i1)
                _targetComponent.exposedReferenceValue =
                    TargetComponent.gameObject.GetComponent(_componentNames[i1]);

            EditorGUI.indentLevel--;
        }

        public void DrawPropertySelector()
        {
            CachePropertiesInTargetComponent();

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
                var i0 = System.Array.IndexOf(_propertyNames, _propertyName.stringValue);
                var i1 = EditorGUI.Popup(_rect, "Property", Mathf.Max(i0, 0), _propertyLabels);
                MoveRectToNextLine();

                // Update the target on selection changes.
                if (i0 != i1)
                {
                    _propertyName.stringValue = _propertyNames[i1];
                    _fieldName.stringValue = _fieldNames[i1];
                }
            }
        }

        public void DrawPropertyOptions()
        {
            if (_propertyTypes.Length == 0) return;

            var pidx = System.Array.IndexOf(_propertyNames, _propertyName.stringValue);
            var type = _propertyTypes[pidx];

            var v0 = _vector0.vector4Value;
            var v1 = _vector1.vector4Value;

            if (type == SerializedPropertyType.Float)
            {
                EditorGUI.BeginChangeCheck();
                v0.x = EditorGUI.FloatField(_rect, "Value at 0", v0.x);
                if (EditorGUI.EndChangeCheck()) _vector0.vector4Value = v0;

                MoveRectToNextLine();
                MoveRectToNextLineInNarrowMode();

                EditorGUI.BeginChangeCheck();
                v1.x = EditorGUI.FloatField(_rect, "Value at 1", v1.x);
                if (EditorGUI.EndChangeCheck()) _vector1.vector4Value = v1;

                MoveRectToNextLine();
                MoveRectToNextLineInNarrowMode();
            }
            else if (type == SerializedPropertyType.Vector3)
            {
                EditorGUI.BeginChangeCheck();
                v0 = EditorGUI.Vector3Field(_rect, "Vector at 0", v0);
                if (EditorGUI.EndChangeCheck()) _vector0.vector4Value = v0;

                MoveRectToNextLine();
                MoveRectToNextLineInNarrowMode();

                EditorGUI.BeginChangeCheck();
                v1 = EditorGUI.Vector3Field(_rect, "Vector at 1", v1);
                if (EditorGUI.EndChangeCheck()) _vector1.vector4Value = v1;

                MoveRectToNextLine();
                MoveRectToNextLineInNarrowMode();
            }
            else if (type == SerializedPropertyType.Quaternion)
            {
                EditorGUI.BeginChangeCheck();
                v0 = EditorGUI.Vector3Field(_rect, "Rotation at 0", v0);
                if (EditorGUI.EndChangeCheck()) _vector0.vector4Value = v0;

                MoveRectToNextLine();
                MoveRectToNextLineInNarrowMode();

                EditorGUI.BeginChangeCheck();
                v1 = EditorGUI.Vector3Field(_rect, "Rotation at 1", v1);
                if (EditorGUI.EndChangeCheck()) _vector1.vector4Value = v1;

                MoveRectToNextLine();
                MoveRectToNextLineInNarrowMode();
            }
            else if (type == SerializedPropertyType.Color)
            {
                EditorGUI.BeginChangeCheck();
                v0 = EditorGUI.ColorField(_rect, "Color at 0", v0);
                if (EditorGUI.EndChangeCheck()) _vector0.vector4Value = v0;

                MoveRectToNextLine();
                MoveRectToNextLineInNarrowMode();

                EditorGUI.BeginChangeCheck();
                v1 = EditorGUI.ColorField(_rect, "Color at 1", v1);
                if (EditorGUI.EndChangeCheck()) _vector1.vector4Value = v1;

                MoveRectToNextLine();
                MoveRectToNextLineInNarrowMode();
            }
        }

        #endregion

        #region Private members

        SerializedProperty _controlNumber;
        SerializedProperty _targetComponent;

        SerializedProperty _propertyName;
        SerializedProperty _fieldName;

        SerializedProperty _vector0;
        SerializedProperty _vector1;

        // Used in component selection drop-down
        string [] _componentNames;
        GameObject _cachedGameObject;

        // Used in property selection drop-down
        string [] _propertyNames;
        string [] _propertyLabels;
        string [] _fieldNames;
        SerializedPropertyType [] _propertyTypes;
        System.Type _cachedComponentType;

        // Labels
        static readonly GUIContent _labelTarget = new GUIContent("Target");

        Rect _rect;

        void MoveRectToNextLine()
        {
            _rect.y += EditorGUIUtility.singleLineHeight + 2;
        }

        void MoveRectToNextLineInNarrowMode()
        {
            if (!EditorGUIUtility.wideMode)
                _rect.y += EditorGUIUtility.singleLineHeight;
        }

        // Enumerate components in the same game object that the target
        // component is attached to.
        void CacheSiblingComponents()
        {
            var go = TargetComponent.gameObject;
            if (_cachedGameObject == go) return;

            _componentNames = go.GetComponents<Component>().
                Select(x => x.GetType().Name).ToArray();

            _cachedGameObject = go;
        }

        // Enumerate properties in the target component.
        void CachePropertiesInTargetComponent()
        {
            var componentType = TargetComponent.GetType();
            if (_cachedComponentType == componentType) return;

            var itr = (new SerializedObject(TargetComponent)).GetIterator();

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
            drawer.DrawCommonSettings();

            if (drawer.TargetComponent == null)
            {
                drawer.DrawPropertyField();
                drawer.DrawPropertyVectorFields();
            }
            else
            {
                drawer.DrawComponentSelector();
                drawer.DrawPropertySelector();
                drawer.DrawPropertyOptions();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetCachedDrawer(property).CalculateHeight();
        }
    }

    #endregion
}
