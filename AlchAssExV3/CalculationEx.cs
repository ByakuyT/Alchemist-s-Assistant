using AlchAssV3;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.Zones;
using System.Linq;
using UnityEngine;

namespace AlchAssExV3
{
    public static class CalculationEx
    {
        #region 自动控制计算
        /// <summary>
        /// 计算定量操作速度
        /// </summary>
        public static void GetQuantitativeSpeed()
        {
            if (VariableEx.EnableQuantitativeStirring)
            {
                var phase = Managers.RecipeMap.path.deletedGraphicsSegments;
                var progress = Managers.RecipeMap.path.segmentLengthToDeletePhysics;
                var curStr = phase + progress;
                VariableEx.StirringLength -= Mathf.Max(0f, curStr - VariableEx.StirringPrevious);
                VariableEx.StirringLength = Mathf.Max(0f, VariableEx.StirringLength);
                VariableEx.InputStirringLength = ($"{VariableEx.StirringLength}", false);
                VariableEx.StirSpeedQuantitative = FunctionEx.GetAutoControlSpeed(VariableEx.StirringLength);
                VariableEx.StirringPrevious = curStr;
            }
            else
                VariableEx.StirSpeedQuantitative = float.MaxValue;

            if (VariableEx.EnableQuantitativeLadling)
            {
                var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
                var curLad = indPos.magnitude;
                if (Managers.Ingredient.ladle.waterStream.isPouring)
                {
                    VariableEx.LadlingLength -= Mathf.Max(0f, VariableEx.LadlingPrevious - curLad);
                    VariableEx.LadlingLength = Mathf.Max(0f, VariableEx.LadlingLength);
                    VariableEx.InputLadlingLength = ($"{VariableEx.LadlingLength}", false);
                }
                VariableEx.LadleSpeedQuantitative = FunctionEx.GetAutoControlSpeed(VariableEx.LadlingLength);
                VariableEx.LadlingPrevious = curLad;
            }
            else
                VariableEx.LadleSpeedQuantitative = float.MaxValue;

            if (VariableEx.EnableQuantitativeRestoring)
            {
                var curRot = Managers.RecipeMap.indicatorRotation.Value;
                if (Managers.Ingredient.ladle.waterStream.isPouring)
                {
                    VariableEx.RestoringAngle -= Mathf.Max(0f, Mathf.Abs(Mathf.DeltaAngle(curRot, VariableEx.RestoringPrevious)));
                    VariableEx.RestoringAngle = Mathf.Max(0f, VariableEx.RestoringAngle);
                    VariableEx.InputRestoringAngle = ($"{VariableEx.RestoringAngle}", false);
                }
                VariableEx.LadleSpeedRestoring = FunctionEx.GetAutoControlSpeed(VariableEx.RestoringAngle);
                VariableEx.RestoringPrevious = curRot;
            }
            else
                VariableEx.LadleSpeedRestoring = float.MaxValue;
        }

