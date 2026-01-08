using Newtonsoft.Json;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Ingredient;
using PotionCraft.ManagersSystem.TMP;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.UIElements.FloatingText;
using PotionCraft.Settings;
using PotionCraft.LocalizationSystem;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using HarmonyLib;
using BepInEx.Configuration;
using System;
using PotionCraft.Core.ValueContainers;

namespace AlchAssV3
{
    public static class Function
    {
        #region Constants
        /// <summary>
        /// Retrieves the switch object and associated message key corresponding to the specified switch dictionary key.
        /// </summary>
        /// <param name="key">The key identifying the switch for which to retrieve information.</param>
        /// <param name="sw">When this method returns, contains the switch object associated with the specified key.</param>
        /// <param name="messageKey">When this method returns, contains the message key string associated with the specified switch.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the specified key does not correspond to a known switch.</exception>
        public static void GetSwitchInfo(Variable.SwitchDictionaryKey key,out Variable.BaseSwitch sw, out string messageKey)
        {
            switch (key)
            {
                case Variable.SwitchDictionaryKey.PathLine:
                    sw = Variable.SwitchPathLine;
                    messageKey = "AlchAssV3_message_path_line";
                    break;
                case Variable.SwitchDictionaryKey.LadleLine:
                    sw = Variable.SwitchLadleLine;
                    messageKey = "AlchAssV3_message_ladle_line";
                    break;
                case Variable.SwitchDictionaryKey.TargetLine:
                    sw = Variable.SwitchTargetLine;
                    messageKey = "AlchAssV3_message_target_line";
                    break;
                case Variable.SwitchDictionaryKey.VortexLine:
                    sw = Variable.SwitchVortexLine;
                    messageKey = "AlchAssV3_message_vortex_line";
                    break;
                case Variable.SwitchDictionaryKey.PathCurve:
                    sw = Variable.SwitchPathCurve;
                    messageKey = "AlchAssV3_message_path_curve";
                    break;
                case Variable.SwitchDictionaryKey.VortexCurve:
                    sw = Variable.SwitchVortexCurve;
                    messageKey = "AlchAssV3_message_vortex_curve";
                    break;
                case Variable.SwitchDictionaryKey.TargetRange:
                    sw = Variable.SwitchTargetRange;
                    messageKey = "AlchAssV3_message_target_range";
                    break;
                case Variable.SwitchDictionaryKey.VortexRange:
                    sw = Variable.SwitchVortexRange;
                    messageKey = "AlchAssV3_message_vortex_range";
                    break;
                case Variable.SwitchDictionaryKey.AreaTracking:
                    sw = Variable.SwitchAreaTracking;
                    messageKey = "AlchAssV3_message_area_tracking";
                    break;
                case Variable.SwitchDictionaryKey.SwampScaling:
                    sw = Variable.SwitchSwampScaling;
                    messageKey = "AlchAssV3_message_swamp_scaling";
                    break;
                case Variable.SwitchDictionaryKey.Transparency:
                    sw = Variable.SwitchTransparency;
                    messageKey = "AlchAssV3_message_transparency";
                    break;
                case Variable.SwitchDictionaryKey.PolarMode:
                    sw = Variable.SwitchPolarMode;
                    messageKey = "AlchAssV3_message_polar_mode";
                    break;
                case Variable.SwitchDictionaryKey.SaltDegreeMode:
                    sw = Variable.SwitchSaltDegreeMode;
                    messageKey = "AlchAssV3_message_salt_degree_mode";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(key), key, null);
                    break;
            }
        }
        #endregion

        #region 窗口与消息
        /// <summary>
        /// 加载调试窗口位置
        /// </summary>
        public static void LoadDebugWindowPos()
        {
            if (!File.Exists(Variable.WindowConfigPath))
            {
                var json = JsonConvert.SerializeObject(Variable.DefaultWindowConfig, Formatting.Indented);
                File.WriteAllText(Variable.WindowConfigPath, json);
            }
            var fileData = File.ReadAllText(Variable.WindowConfigPath);
            var windowConfig = JsonConvert.DeserializeObject<Variable.WindowConfigData>(fileData);
            for (int i = 0; i < Variable.DebugWindowPos.Length; i++)
                if (i < windowConfig.positions.Length)
                {
                    Variable.DebugWindowPos[i] = new Vector3(windowConfig.positions[i].x, windowConfig.positions[i].y, 0f);
                }
                else
                {
                    Variable.DebugWindowPos[i] = new Vector3(Variable.DefaultWindowConfig.positions[i].x, Variable.DefaultWindowConfig.positions[i].y, 0f);
                }
        }

