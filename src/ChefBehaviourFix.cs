using System;
using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using UnityModManagerNet;

namespace ChefBehaviourFix
{
    static class Main
    {
        public static UnityModManager.ModEntry mod;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            Harmony.DEBUG = true;

            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll();

            mod = modEntry;

            return true;
        }      
    }

    [HarmonyPatch]
    static class CraftGoal__ReversePatch {

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(CraftGoal), "HasUsedIngredientsWaitingForProduct")]
        public static bool HasUsedIngredientsWaitingForProduct(CraftGoal instance, Character character) {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(CraftGoal), "GetRetrieveFinishedItemGoal")]
        public static bool GetRetrieveFinishedItemGoal(CraftGoal instance, Character character, CraftingProp craftingProp, out Goal goal) {
            throw new NotImplementedException("stub");
        }
    }
    
    [HarmonyPatch(typeof(CraftGoal), "GetNextSubGoal")]
    static class CraftGoal__GetNextSubGoal__Patch {
        static Goal Postfix(Goal __result, ref CraftGoal __instance, Character character, Goal parent) {
            // System.Text.StringBuilder goal_sb = new System.Text.StringBuilder();
            // System.Text.StringBuilder result_goal_sb = new System.Text.StringBuilder();
            // __instance.BuildDebugString(goal_sb);
            if (__result != null) {
                // __result.BuildDebugString(result_goal_sb);
            } else {
                // result_goal_sb.Append("null");
            }
            // Main.mod.Logger.Warning($"a {character.FirstName} {character.Surname} {goal_sb.ToString()} => {result_goal_sb.ToString()}");
            if (__instance.FollowingRecipe == null) {
                // Main.mod.Logger.Warning("no recipe!");
            }
            else {
                // Main.mod.Logger.Warning($"recipe {__instance.FollowingRecipe.UniqueID}");

                if (__result == null && !CraftGoal__ReversePatch.HasUsedIngredientsWaitingForProduct(__instance, character) && __instance.FollowingRecipe.RequiredPropToWorkOn() != 0)
                {
                    GameTerrain terrain = GameTerrain.Instance;
                    CraftingProp craftingProp = terrain.GetFixedObjectOnTile(__instance.DestTile.x, __instance.DestTile.y) as CraftingProp;
                    if (craftingProp != null && (craftingProp.CraftingFinished || craftingProp.CraftingRecipe == null)) {
                        if (craftingProp.Inventory.GetTotalNutrition() > 0f) {
                            // Main.mod.Logger.Warning("target prop has stuck food");
                            if (__instance.DesiredAmount == int.MaxValue && character.GetRole() == Role.Cook) {
                                // Main.mod.Logger.Warning("is cook");
                                if (CraftGoal__ReversePatch.GetRetrieveFinishedItemGoal(__instance, character, craftingProp, out Goal goal2)) {
                                    // result_goal_sb.Clear();
                                    // goal2.BuildDebugString(result_goal_sb);
                                    // Main.mod.Logger.Warning($"found a new goal: {result_goal_sb.ToString()}");
                                    return goal2;
                                } else {
                                    // Main.mod.Logger.Warning("couldn't find a new goal");
                                }
                            }
                        }
                    }
                }
            }

            return __result;
        }
    }

    [HarmonyPatch(typeof(CraftAnim), "Update")]
    static class CraftAnim__Update__Patch {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var codes = new List<CodeInstruction>(instructions);
            
            var StopRecipe_MethodInfo = typeof(CraftGoal).GetMethod("StopRecipe");

            for (int i = codes.Count - 1; i >= 2; i--) {
                CodeInstruction ci = codes[i];
                if (ci.Calls(StopRecipe_MethodInfo)) {
                    codes.RemoveRange(i - 2, 3);
                    break;
                }
            }
            
            return codes;
        }
    }

    [HarmonyPatch(typeof(CraftGoal), "OnCraftingFinished")]
    static class CraftGoal__OnCraftingFinished__Patch {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var codes = new List<CodeInstruction>(instructions);
            
            var StopRecipe_MethodInfo = typeof(CraftGoal).GetMethod("StopRecipe");

            for (int i = 0; i < codes.Count; i++) {
                CodeInstruction ci = codes[i];
                if (ci.Calls(StopRecipe_MethodInfo)) {
                    codes.RemoveRange(i - 2, 3);
                    break;
                }
            }
            
            return codes;
        }
    }
}