        /// <summary>
        /// 计算效果制动速度
        /// </summary>
        public static void GetEffectSpeed()
        {
            if (Variable.TargetEffect != null)
            {
                var indRot = Managers.RecipeMap.indicatorRotation.Value;
                var targetRot = Variable.TargetEffect.transform.localEulerAngles.z;
                Vector2 targetPos = Variable.TargetEffect.transform.localPosition;
                VariableEx.DeviationRotation = Mathf.Abs(Mathf.DeltaAngle(indRot, targetRot)) / 3f * 25f;

                if (VariableEx.EnableEffectIntersection)
                {
                    var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
                    var dis = Vector2.Distance(indPos, targetPos) - VariableEx.EffectDeviation / 1800f;
                    if (dis > 0f)
                    {
                        VariableEx.SpeedEffect = Mathf.Max(FunctionEx.GetAutoControlSpeed(dis), VariableEx.AutoCrossSpeed.Value);
                        VariableEx.EffectIntersectionPrevious = true;
                    }
                    else if (VariableEx.EffectIntersectionPrevious)
                        VariableEx.SpeedEffect = float.MinValue;
                    else
                        VariableEx.SpeedEffect = FunctionEx.GetAutoControlSpeed(-dis);
                }
                else
                    VariableEx.SpeedEffect = float.MaxValue;

                if (VariableEx.EnableEffectAlignment)
                {
                    var disLd = Vector2.Distance(Variable.ClosestPositions[0], targetPos) - VariableEx.EffectDeviation / 1800f;
                    var disSt = Vector2.Distance(Variable.ClosestPositions[2], targetPos) - VariableEx.EffectDeviation / 1800f;
                    if (disSt > 0f)
                    {
                        VariableEx.StirSpeedEffect = Mathf.Max(FunctionEx.GetAutoControlSpeed(disSt), VariableEx.AutoCrossSpeed.Value);
                        VariableEx.EffectAlignmentStirPrevious = true;
                    }
                    else if (VariableEx.EffectAlignmentStirPrevious)
                        VariableEx.StirSpeedEffect = float.MinValue;
                    else
                        VariableEx.StirSpeedEffect = FunctionEx.GetAutoControlSpeed(-disSt);

                    if (disLd > 0f)
                    {
                        VariableEx.LadleSpeedEffect = Mathf.Max(FunctionEx.GetAutoControlSpeed(disLd), VariableEx.AutoCrossSpeed.Value);
                        VariableEx.EffectAlignmentLadlePrevious = true;
                    }
                    else if (VariableEx.EffectAlignmentLadlePrevious)
                        VariableEx.LadleSpeedEffect = float.MinValue;
                    else
                        VariableEx.LadleSpeedEffect = FunctionEx.GetAutoControlSpeed(-disLd);

                    VariableEx.HeatSpeedEffect = Mathf.Min(VariableEx.StirSpeedEffect, VariableEx.LadleSpeedEffect);
                }
                else
                {
                    VariableEx.StirSpeedEffect = float.MaxValue;
                    VariableEx.LadleSpeedEffect = float.MaxValue;
                    VariableEx.HeatSpeedEffect = float.MaxValue;
                }
            }
            else
            {
                VariableEx.DeviationRotation = 0f;
                VariableEx.SpeedEffect = float.MaxValue;
                VariableEx.StirSpeedEffect = float.MaxValue;
                VariableEx.LadleSpeedEffect = float.MaxValue;
                VariableEx.HeatSpeedEffect = float.MaxValue;
            }
        }

        /// <summary>
        /// 计算漩涡制动速度
        /// </summary>
        public static void GetVortexSpeed()
        {
            var mapindex = Variable.MapId[Managers.RecipeMap.currentMap.potionBase.name];
            if (mapindex != 2 && Variable.VortexIndex[mapindex] >= 0)
            {
                var selVortex = Variable.Vortexs[mapindex][Variable.VortexIndex[mapindex]];
                var vortexPos = new Vector2((float)selVortex.x, (float)selVortex.y);

                if (VariableEx.EnableVortexIntersection)
                {
                    var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
                    var dis = Vector2.Distance(indPos, vortexPos) - (float)selVortex.r;
                    if (dis > 0f)
                    {
                        VariableEx.SpeedVortex = Mathf.Max(FunctionEx.GetAutoControlSpeed(dis), VariableEx.AutoCrossSpeed.Value);
                        VariableEx.VortexIntersectionPrevious = true;
                    }
                    else if (VariableEx.VortexIntersectionPrevious)
                        VariableEx.SpeedVortex = float.MinValue;
                    else
                        VariableEx.SpeedVortex = FunctionEx.GetAutoControlSpeed(-dis);
                }
                else
                    VariableEx.SpeedVortex = float.MaxValue;

                if (VariableEx.EnableVortexAlignment)
                {
                    var disLd = Vector2.Distance(Variable.ClosestPositions[1], vortexPos) - (float)selVortex.r;
                    var disSt = Vector2.Distance(Variable.ClosestPositions[3], vortexPos) - (float)selVortex.r;
                    if (disSt > 0f)
                    {
                        VariableEx.StirSpeedVortex = Mathf.Max(FunctionEx.GetAutoControlSpeed(disSt), VariableEx.AutoCrossSpeed.Value);
                        VariableEx.VortexAlignmentStirPrevious = true;
                    }
                    else if (VariableEx.VortexAlignmentStirPrevious)
                        VariableEx.StirSpeedVortex = float.MinValue;
                    else
                        VariableEx.StirSpeedVortex = FunctionEx.GetAutoControlSpeed(-disSt);

                    if (disLd > 0f)
                    {
                        VariableEx.LadleSpeedVortex = Mathf.Max(FunctionEx.GetAutoControlSpeed(disLd), VariableEx.AutoCrossSpeed.Value);
                        VariableEx.VortexAlignmentLadlePrevious = true;
                    }
                    else if (VariableEx.VortexAlignmentLadlePrevious)
                        VariableEx.LadleSpeedVortex = float.MinValue;
                    else
                        VariableEx.LadleSpeedVortex = FunctionEx.GetAutoControlSpeed(-disLd);

                    VariableEx.HeatSpeedVortex = Mathf.Min(VariableEx.StirSpeedVortex, VariableEx.LadleSpeedVortex);
                }
                else
                {
                    VariableEx.StirSpeedVortex = float.MaxValue;
                    VariableEx.LadleSpeedVortex = float.MaxValue;
                    VariableEx.HeatSpeedVortex = float.MaxValue;
                }
            }
            else
            {
                VariableEx.SpeedVortex = float.MaxValue;
                VariableEx.StirSpeedVortex = float.MaxValue;
                VariableEx.LadleSpeedVortex = float.MaxValue;
                VariableEx.HeatSpeedVortex = float.MaxValue;
            }
        }
        #endregion

