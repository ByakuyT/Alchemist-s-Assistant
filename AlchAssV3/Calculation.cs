using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Mortar;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AlchAssV3
{
    internal class Calculation
    {
        #region 窗口信息计算
        /// <summary>
        /// 计算路径信息
        /// </summary>
        public static string CalculatePath()
        {
            string devTotText = "不可用";
            string devPosText = "不可用";
            string closestDirText = "不可用";
            string deltaAngelText = "不可用";
            string lifeSaltText = "不可用";

            if (!float.IsNaN(Variable.ClosestPositions[0].x))
            {
                Vector2 targetPos = Variable.TargetEffect.transform.localPosition;
                var targetRot = Variable.TargetEffect.transform.localEulerAngles.z;
                var devPos = Vector2.Distance(targetPos, Variable.ClosestPositions[0]) * 1800f;
                var devRot = Mathf.Abs(Mathf.DeltaAngle(Variable.IndicatorRotation, targetRot)) / 3f * 25f;
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
                deltaAngelText = $"{deltaAng}°";
            }

            if (!double.IsNaN(Variable.DangerDistance[0]))
                lifeSaltText = Function.FormatLifeSalt(Variable.DangerDistance[0]);
            return $"""
                总体偏离: {devTotText}
                位置偏离: {devPosText}
                近点方向: {closestDirText}
                目标夹角: {deltaAngelText}
                血盐需求: {lifeSaltText}
                """;
        }

        /// <summary>
        /// 计算加水信息
        /// </summary>
        public static string CalculateLadle()
        {
            string devTotText = "不可用";
            string devPosText = "不可用";
            string closestDirText = "不可用";
            string deltaAngelText = "不可用";
            string lifeSaltText = "不可用";

            if (!float.IsNaN(Variable.ClosestPositions[1].x))
            {
                Vector2 targetPos = Variable.TargetEffect.transform.localPosition;
                var targetRot = Variable.TargetEffect.transform.localEulerAngles.z;
                var devPos = Vector2.Distance(targetPos, Variable.ClosestPositions[1]) * 1800f;
                var devRot = Mathf.Abs(Mathf.DeltaAngle(Variable.IndicatorRotation, targetRot)) / 3f * 25f;
                var devTot = devPos + devRot;

                var lvlPos = devPos <= 100f ? 3 : devPos <= 600f ? 2 : devPos <= 2754f ? 1 : 0;
                var lvlTot = devTot <= 100f ? 3 : devTot <= 600f ? 2 : devPos <= 2754f ? 1 : 0;
                var closestDir = Vector2.SignedAngle(Vector2.right, Variable.ClosestPositions[1] - targetPos);

                devPosText = $"<color=red>L{lvlPos}</color> {devPos}%";
                devTotText = $"<color=red>L{lvlTot}</color> {devTot}%";
                closestDirText = $"{closestDir}°";
            }

            if (!double.IsNaN(Variable.LineDirections[1]) && !double.IsNaN(Variable.LineDirections[2]))
            {
                var deltaAng = Mathf.DeltaAngle((float)Variable.LineDirections[1], (float)Variable.LineDirections[2]);
                deltaAngelText = $"{deltaAng}°";
            }

            if (!double.IsNaN(Variable.DangerDistance[1]))
                lifeSaltText = Function.FormatLifeSalt(Variable.DangerDistance[1]);
            return $"""
                总体偏离: {devTotText}
                位置偏离: {devPosText}
                近点方向: {closestDirText}
                目标夹角: {deltaAngelText}
                血盐需求: {lifeSaltText}
                """;
        }

        /// <summary>
        /// 计算移动信息
        /// </summary>
        public static string CalculateMove()
        {
            var stage = Managers.RecipeMap.path.deletedGraphicsSegments;
            var progress = Managers.RecipeMap.path.segmentLengthToDeletePhysics;
            var stir = stage + progress;
            var pathDir = double.IsNaN(Variable.LineDirections[0]) ? "不可用" : $"{(float)Variable.LineDirections[0]}°";
            var ladleDir = double.IsNaN(Variable.LineDirections[1]) ? "不可用" : $"{(float)Variable.LineDirections[1]}°";
            return $"""
                搅拌进度: {stir}
                路径方向: {pathDir}
                加水方向: {ladleDir}
                """;
        }

        /// <summary>
        /// 计算目标信息
        /// </summary>
        public static string CalculateTarget()
        {
            if (Variable.TargetEffect == null)
                return "";

            var targetId = Variable.TargetEffect.Effect.GetLocalizedTitle();
            Vector2 targetPos = Variable.TargetEffect.transform.localPosition;
            var targetRot = Mathf.DeltaAngle(Variable.TargetEffect.transform.localEulerAngles.z, 0f) / 9f * 25f;
            var posText = Function.FormatPosition(targetPos);
            var rotText = Function.FormatMoonSalt(targetRot);
            var dirText = double.IsNaN(Variable.LineDirections[2]) ? "不可用" : $"{(float)Variable.LineDirections[2]}°";
            return $"""
                目标效果: {targetId}
                {posText}
                旋转盐量: {rotText}
                目标方向: {dirText}
                """;
        }

        /// <summary>
        /// 计算位置信息
        /// </summary>
        public static string CalculatePosition()
        {
            var rot = Mathf.DeltaAngle(Variable.IndicatorRotation, 0f) / 9f * 25f;
            var posText = Function.FormatPosition(Variable.IndicatorPosition);
            var rotText = Function.FormatMoonSalt(rot);
            return $"""
                {posText}
                旋转盐量: {rotText}
                """;
        }

        /// <summary>
        /// 计算偏离信息
        /// </summary>
        public static string CalculateDeviation()
        {
            if (Variable.TargetEffect == null)
                return "";

            Vector2 targetPos = Variable.TargetEffect.transform.localPosition;
            var targetRot = Variable.TargetEffect.transform.localEulerAngles.z;
            var devPos = Vector2.Distance(targetPos, Variable.IndicatorPosition) * 1800f;
            var devRot = Mathf.Abs(Mathf.DeltaAngle(Variable.IndicatorRotation, targetRot)) / 3f * 25f;
            var devTot = devPos + devRot;

            var lvlPos = devPos <= 100f ? 3 : devPos <= 600f ? 2 : devPos <= 2754f ? 1 : 0;
            var lvlRot = devRot <= 100f ? 3 : devRot <= 600f ? 2 : 1;
            var lvlTot = devTot <= 100f ? 3 : devTot <= 600f ? 2 : devPos <= 2754f ? 1 : 0;
            return $"""
                总体偏离: <color=red>L{lvlTot}</color> {devTot}%
                位置偏离: <color=red>L{lvlPos}</color> {devPos}%
                旋转偏离: <color=red>L{lvlRot}</color> {devRot}%
                """;
        }

        /// <summary>
        /// 计算漩涡信息
        /// </summary>
        public static string CalculateVortex()
        {
            Vector2 vortexPos;
            double maxDis;
            double dirVortex;
            var dangerDis = double.NaN;

            if (Managers.RecipeMap.CurrentVortexMapItem != null)
            {
                vortexPos = Managers.RecipeMap.CurrentVortexMapItem.thisTransform.localPosition;
                maxDis = ((CircleCollider2D)Traverse.Create(Managers.RecipeMap.CurrentVortexMapItem).Field("vortexCollider").GetValue()).radius + Variable.IndicatorRadius;
                dirVortex = Vector2.SignedAngle(Vector2.right, vortexPos - Variable.IndicatorPosition);
                dangerDis = Variable.DangerDistance[2];
            }
            else
            {
                if (Variable.CurrentMapID == "Wine")
                    return "";

                var mapindex = Variable.CurrentMapID == "Water" ? 0 : 1;
                var vortexList = mapindex == 0 ? Variable.Vortex_Water : Variable.Vortex_Oil;
                if (Variable.VortexIndex[mapindex] < 0)
                    return "";

                var selVortex = vortexList[Variable.VortexIndex[mapindex]];
                vortexPos = new Vector2((float)selVortex.x, (float)selVortex.y);
                maxDis = selVortex.r;
                dirVortex = Variable.LineDirections[3];
            }

            var distance = Vector2.Distance(vortexPos, Variable.IndicatorPosition);
            var dirText = double.IsNaN(dirVortex) ? "不可用" : $"{(float)dirVortex}°";
            var lifeSaltText = double.IsNaN(dangerDis) ? "不可用" : Function.FormatLifeSalt(dangerDis);
            return $"""
                当前距离: {distance}
                最大距离: {(float)maxDis}
                漩涡方向: {dirText}
                血盐需求: {lifeSaltText}
                """;
        }

        /// <summary>
        /// 计算血量信息
        /// </summary>
        public static string CalculateHealth(float health)
        {
            return $"当前血量: {health * 100f}%";
        }

        /// <summary>
        /// 计算研磨信息
        /// </summary>
        public static string CalculateGrind(Mortar mortar)
        {
            if (mortar.ContainedStack == null)
                return "";
            return $"研磨进度: {mortar.ContainedStack.overallGrindStatus * 100f}%";
        }
        #endregion

        #region 渲染信息计算
        /// <summary>
        /// 计算路径方向
        /// </summary>
        public static double GetPathLineDirection()
        {
            if (Variable.PathPhysical.Count < 2)
                return double.NaN;
            Vector2 pathPos = Variable.PathPhysical[1].Item1;
            return Vector2.SignedAngle(Vector2.right, pathPos - Variable.IndicatorPosition);
        }

        /// <summary>
        /// 计算加水方向
        /// </summary>
        public static double GetLadleLineDirection()
        {
            return Vector2.SignedAngle(Vector2.right, -Variable.IndicatorPosition);
        }

        /// <summary>
        /// 计算目标方向
        /// </summary>
        public static double GetTargetLineDirection()
        {
            if (Variable.TargetEffect == null)
                return double.NaN;

            Vector2 targetPos = Variable.TargetEffect.transform.localPosition;
            return Vector2.SignedAngle(Vector2.right, targetPos - Variable.IndicatorPosition);
        }

        /// <summary>
        /// 计算漩涡方向
        /// </summary>
        public static double GetVortexLineDirection()
        {
            if (Variable.CurrentMapID == "Wine")
                return double.NaN;

            var mapindex = Variable.CurrentMapID == "Water" ? 0 : 1;
            var vortexList = mapindex == 0 ? Variable.Vortex_Water : Variable.Vortex_Oil;
            if (Variable.VortexIndex[mapindex] < 0)
                return double.NaN;

            var selVortex = vortexList[Variable.VortexIndex[mapindex]];
            var vortexPos = new Vector2((float)selVortex.x, (float)selVortex.y);
            return Vector2.SignedAngle(Vector2.right, vortexPos - Variable.IndicatorPosition);
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
            var yDev = Variable.BaseRenderPosition.y - Variable.IndicatorPosition.y;
            List<Vector3> points = [];

            if (Math.Abs(dx) > 1e-5)
            {
                var t = (-80 - Variable.IndicatorPosition.x) / dx;
                var y = Variable.IndicatorPosition.y + t * dy;
                if (y >= -80 && y <= 80)
                    points.Add(new Vector3(-80, (float)y + yDev, 0));
                t = (80 - Variable.IndicatorPosition.x) / dx;
                y = Variable.IndicatorPosition.y + t * dy;
                if (y >= -80 && y <= 80)
                    points.Add(new Vector3(80, (float)y + yDev, 0));
            }
            if (Math.Abs(dy) > 1e-5)
            {
                var t = (-80 - Variable.IndicatorPosition.y) / dy;
                var x = Variable.IndicatorPosition.x + t * dx;
                if (x > -80 && x < 80)
                    points.Add(new Vector3((float)x, -80 + yDev, 0));
                t = (80 - Variable.IndicatorPosition.y) / dy;
                x = Variable.IndicatorPosition.x + t * dx;
                if (x > -80 && x < 80)
                    points.Add(new Vector3((float)x, 80 + yDev, 0));
            }
            Points = [.. points];
        }

        /// <summary>
        /// 生成路径曲线（整列和散列）
        /// </summary>
        public static void InitPathCurve(out List<(Vector3, bool)> PathPhysical, out List<(Vector3[], bool)> PathGraphical, out List<Vector2> swamppos)
        {
            PathPhysical = []; PathGraphical = []; swamppos = [];
            if (!Variable.DerivedEnables[4])
                return;

            var pathHints = Managers.RecipeMap.path.fixedPathHints;
            if (pathHints.Count == 0)
                return;

            var mapTrans = Managers.RecipeMap.currentMap.referencesContainer.transform;
            var pathTrans = Managers.RecipeMap.path.thisTransform;
            var yDev = Variable.BaseRenderPosition.y - Variable.IndicatorPosition.y;
            var stIn = ZonePart.GetZonesActivePartsCount(typeof(SwampZonePart)) > 0;
            var stSet = Vector3.zero;

            PathPhysical.Add((Variable.IndicatorPosition, false));
            for (int i = 0; i < pathHints.Count; i++)
            {
                var hint = pathHints[i];
                var isTp = hint.GetType().Name == "TeleportationFixedHint";
                var points = hint.evenlySpacedPointsFixedPhysics.points.Select(point => mapTrans.InverseTransformPoint(pathTrans.TransformPoint(point))).ToList();
                if (points.Count() < 2) continue;
                if (i == 0) points[0] = Variable.IndicatorPosition;
                if (isTp) points = [points[0], points[points.Count - 1]];
                if (Variable.DerivedEnables[16] && Variable.CurrentMapID == "Oil")
                {
                    Geometry.ScalePath(points, stIn, stSet, isTp, out var pointsSc, out var pos, out var edIn, out var edSet);
                    stIn = edIn; stSet = edSet; points = pointsSc;
                    swamppos.AddRange(pos);
                }

                var pointsDev = points.Select(point => new Vector3(point.x, point.y + yDev, 0));
                PathPhysical.AddRange(points.Skip(1).Select(point => (point, isTp)));
                PathGraphical.Add(([.. pointsDev], isTp));
            }
        }

        /// <summary>
        /// 生成漩涡曲线（参数和散列）
        /// </summary>
        public static void InitVortexCurve(out double[] Parameters, out Vector3[] Points)
        {
            Parameters = [double.NaN, double.NaN, double.NaN, double.NaN, double.NaN]; Points = [];
            if (!Variable.DerivedEnables[5])
                return;

            var curVortex = Managers.RecipeMap.CurrentVortexMapItem;
            if (curVortex == null)
                return;

            Vector2 vortexPos = curVortex.thisTransform.localPosition;
            var maxDis = ((CircleCollider2D)Traverse.Create(curVortex).Field("vortexCollider").GetValue()).radius + Variable.IndicatorRadius;
            var distance = Vector2.Distance(vortexPos, Variable.IndicatorPosition);
            if (distance > maxDis + 1e-5)
                return;

            var v = Variable.IndicatorPosition - vortexPos;
            var maxAng = v.magnitude / Variable.VortexCurveA;
            var rot = (Math.Atan2(v.y, v.x) - maxAng) % (2 * Math.PI);
            var minAng = (maxDis - Variable.MaxVortexDanger) / Variable.VortexCurveA;
            Parameters = [vortexPos.x, vortexPos.y, rot, maxAng, minAng];

            var yDev = Variable.BaseRenderPosition.y - Variable.IndicatorPosition.y;
            Points = new Vector3[Math.Max(10, (int)(distance * 250))];
            for (int i = 0; i < Points.Length; i++)
            {
                var t = i / (double)Points.Length;
                var angle = t * maxAng;
                var radius = Variable.VortexCurveA * angle;
                var x = radius * Math.Cos(angle);
                var y = radius * Math.Sin(angle);
                var x_rot = x * Math.Cos(rot) - y * Math.Sin(rot) + vortexPos.x;
                var y_rot = x * Math.Sin(rot) + y * Math.Cos(rot) + vortexPos.y + yDev;
                Points[i] = new Vector3((float)x_rot, (float)y_rot, 0);
            }
            Points = [.. Points.AddItem(new Vector3(Variable.IndicatorPosition.x, Variable.BaseRenderPosition.y, 0))];
        }

        /// <summary>
        /// 生成边界圈
        /// </summary>
        public static void InitRange(double rad, double cx, double cy, out Vector3[] Points)
        {
            var r = rad - Variable.LineWidth.Value * 0.5;
            var yDev = Variable.BaseRenderPosition.y - Variable.IndicatorPosition.y;
            Points = new Vector3[Math.Max(10, (int)(r * 250))];
            for (int i = 0; i < Points.Length; i++)
            {
                var t = i / (double)Points.Length;
                var angle = t * 2 * Math.PI;
                var x = r * Math.Cos(angle) + cx;
                var y = r * Math.Sin(angle) + cy + yDev;
                Points[i] = new Vector3((float)x, (float)y, 0);
            }
        }

        /// <summary>
        /// 生成关键点
        /// </summary>
        public static void InitPoints(double health, out Vector2[] closest, out Vector2[] defeat, out List<Vector2>[] danger, out List<Vector2>[] intersection, out double[] distance)
        {
            closest = [new Vector2(float.NaN, float.NaN), new Vector2(float.NaN, float.NaN)];
            defeat = [new Vector2(float.NaN, float.NaN), new Vector2(float.NaN, float.NaN), new Vector2(float.NaN, float.NaN)];
            danger = [[], [], []];
            intersection = [[], [], [], []];
            distance = [double.NaN, double.NaN, double.NaN];

            List<(Vector3, bool)> pathLadle = [(Variable.IndicatorPosition, false), (Vector2.zero, false)];
            bool[] inDanger = [
                ZonePart.GetZonesActivePartsCount(typeof(StrongDangerZonePart)) > 0,
                ZonePart.GetZonesActivePartsCount(typeof(WeakDangerZonePart)) > 0,
                ZonePart.GetZonesActivePartsCount(typeof(HealZonePart)) > 0,
            ];

            bool targetVaild = new(); bool vortexVaild = new();
            Vector2 targetPos = new(); Vector2 vortexPos = new();
            double vortexRad = new();
            if (Variable.TargetEffect != null)
            {
                targetPos = Variable.TargetEffect.transform.localPosition;
                targetVaild = true;
            }
            if (Variable.CurrentMapID != "Wine")
            {
                var mapindex = Variable.CurrentMapID == "Water" ? 0 : 1;
                var vortexList = mapindex == 0 ? Variable.Vortex_Water : Variable.Vortex_Oil;
                if (Variable.VortexIndex[mapindex] >= 0)
                {
                    var selVortex = vortexList[Variable.VortexIndex[mapindex]];
                    vortexPos = new Vector2((float)selVortex.x, (float)selVortex.y);
                    vortexRad = selVortex.r;
                    vortexVaild = true;
                }
            }
            var vortexIn = Variable.VortexParameters[3] > Variable.VortexParameters[4];

            var closePathEn = Variable.DerivedEnables[4] && targetVaild;
            var closeLadleEn = Variable.DerivedEnables[1] && targetVaild;
            var targetPathEn = Variable.DerivedEnables[9] && targetVaild;
            var targetLadleEn = Variable.DerivedEnables[10] && targetVaild;
            var vortexPathEn = Variable.DerivedEnables[11] && vortexVaild;
            var vortexLadleEn = Variable.DerivedEnables[12] && vortexVaild;
            var dangerPathEn = Variable.DerivedEnables[13];
            var dangerLadleEn = Variable.DerivedEnables[14];
            var dangerVortexEn = Variable.DerivedEnables[15] && vortexIn;

            var lenPath = Variable.PathPhysical.Count() - 1;
            if (lenPath > 0)
            {
                if (closePathEn)
                {
                    var closePathMin = double.MaxValue;

                    for (var i = 0; i < lenPath; i++)
                    {
                        Vector2 p0 = Variable.PathPhysical[i].Item1;
                        Vector2 p1 = Variable.PathPhysical[i + 1].Item1;
                        var isTp = Variable.PathPhysical[i + 1].Item2;

                        Geometry.SqrDisToPoint(p0, p1, targetPos, isTp, out var closePathDis, out var closePathPos);
                        if (closePathDis < closePathMin)
                        {
                            closePathMin = closePathDis;
                            closest[0] = closePathPos;
                        }
                    }
                }

                if (targetPathEn || vortexPathEn || dangerPathEn)
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

                        var targetPathEnC = targetPathEn && Geometry.RangeAABB(minx, miny, maxx, maxy, targetPos, 1.53);
                        var vortexPathEnC = vortexPathEn && Geometry.RangeAABB(minx, miny, maxx, maxy, vortexPos, vortexRad);
                        var dangerPathEnC = dangerPathEn && Geometry.DangerAABB(minx, miny, maxx, maxy, Variable.CurrentMapID);

                        for (var j = i; j < lt; j++)
                        {
                            Vector2 p0 = Variable.PathPhysical[j].Item1;
                            Vector2 p1 = Variable.PathPhysical[j + 1].Item1;
                            var isTp = Variable.PathPhysical[j + 1].Item2;

                            if (targetPathEnC)
                            {
                                Geometry.TargetRange(p0, p1, targetPos, isTp, out var targetPath);
                                intersection[0].AddRange(targetPath);
                            }
                            if (vortexPathEnC)
                            {
                                Geometry.VortexRange(p0, p1, vortexPos, vortexRad, isTp, out var vortexPath);
                                intersection[2].AddRange(vortexPath);
                            }
                            if (dangerPathEnC)
                            {
                                Geometry.DangerLine(p0, p1, Variable.CurrentMapID, j, isTp, out var dangerPath);
                                dangerPathSum.AddRange(dangerPath);
                            }
                        }
                    }

                    if (dangerPathEn)
                    {
                        Geometry.DefeatLine(Variable.PathPhysical, dangerPathSum, health, inDanger, Variable.CurrentMapID, out defeat[0], out distance[0]);
                        danger[0].AddRange(dangerPathSum.Select(x => x.Item1));
                    }
                }
            }

            if (closeLadleEn)
                Geometry.SqrDisToPoint(Variable.IndicatorPosition, Vector2.zero, targetPos, false, out _, out closest[1]);
            if (targetLadleEn)
                Geometry.TargetRange(Variable.IndicatorPosition, Vector2.zero, targetPos, false, out intersection[1]);
            if (vortexLadleEn)
                Geometry.VortexRange(Variable.IndicatorPosition, Vector2.zero, vortexPos, vortexRad, false, out intersection[3]);
            if (dangerLadleEn)
            {
                Geometry.DangerLine(Variable.IndicatorPosition, Vector2.zero, Variable.CurrentMapID, 0, false, out var dangerLadle);
                Geometry.DefeatLine(pathLadle, dangerLadle, health, inDanger, Variable.CurrentMapID, out defeat[1], out distance[1]);
                danger[1].AddRange(dangerLadle.Select(x => x.Item1));
            }

            if (dangerVortexEn)
            {
                Geometry.DangerSpiral(Variable.VortexParameters, Variable.CurrentMapID, out var dangerVortex);
                Geometry.DefeatSpiral(Variable.VortexParameters, dangerVortex, health, inDanger[0], out defeat[2], out distance[2]);
                danger[2].AddRange(dangerVortex.Select(x => x.Item1));
            }
        }
        #endregion
    }
}
