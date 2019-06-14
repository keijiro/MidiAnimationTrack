using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Klak.Timeline.Midi
{
    // Property drawer implementation for MidiControl
    sealed class MidiControlInternalDrawer
    {
        #region Public properties and methods

        public MidiControlInternalDrawer(SerializedProperty property)
        {
            _mode       = property.FindPropertyRelative("mode");
            _noteFilter = property.FindPropertyRelative("noteFilter");
            _envelope   = property.FindPropertyRelative("envelope");
            _curve      = property.FindPropertyRelative("curve");
            _ccNumber   = property.FindPropertyRelative("ccNumber");

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
            _baseRect = _rect = rect;

            // We only use single-line height controls.
            _rect.height = EditorGUIUtility.singleLineHeight;
        }

        public float GetTotalHeight()
        {
            return _rect.y - _baseRect.y;
        }

        #endregion

        #region Simple UI methods for offline editing

        public void DrawCommonSettings()
        {
            EditorGUI.PropertyField(_rect, _mode, _labelControlMode);
            MoveRectToNextLine();

            if (_mode.enumValueIndex == (int)MidiControl.Mode.NoteEnvelope)
            {
                EditorGUI.PropertyField(_rect, _noteFilter, _labelNoteOctave);
                MoveRectToNextLine();

                var r = _rect;
                r.height = MidiEnvelopeDrawer.GetHeight();
                EditorGUI.PropertyField(r, _envelope);
                _rect.y += r.height;
            }
            else if (_mode.enumValueIndex == (int)MidiControl.Mode.NoteCurve)
            {
                EditorGUI.PropertyField(_rect, _noteFilter, _labelNoteOctave);
                MoveRectToNextLine();

                EditorGUI.PropertyField(_rect, _curve);
                MoveRectToNextLine();
            }
            else // CC
            {
                EditorGUI.PropertyField(_rect, _ccNumber, _labelCCNumber);
                MoveRectToNextLine();
            }

            EditorGUI.PropertyField(_rect, _targetComponent, _labelTarget);
            MoveRectToNextLine();
        }

        #endregion

        #region Detailed UI methods for online editing

        public void DrawComponentSelector()
        {
            CacheSiblingComponents();

            EditorGUI.indentLevel++;

            // Component selection drop-down
            EditorGUI.BeginChangeCheck();

            var index = System.Array.IndexOf(_componentNames, TargetComponent.GetType().Name);
            index = EditorGUI.Popup(_rect, "Component", index, _componentNames);

            if (EditorGUI.EndChangeCheck())
                _targetComponent.exposedReferenceValue =
                    TargetComponent.gameObject.GetComponent(_componentNames[index]);

            MoveRectToNextLine();
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
                EditorGUI.BeginChangeCheck();

                var index = System.Array.IndexOf(_propertyNames, _propertyName.stringValue);
                index = EditorGUI.Popup(_rect, "Property", index, _propertyLabels);

                if (index < 0)
                {
                    _propertyName.stringValue = "";
                    _fieldName.stringValue = "";
                }
                else if (EditorGUI.EndChangeCheck())
                {
                    _propertyName.stringValue = _propertyNames[index];
                    _fieldName.stringValue = _fieldNames[index];
                }

                MoveRectToNextLine();
            }
        }

        #endregion

        #region Property option drawer

        public void DrawPropertyOptions()
        {
            var pidx = System.Array.IndexOf(_propertyNames, _propertyName.stringValue);
            var type = pidx < 0 ? null : (SerializedPropertyType?)_propertyTypes[pidx];

            var v0 = _vector0.vector4Value;
            var v1 = _vector1.vector4Value;

            if (type == SerializedPropertyType.Float)
            {
                EditorGUI.BeginChangeCheck();
                v0.x = EditorGUI.FloatField(_rect, "Value at 0", v0.x);
                if (EditorGUI.EndChangeCheck()) _vector0.vector4Value = v0;

                MoveRectToNextLine();

                EditorGUI.BeginChangeCheck();
                v1.x = EditorGUI.FloatField(_rect, "Value at 1", v1.x);
                if (EditorGUI.EndChangeCheck()) _vector1.vector4Value = v1;

                MoveRectToNextLine();
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

                EditorGUI.BeginChangeCheck();
                v1 = EditorGUI.ColorField(_rect, "Color at 1", v1);
                if (EditorGUI.EndChangeCheck()) _vector1.vector4Value = v1;

                MoveRectToNextLine();
            }
        }

        #endregion

        #region UI resources

        static readonly GUIContent _labelControlMode = new GUIContent("Control Mode");
        static readonly GUIContent _labelCCNumber = new GUIContent("CC Number");
        static readonly GUIContent _labelTarget = new GUIContent("Target");
        static readonly GUIContent _labelNoteOctave = new GUIContent("Note/Octave");

        #endregion

        #region Private members

        SerializedProperty _mode;
        SerializedProperty _noteFilter;
        SerializedProperty _envelope;
        SerializedProperty _curve;
        SerializedProperty _ccNumber;

        SerializedProperty _targetComponent;
        SerializedProperty _propertyName;
        SerializedProperty _fieldName;

        SerializedProperty _vector0;
        SerializedProperty _vector1;

        // Used in component selection drop-down
        string [] _componentNames;
        GameObject _cachedGameObject;

        // Used in property selection drop-down
        string [] _propertyNames = new string [0];
        string [] _propertyLabels;
        string [] _fieldNames;
        SerializedPropertyType [] _propertyTypes;
        System.Type _cachedComponentType;

        Rect _baseRect;
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

    // Custom property drawer for MidiControl
    // Provides a cache for instances of the drawer implementation.
    [CustomPropertyDrawer(typeof(MidiControl), true)]
    sealed class MidiControlDrawer : PropertyDrawer
    {
        Dictionary<string, MidiControlInternalDrawer>
            _drawers = new Dictionary<string, MidiControlInternalDrawer>();

        MidiControlInternalDrawer GetCachedDrawer(SerializedProperty property)
        {
            MidiControlInternalDrawer drawer;

            var path = property.propertyPath;
            _drawers.TryGetValue(path, out drawer);

            if (drawer == null)
            {
                // No instance was found witht the given path,
                // so create a new instance for it.
                drawer = new MidiControlInternalDrawer(property);
                _drawers[path] = drawer;
            }

            return drawer;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var drawer = GetCachedDrawer(property);

            drawer.SetRect(rect);
            drawer.DrawCommonSettings();

            if (drawer.TargetComponent != null)
            {
                drawer.DrawComponentSelector();
                drawer.DrawPropertySelector();
                drawer.DrawPropertyOptions();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetCachedDrawer(property).GetTotalHeight();
        }
    }
}
