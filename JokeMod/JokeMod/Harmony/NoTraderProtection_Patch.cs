using System;
using System.Collections.Generic;
using HarmonyLib;

namespace JokeMod.Harmony
{
    [HarmonyPatch(typeof(TraderArea), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(Vector3i), typeof(Vector3i), typeof(Vector3i), typeof(List<Prefab.PrefabTeleportVolume>) })]
    public class NoTraderProtection_Patch
    {
        public static void Postfix(TraderArea __instance)
        {
            __instance.ProtectSize = Vector3i.zero;
        }
    }
}
