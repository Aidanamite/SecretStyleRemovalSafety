using HarmonyLib;
using SRML;
using SRML.Console;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using Console = SRML.Console.Console;
using Object = UnityEngine.Object;

namespace SecretStyleRemovalSafety
{
    public class Main : ModEntryPoint
    {
        internal static Assembly modAssembly = Assembly.GetExecutingAssembly();
        internal static string modName = $"{modAssembly.GetName().Name}";
        internal static string modDir = $"{System.Environment.CurrentDirectory}\\SRML\\Mods\\{modName}";

        public override void PreLoad()
        {
            HarmonyInstance.PatchAll();
        }
        public static void Log(string message) => Console.Log($"[{modName}]: " + message);
        public static void LogError(string message) => Console.LogError($"[{modName}]: " + message);
        public static void LogWarning(string message) => Console.LogWarning($"[{modName}]: " + message);
        public static void LogSuccess(string message) => Console.LogSuccess($"[{modName}]: " + message);
    }

    [HarmonyPatch(typeof(SlimeAppearanceDirector),"SetModel")]
    class Patch_SlimeAppearanceDirector_SetModel
    {
        static void Prefix(SlimeAppearanceDirector __instance, MonomiPark.SlimeRancher.DataModel.AppearancesModel model)
        {
            var keys = model.selections.Keys.ToList();
            foreach (var key in keys)
            {
                var def = __instance.SlimeDefinitions.GetSlimeByIdentifiableId(key);
                if (!def)
                    model.selections.Remove(key);
                else if (!def.Appearances.FirstOrDefault((x) => x.SaveSet == model.selections[key]))
                    model.selections[key] = def.Appearances.First().SaveSet;
            }
            keys = model.unlocks.Keys.ToList();
            foreach (var key in keys)
            {
                var def = __instance.SlimeDefinitions.GetSlimeByIdentifiableId(key);
                if (!def)
                    model.unlocks.Remove(key);
                else
                {
                    var sets = model.unlocks[key];
                    var sets2 = new SlimeAppearance.AppearanceSaveSet[sets.Count];
                    var apps = def.Appearances.ToList();
                    sets.CopyTo(sets2, 0);
                    foreach (var set in sets2)
                        if (!apps.Exists((x) => x.SaveSet == set))
                            sets.Remove(set);
                }
            }
        }
    }
}