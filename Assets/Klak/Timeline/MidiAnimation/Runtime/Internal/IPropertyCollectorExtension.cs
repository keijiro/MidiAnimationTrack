using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Timeline;

namespace Klak.Timeline.Midi
{
    //
    // Extension methods for IPropertyCollector
    //
    // This class is used to invoke a generic method AddFromName<T> with a
    // dynamic type.
    //
    static class IPropertyCollectorExtension
    {
        static (Type type, MethodInfo method) _genericAddFromName;
        static (Type type, MethodInfo method) _specializedAddFromName;
        static object [] _args2 = new object [2];

        internal static void AddFromName(
            this IPropertyCollector driver,
            Type componentType, GameObject go, string name
        )
        {
            var driverType = driver.GetType();

            if (_genericAddFromName.type != driverType)
            {
                // Retrieve AddFromName<T>(GameObject, string)
                foreach (var m in driverType.GetMethods())
                {
                    if (m.Name != "AddFromName") continue;
                    if (!m.IsGenericMethod) continue;

                    var args = m.GetParameters();
                    if (args.Length != 2) continue;
                    if (args[0].ParameterType != typeof(GameObject)) continue;
                    if (args[1].ParameterType != typeof(string)) continue;

                    _genericAddFromName = (driverType, m);
                    _specializedAddFromName = (null, null);
                    break;
                }
            }

            Debug.Assert(_genericAddFromName.method != null);

            if (_specializedAddFromName.type != componentType)
            {
                // Construct AddFromName<componentType>(GameObject, string)
                _specializedAddFromName = (
                    componentType,
                    _genericAddFromName.method.MakeGenericMethod(componentType)
                );
            }

            // Invoke the specialized method.
            _args2[0] = go;
            _args2[1] = name;
            _specializedAddFromName.method.Invoke(driver, _args2);
        }
    }
}
