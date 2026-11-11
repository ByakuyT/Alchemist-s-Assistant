using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.InteractiveItem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlchAssV4;

public static class Function
{
    #region 快捷键功能

    /// <summary>
    /// 更新控制面板
    /// </summary>
    public static void UpdateMenu()
    {
        if (Variable.KeyMenu.Value.IsDown())
            Variable.ShowMenu = !Variable.ShowMenu;
        if (Variable.ShowMenu)
        {
            Cursor.visible = true;
            UIWindow.ResizeMenu();
        }
    }

    /// <summary>
    /// 更新选择效果
    /// </summary>
    public static void UpdateSelectEffect(InteractiveItem item)
    {
        if (Variable.KeyEffect.Value.IsPressed() && Mouse.current.rightButton.wasPressedThisFrame)
            Variable.TargetEffect = Managers.RecipeMap.currentMap.referencesContainer.potionEffectsOnMap.FirstOrDefault(x => x.name == item.name);
    }

    /// <summary>
    /// 更新选择漩涡
    /// </summary>
    public static void UpdateSelectVortex()
    {
        if (Managers.RecipeMap?.currentMap == null) return;
        var mapname = Managers.RecipeMap.currentMap.potionBase.name;
        if (mapname == "Wine") return;
        var mapid = Variable.MapId[mapname];
        if (Variable.KeyVortex.Value.IsPressed() && Mouse.current.rightButton.wasPressedThisFrame)
        {
            var mousePos = Managers.Cursor.cursor.transform.position;
            if (!Managers.RecipeMap.recipeMapObject.visibilityZoneCollider.OverlapPoint(mousePos)) return;
            var worldPos = Managers.RecipeMap.recipeMapObject.transmitterWindow.ViewToCamera(mousePos);
            var mapPos = Variable.Vec2D.From(Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(worldPos));
            Variable.VortexIndex[mapid] = -1;
            for (var i = 0; i < Variable.Vortices[mapid].Count; i++)
            {
                var vortex = Variable.Vortices[mapid][i];
                if ((mapPos - vortex.Pos).SqrMagnitude <= vortex.Rad * vortex.Rad)
                {
                    Variable.VortexIndex[mapid] = i;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 更新自定义方向线
    /// </summary>
    public static void UpdateCustomLines()
    {
        if (Variable.KeyCustomLine.Value.IsPressed() && Variable.DoCustomLine)
        {
            var mousePos = Managers.Cursor.cursor.transform.position;
            if (!Managers.RecipeMap.recipeMapObject.visibilityZoneCollider.OverlapPoint(mousePos))
            {
                Variable.TargetLineIndex = -1;
                return;
            }

            var worldPos = Managers.RecipeMap.recipeMapObject.transmitterWindow.ViewToCamera(mousePos);
            var mapPos = Variable.Vec2D.From(Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(worldPos));
            var delta = mapPos - Variable.Offset - Variable.Vec2D.From(Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition);
            var dir = (float)Variable.Vec2D.Right.SignedAngle(delta);
            if (Mouse.current.middleButton.isPressed)
            {
                if (Variable.TargetLineIndex < 0) return;
                Variable.CustomLineDirections[Variable.TargetLineIndex] = dir;
                Variable.CustomLineInputs[Variable.TargetLineIndex] = ($"{dir}", false);
            }
            else
            {
                var target = -1;
                var minDis = double.PositiveInfinity;
                for (var i = 0; i < Variable.CustomLineDirections.Count; i++)
                {
                    var theta = Variable.CustomLineDirections[i] / 180 * Math.PI;
                    var dis = Math.Abs(delta.X * Math.Sin(theta) - delta.Y * Math.Cos(theta));
                    if (dis < minDis)
                    {
                        minDis = dis;
                        target = i;
                    }
                }

                Variable.TargetLineIndex = minDis < 0.3 ? target : -1;
                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    if (Variable.TargetLineIndex < 0)
                    {
                        Variable.CustomLineDirections.Add(dir);
                        Variable.CustomLineHovers.Add(false);
                        Variable.CustomLineInputs.Add(($"{dir}", false));
                        Variable.TargetLineIndex = Variable.CustomLineDirections.Count - 1;
                    }
                    else
                    {
                        Variable.CustomLineDirections.RemoveAt(Variable.TargetLineIndex);
                        Variable.CustomLineHovers.RemoveAt(Variable.TargetLineIndex);
                        Variable.CustomLineInputs.RemoveAt(Variable.TargetLineIndex);
                        Variable.TargetLineIndex = -1;
                    }

                    Localization.UpdateCustomLineLabelWidth();
                }
            }
        }
        else
            Variable.TargetLineIndex = -1;
    }

    /// <summary>
    /// 更新自定义标记点
    /// </summary>
    public static void UpdateCustomMarkers()
    {
        if (Variable.KeyCustomMarker.Value.IsPressed() && Variable.DoCustomMarker)
        {
            var mousePos = Managers.Cursor.cursor.transform.position;
            if (!Managers.RecipeMap.recipeMapObject.visibilityZoneCollider.OverlapPoint(mousePos))
            {
                Variable.TargetMarkerIndex = -1;
                return;
            }

            var worldPos = Managers.RecipeMap.recipeMapObject.transmitterWindow.ViewToCamera(mousePos);
            var mapPosV = Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(worldPos);
            var mapPos = new Variable.Vec2D(Algebra.Clamp(mapPosV.x, -80, 80), Algebra.Clamp(mapPosV.y, -80, 80));
            var storagePos = Variable.DisplayPolar ? Algebra.ToPolar(mapPos) : mapPos;
            if (Mouse.current.middleButton.isPressed)
            {
                if (Variable.TargetMarkerIndex < 0) return;
                Variable.CustomMarkerPositions[Variable.TargetMarkerIndex] = storagePos;
                Variable.CustomMarkerInputs[Variable.TargetMarkerIndex] = ($"{storagePos.X}", $"{storagePos.Y}", false, false);
            }
            else
            {
                var target = -1;
                var minDis = double.PositiveInfinity;
                for (var i = 0; i < Variable.CustomMarkerPositions.Count; i++)
                {
                    var dis = mapPos.Distance(Variable.DisplayPolar ? Algebra.FromPolar(Variable.CustomMarkerPositions[i]) : Variable.CustomMarkerPositions[i]);
                    if (dis < minDis)
                    {
                        minDis = dis;
                        target = i;
                    }
                }

                Variable.TargetMarkerIndex = minDis < 0.3 ? target : -1;
                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    if (Variable.TargetMarkerIndex < 0)
                    {
                        Variable.CustomMarkerPositions.Add(storagePos);
                        Variable.CustomMarkerHovers.Add(false);
                        Variable.CustomMarkerInputs.Add(($"{storagePos.X}", $"{storagePos.Y}", false, false));
                        Variable.TargetMarkerIndex = Variable.CustomMarkerPositions.Count - 1;
                    }
                    else
                    {
                        Variable.CustomMarkerPositions.RemoveAt(Variable.TargetMarkerIndex);
                        Variable.CustomMarkerHovers.RemoveAt(Variable.TargetMarkerIndex);
                        Variable.CustomMarkerInputs.RemoveAt(Variable.TargetMarkerIndex);
                        Variable.TargetMarkerIndex = -1;
                    }

                    Localization.UpdateCustomMarkerLabelWidth();
                }
            }
        }
        else
            Variable.TargetMarkerIndex = -1;
    }

    /// <summary>
    /// 更新自定义标记点存储格式
    /// </summary>
    public static void UpdateCustomMarkerPositionType()
    {
        for (var i = 0; i < Variable.CustomMarkerPositions.Count; i++)
        {
            Variable.CustomMarkerPositions[i] = Variable.DisplayPolar ? Algebra.ToPolar(Variable.CustomMarkerPositions[i]) : Algebra.FromPolar(Variable.CustomMarkerPositions[i]);
            Variable.CustomMarkerInputs[i] = ($"{Variable.CustomMarkerPositions[i].X}", $"{Variable.CustomMarkerPositions[i].Y}", false, false);
        }
    }

    /// <summary>
    /// 更新渲染元素开关
    /// </summary>
    public static void UpdateDoFromEnable()
    {
        Variable.DoCustomLine = Variable.EnableCustomLine;
        Variable.DoCustomMarker = Variable.EnableCustomMarker;
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

    #region 调试窗口

    /// <summary>
    /// 生成调试窗口
    /// </summary>
    public static void InitDebugWindow(int index, Room room)
    {
        var window = DebugWindow.Init(LocalizationManager.GetText(Variable.WindowTags[index]), true);
        window.ToForeground();
        window.transform.SetParent(room.transform, false);
        window.transform.localPosition = Variable.WindowPositions[index].Value;
        window.transform.localScale *= Variable.WindowScale.Value;
        Variable.DebugWindows[index] = window;
    }

    /// <summary>
    /// 设置调试窗口标题
    /// </summary>
    public static void SetDebugWindowTitle()
    {
        for (var i = 0; i < Variable.DebugWindows.Length; i++)
            Variable.DebugWindows[i]?.captionText.text = LocalizationManager.GetText(Variable.WindowTags[i]);
    }

    /// <summary>
    /// 保存调试窗口位置
    /// </summary>
    public static void SaveDebugWindowPos()
    {
        for (var i = 0; i < Variable.DebugWindows.Length; i++)
            Variable.WindowPositions[i].Value = Variable.DebugWindows[i].transform.localPosition;
        Variable.MenuRectConfig.Value = Variable.MenuRect;
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

        Variable.MenuRect = Variable.MenuRectConfig.Value;
    }

    #endregion

    #region 加载数据

    /// <summary>
    /// 读取二进制文件
    /// </summary>
    private static byte[] ReadBinaryFile(string path)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        var buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        return buffer;
    }

    /// <summary>
    /// 加载漩涡数据
    /// </summary>
    private static void LoadVortexFromBin(string path, out List<Variable.Vortex> list)
    {
        var data = ReadBinaryFile($"AlchAssV4.Bins.{path}.bin");
        using var reader = new BinaryReader(new MemoryStream(data));
        var len = reader.ReadInt32();
        list = new List<Variable.Vortex>(len);
        for (var i = 0; i < len; i++)
            list.Add(new Variable.Vortex { Pos = new(reader.ReadDouble(), reader.ReadDouble()), Rad = reader.ReadDouble() });
    }

    /// <summary>
    /// 加载区域数据
    /// </summary>
    private static void LoadZoneFromBin(string path, out List<Variable.ZoneCircle> circles, out List<Variable.ZoneBox> boxes, out List<Variable.ZoneBvh> bvh, out int[,] occsum, out Variable.Vec2D origin)
    {
        var data = ReadBinaryFile($"AlchAssV4.Bins.{path}.bin");
        using var reader = new BinaryReader(new MemoryStream(data));
        var numCircle = reader.ReadInt32();
        var numBox = reader.ReadInt32();
        var girdX = reader.ReadInt32();
        var girdY = reader.ReadInt32();
        origin = new(reader.ReadDouble(), reader.ReadDouble());
        var numBvhNode = reader.ReadInt32();
        circles = [];
        for (var i = 0; i < numCircle; i++)
            circles.Add(new Variable.ZoneCircle { Pos = new(reader.ReadDouble(), reader.ReadDouble()), Rad = reader.ReadDouble() });
        boxes = [];
        for (var i = 0; i < numBox; i++)
            boxes.Add(new Variable.ZoneBox { Pos = new(reader.ReadDouble(), reader.ReadDouble()), XHalf = reader.ReadDouble(), YHalf = reader.ReadDouble(), Rot = reader.ReadDouble() });
        occsum = new int[girdX, girdY];
        for (var i = 0; i < girdX; i++)
        for (var j = 0; j < girdY; j++)
            occsum[i, j] = reader.ReadInt32();
        bvh = [];
        for (var i = 0; i < numBvhNode; i++)
        {
            var node = new Variable.ZoneBvh { XMin = reader.ReadDouble(), YMin = reader.ReadDouble(), XMax = reader.ReadDouble(), YMax = reader.ReadDouble(), Left = reader.ReadInt32(), Right = reader.ReadInt32() };
            var circleCount = reader.ReadInt32();
            node.Circles = [];
            for (var j = 0; j < circleCount; j++)
                node.Circles.Add(reader.ReadInt32());
            var boxCount = reader.ReadInt32();
            node.Boxes = [];
            for (var j = 0; j < boxCount; j++)
                node.Boxes.Add(reader.ReadInt32());
            bvh.Add(node);
        }
    }

    /// <summary>
    /// 读取数据文件
    /// </summary>
    public static void LoadData()
    {
        for (var i = 0; i < 2; i++)
            LoadVortexFromBin(Variable.VortexTags[i], out Variable.Vortices[i]);
        for (var i = 0; i < 6; i++)
            LoadZoneFromBin(Variable.ZoneTags[i], out Variable.Circles[i], out Variable.Boxes[i], out Variable.BVH[i],
                out Variable.OccSum[i], out Variable.OccOrigin[i]);
    }

    #endregion
}