using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

public class ItemActionRangedPatch
{
    [HarmonyPatch]
    public class ItemActionRangedPatches
    {
        [HarmonyPatch(typeof(ItemActionRanged), "fireShot")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler_fireShot_ItemActionRanged(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            var fld_ranged_tag = AccessTools.Field(typeof(ItemActionAttack), nameof(ItemActionAttack.RangedTag));

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].LoadsField(fld_ranged_tag))
                {
                    codes.InsertRange(i + 2, new CodeInstruction[]
                    {
                    new CodeInstruction(OpCodes.Ldloc_1),
                    CodeInstruction.LoadField(typeof(EntityAlive), nameof(EntityAlive.MinEventContext)),
                    new CodeInstruction(OpCodes.Dup),
                    new CodeInstruction(OpCodes.Ldloc, 10),
                    CodeInstruction.LoadField(typeof(WorldRayHitInfo), nameof(WorldRayHitInfo.hit)),
                    CodeInstruction.LoadField(typeof(HitInfoDetails), nameof(HitInfoDetails.pos)),
                    CodeInstruction.StoreField(typeof(MinEventParams), nameof(MinEventParams.Position)),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    CodeInstruction.Call(typeof(EntityAlive), nameof(EntityAlive.GetPosition)),
                    CodeInstruction.StoreField(typeof(MinEventParams), nameof(MinEventParams.StartPosition))
                    });
                    break;
                }
            }
            return codes;
        }
    }
}