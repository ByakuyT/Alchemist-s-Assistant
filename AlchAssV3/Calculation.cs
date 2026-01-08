using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Mortar;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.Zones;
using PotionCraft.LocalizationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PotionCraft.Markers;

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
            string devTotText = LocalizationManager.GetText("AlchAssV3_unavailable");
            string devPosText = LocalizationManager.GetText("AlchAssV3_unavailable");
            string closestDirText = LocalizationManager.GetText("AlchAssV3_unavailable");
            string deltaAngleText = LocalizationManager.GetText("AlchAssV3_unavailable");
            string lifeSaltText = LocalizationManager.GetText("AlchAssV3_unavailable");

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
                deltaAngleText = $"{deltaAng}°";
            }

            if (!double.IsNaN(Variable.DangerDistance[0]))
                lifeSaltText = Function.FormatLifeSalt(Variable.DangerDistance[0]);
            return $"""
                {LocalizationManager.GetText("AlchAssV3_subtitle_total_deviation")}{devTotText}
                {LocalizationManager.GetText("AlchAssV3_subtitle_position_deviation")}{devPosText}
                {LocalizationManager.GetText("AlchAssV3_subtitle_closest_point_direction")}{closestDirText}
                {LocalizationManager.GetText("AlchAssV3_subtitle_delta_angle_to_target")}{deltaAngleText}
                {LocalizationManager.GetText("AlchAssV3_subtitle_life_salt")}{lifeSaltText}
                """;
        }

        /// <summary>
        /// 计算加水信息
        /// </summary>
        public static string CalculateLadle()
        {
            string devTotText = LocalizationManager.GetText("AlchAssV3_unavailable");
            string devPosText = LocalizationManager.GetText("AlchAssV3_unavailable");
            string closestDirText = LocalizationManager.GetText("AlchAssV3_unavailable");
            string deltaAngleText = LocalizationManager.GetText("AlchAssV3_unavailable");
            string lifeSaltText = LocalizationManager.GetText("AlchAssV3_unavailable");

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
                var deltaAngle = Mathf.DeltaAngle((float)Variable.LineDirections[1], (float)Variable.LineDirections[2]);
                deltaAngleText = $"{deltaAngle}°";
            }

            if (!double.IsNaN(Variable.DangerDistance[1]))
                lifeSaltText = Function.FormatLifeSalt(Variable.DangerDistance[1]);
            return $"""
                {LocalizationManager.GetText("AlchAssV3_subtitle_total_deviation")}{devTotText}
                {LocalizationManager.GetText("AlchAssV3_subtitle_position_deviation")}{devPosText}
                {LocalizationManager.GetText("AlchAssV3_subtitle_closest_point_direction")}{closestDirText}
                {LocalizationManager.GetText("AlchAssV3_subtitle_delta_angle_to_target")}{deltaAngleText}
                {LocalizationManager.GetText("AlchAssV3_subtitle_life_salt")}{lifeSaltText}
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
            var pathDir = double.IsNaN(Variable.LineDirections[0]) ? LocalizationManager.GetText("AlchAssV3_unavailable") : $"{(float)Variable.LineDirections[0]}°";
            var ladleDir = double.IsNaN(Variable.LineDirections[1]) ? LocalizationManager.GetText("AlchAssV3_unavailable") : $"{(float)Variable.LineDirections[1]}°";
            return $"""
                {LocalizationManager.GetText("AlchAssV3_subtitle_stir_progression")}{stir}
                {LocalizationManager.GetText("AlchAssV3_subtitle_path_direction")}{pathDir}
                {LocalizationManager.GetText("AlchAssV3_subtitle_pour_direction")}{ladleDir}
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
            var rotText = Function.FormatRotation(targetRot);
            var dirText = double.IsNaN(Variable.LineDirections[2]) ? LocalizationManager.GetText("AlchAssV3_unavailable") : $"{(float)Variable.LineDirections[2]}°";
            return $"""
                {LocalizationManager.GetText("AlchAssV3_subtitle_target_effect_id")}{targetId}
                {posText}
                {LocalizationManager.GetText("AlchAssV3_subtitle_rotation_salt")}{rotText}
                {LocalizationManager.GetText("AlchAssV3_subtitle_target_direction")}{dirText}
                """;
        }

        /// <summary>
        /// 计算位置信息
        /// </summary>
        public static string CalculatePosition()
        {
            var rot = Mathf.DeltaAngle(Variable.IndicatorRotation, 0f) / 9f * 25f;
            var posText = Function.FormatPosition(Variable.IndicatorPosition);
            var rotText = Function.FormatRotation(rot);
            return $"""
                {posText}
                {LocalizationManager.GetText("AlchAssV3_subtitle_rotation_salt")}{rotText}
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
                {LocalizationManager.GetText("AlchAssV3_subtitle_total_deviation")}<color=red>L{lvlTot}</color> {devTot}%
                {LocalizationManager.GetText("AlchAssV3_subtitle_position_deviation")}<color=red>L{lvlPos}</color> {devPos}%
                {LocalizationManager.GetText("AlchAssV3_subtitle_angle_deviation")}<color=red>L{lvlRot}</color> {devRot}%
                """;
        }

        /// <summary>
        /// 计算漩涡信息
        /// </summary>
        public static string CalculateVortex()
        {
            Vector2 vortexPosition;
            double maxDistance;
            double vortexDirection = double.NaN;
            double heatDirection = double.NaN;
            double dangerDistance = double.NaN;

            if (Managers.RecipeMap.CurrentVortexMapItem != null)
            {
                vortexPosition = Managers.RecipeMap.CurrentVortexMapItem.thisTransform.localPosition;
                maxDistance = ((CircleCollider2D)Traverse.Create(Managers.RecipeMap.CurrentVortexMapItem).Field("vortexCollider").GetValue()).radius + Variable.IndicatorRadius;
                vortexDirection = Vector2.SignedAngle(Vector2.right, vortexPosition - Variable.IndicatorPosition);
                heatDirection = GetVortexMoveDirection();
                dangerDistance = Variable.DangerDistance[2];
            }
            else
            {
                if (Variable.CurrentMapID == "Wine")
                    return "";

                var mapindex = Variable.CurrentMapID == "Water" ? 0 : 1;
                var vortexList = mapindex == 0 ? Variable.Vortex_Water : Variable.Vortex_Oil;
                if (Variable.VortexIndex[mapindex] < 0)
                    return "";

                var selectedVortex = vortexList[Variable.VortexIndex[mapindex]];
                vortexPosition = new Vector2((float)selectedVortex.x, (float)selectedVortex.y);
                maxDistance = selectedVortex.r;
                vortexDirection = Variable.LineDirections[3];
            }

            var distance = Vector2.Distance(vortexPosition, Variable.IndicatorPosition);
            var vortexDirectionText = double.IsNaN(vortexDirection) ? LocalizationManager.GetText("AlchAssV3_unavailable") : $"{(float)vortexDirection}°";
            var heatDirectionText=double.IsNaN(heatDirection)? LocalizationManager.GetText("AlchAssV3_unavailable") : $"{(float)heatDirection}°";
            var lifeSaltText = double.IsNaN(dangerDistance) ? LocalizationManager.GetText("AlchAssV3_unavailable") : Function.FormatLifeSalt(dangerDistance);
            return $"""
                {LocalizationManager.GetText("AlchAssV3_subtitle_current_vortex_distance")}{distance}
                {LocalizationManager.GetText("AlchAssV3_subtitle_maximal_vortex_distance")}{(float)maxDistance}
                {LocalizationManager.GetText("AlchAssV3_subtitle_vortex_direction")}{vortexDirectionText}
                {LocalizationManager.GetText("AlchAssV3_subtitle_heat_move_direction")}{heatDirectionText}
                {LocalizationManager.GetText("AlchAssV3_subtitle_life_salt")}{lifeSaltText}
                """;
        }

        /// <summary>
        /// 计算血量信息
        /// </summary>
        public static string CalculateHealth(float health)
        {
            return $"{LocalizationManager.GetText("AlchAssV3_subtitle_current_health")}{health * 100f}%";
        }

        /// <summary>
        /// 计算研磨信息
        /// </summary>
        public static string CalculateGrind(Mortar mortar)
        {
            if (mortar.ContainedStack == null)
                return "";
            return $"{LocalizationManager.GetText("AlchAssV3_subtitle_grinding_progression")}{mortar.ContainedStack.overallGrindStatus * 100f}%";
        }

        /// <summary>
        /// 计算调试信息
        /// </summary>
        public static string CalculateDebug()
        {
            //var pathHints=Managers.RecipeMap.path.fixedPathHints;
            //var _ox = pathHints.Count() == 0 ? -1 : pathHints[0].evenlySpacedPointsFixedPhysics.points[0].x;
            //var _oy = pathHints.Count() == 0 ? -1 : pathHints[0].evenlySpacedPointsFixedPhysics.points[0].y;
            //var ox = Managers.RecipeMap.indicator.thisTransform.position.x;
            //var oy = Managers.RecipeMap.indicator.thisTransform.position.y;
            var ox2 = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition.x;
            var oy2 = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition.y;
            var stage = Managers.RecipeMap.path.deletedGraphicsSegments;
            var progress = Managers.RecipeMap.path.segmentLengthToDeletePhysics;
            return $"""
                {Managers.RecipeMap.indicator.thisTransform.localPosition.x}, {Managers.RecipeMap.indicator.thisTransform.localPosition.y}
                ({ox2}, {oy2})
                {stage}
                {progress}
                """;
        }

        /// <summary>
        /// Generates a formatted string representation of the current input and mode variables.
        /// </summary>
        /// <remarks>The returned string includes values from the Variable.KeyMode, Variable.FloatInput,
        /// Variable.FloatInputStage, and Variable.FloatInputStream properties. The format is intended for diagnostic or
        /// logging purposes and may change if the structure of these variables changes.</remarks>
        /// <returns>A multi-line string containing the key mode, a comma-separated list of float inputs in brackets, the float
        /// input stage, and the float input stream.</returns>
        public static string CalculateIO()
        {
            string repr = "";
            foreach (var floatInput in Variable.FloatInput)
            {
                repr += $"{floatInput},";
            }
            repr = $"[{repr}]";
            return $"""
                {Variable.KeyMode}
                {repr}
                {Variable.FloatInputStage}
                {Variable.FloatInputStream}
                """;
        }


        /// <summary>
        /// Retrieves information about a specified hotkey, including its associated switch, display color, and message key.
        /// </summary>
        /// <remarks>This method does not throw exceptions for unrecognized keys; in such cases, the out
        /// parameters may be set to null or default values. The color output is determined by the current state of the
        /// associated switch.</remarks>
        /// <param name="key">The key identifying the hotkey for which information is requested.</param>
        /// <param name="sw">When this method returns, contains the switch associated with the specified hotkey, or null if the key is
        /// not recognized.</param>
        /// <param name="color">When this method returns, contains the display color for the hotkey's state. Set to "green" if the switch is
        /// active; otherwise, "red".</param>
        /// <param name="messageKey">When this method returns, contains the message key string corresponding to the specified hotkey.</param>
        private static void GetHotKeyInfo(Variable.SwitchDictionaryKey key, out Variable.Switch sw, out string color, out string messageKey)
        {
            sw = key switch
            {
                Variable.SwitchDictionaryKey.PathLine => Variable.SwitchPathLine,
                Variable.SwitchDictionaryKey.LadleLine => Variable.SwitchLadleLine,
                Variable.SwitchDictionaryKey.TargetLine => Variable.SwitchTargetLine,
                Variable.SwitchDictionaryKey.VortexLine => Variable.SwitchVortexLine,
                Variable.SwitchDictionaryKey.PathCurve => Variable.SwitchPathCurve,
                Variable.SwitchDictionaryKey.VortexCurve => Variable.SwitchVortexCurve,
                Variable.SwitchDictionaryKey.TargetRange => Variable.SwitchTargetRange,
                Variable.SwitchDictionaryKey.VortexRange => Variable.SwitchVortexRange,
                Variable.SwitchDictionaryKey.AreaTracking => Variable.SwitchAreaTracking,
                Variable.SwitchDictionaryKey.SwampScaling => Variable.SwitchSwampScaling,
                Variable.SwitchDictionaryKey.Transparency => Variable.SwitchTransparency,
                Variable.SwitchDictionaryKey.PolarMode => Variable.SwitchPolarMode,
                Variable.SwitchDictionaryKey.SaltDegreeMode => Variable.SwitchSaltDegreeMode,
                _ => throw new NotImplementedException()
            };
            color = sw.getState() ? "green" : "red";
            messageKey = key switch
            {
                Variable.SwitchDictionaryKey.PathLine => "AlchAssV3_message_path_line",
                Variable.SwitchDictionaryKey.LadleLine => "AlchAssV3_message_ladle_line",
                Variable.SwitchDictionaryKey.TargetLine => "AlchAssV3_message_target_line",
                Variable.SwitchDictionaryKey.VortexLine => "AlchAssV3_message_vortex_line",
                Variable.SwitchDictionaryKey.PathCurve => "AlchAssV3_message_path_curve",
                Variable.SwitchDictionaryKey.VortexCurve => "AlchAssV3_message_vortex_curve",
                Variable.SwitchDictionaryKey.TargetRange => "AlchAssV3_message_target_range",
                Variable.SwitchDictionaryKey.VortexRange => "AlchAssV3_message_vortex_range",
                Variable.SwitchDictionaryKey.AreaTracking => "AlchAssV3_message_area_tracking",
                Variable.SwitchDictionaryKey.SwampScaling => "AlchAssV3_message_swamp_scaling",
                Variable.SwitchDictionaryKey.Transparency => "AlchAssV3_message_transparency",
                Variable.SwitchDictionaryKey.PolarMode => "AlchAssV3_message_polar_mode",
                Variable.SwitchDictionaryKey.SaltDegreeMode => "AlchAssV3_message_salt_degree_mode",
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Generates a formatted string describing the current set of hotkey assignments based on the active key mode
        /// and context.
        /// </summary>
        /// <remarks>The returned string includes color-coded and localized descriptions for each hotkey,
        /// reflecting the current map, vortex, and effect selection states. The output is intended for display in a
        /// user interface to inform users of available hotkey actions and their corresponding keys.</remarks>
        /// <returns>A string containing the hotkey descriptions and their current assignments, formatted for display. The
        /// content and formatting depend on the current key mode and related state.</returns>
        public static string CalculateHotkey()
        {
            string repr = "";
            switch (Variable.KeyMode)
            {
                case "Normal":
                    var mapIndex = Variable.CurrentMapID == "Water" ? 0 : Variable.CurrentMapID == "Oil" ? 1 : 2;
                    repr+= $"""
                        {LocalizationManager.GetText("AlchAssV3_hotkey_switch_key_mode")}{"Backslash"}{Environment.NewLine}
                        """;
                    foreach(Variable.SwitchDictionaryKey key in Enum.GetValues(typeof(Variable.SwitchDictionaryKey)))
                    {
                        try
                        {
                            GetHotKeyInfo(key, out var sw, out var color, out var messageKey);
                            repr += $"""
                            <color={color}>{LocalizationManager.GetText(messageKey)}</color>{Variable.SwitchKeyShortcuts[key].Value.Serialize()}{Environment.NewLine}
                            """;
                        }
                        catch (NotImplementedException)
                        {
                            // Ignore unrecognized keys.
                        }
                    }
                    repr+= $"""
                        <color={(mapIndex == 2 ? "blue" : Variable.VortexIndex[mapIndex] >= 0 ? "green" : "red")}>{LocalizationManager.GetText("AlchAssV3_hotkey_select_prev_vortex")}</color>{Variable.PrevVortexKeyShortcut.Value.Serialize()}
                        <color={(mapIndex == 2 ? "blue" : Variable.VortexIndex[mapIndex] >= 0 ? "green" : "red")}>{LocalizationManager.GetText("AlchAssV3_hotkey_select_next_vortex")}</color>{Variable.NextVortexKeyShortcut.Value.Serialize()}
                        <color={(mapIndex == 2 ? "blue" : Variable.VortexIndex[mapIndex] >= 0 ? "green" : "red")}>{LocalizationManager.GetText("AlchAssV3_hotkey_select_near_vortex")}</color>{Variable.NearestVortexKeyShortcut.Value.Serialize()}
                        <color={(mapIndex == 2 ? "blue" : Variable.VortexIndex[mapIndex] >= 0 ? "red" : "green")}>{LocalizationManager.GetText("AlchAssV3_hotkey_select_no_vortex")}</color>{Variable.UnselectVortexKeyShortcut.Value.Serialize()}
                        <color={(Variable.TargetEffect == null ? "red" : "green")}>{LocalizationManager.GetText("AlchAssV3_hotkey_select_effect")}</color>{Variable.SelectEffectKeyShortcut.Value.Serialize()}
                        """;
                    break;
                case "Input":
                    repr = $"""
                        {LocalizationManager.GetText("AlchAssV3_hotkey_switch_key_mode")}{"Backslash"}
                        Input number: number keys.
                        Input decimal dot: Period key.
                        Input negative sign: Comma key.
                        Submit number: RightBracket.
                        Unsubmit all numbers: LeftBracket.
                        <color={(Variable.FloatInput.Count < 2 ? "red" : "blue")}>Require 2 submitted number, index and angle:</color>
                        Submit Auxiliary line: Return key.
                        <color={(Variable.FloatInput.Count < 1 ? "red" : "blue")}>require 1 submitted number, index:</color>
                        Unsubmit Auxiliary line: Return key + RightControl key.
                        """;
                    break;
                default:
                    break;
            }
            return repr;
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

        public static double GetVortexMoveDirection()
        {
            // Wine base have no vortex.
            if (Variable.CurrentMapID == "Wine")
                return double.NaN;
            // bottle not currently touch a vortex.
            var curVortex = Managers.RecipeMap.CurrentVortexMapItem;
            if (curVortex == null)
                return double.NaN;

            Vector2 vortexPos = curVortex.thisTransform.localPosition;
            var maxDis = ((CircleCollider2D)Traverse.Create(curVortex).Field("vortexCollider").GetValue()).radius + Variable.IndicatorRadius;
            var distance = Vector2.Distance(vortexPos, Variable.IndicatorPosition);
            if (distance > maxDis + 1e-5)
                return double.NaN;
            var v = Variable.IndicatorPosition - vortexPos;
            var num1 = v.magnitude;
            if (num1 < 1e-5)
            {
                return double.NaN;
            }
            var rot = Math.Atan2(num1, Variable.VortexCurveA);
            var num2 = Vector2.SignedAngle(Vector2.right, v) + rot * 180f / Mathf.PI - 180f;
            return num2 > 180f ? num2 - 360f : num2 < -180f ? num2 + 360f : num2;
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
            // Variable.BaseRenderPosition won't update immediately after reload.
            // var yDev = Variable.BaseRenderPosition.y - Variable.IndicatorPosition.y;
            var yDev = Variable.CurrentMapID == "Water" ? -130f : Variable.CurrentMapID == "Oil" ? -283f : -391f;
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
            if (!Variable.SwitchPathCurve.getState())
                return;

            var pathHints = Managers.RecipeMap.path.fixedPathHints;
            if (pathHints.Count == 0)
                return;

            var mapTrans = Managers.RecipeMap.currentMap.referencesContainer.transform;
            var pathTrans = Managers.RecipeMap.path.thisTransform;
            var yDev = Variable.CurrentMapID == "Water" ? -130f : Variable.CurrentMapID == "Oil" ? -283f : -391f;
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
                if (Variable.SwitchSwampScaling.getState() && Variable.CurrentMapID == "Oil")
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
            if (!Variable.SwitchVortexCurve.getState())
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

            var yDev = Variable.CurrentMapID == "Water" ? -130f : Variable.CurrentMapID == "Oil" ? -283f : -391f;
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
            //var yDev = Variable.CurrentMapID == "Water" ? -130f : Variable.CurrentMapID == "Oil" ? -283f : -391f;
            Points = [.. Points.AddItem(new Vector3(Variable.IndicatorPosition.x, yDev + Variable.IndicatorPosition.y, 0))];
        }

        /// <summary>
        /// 生成边界圈
        /// </summary>
        public static void InitRange(double rad, double cx, double cy, out Vector3[] Points)
        {
            var r = rad - Variable.LineWidth.Value * 0.5;
            var yDev = Variable.CurrentMapID == "Water" ? -130f : Variable.CurrentMapID == "Oil" ? -283f : -391f;
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

            var closePathEn = Variable.SwitchPathCurve.getState() && targetVaild;
            var closeLadleEn = Variable.SwitchLadleLine.getState() && targetVaild;
            var targetPathEn = Variable.SwitchPathTargetPoint.getState() && targetVaild;
            var targetLadleEn = Variable.SwitchLadleTargetPoint.getState() && targetVaild;
            var vortexPathEn = Variable.SwitchPathVortexPoint.getState() && vortexVaild;
            var vortexLadleEn = Variable.SwitchLadleVortexPoint.getState() && vortexVaild;
            var dangerPathEn = Variable.SwitchPathDangerPoint.getState();
            var dangerLadleEn = Variable.SwitchLadleDangerPoint.getState();
            var dangerVortexEn = Variable.SwitchVortexDangerPoint.getState() && vortexIn;

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