        /// <summary>
        /// 计算骷髅制动速度
        /// </summary>
        public static void GetDangerSpeed()
        {
            if (VariableEx.EnableDangerIntersection)
            {
                var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
                var disList = Variable.DangerPositions.Where(list => list.Any()).Select(list => list[0]).Select(pos => Vector2.Distance(indPos, pos));
                if (disList.Any())
                {
                    var dis = disList.Min();
                    if (ZonePart.GetZonesActivePartsCount(typeof(StrongDangerZonePart)) + ZonePart.GetZonesActivePartsCount(typeof(StrongDangerZonePart)) > 0)
                    {
                        VariableEx.SpeedDanger = Mathf.Max(FunctionEx.GetAutoControlSpeed(dis), VariableEx.AutoCrossSpeed.Value);
                        VariableEx.DangerIntersectionPrevious = true;
                    }
                    else if (VariableEx.DangerIntersectionPrevious)
                        VariableEx.SpeedDanger = float.MinValue;
                    else
                        VariableEx.SpeedDanger = FunctionEx.GetAutoControlSpeed(dis);
                }
                else if (VariableEx.DangerIntersectionPrevious)
                    VariableEx.SpeedDanger = float.MinValue;
                else
                    VariableEx.SpeedDanger = float.MaxValue;
            }
            else
                VariableEx.SpeedDanger = float.MaxValue;

            if (VariableEx.EnableDangerAlignment)
            {
                var disSt = VariableEx.HealthThreshold / 100f + (float)Variable.DangerDistancePath * 0.4f - 1f;
                var disLd = VariableEx.HealthThreshold / 100f + (float)Variable.DangerDistanceLadle * 0.4f - 1f;
                var disHt = VariableEx.HealthThreshold / 100f + (float)Variable.DangerDistanceVortex * 0.4f - 1f;

                float speedSt = float.MaxValue, speedLd = float.MaxValue, speedHt = float.MaxValue;
                if (Variable.DangerDistancePath > 0)
                {
                    if (disSt > 0f)
                    {
                        speedSt = VariableEx.DangerOutStirPrevious ? float.MinValue : Mathf.Max(FunctionEx.GetAutoControlSpeed(disSt), VariableEx.AutoCrossSpeed.Value);
                        VariableEx.DangerAlignmentStirPrevious = true;
                    }
                    else if (VariableEx.DangerAlignmentStirPrevious)
                        speedSt = float.MinValue;
                    else
                        speedSt = FunctionEx.GetAutoControlSpeed(-disSt);
                }
                else
                {
                    if (VariableEx.DangerAlignmentStirPrevious)
                        speedSt = float.MinValue;
                    VariableEx.DangerOutStirPrevious = true;
                }

                if (Variable.DangerDistanceLadle > 0)
                {
                    if (disLd > 0f)
                    {
                        speedLd = VariableEx.DangerOutLadlePrevious ? float.MinValue : Mathf.Max(FunctionEx.GetAutoControlSpeed(disLd), VariableEx.AutoCrossSpeed.Value);
                        VariableEx.DangerAlignmentLadlePrevious = true;
                    }
                    else if (VariableEx.DangerAlignmentLadlePrevious)
                        speedLd = float.MinValue;
                    else
                        speedLd = FunctionEx.GetAutoControlSpeed(-disLd);
                }
                else
                {
                    if (VariableEx.DangerAlignmentLadlePrevious)
                        speedLd = float.MinValue;
                    VariableEx.DangerOutLadlePrevious = true;
                }

                if (Variable.DangerDistanceVortex > 0)
                {
                    if (disHt > 0f)
                    {
                        speedHt = VariableEx.DangerOutHeatPrevious ? float.MinValue : Mathf.Max(FunctionEx.GetAutoControlSpeed(disHt), VariableEx.AutoCrossSpeed.Value);
                        VariableEx.DangerAlignmentHeatPrevious = true;
                    }
                    else if (VariableEx.DangerAlignmentHeatPrevious)
                        speedHt = float.MinValue;
                    else
                        speedHt = FunctionEx.GetAutoControlSpeed(-disHt);
                }
                else
                {
                    if (VariableEx.DangerAlignmentHeatPrevious)
                        speedHt = float.MinValue;
                    VariableEx.DangerOutHeatPrevious = true;
                }

                VariableEx.StirSpeedDanger = Mathf.Min(speedLd, speedHt);
                VariableEx.LadleSpeedDanger = Mathf.Min(speedSt, speedHt);
                VariableEx.HeatSpeedDanger = Mathf.Min(speedSt, speedLd);
            }
            else
            {
                VariableEx.StirSpeedDanger = float.MaxValue;
                VariableEx.LadleSpeedDanger = float.MaxValue;
                VariableEx.HeatSpeedDanger = float.MaxValue;
            }
        }

