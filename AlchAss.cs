using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem.Cursor;
using PotionCraft.ManagersSystem.RecipeMap;
using PotionCraft.ManagersSystem.SaveLoad;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.Cauldron;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.Mortar;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.SolventDirectionHint;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.VortexMapItem;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ObjectBased.UIElements;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
//药水瓶半径0.74，效果半径0.79
namespace AlchAss
{
    [BepInPlugin("AlchAss", "Alchemist's Assistant", "2.9.0")]
    public class AlchAss : BaseUnityPlugin
    {
        public static ConfigEntry<bool> enableGrindStatus;
        public static ConfigEntry<bool> enableHealthStatus;
        public static ConfigEntry<bool> enableVortexStatus;
        public static ConfigEntry<bool> enableStirStatus;
        public static ConfigEntry<bool> enablePositionStatus;
        public static ConfigEntry<bool> enableDeviationStatus;
        public static ConfigEntry<bool> enableClosestPathStatus;
        public static ConfigEntry<bool> enableClosestLadleStatus;

        public static ConfigEntry<bool> enableShuttingDown;
        public static ConfigEntry<bool> enableGrindSpeed;
        public static ConfigEntry<bool> enableStirSpeed;
        public static ConfigEntry<bool> enableLadleSpeed;
        public static ConfigEntry<bool> enableHeatSpeed;
        public static ConfigEntry<bool> enableBrewMore;
        public static ConfigEntry<bool> enableDirectionLine;
        public static ConfigEntry<bool> enableGrindAll;
        public static ConfigEntry<bool> enableVortexEdge;

        public static ConfigEntry<float> windowScale;

        public static DebugWindow grindDebugWindow;
        public static DebugWindow healthDebugWindow;
        public static DebugWindow vortexDebugWindow;
        public static DebugWindow stirDebugWindow;
        public static DebugWindow positionDebugWindow;
        public static DebugWindow deviationDebugWindow;
        public static DebugWindow closestPathDebugWindow;
        public static DebugWindow closestLadleDebugWindow;

        public static Vector2 positionGrindDebugWindow = Vector2.zero;
        public static Vector2 positionHealthDebugWindow = Vector2.zero;
        public static Vector2 positionVortexDebugWindow = Vector2.zero;
        public static Vector2 positionStirDebugWindow = Vector2.zero;
        public static Vector2 positionPositionDebugWindow = Vector2.zero;
        public static Vector2 positionDeviationDebugWindow = Vector2.zero;
        public static Vector2 positionClosestPathDebugWindow = Vector2.zero;
        public static Vector2 positionClosestLadleDebugWindow = Vector2.zero;

        public static bool directionLine = false;
        public static bool vortexEdgeControl = false;
        public static bool endMode = false;
        public static bool xOy = false;
        public static float vortexEdgeOn = float.MaxValue;
        public static float endDirection = 0f;
        public static float zoneStir = 0f;
        public static string hoveredItemName = "";
        public static Room lab = null;
        public static Sprite spriteOld = null;
        public static SolventDirectionHint solventDirectionHint = null;
        public static readonly List<DebugWindow> foreground_queue = new();
        public static readonly string grindHeatPath = "AlchAssGrindHeatConfig.txt";
        public static readonly string speedBrewPath = "AlchAssSpeedBrewConfig.txt";
        public static readonly string windowPath = "AlchAssWindowConfig.txt";

