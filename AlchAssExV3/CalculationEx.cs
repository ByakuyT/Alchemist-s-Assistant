using AlchAssV3;
using HarmonyLib;
using PotionCraft.ManagersSystem;
using UnityEngine;

namespace AlchAssExV3
{
    public static class CalculationEx
    {
        #region 制动计算
        /// <summary>
        /// 最近点制动
        /// </summary>
        public static void GetClosestControl()
        {
            VariableEx.ClosestSpeed = [float.MaxValue, float.MaxValue];
            if (VariableEx.EnableClosestControl && Variable.TargetEffect != null)
            {
                var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
                for (var i = 0; i < 2; i++)
                {
                    if (!float.IsNaN(Variable.ClosestPositions[i].x))
                    {
                        var dist = Vector2.Distance(indPos, Variable.ClosestPositions[i]);
                        VariableEx.ClosestSpeed[i] = FunctionEx.GetControlSpeed(dist);
                    }
                    else
                        VariableEx.ClosestSpeed[i] = float.MaxValue;
                }
            }
        }

        /// <summary>
        /// 接近点制动
        /// </summary>
        public static void GetProximityControl()
        {
            VariableEx.ProximitySpeed = [float.MaxValue, float.MaxValue];
            if (VariableEx.EnableProximityControl && Variable.TargetEffect != null)
            {
                var tarPos = Variable.TargetEffect.transform.localPosition;
                for (var i = 0; i < 2; i++)
                {
                    if (!float.IsNaN(Variable.ClosestPositions[1 - i].x))
                    {
                        var dist = Vector2.Distance(tarPos, Variable.ClosestPositions[1 - i]);
                        VariableEx.ProximitySpeed[i] = FunctionEx.GetControlSpeed(dist);
                    }
                    else
                        VariableEx.ProximitySpeed[i] = float.MaxValue;
                }
            }
        }

        /// <summary>
        /// 漩涡制动
        /// </summary>
        public static void GetEdgeControl()
        {
            if (VariableEx.EnableEdgeControl)
            {
                var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
                if (Managers.RecipeMap.CurrentVortexMapItem != null)
                {
                    var vorPos = Managers.RecipeMap.CurrentVortexMapItem.thisTransform.localPosition;
                    var vorRad = ((CircleCollider2D)Traverse.Create(Managers.RecipeMap.CurrentVortexMapItem).Field("vortexCollider").GetValue()).radius + 0.74f;
                    var dist = vorRad - Vector2.Distance(vorPos, indPos);
                    if ((indPos - VariableEx.EnterPosition).sqrMagnitude > 1e-5)
                        VariableEx.EdgeSpeed = FunctionEx.GetControlSpeed(dist);
                    return;
                }
                var mapid = Variable.MapId[Managers.RecipeMap.currentMap.potionBase.name];
                if (mapid != 2 && Variable.VortexIndex[mapid] >= 0)
                {
                    var vorSel = Variable.Vortexs[mapid][Variable.VortexIndex[mapid]];
                    var dist = Vector2.Distance(new((float)vorSel.x, (float)vorSel.y), indPos) - (float)vorSel.r;
                    VariableEx.EdgeSpeed = Mathf.Max(VariableEx.ControlEnterSpeed.Value, FunctionEx.GetControlSpeed(dist));
                    return;
                }
            }
            VariableEx.EdgeSpeed = float.MaxValue;
        }
        #endregion

        #region 定量速度计算
        /// <summary>
        /// 计算定量搅拌速度
        /// </summary>
        public static void GetStirSet()
        {
            VariableEx.StirSetSpeed = float.MaxValue;
            if (VariableEx.EnableStirSet)
            {
                var stage = Managers.RecipeMap.path.deletedGraphicsSegments + Managers.RecipeMap.path.segmentLengthToDeletePhysics;
                var dist = Mathf.Max(VariableEx.StirSetTarget - stage, 0f);
                VariableEx.StirSetSpeed = FunctionEx.GetControlSpeed(dist);
            }
        }

        /// <summary>
        /// 计算定量搅拌速度
        /// </summary>
        public static void GetLadleSet()
        {
            VariableEx.LadleSetSpeed = float.MaxValue;
            if (VariableEx.EnableLadleSet)
            {
                var indis = (Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset).magnitude;
                var dist = Mathf.Max(indis - VariableEx.LadleSetTarget, 0f);
                VariableEx.LadleSetSpeed = FunctionEx.GetControlSpeed(dist);
            }
        }
        #endregion
    }
}
