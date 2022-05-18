using HarmonyLib;
using System.Reflection;

namespace Harmony
{
  public class Harmony_TE_ObjectManipulator : IModApi
  {
    public void InitMod(Mod _modInstance)
    {
        Log.Out(" Loading Patch: " + GetType());

        var harmony = new HarmonyLib.Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    [HarmonyPatch(typeof(GameManager), "SetCursorEnabledOverride")]
    public class GameManager_SetCursorEnabledOverride
    {
      static bool Prefix(GameManager __instance, ref bool ___bCursorVisibleOverrideState)
      {
        if (!___bCursorVisibleOverrideState)
        {
          ___bCursorVisibleOverrideState = true;
        }

        return true;
      }
    }
  }

}