        /// <summary>
        /// 计算沼泽制动速度
        /// </summary>
        public static void GetSwampSpeed()
        {
            if (VariableEx.EnableSwampIntersection)
            {
                if (Variable.SwampPositions.Any())
                {
                    var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
                    var dis = Vector2.Distance(indPos, Variable.SwampPositions[0]);
                    if (ZonePart.GetZonesActivePartsCount(typeof(SwampZonePart)) > 0)
                    {
                        VariableEx.SpeedSwamp = Mathf.Max(FunctionEx.GetAutoControlSpeed(dis), VariableEx.AutoCrossSpeed.Value);
                        VariableEx.SwampIntersectionPrevious = true;
                    }
                    else if (VariableEx.SwampIntersectionPrevious)
                        VariableEx.SpeedSwamp = float.MinValue;
                    else
                        VariableEx.SpeedSwamp = FunctionEx.GetAutoControlSpeed(dis);
                }
                else if (VariableEx.SwampIntersectionPrevious)
                    VariableEx.SpeedSwamp = float.MinValue;
                else
                    VariableEx.SpeedSwamp = float.MaxValue;
            }
            else
                VariableEx.SpeedSwamp = float.MaxValue;

            if (VariableEx.EnableSwampAlignment)
            {
                if (Variable.DistanceSwamp > 0)
                {
                    VariableEx.LadleSpeedSwamp = VariableEx.SwampOutStirPrevious ? float.MinValue : Mathf.Max(FunctionEx.GetAutoControlSpeed((float)Variable.DistanceSwamp), VariableEx.AutoCrossSpeed.Value);
                    VariableEx.SwampAlignmentStirPrevious = true;
                }
                else
                {
                    VariableEx.LadleSpeedSwamp = VariableEx.SwampAlignmentStirPrevious ? float.MinValue : float.MaxValue;
                    VariableEx.SwampOutStirPrevious = true;
                }

                VariableEx.HeatSpeedSwamp = VariableEx.LadleSpeedSwamp;
            }
            else
            {
                VariableEx.LadleSpeedSwamp = float.MaxValue;
                VariableEx.HeatSpeedSwamp = float.MaxValue;
            }
        }
    }
}
