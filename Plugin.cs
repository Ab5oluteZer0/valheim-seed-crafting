using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace SeedCrafting
{
    // Crafting: 1x w pelni wyrosla roslina (zebrane warzywo) -> 2x nasiono tego samego typu.
    // Rejestracja recipe bezposrednio przez natywne API gry (ObjectDB.m_recipes), Harmony postfix
    // na ObjectDB.Awake w momencie gdy ObjectDB jest juz realnie wypelniony (po wejsciu do swiata).
    //
    // Uwaga: Jotunn.ItemManager (CustomRecipe/Mock<T>) NIE zostal tu uzyty, bo w tej wersji gry
    // ItemManager.OnVanillaItemsAvailable odpala sie tylko raz, na pustym ObjectDB z ekranu
    // startowego, i Jotunn nigdy nie rozwiazuje Mockow do prawdziwych prefabow (zweryfikowane
    // w logu: recipe sie "rejestrowal" ale nigdy nie byl widoczny przy stole warsztatowym).
    // Bezposrednie uzycie ObjectDB.m_recipes omija ten problem.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class SeedCraftingPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.ab5olutezer0.valheim.seedcrafting";
        public const string PluginName = "Seed Crafting";
        public const string PluginVersion = "1.0.2";

        // (nazwa prefabu warzywa, nazwa prefabu nasiona, ile nasion za 1 warzywo)
        private static readonly (string CropItem, string SeedItem, int SeedAmount)[] Recipes =
        {
            ("Carrot", "CarrotSeeds", 2),
            ("Turnip", "TurnipSeeds", 2),
            ("Onion", "OnionSeeds", 2),
            ("Kale", "KaleSeeds", 2),
            ("Poteitr", "PoteitrSeeds", 2),
        };

        private const string CraftingStationPrefab = "piece_workbench";

        internal static ManualLogSource Log;
        private static bool _recipesAdded;

        private void Awake()
        {
            Log = Logger;
            // Gra prosi mody o ustawienie tej flagi: w menu pojawia sie napis, ze gra jest
            // zmodowana (Iron Gate wymaga oznaczania modow jako nieoficjalnych).
            Game.isModded = true;
            new Harmony(PluginGUID).PatchAll(typeof(SeedCraftingPlugin).Assembly);
        }

        [HarmonyPatch(typeof(ObjectDB), "Awake")]
        private static class ObjectDB_Awake_Patch
        {
            private static void Postfix(ObjectDB __instance)
            {
                if (_recipesAdded || __instance == null || __instance.m_items == null || __instance.m_items.Count == 0)
                    return;

                _recipesAdded = true;
                AddSeedRecipes(__instance);
            }
        }

        private static void AddSeedRecipes(ObjectDB objectDB)
        {
            CraftingStation station = null;
            GameObject stationPrefab = ZNetScene.instance != null ? ZNetScene.instance.GetPrefab(CraftingStationPrefab) : null;
            if (stationPrefab != null)
                station = stationPrefab.GetComponent<CraftingStation>();

            if (station == null)
                Log.LogWarning($"Nie znaleziono crafting station '{CraftingStationPrefab}' - przepisy beda bez wymaganej stacji.");

            foreach (var (cropItem, seedItem, seedAmount) in Recipes)
            {
                GameObject cropPrefab = objectDB.GetItemPrefab(cropItem);
                GameObject seedPrefab = objectDB.GetItemPrefab(seedItem);

                if (cropPrefab == null)
                {
                    Log.LogWarning($"Pomijam recipe {cropItem}->{seedItem}: brak prefabu warzywa '{cropItem}' w ObjectDB.");
                    continue;
                }

                if (seedPrefab == null)
                {
                    Log.LogWarning($"Pomijam recipe {cropItem}->{seedItem}: brak prefabu nasiona '{seedItem}' w ObjectDB.");
                    continue;
                }

                ItemDrop cropItemDrop = cropPrefab.GetComponent<ItemDrop>();
                ItemDrop seedItemDrop = seedPrefab.GetComponent<ItemDrop>();

                if (cropItemDrop == null || seedItemDrop == null)
                {
                    Log.LogWarning($"Pomijam recipe {cropItem}->{seedItem}: brak komponentu ItemDrop na prefabie.");
                    continue;
                }

                var recipe = ScriptableObject.CreateInstance<Recipe>();
                recipe.name = $"Recipe_SeedCrafting_{cropItem}";
                recipe.m_item = seedItemDrop;
                recipe.m_amount = seedAmount;
                recipe.m_enabled = true;
                recipe.m_craftingStation = station;
                recipe.m_minStationLevel = 1;
                recipe.m_resources = new[]
                {
                    new Piece.Requirement
                    {
                        m_resItem = cropItemDrop,
                        m_amount = 1,
                        m_amountPerLevel = 0
                    }
                };

                objectDB.m_recipes.Add(recipe);
                Log.LogInfo($"Dodano recipe: 1x {cropItem} -> {seedAmount}x {seedItem}");
            }
        }
    }
}
