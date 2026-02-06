using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.InteractiveItem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlchAssV3
{
    public static class Function
    {
        #region 窗口相关
        /// <summary>
        /// 生成调试窗口
        /// </summary>
        public static void InitDebugWindow(int index, Room room)
        {
            var Window = DebugWindow.Init(LocalizationManager.GetText(Variable.WindowTags[index]), true);
            Window.ToForeground();
            Window.transform.SetParent(room.transform, false);
            Window.transform.localPosition = Variable.WindowPositions[index].Value;
            Window.transform.localScale *= Variable.WindowScale.Value;
            Variable.DebugWindows[index] = Window;
        }

        /// <summary>
        /// 恢复调试窗口
        /// </summary>
        public static void RestoreDebugWindows()
        {
            for (var i = 0; i < Variable.DebugWindows.Length; i++)
            {
                Variable.DebugWindows[i].Visible = true;
                Variable.DebugWindows[i].transform.localPosition = Variable.WindowPositions[i].Value;
            }
            Variable.WindowRect = Variable.WindowRectConfig.Value;
        }

        /// <summary>
        /// 保存调试窗口位置
        /// </summary>
        public static void SaveDebugWindowPos()
        {
            for (var i = 0; i < Variable.DebugWindows.Length; i++)
                Variable.WindowPositions[i].Value = Variable.DebugWindows[i].transform.localPosition;
            Variable.WindowRectConfig.Value = Variable.WindowRect;
        }

        public static void SetDebugWindowTitle()
        {
            for (var i = 0; i < Variable.DebugWindows.Length; i++)
                Variable.DebugWindows[i]?.captionText.text = LocalizationManager.GetText($"{Variable.WindowTags[i]}信息");
        }
        #endregion

        #region 快捷键功能
        /// <summary>
        /// 控制面板开关
        /// </summary>
        public static void UpdateWindow()
        {
            if (Variable.KeyWindow.Value.IsDown())
                Variable.ShowWindow = !Variable.ShowWindow;
            if (Variable.ShowWindow)
            {
                Cursor.visible = true;
                UIWindow.Resizing();
            }
        }

        /// <summary>
        /// 更新选择效果
        /// </summary>
        public static void UpdateSelectEffect(InteractiveItem item)
        {
            if (Variable.KeyEffect.Value.IsPressed() && Mouse.current.rightButton.wasPressedThisFrame)
            {
                var name = item.name;
                if (name == null)
                    return;
                Variable.TargetEffect = Managers.RecipeMap.currentMap.referencesContainer.potionEffectsOnMap.FirstOrDefault(item => item.name == name);
            }
        }

        /// <summary>
        /// 更新选择漩涡
        /// </summary>
        public static void UpdateSelectVortex()
        {
            if (Managers.RecipeMap?.currentMap == null || Managers.RecipeMap.currentMap.potionBase.name == "Wine")
                return;

            var mapid = Variable.MapId[Managers.RecipeMap.currentMap.potionBase.name];
            if (Variable.KeyVortex.Value.IsPressed() && Mouse.current.rightButton.wasPressedThisFrame)
            {
                var mousePos = Managers.Cursor.cursor.transform.position;
                if (!Managers.RecipeMap.recipeMapObject.visibilityZoneCollider.OverlapPoint(mousePos))
                    return;

                var worldPos = Managers.RecipeMap.recipeMapObject.transmitterWindow.ViewToCamera(mousePos);
                var mapPos = Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(worldPos);
                Variable.VortexIndex[mapid] = -1;
                for (var i = 0; i < Variable.Vortexs[mapid].Count; i++)
                {
                    var vortex = Variable.Vortexs[mapid][i];
                    var dx = mapPos.x - vortex.x;
                    var dy = mapPos.y - vortex.y;
                    if (dx * dx + dy * dy <= vortex.r * vortex.r)
                    {
                        Variable.VortexIndex[mapid] = i;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 更新自定义直线
        /// </summary>
        public static void UpdateCustomLines()
        {
            if (Variable.KeyCustom.Value.IsPressed() && Variable.DoCustomLine)
            {
                var mousePos = Managers.Cursor.cursor.transform.position;
                if (!Managers.RecipeMap.recipeMapObject.visibilityZoneCollider.OverlapPoint(mousePos))
                    return;

                var worldPos = Managers.RecipeMap.recipeMapObject.transmitterWindow.ViewToCamera(mousePos);
                var mapPos = Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(worldPos);
                var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
                var delta = mapPos - indPos;

                if (Mouse.current.middleButton.isPressed)
                {
                    if (Variable.TargetLineIndex < 0)
                        return;

                    var dir = Vector2.SignedAngle(Vector2.right, delta);
                    dir = dir < 0 ? dir + 360 : dir;
                    Variable.CustomLineDirections[Variable.TargetLineIndex] = dir;
                    Variable.Inputs[Variable.TargetLineIndex] = (dir.ToString(), false);
                }
                else
                {
                    var target = -1;
                    var minDis = double.MaxValue;
                    for (var i = 0; i < Variable.CustomLineDirections.Count; i++)
                    {
                        var theta = Variable.CustomLineDirections[i] / 180 * Math.PI;
                        var dis = Math.Abs(delta.x * Math.Sin(theta) - delta.y * Math.Cos(theta));
                        if (dis < minDis)
                        {
                            minDis = dis;
                            target = i;
                        }
                    }
                    if (minDis < 0.3)
                        Variable.TargetLineIndex = target;
                    else
                        Variable.TargetLineIndex = -1;

                    if (Mouse.current.rightButton.wasPressedThisFrame)
                    {
                        if (Variable.TargetLineIndex < 0)
                        {
                            var dir = Vector2.SignedAngle(Vector2.right, delta);
                            dir = dir < 0 ? dir + 360 : dir;
                            Variable.CustomLineDirections.Add(dir);
                            Variable.CustomLineHovers.Add(false);
                            Variable.Inputs.Add((dir.ToString(), false));
                            Variable.TargetLineIndex = Variable.CustomLineDirections.Count - 1;
                        }
                        else
                        {
                            Variable.CustomLineDirections.RemoveAt(Variable.TargetLineIndex);
                            Variable.CustomLineHovers.RemoveAt(Variable.TargetLineIndex);
                            Variable.Inputs.RemoveAt(Variable.TargetLineIndex);
                            Variable.TargetLineIndex = -1;
                        }
                    }
                }
            }
            else
                Variable.TargetLineIndex = -1;
        }
        #endregion

        #region 功能开关
        /// <summary>
        /// 渲染元素开关
        /// </summary>
        public static void UpdateDoFromEnable()
        {
            Variable.DoCustomLine = Variable.EnableCustomLine;
            Variable.DoPathCurve = Variable.EnablePathCurve;
            Variable.DoVortexCurve = Variable.EnableVortexCurve;
            Variable.DoEffectRange = Variable.EnableEffectRange;
            Variable.DoVortexRange = Variable.EnableVortexRange;
            Variable.DoTransparency = Variable.EnableTransparency;
            Variable.DoPathEffectPoint = Variable.EnablePathCurve && Variable.EnableEffectRange;
            Variable.DoLadleEffectPoint = Variable.EnableLadleLine && Variable.EnableEffectRange;
            Variable.DoPathVortexPoint = Variable.EnablePathCurve && Variable.EnableVortexRange;
            Variable.DoLadleVortexPoint = Variable.EnableLadleLine && Variable.EnableVortexRange;
            Variable.DoPathDangerPoint = Variable.EnablePathCurve && Variable.EnableDangerSimulation;
            Variable.DoLadleDangerPoint = Variable.EnableLadleLine && Variable.EnableDangerSimulation;
            Variable.DoVortexDangerPoint = Variable.EnableVortexCurve && Variable.EnableDangerSimulation;
            Variable.DoSwampPoint = Variable.EnablePathCurve && Variable.EnableSwampSimulation;
            Variable.DoLines[0] = Variable.EnablePathLine && Variable.EnablePathCurve;
            Variable.DoLines[1] = Variable.EnableLadleLine;
            Variable.DoLines[2] = Variable.EnableEffectLine;
            Variable.DoLines[3] = Variable.EnableVortexLine;
            Variable.DoLines[4] = Variable.EnableTangentLine && Variable.EnableVortexCurve;
        }
        #endregion

        #region 加载数据
        /// <summary>
        /// 读取二进制文件
        /// </summary>
        public static byte[] ReadBinaryFile(string path)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
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
        public static void LoadFromBins()
        {
            LoadVortexFromBin("Vortex_Water", out Variable.Vortexs[0]);
            LoadVortexFromBin("Vortex_Oil", out Variable.Vortexs[1]);
            LoadZoneFromBin("Strong_Water", out Variable.Strongs[0], out Variable.StrongBVHs[0]);
            LoadZoneFromBin("Strong_Oil", out Variable.Strongs[1], out Variable.StrongBVHs[1]);
            LoadZoneFromBin("Strong_Wine", out Variable.Strongs[2], out Variable.StrongBVHs[2]);
            LoadZoneFromBin("Weak_Wine", out Variable.WeakWine, out Variable.WeakWineBVH);
            LoadZoneFromBin("Heal_Wine", out Variable.HealWine, out Variable.HealWineBVH);
            LoadZoneFromBin("Swamp_Oil", out Variable.SwampOil, out Variable.SwampOilBVH);
        }
        #endregion

        #region 格式化文本
        /// <summary>
        /// 格式化位置文本
        /// </summary>
        public static string FormatPosition(Vector2 position)
        {
            if (Variable.DisplayPolar)
                return $"{(position.magnitude, Vector2.SignedAngle(Vector2.right, position))}";
            return $"{(position.x, position.y)}";
        }

        /// <summary>
        /// 格式化月盐文本
        /// </summary>
        public static string FormatMoonSalt(float rotation)
        {
            var angle = Mathf.DeltaAngle(rotation, 0f);
            if (!Variable.DisplaySalt)
                return $"{angle}°";
            if (angle < 0)
                return $"<sprite=\"IconsAtlas\" name=\"MoonSalt\"> {-angle / 9f * 25f}";
            return $"<sprite=\"IconsAtlas\" name=\"SunSalt\"> {angle / 9f * 25f}";
        }

        /// <summary>
        /// 格式化血盐文本
        /// </summary>
        public static string FormatLifeSalt(double DangerDistance)
        {
            var hp = (float)DangerDistance * 40f - 100f;
            if (!Variable.DisplaySalt)
                return $"{hp}%";
            return $"<sprite=\"IconsAtlas\" name=\"LifeSalt\"> {hp * 2.5f}";
        }

        /// <summary>
        /// 格式化本地化文本
        /// </summary>
        public static void FormatLocalization()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string[] tags = ["Label", "Title", "Button"];
            foreach (var tag in tags)
                Localization.RegisterLocalization($"AlchAssV3.Locs.{tag}.json", assembly);
        }
        #endregion
    }
}