        /// <summary>
        /// 保存调试窗口位置
        /// </summary>
        public static void SaveDebugWindowPos()
        {
            var windowConfig = new Variable.WindowConfigData
            {
                positions = new Variable.WindowConfigData.PositionData[Variable.DebugWindowPos.Length]
            };
            for (int i = 0; i < Variable.DebugWindowPos.Length; i++)
            {
                windowConfig.positions[i] = new Variable.WindowConfigData.PositionData
                {
                    tag = Variable.DefaultWindowConfig.positions[i].tag,
                    x = Variable.DebugWindows[i] != null ? Variable.DebugWindows[i].transform.position.x : Variable.DebugWindowPos[i].x,
                    y = Variable.DebugWindows[i] != null ? Variable.DebugWindows[i].transform.position.y : Variable.DebugWindowPos[i].y,
                };
            }
            var json = JsonConvert.SerializeObject(windowConfig, Formatting.Indented);
            File.WriteAllText(Variable.WindowConfigPath, json);
        }

        /// <summary>
        /// 生成调试窗口
        /// </summary>
        public static void InitDebugWindow(int index, Room room)
        {
            if (!Variable.EnableWindows[index].Value || Variable.DebugWindows[index] != null)
                return;
            var Window = DebugWindow.Init(LocalizationManager.GetText(Variable.DefaultWindowConfig.positions[index].tag), true);
            Variable.ActiveDebugWindows.Add(Window);
            Window.ToForeground();
            Window.transform.SetParent(room.transform, false);
            Window.transform.localPosition = Variable.DebugWindowPos[index];
            Window.transform.localScale *= (float)Variable.WindowScale.Value;
            if (index != 11)
                Window.transform.Find("Maximized/Head").gameObject.SetActive(false);
            Variable.DebugWindows[index] = Window;
        }

