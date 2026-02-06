using HarmonyLib;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Mortar;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using UnityEngine;

namespace AlchAssV3
{
    public static class Calculation
    {
        #region 窗口信息计算
        /// <summary>
        /// 计算路径信息
        /// </summary>
        public static string CalculatePath()
        {
            string devTotText = LocalizationManager.GetText("不可用");
            string devPosText = LocalizationManager.GetText("不可用");
            string closestDirText = LocalizationManager.GetText("不可用");
            string deltaAngleText = LocalizationManager.GetText("不可用");
            string lifeSaltText = LocalizationManager.GetText("不可用");
            string swampDisText = LocalizationManager.GetText("不可用");

            if (!float.IsNaN(Variable.ClosestPositions[0].x))
            {
                Vector2 targetPos = Variable.TargetEffect.transform.localPosition;
                var targetRot = Variable.TargetEffect.transform.localEulerAngles.z;
                var devPos = Vector2.Distance(targetPos, Variable.ClosestPositions[0]) * 1800f;
                var devRot = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, targetRot)) / 3f * 25f;
                var devTot = devPos + devRot;

                var lvlPos = devPos <= 100f ? 3 : devPos <= 600f ? 2 : devPos <= 2754f ? 1 : 0;
                var lvlTot = devTot <= 100f ? 3 : devTot <= 600f ? 2 : devPos <= 2754f ? 1 : 0;
                var closestDir = Vector2.SignedAngle(Vector2.right, Variable.ClosestPositions[0] - targetPos);

                devPosText = $"<color=red>L{lvlPos}</color> {devPos}%";
                devTotText = $"<color=red>L{lvlTot}</color> {devTot}%";
                closestDirText = $"{closestDir}°";
            }