        public void Awake()
        {
            enableGrindStatus = Config.Bind("信息窗口", "研磨信息", true, "开启后，显示研磨进度.");
            enableHealthStatus = Config.Bind("信息窗口", "血量信息", true, "开启后，显示血量.");
            enableVortexStatus = Config.Bind("信息窗口", "漩涡信息", true, "开启后，显示漩涡中心方向和夹角.");
            enableStirStatus = Config.Bind("信息窗口", "搅拌信息", true, "开启后，显示搅拌阶段、搅拌进度和搅拌方向.");
            enablePositionStatus = Config.Bind("信息窗口", "位置信息", true, "开启后，显示位置、方向、旋转和折算盐量，以及与所接触效果的差值.");
            enableDeviationStatus = Config.Bind("信息窗口", "偏离信息", true, "开启后，显示与所接触效果的总体、位置和旋转偏差度.");
            enableClosestPathStatus = Config.Bind("信息窗口", "路径信息", true, "开启后，显示路径最近点的目标效果和偏差度.");
            enableClosestLadleStatus = Config.Bind("信息窗口", "加水信息", true, "开启后，显示加水最近点的目标效果和偏差度.");
            enableDirectionLine = Config.Bind("信息窗口", "方向提示", true, "开启后，按下 / 键显示当前搅拌方向提示线.");
            windowScale = Config.Bind("信息窗口", "窗口缩放", 1f, "信息窗口显示的大小比例.");

            enableShuttingDown = Config.Bind("操作控制", "允许漩涡急停", true, "开启后，右键点击风箱把手将使药剂热量值变为指定值.");
            enableGrindAll = Config.Bind("操作控制", "允许瞬间研磨", true, "开启后，右键点击研杵把手将使药材研磨度变为指定值.");
            enableVortexEdge = Config.Bind("操作控制", "允许漩涡贴边", true, "开启后，按下 ' 键后药水瓶即将离开漩涡边缘时将逐渐减速搅拌和加水直到停止.");
            enableGrindSpeed = Config.Bind("操作控制", "允许研磨减速", true, "开启后，按住 Z 或 X 键将使研磨减速至相应指定比例.");
            enableStirSpeed = Config.Bind("操作控制", "允许搅拌减速", true, "开启后，按住 Z 或 X 键将使搅拌减速至相应指定比例.");
            enableLadleSpeed = Config.Bind("操作控制", "允许加水减速", true, "开启后，按住 Z 或 X 键将使加水减速至相应指定比例.");
            enableHeatSpeed = Config.Bind("操作控制", "允许加热减速", true, "开启后，按住 Z 或 X 键将使加热减速至相应指定比例.");
            enableBrewMore = Config.Bind("操作控制", "允许大批炼药", true, "开启后，按住 Z 或 X 键将使炼药数量增加至相应指定比例.");

            if (!File.Exists(Path.Combine(Paths.PluginPath, grindHeatPath)))
                File.WriteAllText(Path.Combine(Paths.PluginPath, grindHeatPath), "100");
            if (!File.Exists(Path.Combine(Paths.PluginPath, speedBrewPath)))
                File.WriteAllText(Path.Combine(Paths.PluginPath, speedBrewPath), "10\n100");
            if (!File.Exists(Path.Combine(Paths.PluginPath, windowPath)))
                File.WriteAllText(Path.Combine(Paths.PluginPath, windowPath), null);
            var lines = File.ReadAllLines(Path.Combine(Paths.PluginPath, windowPath));
            if (lines.Length > 7)
            {
                positionGrindDebugWindow = Helper.StringToVector2(lines[0]);
                positionHealthDebugWindow = Helper.StringToVector2(lines[1]);
                positionVortexDebugWindow = Helper.StringToVector2(lines[2]);
                positionStirDebugWindow = Helper.StringToVector2(lines[3]);
                positionPositionDebugWindow = Helper.StringToVector2(lines[4]);
                positionDeviationDebugWindow = Helper.StringToVector2(lines[5]);
                positionClosestPathDebugWindow = Helper.StringToVector2(lines[6]);
                positionClosestLadleDebugWindow = Helper.StringToVector2(lines[7]);
            }

            LocalizationManager.OnInitialize.AddListener(Helper.SetModLocalization);
            Harmony.CreateAndPatchAll(typeof(AlchAss));
            Logger.LogInfo("插件加载完成！");
        }

