using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Prototypes.Core.Utils
{

    public static class ReflectionHelpers
    {
        public static IEnumerable<Assembly> UserDefinedAssemblies()
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(a =>
                !a.GlobalAssemblyCache &&
                !a.FullName.StartsWith("mscorlib") &&
                !a.FullName.StartsWith("netstandard") &&
                !a.FullName.StartsWith("nunit") &&
                !a.FullName.StartsWith("System") &&
                !a.FullName.StartsWith("UnityEngine") &&
                !a.FullName.StartsWith("UnityEditor") &&
                !a.FullName.StartsWith("Unity") &&
                !a.FullName.StartsWith("Mono") &&
                !a.FullName.StartsWith("Bee") &&
                !a.FullName.StartsWith("Newtonsoft"));
        }

        public static IEnumerable<Type> GetTypesWithAttribute<T>(this IEnumerable<Assembly> assemblies) where T : Attribute
        {
            return assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttributes(typeof(T), true).Length > 0);
        }

        public static T GetAttribute<T>(this Type type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).FirstOrDefault() as T;
        }
    }
}
