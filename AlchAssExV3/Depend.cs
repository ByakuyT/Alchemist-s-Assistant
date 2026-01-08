using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Potion.Entities;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AlchAssExV3
{
    public static class Depend
    {
        #region 工具方法
        public static bool CanBrewTimes(IRecipeBookPageContent recipePageContent, int ingredientCount, int brewingTimes)
        {
            var requiredComponents = RecipeBookRecipeBrewController.GetUsedDuringBrewingIngredientsAmount(
                recipePageContent.GetComponentsToUseInBrewWithPreparedIngredients(),
                recipePageContent.GetComponentsToUseInBrewWithoutPreparedIngredients(),
                ingredientCount * brewingTimes,
                true);
            foreach (var requiredComponent in requiredComponents)
            {
                if (requiredComponent.Type == AlchemySubstanceComponentType.InventoryItem)
                {
                    var inventoryItem = requiredComponent.Component as InventoryItem;
                    if (inventoryItem == null)
                        continue;
                    int availableAmount = Managers.Player.Inventory.GetItemCount(inventoryItem);
                    if (availableAmount < requiredComponent.Amount)
                        return false;
                }
            }
            return true;
        }
        public static float CalculateControlSpeedFactor(float distance)
        {
            var normalizedDistance = distance / Variable.ControlAreaThreshold.Value;
            var baseSpeedFactor = Mathf.Pow(normalizedDistance, Variable.ControlSlowdownStrength.Value);
            var asymptoteCalc = normalizedDistance / (normalizedDistance + Variable.ControlAsymptoteFactor.Value);
            return baseSpeedFactor * asymptoteCalc;
        }
        #endregion
    }
}
