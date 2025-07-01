using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Reflection.RPC
{
    public class RPCHooker<ModelType> where ModelType : class, IReflectiveModel
    {
        private object _model;
        private Harmony _harmony;

        private BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public RPCHooker(ref ModelType model)
        {
            _model = model;
            _harmony = new Harmony("RPC Hooks");
        }

        public void Hook()
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            GetMethods(_model, methods);
            Debug.Log($"Methods Count: {methods.Count}");

            foreach (MethodInfo method in methods)
            {
                HarmonyMethod patch = new HarmonyMethod(typeof(TargetTestRPC).GetMethod(nameof(TargetTestRPC.Target)));
                _harmony.Patch(method, postfix: patch);
            }
        }

        private void GetMethods(object obj, List<MethodInfo> methods)
        {
            foreach (MethodInfo method in obj.GetType().GetMethods(_bindingFlags))
            {
                if (method.GetCustomAttribute(typeof(RPCAttribute), false) != null)
                    methods.Add(method);
            }

            foreach (FieldInfo field in obj.GetType().GetFields(_bindingFlags))
            {
                if (field.FieldType.IsClass)
                    GetMethods(field.GetValue(obj), methods);
            }
        }

        public static void ApplyHooks()
        {
            //Nota de Lean: No usen Linq en su libreria.
            //Es exesivamente lento y sobrecarga el garbage collector.
            //Esto es solo para un ejemplo.

            // List<MethodInfo> rpcMethods = AppDomain.CurrentDomain.GetAssemblies()
            //     .SelectMany(assemblys => assemblys.GetTypes())
            //     .SelectMany(types => types.GetMethods(
            //     BindingFlags.Public | BindingFlags.NonPublic |
            //     BindingFlags.Instance | BindingFlags.Static))
            //     .Where(methods => methods.GetCustomAttribute<RPCAttribute>() != null &&
            //                       methods.ReturnType == typeof(void) &&
            //                       methods.GetParameters().Length == 0)
            //     .ToList();

            // foreach (MethodInfo method in rpcMethods)
            // {
            //     HarmonyMethod patch = new HarmonyMethod(typeof(TargetTestRPC).GetMethod(nameof(TargetTestRPC.Target)));
            //     harmony.Patch(method, postfix: patch);
            // }
        }
    }
}