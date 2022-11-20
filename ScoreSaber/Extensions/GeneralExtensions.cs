#region

using System;
using System.Reflection;

#endregion

namespace ScoreSaber.Extensions {
    internal static class GeneralExtensions {
        private const BindingFlags _allBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        internal static void SetStaticField(this object obj, string fieldName, object value) {

            (obj is Type type ? type : obj.GetType())
                .GetField(fieldName, _allBindingFlags)
                ?.SetValue(obj, value);
        }
    }
}