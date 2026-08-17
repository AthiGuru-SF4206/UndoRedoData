using System;
using System.Collections.Generic;
using System.Reflection;
using System.Dynamic;
using Syncfusion.Blazor.Data;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Class performs property access and cache the getter method for better performance.
    /// </summary>
    internal class PropertyInfoHelper : ReflectionHelper
    {
        public object GetObject(string nameSpace, object from)
        {
            if (string.IsNullOrEmpty(nameSpace))
            {
                return from;
            }

            return this.GetValue(from, nameSpace);
        }
    }

    internal class PropertyInfoHelper<T> : ReflectionHelper<T>
    {
        public object GetObject(string nameSpace, T from)
        {
            if (string.IsNullOrEmpty(nameSpace))
            {
                return from!;
            }

            return this.GetValue(from, nameSpace);
        }

        public object GetObject(string nameSpace, object from)
        {
            if (string.IsNullOrEmpty(nameSpace))
            {
                return from;
            }

            return this.GetValue(from, nameSpace);
        }
    }
}
