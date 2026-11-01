using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using PotionCraft.Assemblies.DataBaseSystem.PreparedObjects;
using PotionCraft.LocalizationSystem;
using UnityEngine;

namespace AlchAssV4;

public static class Localization
{
    #region 格式化文本

    /// <summary>
    /// 格式化位置文本
    /// </summary>
    public static string FormatPosition(Variable.Vec2D position)
    {
        if (!Variable.DisplayPolar)
            return $"{(position.X, position.Y)}";
        var polar = Algebra.ToPolar(position);
        return $"{(polar.X, polar.Y)}";
    }

    /// <summary>
    /// 格式化月盐文本
    /// </summary>
    public static string FormatMoonSalt(double rotation)
    {
        var angle = Algebra.DeltaAngle(rotation, 0.0);
        return !Variable.DisplaySalt ? $"{(float)angle}'" :
            angle < 0.0 ? $"<sprite=\"IconsAtlas\" name=\"MoonSalt\"> {(float)(-angle / 9 * 25)}" :
            $"<sprite=\"IconsAtlas\" name=\"SunSalt\"> {(float)(angle / 9f * 25f)}";
    }

    /// <summary>
    /// 格式化血盐文本
    /// </summary>
    public static string FormatLifeSalt(double dangerDistance)
    {
        var hp = Variable.DangerStrongDamage * dangerDistance * 100.0 - 100.0;
        return !Variable.DisplaySalt
            ? $"{(float)hp}%"
            : $"<sprite=\"IconsAtlas\" name=\"LifeSalt\"> {(float)(hp * 2.5)}";
    }

    /// <summary>
    /// 格式化本地化文本
    /// </summary>
    public static void FormatLocalization()
    {
        var assembly = Assembly.GetExecutingAssembly();
        string[] tags = ["Label", "Title", "Button"];
        foreach (var tag in tags)
            RegisterLocalization($"AlchAssV4.Locs.{tag}.json", assembly);
    }

    /// <summary>
    /// 更新自定义方向线序号列宽
    /// </summary>
    public static void UpdateCustomLineLabelWidth()
    {
        var cnt = (int)Math.Ceiling(Math.Log10(Variable.CustomLineDirections.Count + 1));
        Variable.CustomLineLabelWidth = GetLabelWidth([new('0', cnt)], false);
    }

    /// <summary>
    /// 更新自定义标记点序号列宽
    /// </summary>
    public static void UpdateCustomMarkerLabelWidth()
    {
        var cnt = (int)Math.Ceiling(Math.Log10(Variable.CustomMarkerPositions.Count + 1));
        Variable.CustomMarkerLabelWidth = GetLabelWidth([new('0', cnt)], false);
    }

    /// <summary>
    /// 计算标签长度
    /// </summary>
    private static float GetLabelWidth(string[] labels, bool loc)
    {
        return labels.Select(label =>
            loc
                ? Variable.LabelStyle.CalcSize(new GUIContent(LocalizationManager.GetText(label) + "0")).x
                : Variable.LabelStyle.CalcSize(new GUIContent(label + "0")).x).Prepend(0f).Max();
    }

    #endregion

    #region 本地化

    /// <summary>
    /// 读取本地化文件
    /// </summary>
    private static List<Variable.Localization> LoadLocalization(string path, Assembly assembly)
    {
        using var reader = new StreamReader(assembly.GetManifestResourceStream(path));
        return JsonConvert.DeserializeObject<List<Variable.Localization>>(reader.ReadToEnd());
    }

    /// <summary>
    /// 注册本地化文本
    /// </summary>
    private static void RegisterLocalization(string path, Assembly assembly)
    {
        var locs = LoadLocalization(path, assembly);
        var data = Traverse.Create(typeof(LocalizationManager)).Field("localizationData").GetValue<LocalizationData>();
        foreach (var loc in locs)
        foreach (var locale in Enum.GetValues(typeof(LocalizationManager.Locale)))
            data.Add((int)locale, loc.key, loc.values.ContainsKey($"{locale}") ? loc.values[$"{locale}"] : loc.key);
    }

    #endregion
}