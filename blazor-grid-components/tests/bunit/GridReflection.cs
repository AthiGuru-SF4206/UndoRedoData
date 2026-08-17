using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#nullable enable
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

using Syncfusion.Blazor.Grids;

namespace Syncfusion.Blazor.Tests.Grids
{
    internal class GridReflection
    {

        public static T GetPropertyValue<T>(object obj, string propertyName)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var property = GetPropertyInfo(obj.GetType(), propertyName);
            if (property == null)
                throw new ArgumentException($"Property '{propertyName}' not found on type '{obj.GetType().Name}'");

            return (T)property.GetValue(obj);
        }

        /// <summary>
        /// Set internal/private property value dynamically
        /// </summary>
        public static void SetPropertyValue(object obj, string propertyName, object value)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var property = GetPropertyInfo(obj.GetType(), propertyName);
            if (property == null)
                throw new ArgumentException($"Property '{propertyName}' not found on type '{obj.GetType().Name}'");

            // Handle read-only properties by using reflection to bypass backing field
            if (!property.CanWrite)
            {
                SetReadOnlyProperty(obj, property, value);
            }
            else
            {
                property.SetValue(obj, value);
            }
        }

        /// <summary>
        /// Get property value with null-safe handling
        /// </summary>
        public static T GetPropertyValue<T>(object obj, string propertyName, T defaultValue)
        {
            try
            {
                return GetPropertyValue<T>(obj, propertyName);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Get property info (includes private/internal properties)
        /// </summary>
        private static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase;
            return type.GetProperty(propertyName, flags);
        }

        /// <summary>
        /// Set read-only property by finding and setting backing field
        /// </summary>
        private static void SetReadOnlyProperty(object obj, PropertyInfo property, object value)
        {
            var type = obj.GetType();
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            // Try common backing field patterns
            var backingFieldPatterns = new[]
            {
                $"<{property.Name}>k__BackingField",  // Auto-property backing field
                $"_{property.Name}",                    // _PropertyName
                $"m_{property.Name}",                   // m_PropertyName
                $"__{property.Name}",                   // __PropertyName
            };

            foreach (var fieldName in backingFieldPatterns)
            {
                var field = type.GetField(fieldName, flags);
                if (field != null)
                {
                    field.SetValue(obj, value);
                    return;
                }
            }

            // If no backing field found, try using reflection to bypass setter
            var setMethod = property.GetSetMethod(true);
            if (setMethod != null)
            {
                setMethod.Invoke(obj, new[] { value });
            }
            else
            {
                throw new InvalidOperationException($"Cannot set read-only property '{property.Name}' on type '{type.Name}'");
            }
        }

        /// <summary>
        /// Get all property values as dictionary
        /// </summary>
        public static Dictionary<string, object> GetAllProperties(object obj)
        {
            if (obj == null)
                return new Dictionary<string, object>();

            var result = new Dictionary<string, object>();
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (var property in obj.GetType().GetProperties(flags))
            {
                try
                {
                    result[property.Name] = property.GetValue(obj);
                }
                catch
                {
                    result[property.Name] = "[Unable to read]";
                }
            }

            return result;
        }

        /// <summary>
        /// Get nested property value using dot notation (e.g., "PagerRef.ExternalMessage")
        /// </summary>
        public static T GetNestedPropertyValue<T>(object obj, string nestedPropertyPath)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (string.IsNullOrEmpty(nestedPropertyPath))
                throw new ArgumentException("Property path cannot be null or empty", nameof(nestedPropertyPath));

            var properties = nestedPropertyPath.Split('.');
            object current = obj;

            foreach (var propertyName in properties)
            {
                if (current == null)
                    throw new InvalidOperationException($"Cannot access property '{propertyName}' on null object");

                current = GetPropertyValue<object>(current, propertyName);
            }

            return (T)current;
        }
    }
}



