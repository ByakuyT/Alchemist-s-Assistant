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
//药水瓶半径0.74，效果半径0.79
namespace AlchAss
{
    [BepInPlugin("AlchAss", "Alchemist's Assistant", "3.0.0")]
    public class AlchAss : BaseUnityPlugin
    {
        public static ConfigEntry<bool> enableGrindStatus;
        public static ConfigEntry<bool> enableHealthStatus;
        public static ConfigEntry<bool> enableVortexStatus;
        public static ConfigEntry<bool> enableStirStatus;
        public static ConfigEntry<bool> enableZoneStatus;
        public static ConfigEntry<bool> enablePositionStatus;
        public static ConfigEntry<bool> enableTargetStatus;
        public static ConfigEntry<bool> enableDeviationStatus;
        public static ConfigEntry<bool> enablePathStatus;
        public static ConfigEntry<bool> enableLadleStatus;
        public static ConfigEntry<bool> enableTooltip;
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
        public static DebugWindow zoneDebugWindow;
        public static DebugWindow positionDebugWindow;
        public static DebugWindow targetDebugWindow;
        public static DebugWindow deviationDebugWindow;
        public static DebugWindow pathDebugWindow;
        public static DebugWindow ladleDebugWindow;
        public static DebugWindow tooltipDebugWindow;
        public static Vector2 positionGrindDebugWindow = Vector2.zero;
        public static Vector2 positionHealthDebugWindow = Vector2.zero;
        public static Vector2 positionVortexDebugWindow = Vector2.zero;
        public static Vector2 positionStirDebugWindow = Vector2.zero;
        public static Vector2 positionZoneDebugWindow = Vector2.zero;
        public static Vector2 positionPositionDebugWindow = Vector2.zero;
        public static Vector2 positionDeviationDebugWindow = Vector2.zero;
        public static Vector2 positionPathDebugWindow = Vector2.zero;
        public static Vector2 positionLadleDebugWindow = Vector2.zero;
        public static Vector2 positionTargetDebugWindow = Vector2.zero;
        public static Vector2 positionTooltipDebugWindow = Vector2.zero;
        public static bool directionLine = false;
        public static bool vortexEdgeControl = false;
        public static bool endMode = false;
        public static bool xOy = false;
        public static float vortexEdgeOn = float.MaxValue;
        public static float[] zoneLen = new float[4];
        public static Vector3 prePost = Vector3.zero;
        public static string hoveredItemName = "";
        public static Room lab = null;
        public static Texture2D texture = null;
        public static SpriteRenderer[] lineRenderer = new SpriteRenderer[3];
        public static Color[] lineColor = { new(0.75f, 0.1f, 0.1f, 0.75f), new(0.1f, 0.75f, 0.1f, 0.75f), new(0.1f, 0.1f, 0.75f, 0.75f) };
        public static float[] lineDirection = new float[3];
        public static SolventDirectionHint solventDirectionHint = null;
        public static readonly List<DebugWindow> foreground_queue = new();
        public static readonly string grindHeatPath = "AlchAssGrindHeatConfig.txt";
        public static readonly string speedBrewPath = "AlchAssSpeedBrewConfig.txt";
        public static readonly string windowPath = "AlchAssWindowConfig.txt";
        public void Awake()
        {
            enableGrindStatus = Config.Bind("信息窗口", "研磨信息", true, "开启后，显示研磨进度.");
            enableHealthStatus = Config.Bind("信息窗口", "血量信息", true, "开启后，显示血量.");
            enableVortexStatus = Config.Bind("信息窗口", "漩涡信息", true, "开启后，显示当前位置到所接触的漩涡中心距离和可以接触漩涡的最大距离、加水方向与漩涡方向的夹角和可以加水外拉的最大夹角.");
            enablePositionStatus = Config.Bind("信息窗口", "位置信息", true, "开启后，显示直角坐标或极坐标的位置和旋转量.");
            enableStirStatus = Config.Bind("信息窗口", "移动信息", true, "开启后，显示搅拌阶段、进度、方向和加水方向.");
            enableZoneStatus = Config.Bind("信息窗口", "区域信息", true, "开启后，显示通过沼泽、骷髅、碎骨和回复区域的总长，读取存档时重置.");
            enableTargetStatus = Config.Bind("信息窗口", "目标信息", true, "开启后，显示目标效果的名称、直角坐标或极坐标的位置、旋转量和目标方向.");
            enableDeviationStatus = Config.Bind("信息窗口", "偏离信息", true, "需要开启目标信息\n开启后，显示目标效果与当前位置的偏差度.");
            enablePathStatus = Config.Bind("信息窗口", "路径信息", true, "需要开启目标信息\n开启后，显示目标效果与路径最近点或搅拌末端的偏差度、目标方向与当前搅拌方向的夹角.");
            enableLadleStatus = Config.Bind("信息窗口", "加水信息", true, "需要开启目标信息\n开启后，显示目标效果与加水最近点的偏差度、目标方向与当前加水方向的夹角.");
            enableTooltip = Config.Bind("信息窗口", "键位表", true, "开启后，显示所有附加操作的键位表.");
            windowScale = Config.Bind("信息窗口", "窗口缩放", 1f, "信息窗口显示的大小比例.");
            enableShuttingDown = Config.Bind("操作控制", "允许定量加热", true, "开启后，右键点击风箱把手将使药剂热量值变为指定值.");
            enableGrindAll = Config.Bind("操作控制", "允许定量研磨", true, "开启后，右键点击研杵把手将使药材研磨度变为指定值.");
            enableVortexEdge = Config.Bind("操作控制", "允许漩涡制动", true, "开启后，按下 ' 键后药水瓶即将离开漩涡边缘时将逐渐减速搅拌和加水直到停止.");
            enableGrindSpeed = Config.Bind("操作控制", "允许研磨减速", true, "开启后，按住 Z 或 X 键将使研磨减速至相应指定比例.");
            enableStirSpeed = Config.Bind("操作控制", "允许搅拌减速", true, "开启后，按住 Z 或 X 键将使搅拌减速至相应指定比例.");
            enableLadleSpeed = Config.Bind("操作控制", "允许加水减速", true, "开启后，按住 Z 或 X 键将使加水减速至相应指定比例.");
            enableHeatSpeed = Config.Bind("操作控制", "允许加热减速", true, "开启后，按住 Z 或 X 键将使加热减速至相应指定比例.");
            enableBrewMore = Config.Bind("操作控制", "允许大批炼药", true, "开启后，按住 Z 或 X 键将使炼药数量增加至相应指定比例.");
            enableDirectionLine = Config.Bind("操作控制", "允许辅助示线", true, "开启后，按下 / 键显示当前搅拌方向、加水方向和目标方向的提示线.");
            if (!enableTargetStatus.Value)
                enableDeviationStatus.Value = enablePathStatus.Value = enableLadleStatus.Value = false;
            if (!File.Exists(Path.Combine(Paths.PluginPath, grindHeatPath)))
                File.WriteAllText(Path.Combine(Paths.PluginPath, grindHeatPath), "100");
            if (!File.Exists(Path.Combine(Paths.PluginPath, speedBrewPath)))
                File.WriteAllText(Path.Combine(Paths.PluginPath, speedBrewPath), "10\n100");
            if (!File.Exists(Path.Combine(Paths.PluginPath, windowPath)))
                File.WriteAllText(Path.Combine(Paths.PluginPath, windowPath), null);
            var lines = File.ReadAllLines(Path.Combine(Paths.PluginPath, windowPath));
            if (lines.Length > 10)
            {
                positionGrindDebugWindow = Helper.StringToVector2(lines[0]);
                positionHealthDebugWindow = Helper.StringToVector2(lines[1]);
                positionVortexDebugWindow = Helper.StringToVector2(lines[2]);
                positionStirDebugWindow = Helper.StringToVector2(lines[3]);
                positionPositionDebugWindow = Helper.StringToVector2(lines[4]);
                positionDeviationDebugWindow = Helper.StringToVector2(lines[5]);
                positionPathDebugWindow = Helper.StringToVector2(lines[6]);
                positionLadleDebugWindow = Helper.StringToVector2(lines[7]);
                positionTargetDebugWindow = Helper.StringToVector2(lines[8]);
                positionTooltipDebugWindow = Helper.StringToVector2(lines[9]);
                positionZoneDebugWindow = Helper.StringToVector2(lines[10]);
            }
            LocalizationManager.OnInitialize.AddListener(Helper.SetModLocalization);
            Harmony.CreateAndPatchAll(typeof(AlchAss));
            Logger.LogInfo("Alchemist's Assistant 加载完成！");
        }
        public void Update()
        {
            if (enableDirectionLine.Value)
                Controler.DirectionLine();
            if (enableShuttingDown.Value)
                Controler.CoolDown();
            if (enableGrindAll.Value)
                Controler.GrindAll();
            if (enableVortexEdge.Value)
                Controler.VortexEdge();
            if (enablePathStatus.Value)
                Controler.EndMode();
            if (enablePositionStatus.Value)
                Controler.PositionMode();
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
            writer.WriteLine(pathDebugWindow != null ? pathDebugWindow.transform.position : "");
            writer.WriteLine(ladleDebugWindow != null ? ladleDebugWindow.transform.position : "");
            writer.WriteLine(targetDebugWindow != null ? targetDebugWindow.transform.position : "");
            writer.WriteLine(tooltipDebugWindow != null ? tooltipDebugWindow.transform.position : "");
            writer.WriteLine(zoneDebugWindow != null ? zoneDebugWindow.transform.position : "");
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveLoadManager), "LoadProgressState")]
        public static void ResetZoneLen()
        {
            Array.Clear(zoneLen, 0, zoneLen.Length);
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
                    Controler.GrindSlowDown(ref pestleLinearSpeed, ref pestleAngularSpeed, 1);
                else if (Keyboard.current.zKey.isPressed)
                    Controler.GrindSlowDown(ref pestleLinearSpeed, ref pestleAngularSpeed, 0);
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Cauldron), "UpdateStirringValue")]
        public static void StirSlowDown(ref float ___StirringValue)
        {
            if (enableStirSpeed.Value)
            {
                if (Keyboard.current.xKey.isPressed)
                    Controler.StirSlowDown(ref ___StirringValue, 1);
                else if (Keyboard.current.zKey.isPressed)
                    Controler.StirSlowDown(ref ___StirringValue, 0);
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
                    Controler.LadleSlowDown(ref __result, 1);
                else if (Keyboard.current.zKey.isPressed)
                    Controler.LadleSlowDown(ref __result, 0);
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
                    Controler.HeatSlowDown(ref __state, 1);
                else if (Keyboard.current.zKey.isPressed)
                    Controler.HeatSlowDown(ref __state, 0);
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
            if (enablePathStatus.Value)
                if (pathDebugWindow == null)
                    pathDebugWindow = Helper.CreateDebugWindow("dialog_path_status", positionPathDebugWindow);
            if (enableLadleStatus.Value)
                if (ladleDebugWindow == null)
                    ladleDebugWindow = Helper.CreateDebugWindow("dialog_ladle_status", positionLadleDebugWindow);
            if (enableTargetStatus.Value)
                if (targetDebugWindow == null)
                    targetDebugWindow = Helper.CreateDebugWindow("dialog_target_status", positionTargetDebugWindow);
            if (enableZoneStatus.Value)
                if (zoneDebugWindow == null)
                    zoneDebugWindow = Helper.CreateDebugWindow("dialog_zone_status", positionZoneDebugWindow);
            if (enableTooltip.Value)
                if (tooltipDebugWindow == null)
                    tooltipDebugWindow = Helper.CreateDebugWindow("dialog_tooltip", positionTooltipDebugWindow);
            healthDebugWindow?.ShowText(InfoCalc.HealthCalc(___health));
            vortexDebugWindow?.ShowText(InfoCalc.VortexCalc());
            stirDebugWindow?.ShowText(InfoCalc.MoveCalc());
            positionDebugWindow?.ShowText(InfoCalc.PositionCalc());
            targetDebugWindow?.ShowText(InfoCalc.TargetCalc());
            deviationDebugWindow?.ShowText(InfoCalc.DeviationCalc());
            pathDebugWindow?.ShowText(InfoCalc.PathCalc());
            ladleDebugWindow?.ShowText(InfoCalc.LadleCalc());
            zoneDebugWindow?.ShowText(InfoCalc.ZoneCalc());
            tooltipDebugWindow?.ShowText(LocalizationManager.GetText("tooltip"));
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
                        Controler.BrewRecipe(ref count, recipePageContent, 1);
                    else if (Keyboard.current.zKey.isPressed)
                        Controler.BrewRecipe(ref count, recipePageContent, 0);
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
                if (texture == null)
                {
                    texture = new(1, 1);
                    texture.SetPixels(new Color[1] { Color.white });
                    texture.Apply();
                }
                for (int i = 0; i < 3; i++)
                    if (lineRenderer[i] == null)
                    {
                        var newLine = new GameObject("SecondLine")
                        {
                            layer = __instance.gameObject.layer
                        };
                        lineRenderer[i] = newLine.AddComponent<SpriteRenderer>();
                        lineRenderer[i].sortingLayerName = ___spriteRenderer.sortingLayerName;
                        lineRenderer[i].sortingOrder = ___spriteRenderer.sortingOrder;
                        lineRenderer[i].drawMode = SpriteDrawMode.Tiled;
                        lineRenderer[i].sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1.0f, 0, SpriteMeshType.FullRect);
                        lineRenderer[i].color = lineColor[i];
                        lineRenderer[i].size = new Vector2(200f, 0.075f);
                    }
                if (directionLine)
                {
                    ___spriteRenderer.enabled = false;
                    for (int i = 0; i < 3; i++)
                    {
                        lineRenderer[i].enabled = true;
                        lineRenderer[i].transform.position = __instance.transform.position;
                        lineRenderer[i].transform.localEulerAngles = new Vector3(0f, 0f, lineDirection[i]);
                    }
                }
                else
                {
                    ___spriteRenderer.enabled = true;
                    for (int i = 0; i < 3; i++)
                        lineRenderer[i].enabled = false;
                }
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
            if (enableTargetStatus.Value)
                if (Mouse.current.rightButton.wasPressedThisFrame)
                    hoveredItemName = ___hoveredInteractiveItem?.transform.name;
        }
    }
}