            if (!double.IsNaN(Variable.LineDirections[0]) && !double.IsNaN(Variable.LineDirections[2]))
            {
                var deltaAng = Mathf.DeltaAngle((float)Variable.LineDirections[0], (float)Variable.LineDirections[2]);
                deltaAngleText = $"{deltaAng}°";
            }
            if (!double.IsNaN(Variable.DangerDistancePath))
                lifeSaltText = Function.FormatLifeSalt(Variable.DangerDistancePath);
            if (!double.IsNaN(Variable.DistanceSwamp))
                swampDisText = $"{(float)Variable.DistanceSwamp}";
            return $"""
                {LocalizationManager.GetText("总体偏离")}: {devTotText}
                {LocalizationManager.GetText("位置偏离")}: {devPosText}
                {LocalizationManager.GetText("近点方向")}: {closestDirText}
                {LocalizationManager.GetText("效果夹角")}: {deltaAngleText}
                {LocalizationManager.GetText("加血需求")}: {lifeSaltText}
                {LocalizationManager.GetText("沼泽长度")}: {swampDisText}
                """;
        }

        /// <summary>
        /// 计算加水信息
        /// </summary>
        public static string CalculateLadle()
        {
            string devTotText = LocalizationManager.GetText("不可用");
            string devPosText = LocalizationManager.GetText("不可用");
            string closestDirText = LocalizationManager.GetText("不可用");
            string deltaAngleText = LocalizationManager.GetText("不可用");
            string lifeSaltText = LocalizationManager.GetText("不可用");

            if (!float.IsNaN(Variable.ClosestPositions[2].x))
            {
                Vector2 targetPos = Variable.TargetEffect.transform.localPosition;
                var targetRot = Variable.TargetEffect.transform.localEulerAngles.z;
                var devPos = Vector2.Distance(targetPos, Variable.ClosestPositions[2]) * 1800f;
                var devRot = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, targetRot)) / 3f * 25f;
                var devTot = devPos + devRot;

                var lvlPos = devPos <= 100f ? 3 : devPos <= 600f ? 2 : devPos <= 2754f ? 1 : 0;
                var lvlTot = devTot <= 100f ? 3 : devTot <= 600f ? 2 : devPos <= 2754f ? 1 : 0;
                var closestDir = Vector2.SignedAngle(Vector2.right, Variable.ClosestPositions[2] - targetPos);

                devPosText = $"<color=red>L{lvlPos}</color> {devPos}%";
                devTotText = $"<color=red>L{lvlTot}</color> {devTot}%";
                closestDirText = $"{closestDir}°";
            }

            if (!double.IsNaN(Variable.LineDirections[1]) && !double.IsNaN(Variable.LineDirections[2]))
            {
                var deltaAng = Mathf.DeltaAngle((float)Variable.LineDirections[1], (float)Variable.LineDirections[2]);
                deltaAngleText = $"{deltaAng}°";
            }
            if (!double.IsNaN(Variable.DangerDistanceLadle))
                lifeSaltText = Function.FormatLifeSalt(Variable.DangerDistanceLadle);
            return $"""
                {LocalizationManager.GetText("总体偏离")}: {devTotText}
                {LocalizationManager.GetText("位置偏离")}: {devPosText}
                {LocalizationManager.GetText("近点方向")}: {closestDirText}
                {LocalizationManager.GetText("效果夹角")}: {deltaAngleText}
                {LocalizationManager.GetText("加血需求")}: {lifeSaltText}
                """;
        }

        /// <summary>
        /// 计算移动信息
        /// </summary>
        public static string CalculateMove()
        {
            var phase = Managers.RecipeMap.path.deletedGraphicsSegments;
            var progress = Managers.RecipeMap.path.segmentLengthToDeletePhysics;
            var pathDir = double.IsNaN(Variable.LineDirections[0]) ? LocalizationManager.GetText("不可用") : $"{(float)Variable.LineDirections[0]}°";
            var ladleDir = double.IsNaN(Variable.LineDirections[1]) ? LocalizationManager.GetText("不可用") : $"{(float)Variable.LineDirections[1]}°";
            var vortexText = LocalizationManager.GetText("不可用");
            if (Managers.RecipeMap.CurrentVortexMapItem != null)
            {
                var p = Managers.RecipeMap.CurrentVortexMapItem.thisTransform.localPosition - Variable.Offset;
                var r = ((CircleCollider2D)Traverse.Create(Managers.RecipeMap.CurrentVortexMapItem).Field("vortexCollider").GetValue()).radius + 0.74f;
                var a = (float)Variable.VortexA;
                var b = r * r / (p.magnitude * Mathf.Sqrt(r * r + a * a));
                var vortexDir = (Mathf.Atan2(p.y, p.x) + Mathf.Asin(b)) * Mathf.Rad2Deg;
                vortexText = $"{(vortexDir > 0f ? vortexDir - 180f : vortexDir + 180f)}°";
            }
            if (Variable.DisplayStage)
                return $"""
                {LocalizationManager.GetText("搅拌阶段")}: {phase}
                {LocalizationManager.GetText("阶段进度")}: {progress}
                {LocalizationManager.GetText("路径方向")}: {pathDir}
                {LocalizationManager.GetText("加水方向")}: {ladleDir}
                {LocalizationManager.GetText("漩涡切角")}: {vortexText}
                """;
            return $"""
                {LocalizationManager.GetText("搅拌进度")}: {phase + progress}
                {LocalizationManager.GetText("路径方向")}: {pathDir}
                {LocalizationManager.GetText("加水方向")}: {ladleDir}
                {LocalizationManager.GetText("漩涡切角")}: {vortexText}
                """;
        }

        /// <summary>
        /// 计算目标效果
        /// </summary>
        public static string CalculateEffect()
        {
            if (Variable.TargetEffect == null)
                return "";

            var targetId = Variable.TargetEffect.Effect.GetLocalizedTitle();
            Vector2 targetPos = Variable.TargetEffect.transform.localPosition;
            var targetRot = Mathf.DeltaAngle(Variable.TargetEffect.transform.localEulerAngles.z, 0f) / 9f * 25f;
            var posText = Function.FormatPosition(targetPos);
            var rotText = Function.FormatMoonSalt(targetRot);
            var dirText = double.IsNaN(Variable.LineDirections[2]) ? LocalizationManager.GetText("不可用") : $"{(float)Variable.LineDirections[2]}°";
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
            var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
            var offPos = Managers.RecipeMap.indicator.thisTransform.localPosition;
            var indRot = Managers.RecipeMap.indicatorRotation.Value;
            var posText = Function.FormatPosition(indPos);
            var offText = Function.FormatPosition(offPos);
            var rotText = Function.FormatMoonSalt(indRot);
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
            if (Variable.TargetEffect == null)
                return "";

            Vector2 targetPos = Variable.TargetEffect.transform.localPosition;
            var targetRot = Variable.TargetEffect.transform.localEulerAngles.z;
            var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
            var indRot = Managers.RecipeMap.indicatorRotation.Value;

            var devPos = Vector2.Distance(targetPos, indPos) * 1800f;
            var devRot = Mathf.Abs(Mathf.DeltaAngle(indRot, targetRot)) / 3f * 25f;
            var devTot = devPos + devRot;

            var lvlPos = devPos <= 100f ? 3 : devPos <= 600f ? 2 : devPos <= 2754f ? 1 : 0;
            var lvlRot = devRot <= 100f ? 3 : devRot <= 600f ? 2 : 1;
            var lvlTot = devTot <= 100f ? 3 : devTot <= 600f ? 2 : devPos <= 2754f ? 1 : 0;
            return $"""
                {LocalizationManager.GetText("总体偏离")}: <color=red>L{lvlTot}</color> {devTot}%
                {LocalizationManager.GetText("位置偏离")}: <color=red>L{lvlPos}</color> {devPos}%
                {LocalizationManager.GetText("旋转偏离")}: <color=red>L{lvlRot}</color> {devRot}%
                """;
        }

        /// <summary>
        /// 计算活跃漩涡
        /// </summary>
        public static string CalculateVortex()
        {
            if (Managers.RecipeMap.CurrentVortexMapItem == null)
                return "";

            var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
            var vortexPos = Managers.RecipeMap.CurrentVortexMapItem.thisTransform.localPosition;

            var disText = $"{Vector2.Distance(vortexPos, indPos)}";
            var maxText = $"{((CircleCollider2D)Traverse.Create(Managers.RecipeMap.CurrentVortexMapItem).Field("vortexCollider").GetValue()).radius + 0.74f}";
            var dirText = $"{Vector2.SignedAngle(Vector2.right, vortexPos - indPos)}°";
            var tanText = double.IsNaN(Variable.LineDirections[4]) ? LocalizationManager.GetText("不可用") : $"{(float)Variable.LineDirections[4]}°";
            var lfsText = double.IsNaN(Variable.DangerDistanceVortex) ? LocalizationManager.GetText("不可用") : Function.FormatLifeSalt(Variable.DangerDistanceVortex);
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
            if (mapindex == 2 || Variable.VortexIndex[mapindex] < 0)
                return "";

            var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
            var selVortex = Variable.Vortexs[mapindex][Variable.VortexIndex[mapindex]];
            var vortexPos = new Vector2((float)selVortex.x, (float)selVortex.y);

            var disText = $"{Vector2.Distance(vortexPos, indPos)}";
            var maxText = $"{(float)selVortex.r}";
            var dirText = double.IsNaN(Variable.LineDirections[3]) ? LocalizationManager.GetText("不可用") : $"{(float)Variable.LineDirections[3]}°";
            var strText = float.IsNaN(Variable.ClosestPositions[1].x) ? LocalizationManager.GetText("不可用") : $"{Vector2.Distance(vortexPos, Variable.ClosestPositions[1])}";
            var ldlText = float.IsNaN(Variable.ClosestPositions[3].x) ? LocalizationManager.GetText("不可用") : $"{Vector2.Distance(vortexPos, Variable.ClosestPositions[3])}";
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
            if (mortar.ContainedStack == null)
                return "";
            return $"{LocalizationManager.GetText("研磨进度")}: {mortar.ContainedStack.overallGrindStatus * 100f}%";
        }
        #endregion

        #region 渲染信息计算
        /// <summary>
        /// 更新偏移
        /// </summary>
        public static void UpdateOffset()
        {
            if (Variable.DisplayOffset)
                Variable.Offset = Managers.RecipeMap.indicator.thisTransform.localPosition;
            else
                Variable.Offset = Vector3.zero;
        }

        /// <summary>
        /// 计算路径方向
        /// </summary>
        public static void GetPathLineDirection()
        {
            if (!Variable.DoLines[0] || Variable.PathPhysical.Count < 2)
                Variable.LineDirections[0] = double.NaN;
            else
                Variable.LineDirections[0] = Vector2.SignedAngle(Vector2.right, Variable.PathPhysical[1].Item1 - Variable.PathPhysical[0].Item1);
        }

        /// <summary>
        /// 计算加水方向
        /// </summary>
        public static void GetLadleLineDirection()
        {
            if (!Variable.DoLines[1])
                Variable.LineDirections[1] = double.NaN;
            else
                Variable.LineDirections[1] = Vector2.SignedAngle(Vector2.right, -Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition);
        }

        /// <summary>
        /// 计算目标方向
        /// </summary>
        public static void GetTargetLineDirection()
        {
            if (!Variable.DoLines[2] || Variable.TargetEffect == null)
                Variable.LineDirections[2] = double.NaN;
            else
            {
                var targetPos = Variable.TargetEffect.transform.localPosition;
                var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
                Variable.LineDirections[2] = Vector2.SignedAngle(Vector2.right, targetPos - indPos);
            }
        }

        /// <summary>
        /// 计算漩涡方向
        /// </summary>
        public static void GetVortexLineDirection()
        {
            if (!Variable.DoLines[3])
                Variable.LineDirections[3] = double.NaN;
            else
            {
                var mapindex = Variable.MapId[Managers.RecipeMap.currentMap.potionBase.name];
                if (mapindex == 2 || Variable.VortexIndex[mapindex] < 0)
                    Variable.LineDirections[3] = double.NaN;
                else
                {
                    var selVortex = Variable.Vortexs[mapindex][Variable.VortexIndex[mapindex]];
                    var vortexPos = new Vector3((float)selVortex.x, (float)selVortex.y);
                    var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
                    Variable.LineDirections[3] = Vector2.SignedAngle(Vector2.right, vortexPos - indPos);
                }
            }
        }

        /// <summary>
        /// 计算漩涡切线
        /// </summary>
        public static void GetTangentLineDirection()
        {
            if (!Variable.DoLines[4] || double.IsNaN(Variable.VortexX))
                Variable.LineDirections[4] = double.NaN;
            else
            {
                var dir = Variable.VortexMaxAngle + Variable.VortexRotation + Math.Atan(Variable.VortexMaxAngle);
                var dirdeg = dir * 180 / Math.PI;
                Variable.LineDirections[4] = dirdeg % 360 - 180;
            }
        }

        /// <summary>
        /// 生成直线
        /// </summary>
        public static void InitLine(double theta, out Vector3[] Points)
        {
            Points = [];
            if (double.IsNaN(theta))
                return;

            var rad = theta * Math.PI / 180;
            var dx = Math.Cos(rad);
            var dy = Math.Sin(rad);
            var mapTrans = Managers.RecipeMap.currentMap.referencesContainer.transform;
            var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
            List<Vector3> points = [];

            if (Math.Abs(dx) > 1e-5)
            {
                var t = (-80 - indPos.x) / dx;
                var y = indPos.y + t * dy;
                if (y >= -80 && y <= 80)
                    points.Add(mapTrans.TransformPoint(new(-80, (float)y)));
                t = (80 - indPos.x) / dx;
                y = indPos.y + t * dy;
                if (y >= -80 && y <= 80)
                    points.Add(mapTrans.TransformPoint(new(80, (float)y)));
            }
            if (Math.Abs(dy) > 1e-5)
            {
                var t = (-80 - indPos.y) / dy;
                var x = indPos.x + t * dx;
                if (x > -80 && x < 80)
                    points.Add(mapTrans.TransformPoint(new((float)x, -80)));
                t = (80 - indPos.y) / dy;
                x = indPos.x + t * dx;
                if (x > -80 && x < 80)
                    points.Add(mapTrans.TransformPoint(new((float)x, 80)));
            }
            Points = [.. points];
        }

        /// <summary>
        /// 生成路径曲线（整列和散列）
        /// </summary>
        public static void InitPathCurve()
        {
            Variable.PathPhysical = []; Variable.PathGraphical = []; Variable.SwampPositions = []; Variable.DistanceSwamp = double.NaN;
            if (!Variable.DoPathCurve)
                return;

            var pathHints = Managers.RecipeMap.path.fixedPathHints;
            if (pathHints.Count == 0)
                return;

            var mapTrans = Managers.RecipeMap.currentMap.referencesContainer.transform;
            var pathTrans = Managers.RecipeMap.path.thisTransform;
            var stIn = ZonePart.GetZonesActivePartsCount(typeof(SwampZonePart)) > 0;
            var stSet = Vector3.zero;
            var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var mapId = Managers.RecipeMap.currentMap.potionBase.name;
            var lineIn = stIn;
            List<(Vector2, int, double)> swampPos = [];

            Variable.PathPhysical.Add((indPos + Variable.Offset, false));
            for (int i = 0; i < pathHints.Count; i++)
            {
                var hint = pathHints[i];
                var isTp = hint.GetType().Name == "TeleportationFixedHint";
                var points = hint.evenlySpacedPointsFixedPhysics.points.Select(point => mapTrans.InverseTransformPoint(pathTrans.TransformPoint(point))).ToList();
                if (points.Count() < 2) continue;
                if (i == 0) points[0] = indPos;
                if (isTp) points = [points[0], points[points.Count - 1]];
                if (Variable.DoSwampPoint && mapId == "Oil")
                {
                    Geometry.ScalePath(points, stIn, stSet, Variable.PathPhysical.Count - 1, isTp, out var pointsSc, out var pos, out var edIn, out var edSet);
                    stIn = edIn; stSet = edSet; points = pointsSc;
                    swampPos.AddRange(pos);
                }

                Variable.PathPhysical.AddRange(points.Skip(1).Select(point => (point + Variable.Offset, isTp)));
                Variable.PathGraphical.Add(([.. points.Select(point => mapTrans.TransformPoint(point + Variable.Offset))], isTp));
            }
            Variable.SwampPositions.AddRange(swampPos.Select(x => x.Item1));
            if (Variable.DoSwampPoint && mapId == "Oil")
                Geometry.SwampLine(Variable.PathPhysical, swampPos, lineIn, out Variable.DistanceSwamp);
        }

        /// <summary>
        /// 生成漩涡曲线（参数和散列）
        /// </summary>
        public static void InitVortexCurve()
        {
            Variable.VortexX = double.NaN; Variable.VortexY = double.NaN; Variable.VortexRotation = double.NaN;
            Variable.VortexMaxAngle = double.NaN; Variable.VortexMinAngle = double.NaN; Variable.VortexGraphical = [];
            if (!Variable.DoVortexCurve)
                return;

            var curVortex = Managers.RecipeMap.CurrentVortexMapItem;
            if (curVortex == null)
                return;

            var vortexPos = curVortex.thisTransform.localPosition;
            var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
            var maxDis = ((CircleCollider2D)Traverse.Create(curVortex).Field("vortexCollider").GetValue()).radius + 0.74;
            var distance = Vector2.Distance(vortexPos, indPos);
            if (distance > maxDis + 1e-5)
                return;

            var v = indPos - vortexPos;
            var maxAng = v.magnitude / Variable.VortexA;
            var rot = (Math.Atan2(v.y, v.x) - maxAng) % (2 * Math.PI);
            Variable.VortexMinAngle = (maxDis - 1.55) / Variable.VortexA;
            Variable.VortexX = vortexPos.x;
            Variable.VortexY = vortexPos.y;
            Variable.VortexRotation = rot;
            Variable.VortexMaxAngle = maxAng;

            var mapTrans = Managers.RecipeMap.currentMap.referencesContainer.transform;
            Variable.VortexGraphical = new Vector3[Math.Max(10, (int)(distance * 250))];
            for (int i = 0; i < Variable.VortexGraphical.Length; i++)
            {
                var t = i / (double)Variable.VortexGraphical.Length;
                var angle = t * maxAng;
                var radius = Variable.VortexA * angle;
                var x = radius * Math.Cos(angle);
                var y = radius * Math.Sin(angle);
                var x_rot = x * Math.Cos(rot) - y * Math.Sin(rot) + vortexPos.x;
                var y_rot = x * Math.Sin(rot) + y * Math.Cos(rot) + vortexPos.y;
                Variable.VortexGraphical[i] = mapTrans.TransformPoint(new((float)x_rot, (float)y_rot));
            }
            Variable.VortexGraphical = [.. Variable.VortexGraphical.AddItem(mapTrans.TransformPoint(indPos))];
        }

        /// <summary>
        /// 生成边界圈
        /// </summary>
        public static void InitRange(double rad, double cx, double cy, out Vector3[] Points)
        {
            var r = rad - Variable.LineWidth.Value * 0.5;
            Points = new Vector3[Math.Max(10, (int)(r * 250))];
            var mapTrans = Managers.RecipeMap.currentMap.referencesContainer.transform;
            for (int i = 0; i < Points.Length; i++)
            {
                var t = i / (double)Points.Length;
                var angle = t * 2 * Math.PI;
                var x = r * Math.Cos(angle) + cx;
                var y = r * Math.Sin(angle) + cy;
                Points[i] = mapTrans.TransformPoint(new((float)x, (float)y));
            }
        }

        /// <summary>
        /// 生成关键点
        /// </summary>
        public static void InitPoints(double health)
        {
            Variable.ClosestPositions = [new Vector2(float.NaN, float.NaN), new Vector2(float.NaN, float.NaN), new Vector2(float.NaN, float.NaN), new Vector2(float.NaN, float.NaN)];
            Variable.DefeatPositions = [new Vector2(float.NaN, float.NaN), new Vector2(float.NaN, float.NaN), new Vector2(float.NaN, float.NaN)];
            Variable.DangerPositions = [[], [], []];
            Variable.IntersectionPositions = [[], [], [], []];
            Variable.DangerDistancePath = double.NaN;
            Variable.DangerDistanceLadle = double.NaN;
            Variable.DangerDistanceVortex = double.NaN;

            var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
            List<(Vector3, bool)> pathLadle = [(indPos, false), (Variable.Offset, false)];
            bool[] inDanger = [
                ZonePart.GetZonesActivePartsCount(typeof(StrongDangerZonePart)) > 0,
                ZonePart.GetZonesActivePartsCount(typeof(WeakDangerZonePart)) > 0,
                ZonePart.GetZonesActivePartsCount(typeof(HealZonePart)) > 0];

            bool effectVaild = new(); bool vortexVaild = new();
            Vector2 effectPos = new(); Vector2 vortexPos = new();
            double vortexRad = new();
            var mapId = Managers.RecipeMap.currentMap.potionBase.name;
            if (Variable.TargetEffect != null)
            {
                effectPos = Variable.TargetEffect.transform.localPosition;
                effectVaild = true;
            }
            if (mapId != "Wine")
            {
                var mapindex = Variable.MapId[mapId];
                if (Variable.VortexIndex[mapindex] >= 0)
                {
                    var selVortex = Variable.Vortexs[mapindex][Variable.VortexIndex[mapindex]];
                    vortexPos = new Vector2((float)selVortex.x, (float)selVortex.y);
                    vortexRad = selVortex.r;
                    vortexVaild = true;
                }
            }
            var vortexIn = Variable.VortexMaxAngle > Variable.VortexMinAngle;

            var closeEPathEn = Variable.DoPathCurve && effectVaild;
            var closeELadleEn = Variable.DoLines[1] && effectVaild;
            var closeVPathEn = Variable.DoPathCurve && vortexVaild;
            var closeVLadleEn = Variable.DoLines[1] && vortexVaild;
            var effectPathEn = Variable.DoPathEffectPoint && effectVaild;
            var effectLadleEn = Variable.DoLadleEffectPoint && effectVaild;
            var vortexPathEn = Variable.DoPathVortexPoint && vortexVaild;
            var vortexLadleEn = Variable.DoLadleVortexPoint && vortexVaild;
            var dangerPathEn = Variable.DoPathDangerPoint;
            var dangerLadleEn = Variable.DoLadleDangerPoint;
            var dangerVortexEn = Variable.DoVortexDangerPoint && vortexIn;

            var lenPath = Variable.PathPhysical.Count() - 1;
            if (lenPath > 0)
            {
                if (closeEPathEn || closeVPathEn)
                {
                    var closeEPathMin = double.MaxValue;
                    var closeVPathMin = double.MaxValue;

                    for (var i = 0; i < lenPath; i++)
                    {
                        Vector2 p0 = Variable.PathPhysical[i].Item1;
                        Vector2 p1 = Variable.PathPhysical[i + 1].Item1;
                        var isTp = Variable.PathPhysical[i + 1].Item2;

                        if (closeEPathEn)
                        {
                            Geometry.SqrDisToPoint(p0, p1, effectPos, isTp, out var closeEPathDis, out var closeEPathPos);
                            if (closeEPathDis < closeEPathMin)
                            {
                                closeEPathMin = closeEPathDis;
                                Variable.ClosestPositions[0] = closeEPathPos;
                            }
                        }

                        if (closeVPathEn)
                        {
                            Geometry.SqrDisToPoint(p0, p1, vortexPos, isTp, out var closeVPathDis, out var closeVPathPos);
                            if (closeVPathDis < closeVPathMin)
                            {
                                closeVPathMin = closeVPathDis;
                                Variable.ClosestPositions[1] = closeVPathPos;
                            }
                        }
                    }
                }

                if (effectPathEn || vortexPathEn || dangerPathEn)
                {
                    List<(Vector2, int, double, int)> dangerPathSum = [];

                    for (var i = 0; i < lenPath; i += 100)
                    {
                        var lt = Math.Min(lenPath, i + 100);
                        var minx = -double.MaxValue; var maxx = double.MaxValue;
                        var miny = -double.MaxValue; var maxy = double.MaxValue;

                        for (var j = i; j <= lt; j++)
                        {
                            var x = Variable.PathPhysical[j].Item1.x;
                            var y = Variable.PathPhysical[j].Item1.y;
                            minx = Math.Min(minx, x); maxx = Math.Max(maxx, x);
                            miny = Math.Min(miny, y); maxy = Math.Max(maxy, y);
                        }

                        var effectPathEnC = effectPathEn && Geometry.RangeAABB(minx, miny, maxx, maxy, effectPos, 1.53);
                        var vortexPathEnC = vortexPathEn && Geometry.RangeAABB(minx, miny, maxx, maxy, vortexPos, vortexRad);
                        var dangerPathEnC = dangerPathEn && Geometry.DangerAABB(minx, miny, maxx, maxy, mapId);

                        for (var j = i; j < lt; j++)
                        {
                            Vector2 p0 = Variable.PathPhysical[j].Item1;
                            Vector2 p1 = Variable.PathPhysical[j + 1].Item1;
                            var isTp = Variable.PathPhysical[j + 1].Item2;

                            if (effectPathEnC)
                            {
                                Geometry.TargetRange(p0, p1, effectPos, isTp, out var effectPath);
                                Variable.IntersectionPositions[0].AddRange(effectPath);
                            }
                            if (vortexPathEnC)
                            {
                                Geometry.VortexRange(p0, p1, vortexPos, vortexRad, isTp, out var vortexPath);
                                Variable.IntersectionPositions[2].AddRange(vortexPath);
                            }
                            if (dangerPathEnC)
                            {
                                Geometry.DangerLine(p0, p1, mapId, j, isTp, out var dangerPath);
                                dangerPathSum.AddRange(dangerPath);
                            }
                        }
                    }

                    if (dangerPathEn)
                    {
                        Geometry.DefeatLine(Variable.PathPhysical, dangerPathSum, health, inDanger, mapId,
                            out Variable.DefeatPositions[0], out Variable.DangerDistancePath);
                        Variable.DangerPositions[0].AddRange(dangerPathSum.Select(x => x.Item1));
                    }
                }
            }

            if (closeELadleEn)
                Geometry.SqrDisToPoint(indPos, Variable.Offset, effectPos, false, out _, out Variable.ClosestPositions[2]);
            if (closeVLadleEn)
                Geometry.SqrDisToPoint(indPos, Variable.Offset, vortexPos, false, out _, out Variable.ClosestPositions[3]);
            if (effectLadleEn)
                Geometry.TargetRange(indPos, Variable.Offset, effectPos, false, out Variable.IntersectionPositions[1]);
            if (vortexLadleEn)
                Geometry.VortexRange(indPos, Variable.Offset, vortexPos, vortexRad, false, out Variable.IntersectionPositions[3]);
            if (dangerLadleEn)
            {
                Geometry.DangerLine(indPos, Variable.Offset, mapId, 0, false, out var dangerLadle);
                Geometry.DefeatLine(pathLadle, dangerLadle, health, inDanger, mapId, out Variable.DefeatPositions[1], out Variable.DangerDistanceLadle);
                Variable.DangerPositions[1].AddRange(dangerLadle.Select(x => x.Item1));
            }

            if (dangerVortexEn)
            {
                Geometry.DangerSpiral(Variable.VortexX, Variable.VortexY, Variable.VortexRotation,
                    Variable.VortexMaxAngle, Variable.VortexMinAngle, mapId, out var dangerVortex);
                Geometry.DefeatSpiral(Variable.VortexX, Variable.VortexY, Variable.VortexRotation, Variable.VortexMaxAngle,
                    dangerVortex, health, inDanger[0], out Variable.DefeatPositions[2], out Variable.DangerDistanceVortex);
                Variable.DangerPositions[2].AddRange(dangerVortex.Select(x => x.Item1));
            }
        }
        #endregion
    }
}
