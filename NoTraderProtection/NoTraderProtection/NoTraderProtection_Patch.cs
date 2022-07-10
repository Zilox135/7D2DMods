using System;
using HarmonyLib;

namespace Harmony
{
    [HarmonyPatch(typeof(TraderArea), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(Vector3i), typeof(Vector3i), typeof(Vector3i), typeof(Vector3i), typeof(Vector3i) })]
    public class NoTraderProtection_Patch
    {
        public static void Postfix(TraderArea __instance)
        {
            __instance.ProtectSize = Vector3i.zero;
        }
    }
}