        public void Update()
        {
            if (enableDirectionLine.Value)
                InfoCalc.DirectionLine();
            if (enableShuttingDown.Value)
                InfoCalc.CoolDown();
            if (enableGrindAll.Value)
                InfoCalc.GrindAll();
            if (enableVortexEdge.Value)
                InfoCalc.VortexEdge();
            if (enableClosestPathStatus.Value)
                InfoCalc.EndMode();
            if (enablePositionStatus.Value)
                InfoCalc.PositionMode();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveLoadManager), "SaveProgressToPool")]
        public static void SaveWindowsPosition()
        {
            using StreamWriter writer = new(Path.Combine(Paths.PluginPath, windowPath));
            writer.WriteLine(grindDebugWindow != null ? grindDebugWindow.transform.position : "");
            writer.WriteLine(healthDebugWindow != null ? healthDebugWindow.transform.position : "");
            writer.WriteLine(vortexDebugWindow != null ? vortexDebugWindow.transform.position : "");
            writer.WriteLine(stirDebugWindow != null ? stirDebugWindow.transform.position : "");
            writer.WriteLine(positionDebugWindow != null ? positionDebugWindow.transform.position : "");
            writer.WriteLine(deviationDebugWindow != null ? deviationDebugWindow.transform.position : "");
            writer.WriteLine(closestPathDebugWindow != null ? closestPathDebugWindow.transform.position : "");
            writer.WriteLine(closestLadleDebugWindow != null ? closestLadleDebugWindow.transform.position : "");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MovableUIItem), "FixOutOfBoundsCase")]
        public static bool WindowFixOutOfBoundsCase(MovableUIItem __instance)
        {
            return __instance is not DebugWindow;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Window), "ToForeground")]
        public static bool WindowForeground(Window __instance)
        {
            if (__instance is not DebugWindow dbg)
                return true;
            if (!foreground_queue.Contains(dbg))
                return false;
            foreground_queue.Remove(dbg);
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Room), "Awake")]
        public static void InitDebugWindows(Room __instance)
        {
            if (__instance.roomIndex == RoomIndex.Laboratory)
                lab = __instance;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SubstanceGrinding), "TryToGrind")]
        public static void GrindSlowDown(ref float pestleLinearSpeed, ref float pestleAngularSpeed)
        {
            if (enableGrindSpeed.Value)
            {
                if (Keyboard.current.xKey.isPressed)
                    InfoCalc.GrindSlowDown(ref pestleLinearSpeed, ref pestleAngularSpeed, 1);
                else if (Keyboard.current.zKey.isPressed)
                    InfoCalc.GrindSlowDown(ref pestleLinearSpeed, ref pestleAngularSpeed, 0);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Cauldron), "UpdateStirringValue")]
        public static void StirSlowDown(ref float ___StirringValue)
        {
            if (enableStirSpeed.Value)
            {
                if (Keyboard.current.xKey.isPressed)
                    InfoCalc.StirSlowDown(ref ___StirringValue, 1);
                else if (Keyboard.current.zKey.isPressed)
                    InfoCalc.StirSlowDown(ref ___StirringValue, 0);
            }
            if (vortexEdgeControl)
                ___StirringValue *= Mathf.Clamp01(vortexEdgeOn);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecipeMapManager), "GetSpeedOfMovingTowardsBase")]
        public static void LadleSlowDown(ref float __result)
        {
            if (enableLadleSpeed.Value)
            {
                if (Keyboard.current.xKey.isPressed)
                    InfoCalc.LadleSlowDown(ref __result, 1);
                else if (Keyboard.current.zKey.isPressed)
                    InfoCalc.LadleSlowDown(ref __result, 0);
            }
            if (vortexEdgeControl)
                __result *= Mathf.Clamp01(vortexEdgeOn);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RecipeMapManager), "MoveIndicatorTowardsVortex")]
        public static void HeatSlowDown(ref float __state)
        {
            if (enableHeatSpeed.Value)
            {
                if (Keyboard.current.xKey.isPressed)
                    InfoCalc.HeatSlowDown(ref __state, 1);
                else if (Keyboard.current.zKey.isPressed)
                    InfoCalc.HeatSlowDown(ref __state, 0);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecipeMapManager), "MoveIndicatorTowardsVortex")]
        public static void HeatSlowDownEnd(float __state)
        {
            if (enableHeatSpeed.Value)
                if (__state != 0f)
                    if (Keyboard.current.xKey.isPressed || Keyboard.current.zKey.isPressed)
                        Settings<RecipeMapManagerVortexSettings>.Asset.vortexMovementSpeed = __state;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mortar), "Update")]
        public static void GrindStatus(Mortar __instance)
        {
            if (enableGrindStatus.Value)
                if (grindDebugWindow == null)
                    grindDebugWindow = Helper.CreateDebugWindow("dialog_grind_status", positionGrindDebugWindow);
            grindDebugWindow?.ShowText(InfoCalc.GrindCalc(__instance));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(IndicatorMapItem), "UpdateByCollection")]
        public static void PotionStatus(float ___health)
        {
            if (enableHealthStatus.Value)
                if (healthDebugWindow == null)
                    healthDebugWindow = Helper.CreateDebugWindow("dialog_health_status", positionHealthDebugWindow);
            if (enableVortexStatus.Value)
                if (vortexDebugWindow == null)
                    vortexDebugWindow = Helper.CreateDebugWindow("dialog_vortex_status", positionVortexDebugWindow);
            if (enableStirStatus.Value)
                if (stirDebugWindow == null)
                    stirDebugWindow = Helper.CreateDebugWindow("dialog_stir_status", positionStirDebugWindow);
            if (enablePositionStatus.Value)
                if (positionDebugWindow == null)
                    positionDebugWindow = Helper.CreateDebugWindow("dialog_position_status", positionPositionDebugWindow);
            if (enableDeviationStatus.Value)
                if (deviationDebugWindow == null)
                    deviationDebugWindow = Helper.CreateDebugWindow("dialog_deviation_status", positionDeviationDebugWindow);
            if (enableClosestPathStatus.Value)
                if (closestPathDebugWindow == null)
                    closestPathDebugWindow = Helper.CreateDebugWindow("dialog_path_status", positionClosestPathDebugWindow);
            if (enableClosestLadleStatus.Value)
                if (closestLadleDebugWindow == null)
                    closestLadleDebugWindow = Helper.CreateDebugWindow("dialog_ladle_status", positionClosestLadleDebugWindow);

            healthDebugWindow?.ShowText(InfoCalc.HealthCalc(___health));
            vortexDebugWindow?.ShowText(InfoCalc.VortexCalc());
            stirDebugWindow?.ShowText(InfoCalc.StirCalc());
            if (positionDebugWindow != null || deviationDebugWindow != null)
            {
                var posdev = InfoCalc.PositionDeviationCalc();
                positionDebugWindow?.ShowText(posdev.Item1);
                deviationDebugWindow?.ShowText(posdev.Item2);
            }
            closestPathDebugWindow?.ShowText(InfoCalc.PathCalc());
            closestLadleDebugWindow?.ShowText(InfoCalc.LadleCalc());
            if (AlchAss.solventDirectionHint != null)
                Traverse.Create(AlchAss.solventDirectionHint).Method("OnPositionOnMapChanged", Array.Empty<object>()).GetValue();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RecipeBookRecipeBrewController), "BrewRecipe")]
        public static void BrewMore(ref int count, IRecipeBookPageContent recipePageContent)
        {
            if (enableBrewMore.Value)
                if (count > 1)
                {
                    if (Keyboard.current.xKey.isPressed)
                        InfoCalc.BrewRecipe(ref count, recipePageContent, 1);
                    else if (Keyboard.current.zKey.isPressed)
                        InfoCalc.BrewRecipe(ref count, recipePageContent, 0);
                }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SolventDirectionHint), "OnPositionOnMapChanged")]
        public static void DirectionLineUpdate(SolventDirectionHint __instance, SpriteRenderer ___spriteRenderer)
        {
            if (enableDirectionLine.Value)
            {
                if (solventDirectionHint == null)
                    solventDirectionHint = __instance;
                if (directionLine)
                {
                    if (spriteOld == null)
                        spriteOld = ___spriteRenderer.sprite;
                    Texture2D texture = new(1, 1);
                    texture.SetPixels(new Color[1] { new(0.75f, 0.1f, 0.1f, 0.75f) });
                    texture.Apply();
                    Sprite redLineSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1.0f, 0, SpriteMeshType.FullRect);
                    ___spriteRenderer.sprite = redLineSprite;
                    ___spriteRenderer.size = new Vector2(0.075f, 100f);
                    __instance.transform.localEulerAngles = new Vector3(0f, 0f, endDirection);
                }
                else if (spriteOld != null)
                    ___spriteRenderer.sprite = spriteOld;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(VortexMapItemCollider), "OnTriggerEnter2D")]
        public static void VortexDistanceEnter()
        {
            if (vortexEdgeControl)
                vortexEdgeOn = float.MinValue;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(VortexMapItemCollider), "OnTriggerExit2D")]
        public static void VortexDistanceExit()
        {
            if (vortexEdgeControl)
                vortexEdgeOn = float.MaxValue;
        }

        [HarmonyPatch(typeof(CursorManager), "UpdateDebugWindow")]
        [HarmonyPostfix]
        public static void UpdateDebugWindowPostfix(InteractiveItem ___hoveredInteractiveItem)
        {
            if (enableClosestPathStatus.Value || enableClosestLadleStatus.Value)
                if (Mouse.current.rightButton.wasPressedThisFrame)
                    hoveredItemName = ___hoveredInteractiveItem?.transform.name;
        }
    }
}
