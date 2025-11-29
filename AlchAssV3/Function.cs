using Newtonsoft.Json;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Ingredient;
using PotionCraft.ManagersSystem.TMP;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.UIElements.FloatingText;
using PotionCraft.Settings;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AlchAssV3
{
    internal class Function
    {
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
                Variable.DebugWindowPos[i] = new Vector3(windowConfig.positions[i].x, windowConfig.positions[i].y, 0f);
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
            var Window = DebugWindow.Init(Variable.DefaultWindowConfig.positions[index].tag, true);
            Variable.ActiveDebugWindows.Add(Window);
            Window.ToForeground();
            Window.transform.SetParent(room.transform, false);
            Window.transform.localPosition = Variable.DebugWindowPos[index];
            Window.transform.localScale *= (float)Variable.WindowScale.Value;
            Window.transform.Find("Maximized/Head").gameObject.SetActive(false);
            Variable.DebugWindows[index] = Window;
        }

        /// <summary>
        /// 生成弹窗消息
        /// </summary>
        public static void SpawnMessageText(string message)
        {
            var cursorPosition = Managers.Cursor.cursor.transform.position;
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
            if (Variable.PolarMode)
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
        /// 格式化月盐文本
        /// </summary>
        public static string FormatMoonSalt(float rotation)
        {
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
        /// 极坐标模式开关
        /// </summary>
        public static void UpdatePolarMode()
        {
            if (Variable.KeyPolarMode.Value.IsDown())
            {
                Variable.PolarMode = !Variable.PolarMode;
                SpawnMessageText($"极坐标模式{(Variable.PolarMode ? "已开启" : "已关闭")}");
            }
        }

        /// <summary>
        /// 渲染元素开关
        /// </summary>
        public static void UpdateEnables()
        {
            for (int i = 0; i < Variable.Enables.Length; i++)
                if (Variable.Keys[i].Value.IsDown())
                {
                    Variable.Enables[i] = !Variable.Enables[i];
                    SpawnMessageText($"{Variable.MessageText[i]}{(Variable.Enables[i] ? "已开启" : "已关闭")}");
                }
        }

        /// <summary>
        /// 派生渲染元素开关
        /// </summary>
        public static void UpdateDerivedEnables()
        {
            Variable.DerivedEnables[0] = Variable.Enables[0] && Variable.Enables[4];
            Variable.DerivedEnables[1] = Variable.Enables[1];
            Variable.DerivedEnables[2] = Variable.Enables[2];
            Variable.DerivedEnables[3] = Variable.Enables[3];
            Variable.DerivedEnables[4] = Variable.Enables[4];
            Variable.DerivedEnables[5] = Variable.Enables[5];
            Variable.DerivedEnables[6] = Variable.Enables[6];
            Variable.DerivedEnables[7] = Variable.Enables[7];
            Variable.DerivedEnables[8] = Variable.Enables[10];
            Variable.DerivedEnables[9] = Variable.Enables[4] && Variable.Enables[6];
            Variable.DerivedEnables[10] = Variable.Enables[1] && Variable.Enables[6];
            Variable.DerivedEnables[11] = Variable.Enables[4] && Variable.Enables[7];
            Variable.DerivedEnables[12] = Variable.Enables[1] && Variable.Enables[7];
            Variable.DerivedEnables[13] = Variable.Enables[4] && Variable.Enables[8];
            Variable.DerivedEnables[14] = Variable.Enables[1] && Variable.Enables[8];
            Variable.DerivedEnables[15] = Variable.Enables[5] && Variable.Enables[8];
            Variable.DerivedEnables[16] = Variable.Enables[4] && Variable.Enables[9];
        }

        /// <summary>
        /// 选择效果
        /// </summary>
        public static void UpdateSelectEffect(InteractiveItem item)
        {
            if (Variable.KeySelectEffect.Value.IsDown() && item != null)
            {
                var name = item.name;
                if (name == null)
                    return;
                Variable.TargetEffect = Managers.RecipeMap.currentMap.referencesContainer.potionEffectsOnMap.FirstOrDefault(item => item.name == name);
                if (Variable.TargetEffect != null)
                    SpawnMessageText($"已选中效果: {Variable.TargetEffect.Effect.GetLocalizedTitle()}");
                else
                    SpawnMessageText($"已取消选中效果");
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

            if (Variable.KeyNextVortex.Value.IsDown())
            {
                Variable.VortexIndex[mapindex]++;
                if (Variable.VortexIndex[mapindex] >= vortexList.Count)
                    Variable.VortexIndex[mapindex] = 0;
                SpawnMessageText($"已选择第 {Variable.VortexIndex[mapindex] + 1} 个漩涡");
            }

            if (Variable.KeyPrevVortex.Value.IsDown())
            {
                Variable.VortexIndex[mapindex]--;
                if (Variable.VortexIndex[mapindex] < 0)
                    Variable.VortexIndex[mapindex] = vortexList.Count - 1;
                SpawnMessageText($"已选择第 {Variable.VortexIndex[mapindex] + 1} 个漩涡");
            }

            if (Variable.KeyNoneVortex.Value.IsDown())
            {
                Variable.VortexIndex[mapindex] = -1;
                SpawnMessageText("已取消漩涡选择");
            }

            if (Variable.KeyNearVortex.Value.IsDown())
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
                SpawnMessageText($"已选择第 {Variable.VortexIndex[mapindex] + 1} 个漩涡");
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
