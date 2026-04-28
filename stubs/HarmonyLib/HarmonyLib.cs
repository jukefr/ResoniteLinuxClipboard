using System;

namespace HarmonyLib {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class HarmonyPatchAttribute : Attribute {
        public Type Type { get; }
        public string MethodName { get; }
        public Type[] ParameterTypes { get; }

        public HarmonyPatchAttribute(Type type) {
            Type = type;
        }

        public HarmonyPatchAttribute(Type type, string methodName) {
            Type = type;
            MethodName = methodName;
        }

        public HarmonyPatchAttribute(Type type, Type[] parameterTypes) {
            Type = type;
            ParameterTypes = parameterTypes;
        }
    }

    public class Harmony {
        public Harmony(string id) { }
        public void PatchAll() { }
        public void Patch(Type type) { }
    }
}
