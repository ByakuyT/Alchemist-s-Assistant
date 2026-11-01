using System;
using HarmonyLib;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Mortar;
using UnityEngine;

namespace AlchAssV4;

public static class Calculation
{
    #region 窗口信息计算

    /// <summary>
    /// 计算目标效果
    /// </summary>
    public static string CalculateEffect()
    {
        if (Variable.TargetEffect == null) return "";
        var targetId = Variable.TargetEffect.Effect.GetLocalizedTitle();
        var targetPos = Variable.Vec2D.From(Variable.TargetEffect.transform.localPosition);
        var targetRot = Variable.TargetEffect.transform.localEulerAngles.z;
        var posText = Localization.FormatPosition(targetPos);
        var rotText = Localization.FormatMoonSalt(targetRot);
        var dirText = Variable.LineDirections[2].HasValue ? $"{(float)Variable.LineDirections[2]}'" : "";
        return $"""
                {LocalizationManager.GetText("目标效果")}: {targetId}
                {LocalizationManager.GetText("坐标位置")}: {posText}
                {LocalizationManager.GetText("旋转角度")}: {rotText}
                {LocalizationManager.GetText("效果方向")}: {dirText}
                """;
    }

    /// <summary>
    /// 计算酿造信息
    /// </summary>
    public static string CalculateBrewing(float health)
    {
        var colPos = Variable.Vec2D.From(Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition) +
                     Variable.Offset;
        var offPos = Variable.Vec2D.From(Managers.RecipeMap.indicator.thisTransform.localPosition);
        var indRot = Managers.RecipeMap.indicatorRotation.Value;
        var posText = Localization.FormatPosition(colPos);
        var offText = Localization.FormatPosition(offPos);
        var rotText = Localization.FormatMoonSalt(indRot);
        return $"""
                {LocalizationManager.GetText("坐标位置")}: {posText}
                {LocalizationManager.GetText("碰撞偏移")}: {offText}
                {LocalizationManager.GetText("旋转角度")}: {rotText}
                {LocalizationManager.GetText("当前血量")}: {health * 100f}%
                """;
    }

    /// <summary>
    /// 计算效果偏离
    /// </summary>
    public static string CalculateDeviation()
    {
        if (Variable.TargetEffect == null) return "";
        var targetPos = Variable.Vec2D.From(Variable.TargetEffect.transform.localPosition);
        var targetRot = Variable.TargetEffect.transform.localEulerAngles.z;
        var colPos = Variable.Vec2D.From(Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition) +
                     Variable.Offset;
        var indRot = Managers.RecipeMap.indicatorRotation.Value;
        var devPos0 = colPos.Distance(targetPos) * 1800;
        var devPos2 = colPos.Distance(targetPos + Variable.Offset) * 1800;
        var devPos = devPos0 <= 2754 && devPos2 <= 600 ? devPos2 : devPos0;
        var devRot = Math.Abs(Algebra.DeltaAngle(indRot, targetRot)) / 3 * 25;
        var devTot = devPos + devRot;
        var lvlPos = devPos <= 100 ? 3 : devPos <= 600 ? 2 : devPos <= 2754 ? 1 : 0;
        var lvlRot = devRot <= 100 ? 3 : devRot <= 600 ? 2 : 1;
        var lvlTot = devTot <= 100 ? 3 : devTot <= 600 ? 2 : devPos <= 2754 ? 1 : 0;
        return $"""
                {LocalizationManager.GetText("总体偏离")}: <color=red>L{lvlTot}</color> {(float)devTot}%
                {LocalizationManager.GetText("位置偏离")}: <color=red>L{lvlPos}</color> {(float)devPos}%
                {LocalizationManager.GetText("旋转偏离")}: <color=red>L{lvlRot}</color> {(float)devRot}%
                """;
    }

    /// <summary>
    /// 计算活跃漩涡
    /// </summary>
    public static string CalculateVortex()
    {
        if (Managers.RecipeMap.CurrentVortexMapItem == null) return "";
        var colPos = Variable.Vec2D.From(Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition) +
                     Variable.Offset;
        var vortexPos = Variable.Vec2D.From(Managers.RecipeMap.CurrentVortexMapItem.thisTransform.localPosition);
        var disText = $"{(float)vortexPos.Distance(colPos)}";
        var maxText =
            $"{((CircleCollider2D)Traverse.Create(Managers.RecipeMap.CurrentVortexMapItem).Field("vortexCollider").GetValue()).radius + Variable.IndicatorRad}";
        var dirText = $"{(float)Variable.Vec2D.Right.SignedAngle(vortexPos - colPos)}";
        var tanText = Variable.LineDirections[4].HasValue ? $"{(float)Variable.LineDirections[4]}'" : "";
        var lfsText = Variable.DangerDistance[2].HasValue
            ? Localization.FormatLifeSalt(Variable.DangerDistance[2].Value)
            : "";
        return $"""
                {LocalizationManager.GetText("漩涡距离")}: {disText}
                {LocalizationManager.GetText("最大距离")}: {maxText}
                {LocalizationManager.GetText("漩涡方向")}: {dirText}
                {LocalizationManager.GetText("漩涡切向")}: {tanText}
                {LocalizationManager.GetText("加血需求")}: {lfsText}
                """;
    }

    /// <summary>
    /// 计算目标漩涡
    /// </summary>
    public static string CalculateTargetVortex()
    {
        var mapindex = Variable.MapId[Managers.RecipeMap.currentMap.potionBase.name];
        if (mapindex == 2 || Variable.VortexIndex[mapindex] < 0) return "";
        var colPos = Variable.Vec2D.From(Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition) +
                     Variable.Offset;
        var selVortex = Variable.Vortices[mapindex][Variable.VortexIndex[mapindex]];
        var disText = $"{(float)selVortex.Pos.Distance(colPos)}";
        var maxText = $"{(float)selVortex.Rad}";
        var dirText = Variable.LineDirections[3].HasValue ? $"{(float)Variable.LineDirections[3].Value}'" : "";
        var strText = Variable.ClosestPositions[1].HasValue
            ? $"{(float)selVortex.Pos.Distance(Variable.ClosestPositions[1].Value)}"
            : "";
        var ldlText = Variable.ClosestPositions[3].HasValue
            ? $"{(float)selVortex.Pos.Distance(Variable.ClosestPositions[3].Value)}"
            : "";
        return $"""
                {LocalizationManager.GetText("漩涡距离")}: {disText}
                {LocalizationManager.GetText("路径近点")}: {strText}
                {LocalizationManager.GetText("加水近点")}: {ldlText}
                {LocalizationManager.GetText("最大距离")}: {maxText}
                {LocalizationManager.GetText("漩涡方向")}: {dirText}
                """;
    }

    /// <summary>
    /// 计算研磨信息
    /// </summary>
    public static string CalculateGrind(Mortar mortar)
    {
        return mortar.ContainedStack == null
            ? ""
            : $"{LocalizationManager.GetText("研磨进度")}: {mortar.ContainedStack.overallGrindStatus * 100}%";
    }

    #endregion

    #region 渲染信息计算

    /// <summary>
    /// 更新偏移
    /// </summary>
    public static void UpdateOffset()
    {
        Variable.Offset = Variable.DisplayOffset
            ? Variable.Vec2D.From(Managers.RecipeMap.indicator.thisTransform.localPosition)
            : Variable.Vec2D.Zero;
    }

    #endregion
}