        /// <summary>
        /// 生成弹窗消息
        /// </summary>
        public static void SpawnMessageText(string message)
        {
            var cursorPosition = Managers.Cursor.cursor.transform.position;
            cursorPosition.x += UnityEngine.Random.Range(-3f, 3f);
            cursorPosition.y += UnityEngine.Random.Range(-3f, 3f);
            var commonAtlasName = Settings<TMPManagerSettings>.Asset.CommonAtlasName;
            var formattedText = $"<voffset=0.085em><size=81%><sprite=\"{commonAtlasName}\" name=\"SpeechBubble ExclamationMark Icon\"></size>\u202f{message}";
            var textContent = new CollectedFloatingText.FloatingTextContent(formattedText, CollectedFloatingText.FloatingTextContent.Type.Text, 0f);
            var ingredientManagerAsset = typeof(Settings<IngredientManagerSettings>).GetProperty("Asset", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            var collectedFloatingTextField = ingredientManagerAsset.GetType().GetProperty("CollectedFloatingText", BindingFlags.NonPublic | BindingFlags.Instance);
            var collectedFloatingText = collectedFloatingTextField.GetValue(ingredientManagerAsset) as CollectedFloatingText;
            CollectedFloatingText.SpawnNewText(collectedFloatingText.gameObject, cursorPosition, [textContent], Managers.Game.Cam.transform, false, false);
        }
        #endregion

        #region 格式化文本
        /// <summary>
        /// 格式化位置文本
        /// </summary>
        public static string FormatPosition(Vector2 position)
        {
            if (Variable.SwitchPolarMode.getState())
                return $"""
                    r: {position.magnitude}
                    θ: {Vector2.SignedAngle(Vector2.right, position)}°
                    """;
            return $"""
                x: {position.x}
                y: {position.y}
                """;
        }

        /// <summary>
        /// 格式化月旋转文本
        /// </summary>
        public static string FormatRotation(float rotation)
        {
            if (!Variable.SwitchSaltDegreeMode.getState()) return $"{rotation * 9f / 25f}°";
            if (rotation < 0)
                return $"<sprite=\"IconsAtlas\" name=\"MoonSalt\"> {-rotation}";
            return $"<sprite=\"IconsAtlas\" name=\"SunSalt\"> {rotation}";
        }

        /// <summary>
        /// 格式化血盐文本
        /// </summary>
        public static string FormatLifeSalt(double DangerDistance)
        {
            double salt = DangerDistance > 2.5 ? (DangerDistance - 2.5) * 100 : 0;
            return $"<sprite=\"IconsAtlas\" name=\"LifeSalt\"> {(float)salt}";
        }
        #endregion

        #region 快捷键功能
        /// <summary>
        /// Toggles the application's key mode between "Normal" and "Input" when the Backslash key is pressed.
        /// </summary>
        /// <remarks>This method should be called regularly, such as within an input or update loop, to
        /// detect and respond to the Backslash key press. The key mode is switched only if the current mode is either
        /// "Normal" or "Input"; other values are ignored.</remarks>
        public static void UpdateKeyMode()
        {
            if (new KeyboardShortcut(KeyCode.Backslash).IsDown())
            {
                switch (Variable.KeyMode)
                {
                    case "Normal":
                        Variable.KeyMode = "Input";
                        break;
                    case "Input":
                        Variable.KeyMode = "Normal";
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Processes keyboard input to update the floating-point input stream and auxiliary line directions when in
        /// input mode.
        /// </summary>
        /// <remarks>This method should be called regularly (such as once per frame) to capture and
        /// process user keyboard input for floating-point values. It interprets specific key presses to build numeric
        /// input, enqueue values, or modify auxiliary line directions. The method only processes input when the key
        /// mode is set to "Input".</remarks>
        public static void UpdateFloatInput()
        {
            
            if (Variable.KeyMode == "Input")
            {
                if (new KeyboardShortcut(KeyCode.Alpha0).IsDown())
                {
                    Variable.FloatInputStream += "0";
                }
                else if (new KeyboardShortcut(KeyCode.Alpha1).IsDown())
                {
                    Variable.FloatInputStream += "1";
                }
                else if (new KeyboardShortcut(KeyCode.Alpha2).IsDown())
                {
                    Variable.FloatInputStream += "2";
                }
                else if (new KeyboardShortcut(KeyCode.Alpha3).IsDown())
                {
                    Variable.FloatInputStream += "3";
                }
                else if (new KeyboardShortcut(KeyCode.Alpha4).IsDown())
                {
                    Variable.FloatInputStream += "4";
                }
                else if (new KeyboardShortcut(KeyCode.Alpha5).IsDown())
                {
                    Variable.FloatInputStream += "5";
                }
                else if (new KeyboardShortcut(KeyCode.Alpha6).IsDown())
                {
                    Variable.FloatInputStream += "6";
                }
                else if (new KeyboardShortcut(KeyCode.Alpha7).IsDown())
                {
                    Variable.FloatInputStream += "7";
                }
                else if (new KeyboardShortcut(KeyCode.Alpha8).IsDown())
                {
                    Variable.FloatInputStream += "8";
                }
                else if (new KeyboardShortcut(KeyCode.Alpha9).IsDown())
                {
                    Variable.FloatInputStream += "9";
                }
                else if (new KeyboardShortcut(KeyCode.Period).IsDown())
                {
                    Variable.FloatInputStream += ".";
                }
                else if (new KeyboardShortcut(KeyCode.Comma).IsDown())
                {
                    Variable.FloatInputStream += "-";
                }
                else if (new KeyboardShortcut(KeyCode.Semicolon).IsDown())
                {
                    Variable.FloatInput.Enqueue(Variable.IndicatorPosition.x);
                }
                else if (new KeyboardShortcut(KeyCode.Quote).IsDown())
                {
                    Variable.FloatInput.Enqueue(Variable.IndicatorPosition.y);
                }
                else if (new KeyboardShortcut(KeyCode.Backspace).IsDown())
                {
                    Variable.FloatInputStream = "";
                }
                else if (new KeyboardShortcut(KeyCode.LeftBracket).IsDown())
                {
                    Variable.FloatInput = [];
                    Variable.FloatInputStream = "";
                }
                else if (new KeyboardShortcut(KeyCode.RightBracket).IsDown())
                {
                    try
                    {
                        Variable.FloatInput.Enqueue(double.Parse(Variable.FloatInputStream));
                    }
                    catch (System.Exception) { }
                    finally
                    {
                        Variable.FloatInputStream = "";
                    }
                }
                else if (new KeyboardShortcut(KeyCode.Return).IsDown())
                {
                    if (Variable.FloatInput.Count >= 2)
                    {
                        int index = (int)Variable.FloatInput.Dequeue();
                        index = Mathf.Min(index, Variable.MaxAuxiliaryLines - 1);
                        index = Mathf.Max(index, 0);
                        Variable.AuxiliaryLineDirections[index] = 90 - Variable.FloatInput.Dequeue();
                    }
                }
                else if (new KeyboardShortcut(KeyCode.Return, KeyCode.RightControl).IsDown())
                {
                    if (Variable.FloatInput.Count >= 1)
                    {
                        int index = (int)Variable.FloatInput.Dequeue();
                        index = Mathf.Min(index, Variable.MaxAuxiliaryLines - 1);
                        index = Mathf.Max(index, 0);
                        Variable.AuxiliaryLineDirections[index] = double.NaN;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the state of all switch controls based on the current key input and key mode.
        /// </summary>
        /// <remarks>This method toggles the enabled or disabled state of each switch when its associated
        /// key shortcut is pressed and the key mode is set to "Normal". A message is displayed to indicate the new
        /// state of each switch. This method should be called in response to user input events where switch state
        /// changes are expected.</remarks>
        public static void UpdateSwitches()
        {
            foreach(Variable.SwitchDictionaryKey key in Enum.GetValues(typeof(Variable.SwitchDictionaryKey)))
            {
                if (Variable.KeyMode == "Normal")
                {
                    GetSwitchInfo(key, out var sw, out var messageKey);
                    //if (sw.Key.IsDown())
                    if (Variable.SwitchKeyShortcuts[key].Value.IsDown())
                    {
                        sw.state = !sw.state;
                        SpawnMessageText($"""{LocalizationManager.GetText(messageKey)}{(sw.getState() ? LocalizationManager.GetText("AlchAssV3_enabled") : LocalizationManager.GetText("AlchAssV3_disabled"))}""");
                    }
                }
            }
        }

        /// <summary>
        /// 选择效果
        /// </summary>
        public static void UpdateSelectEffect(InteractiveItem item)
        {
            if (Variable.KeyMode == "Normal" && Variable.SelectEffectKeyShortcut.Value.IsDown() && item != null )
            {
                var name = item.name;
                if (name == null)
                    return;
                Variable.TargetEffect = Managers.RecipeMap.currentMap.referencesContainer.potionEffectsOnMap.FirstOrDefault(item => item.name == name);
                if (Variable.TargetEffect != null)
                    SpawnMessageText($"""{LocalizationManager.GetText("AlchAssV3_popup_selected_effect")}{Variable.TargetEffect.Effect.GetLocalizedTitle()}""");
                else
                    SpawnMessageText($"""{LocalizationManager.GetText("AlchAssV3_subtitle_unselected_effect")}""");
            }
        }

        /// <summary>
        /// 选择漩涡
        /// </summary>
        public static void UpdateSelectVortex()
        {
            if (Variable.CurrentMapID == null || Variable.CurrentMapID == "Wine")
                return;
            var mapindex = Variable.CurrentMapID == "Water" ? 0 : 1;
            var vortexList = mapindex == 0 ? Variable.Vortex_Water : Variable.Vortex_Oil;

            if (Variable.KeyMode == "Normal" && Variable.NextVortexKeyShortcut.Value.IsDown())
            {
                Variable.VortexIndex[mapindex]++;
                if (Variable.VortexIndex[mapindex] >= vortexList.Count)
                    Variable.VortexIndex[mapindex] = 0;
                SpawnMessageText(string.Format(LocalizationManager.GetText("AlchAssV3_popup_selected_vortex"), Variable.VortexIndex[mapindex] + 1));
            }

            if (Variable.KeyMode == "Normal" && Variable.PrevVortexKeyShortcut.Value.IsDown())
            {
                Variable.VortexIndex[mapindex]--;
                if (Variable.VortexIndex[mapindex] < 0)
                    Variable.VortexIndex[mapindex] = vortexList.Count - 1;
                SpawnMessageText(string.Format(LocalizationManager.GetText("AlchAssV3_popup_selected_vortex"), Variable.VortexIndex[mapindex] + 1));
            }
            if (Variable.KeyMode == "Normal" && Variable.UnselectVortexKeyShortcut.Value.IsDown())
            {
                Variable.VortexIndex[mapindex] = -1;
                SpawnMessageText(LocalizationManager.GetText("AlchAssV3_popup_unselected_vortex"));
            }
            if (Variable.KeyMode == "Normal" && Variable.NearestVortexKeyShortcut.Value.IsDown())
            {
                var indicatorPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
                var nearestIndex = -1;
                var nearestDist = double.MaxValue;
                for (int i = 0; i < vortexList.Count; i++)
                {
                    var dx = indicatorPos.x - vortexList[i].x;
                    var dy = indicatorPos.y - vortexList[i].y;
                    var dist = dx * dx + dy * dy;
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestIndex = i;
                    }
                }
                Variable.VortexIndex[mapindex] = nearestIndex;
                SpawnMessageText(string.Format(LocalizationManager.GetText("AlchAssV3_popup_selected_vortex"), Variable.VortexIndex[mapindex] + 1));
            }
        }
        #endregion

        #region 加载数据集
        /// <summary>
        /// 读取二进制文件
        /// </summary>
        public static byte[] ReadBinaryFile(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(path);
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        /// <summary>
        /// 加载漩涡数据
        /// </summary>
        public static void LoadVortexFromBin(string path, out List<Variable.Vortex> list)
        {
            var data = ReadBinaryFile($"AlchAssV3.Bins.{path}.bin");
            var reader = new BinaryReader(new MemoryStream(data));
            var len = reader.ReadInt32();
            list = new List<Variable.Vortex>(len);
            for (var i = 0; i < len; i++)
                list.Add(new Variable.Vortex { x = reader.ReadDouble(), y = reader.ReadDouble(), r = reader.ReadDouble() });
        }

        /// <summary>
        /// 加载区域数据
        /// </summary>
        public static void LoadZoneFromBin(string path, out List<Variable.Shape> listShape, out List<Variable.Node> listNode)
        {
            var data = ReadBinaryFile($"AlchAssV3.Bins.{path}.bin");
            var reader = new BinaryReader(new MemoryStream(data));
            var len_lines = reader.ReadInt32();
            var len_arcs = reader.ReadInt32();
            var len_nodes = reader.ReadInt32();
            listShape = new List<Variable.Shape>(len_lines + len_arcs);
            listNode = new List<Variable.Node>(len_nodes);

            for (var i = 0; i < len_lines; i++)
            {
                var x1 = reader.ReadDouble();
                var y1 = reader.ReadDouble();
                var x2 = reader.ReadDouble();
                var y2 = reader.ReadDouble();
                listShape.Add(new Variable.Shape.Line(x1, y1, x2, y2));
            }
            for (var i = 0; i < len_arcs; i++)
            {
                var x = reader.ReadDouble();
                var y = reader.ReadDouble();
                var r = reader.ReadDouble();
                var start = reader.ReadDouble();
                var end = reader.ReadDouble();
                listShape.Add(new Variable.Shape.Arc(x, y, r, start, end));
            }

            for (var i = 0; i < len_nodes; i++)
            {
                var isLeaf = reader.ReadBoolean();
                reader.ReadBytes(7);
                var minX = reader.ReadDouble();
                var minY = reader.ReadDouble();
                var maxX = reader.ReadDouble();
                var maxY = reader.ReadDouble();

                if (isLeaf)
                {
                    var itemCount = reader.ReadInt32();
                    var items = new int[itemCount];
                    for (var j = 0; j < itemCount; j++)
                        items[j] = reader.ReadInt32();
                    listNode.Add(new Variable.Node.LeafNode(minX, minY, maxX, maxY, items));
                }
                else
                {
                    var left = reader.ReadInt32();
                    var right = reader.ReadInt32();
                    listNode.Add(new Variable.Node.InternalNode(minX, minY, maxX, maxY, left, right));
                }
            }
        }

        /// <summary>
        /// 加载二进制资源
        /// </summary>
        public static void LoadFromBin()
        {
            LoadVortexFromBin("Vortex_Water", out Variable.Vortex_Water);
            LoadVortexFromBin("Vortex_Oil", out Variable.Vortex_Oil);
            LoadZoneFromBin("Strong_Water", out Variable.Strong_Water, out Variable.Strong_Water_BVH);
            LoadZoneFromBin("Strong_Oil", out Variable.Strong_Oil, out Variable.Strong_Oil_BVH);
            LoadZoneFromBin("Strong_Wine", out Variable.Strong_Wine, out Variable.Strong_Wine_BVH);
            LoadZoneFromBin("Weak_Wine", out Variable.Weak_Wine, out Variable.Weak_Wine_BVH);
            LoadZoneFromBin("Heal_Wine", out Variable.Heal_Wine, out Variable.Heal_Wine_BVH);
            LoadZoneFromBin("Swamp_Oil", out Variable.Swamp_Oil, out Variable.Swamp_Oil_BVH);
        }
        #endregion
    }
}
