using System;
using System.Collections.Generic;
using System.Linq;
using PotionCraft.ManagersSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlchAssV4;

public static class Rendering
{
    #region 生成材质

    /// <summary>
    /// 生成实线材质
    /// </summary>
    private static Material CreateSolidMaterial()
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return new Material(Shader.Find("Sprites/Default")) { mainTexture = texture };
    }

    /// <summary>
    /// 生成虚线材质
    /// </summary>
    private static Material CreateDashedMaterial()
    {
        var texture = new Texture2D(10, 1);
        var pixels = new Color[10];
        for (var x = 0; x < 10; x++)
            pixels[x] = x < 5 ? Color.white : Color.clear;
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        return new Material(Shader.Find("Sprites/Default")) { mainTexture = texture };
    }

    /// <summary>
    /// 生成矩形精灵
    /// </summary>
    private static Sprite CreateSquareSprite()
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }

    /// <summary>
    /// 生成圆形精灵
    /// </summary>
    private static Sprite CreateRoundSprite()
    {
        var texture = new Texture2D(500, 500);
        var pixels = new Color[250000];
        var center = new Variable.Vec2D(249.5, 249.5);
        for (var y = 0; y < 500; y++)
        for (var x = 0; x < 500; x++)
        {
            var pos = new Variable.Vec2D(x, y);
            var dis2 = (pos - center).SqrMagnitude;
            pixels[y * 500 + x] = dis2 <= 62500 ? Color.white : Color.clear;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        var pixelsPerUnit = 250 / Variable.LineWidth.Value;
        return Sprite.Create(texture, new Rect(0, 0, 500, 500), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    /// <summary>
    /// 生成材质和精灵
    /// </summary>
    public static void CreateMaterialAndSprites()
    {
        Variable.SolidMaterial = CreateSolidMaterial();
        Variable.DashedMaterial = CreateDashedMaterial();
        Variable.SquareSprite = CreateSquareSprite();
        Variable.RoundSprite = CreateRoundSprite();
    }

    #endregion

    #region 更新渲染器

    /// <summary>
    /// 生成曲线渲染器
    /// </summary>
    private static void InitLineRenderer(Material material, Color color, int order, out LineRenderer curve)
    {
        var obj = new GameObject("Renderer") { layer = 8 };
        curve = obj.AddComponent<LineRenderer>();
        curve.textureMode = LineTextureMode.Tile;
        curve.startWidth = Variable.LineWidth.Value;
        curve.endWidth = Variable.LineWidth.Value;
        curve.sortingLayerName = "RecipeMapIndicator";
        curve.sortingOrder = order;
        curve.sharedMaterial = material;
        curve.startColor = color;
        curve.endColor = color;
    }

    /// <summary>
    /// 生成精灵渲染器
    /// </summary>
    private static void InitSpriteRenderer(Sprite material, Color color, int order, out SpriteRenderer sprite)
    {
        var obj = new GameObject("Renderer") { layer = 8 };
        sprite = obj.AddComponent<SpriteRenderer>();
        sprite.sortingLayerName = "RecipeMapIndicator";
        sprite.sortingOrder = order;
        sprite.sprite = material;
        sprite.color = color;
        sprite.transform.localScale = new(Variable.NodeSize.Value, Variable.NodeSize.Value, 1);
    }

    #endregion

    #region 渲染元素

    /// <summary>
    /// 渲染点
    /// </summary>
    public static void SetNodeRenderers()
    {
        bool[] ClosestEnables = [Variable.DoPathCurve, Variable.DoPathCurve, Variable.DoLines[1], Variable.DoLines[1]];
        bool[] IntersectionEnables = [Variable.DoPathEffectPoint, Variable.DoLadleEffectPoint, Variable.DoPathVortexPoint, Variable.DoLadleVortexPoint];
        bool[] DangerEnables = [Variable.DoPathDangerPoint, Variable.DoLadleDangerPoint, Variable.DoVortexDangerPoint];
        var mapTrans = Managers.RecipeMap.currentMap.referencesContainer.transform;
        for (var i = 0; i < 4; i++)
        {
            if (ClosestEnables[i] && Variable.ClosestPositions[i].HasValue)
            {
                var posDev = mapTrans.TransformPoint(Variable.ClosestPositions[i].Value.ToVec3());
                if (Variable.ClosestPoints[i] == null)
                    InitSpriteRenderer(Variable.SquareSprite, Variable.ColorClosest.Value, 4, out Variable.ClosestPoints[i]);
                Variable.ClosestPoints[i].transform.position = posDev;
            }
            else if (Variable.ClosestPoints[i] != null)
                Object.Destroy(Variable.ClosestPoints[i].gameObject);

            if (IntersectionEnables[i] && Variable.IntersectionPositions[i].Count > 0)
            {
                for (var j = 0; j < Variable.IntersectionPositions[i].Count; j++)
                {
                    var posDev = mapTrans.TransformPoint(Variable.IntersectionPositions[i][j].ToVec3());
                    if (Variable.IntersectionPoints[i].Count > j)
                        Variable.IntersectionPoints[i][j].transform.position = posDev;
                    else
                    {
                        InitSpriteRenderer(Variable.SquareSprite, Variable.ColorIntersection.Value, 4, out var point);
                        point.transform.position = posDev;
                        Variable.IntersectionPoints[i].Add(point);
                    }
                }

                while (Variable.IntersectionPoints[i].Count > Variable.IntersectionPositions[i].Count)
                {
                    Object.Destroy(Variable.IntersectionPoints[i].Last().gameObject);
                    Variable.IntersectionPoints[i].RemoveAt(Variable.IntersectionPoints[i].Count - 1);
                }
            }
            else
            {
                foreach (var point in Variable.IntersectionPoints[i])
                    Object.Destroy(point.gameObject);
                Variable.IntersectionPoints[i].Clear();
            }
        }

        for (var i = 0; i < 3; i++)
        {
            if (DangerEnables[i] && Variable.DefeatPositions[i].HasValue)
            {
                var posDev = mapTrans.TransformPoint(Variable.DefeatPositions[i].Value.ToVec3());
                if (Variable.DefeatPoints[i] == null)
                    InitSpriteRenderer(Variable.SquareSprite, Variable.ColorDefeat.Value, 4, out Variable.DefeatPoints[i]);
                Variable.DefeatPoints[i].transform.position = posDev;
            }
            else if (Variable.DefeatPoints[i] != null)
                Object.Destroy(Variable.DefeatPoints[i].gameObject);

            if (DangerEnables[i] && Variable.DangerPositions[i].Count > 0)
            {
                for (var j = 0; j < Variable.DangerPositions[i].Count; j++)
                {
                    var posDev = mapTrans.TransformPoint(Variable.DangerPositions[i][j].ToVec3());
                    if (Variable.DangerPoints[i].Count > j)
                        Variable.DangerPoints[i][j].transform.position = posDev;
                    else
                    {
                        InitSpriteRenderer(Variable.SquareSprite, Variable.ColorIntersection.Value, 4, out var point);
                        point.transform.position = posDev;
                        Variable.DangerPoints[i].Add(point);
                    }
                }

                while (Variable.DangerPoints[i].Count > Variable.DangerPositions[i].Count)
                {
                    Object.Destroy(Variable.DangerPoints[i].Last().gameObject);
                    Variable.DangerPoints[i].RemoveAt(Variable.DangerPoints[i].Count - 1);
                }
            }
            else
            {
                foreach (var point in Variable.DangerPoints[i])
                    Object.Destroy(point.gameObject);
                Variable.DangerPoints[i].Clear();
            }
        }

        if (Variable.DoSwampPoint && Variable.SwampPositions.Count > 0)
        {
            for (var i = 0; i < Variable.SwampPositions.Count; i++)
            {
                var posDev = mapTrans.TransformPoint(Variable.SwampPositions[i].ToVec3());
                if (Variable.SwampPoints.Count > i)
                    Variable.SwampPoints[i].transform.position = posDev;
                else
                {
                    InitSpriteRenderer(Variable.SquareSprite, Variable.ColorIntersection.Value, 4, out var point);
                    point.transform.position = posDev;
                    Variable.SwampPoints.Add(point);
                }
            }

            while (Variable.SwampPoints.Count > Variable.SwampPositions.Count)
            {
                Object.Destroy(Variable.SwampPoints.Last().gameObject);
                Variable.SwampPoints.RemoveAt(Variable.SwampPoints.Count - 1);
            }
        }
        else
        {
            foreach (var point in Variable.SwampPoints)
                Object.Destroy(point.gameObject);
            Variable.SwampPoints.Clear();
        }

        for (var i = 0; i < 2; i++)
        {
            if (Variable.HandoffPositions[i].HasValue)
            {
                var posDev = mapTrans.TransformPoint(Variable.HandoffPositions[i].Value.ToVec3());
                if (Variable.HandoffPoints[i] == null)
                    InitSpriteRenderer(Variable.SquareSprite, Variable.ColorHandoff.Value, 4, out Variable.HandoffPoints[i]);
                Variable.HandoffPoints[i].transform.position = posDev;
            }
            else if (Variable.HandoffPoints[i] != null)
                Object.Destroy(Variable.HandoffPoints[i].gameObject);
        }
    }

    /// <summary>
    /// 渲染直线
    /// </summary>
    public static void SetLineRenderers()
    {
        Variable.BaseLadleRenderer.enabled = !Variable.DoLines[1];
        for (var i = 0; i < 5; i++)
        {
            if (Variable.DoLines[i])
            {
                InitLine(Variable.LineDirections[i], out var points);
                if (points.Length == 2)
                {
                    if (Variable.Lines[i] == null)
                        InitLineRenderer(Variable.SolidMaterial, Variable.ColorLines[i].Value, 3, out Variable.Lines[i]);
                    Variable.Lines[i].SetPositions(points);
                }
                else if (Variable.Lines[i] != null)
                    Object.Destroy(Variable.Lines[i].gameObject);
            }
            else if (Variable.Lines[i] != null)
                Object.Destroy(Variable.Lines[i].gameObject);
        }
    }

    /// <summary>
    /// 渲染曲线
    /// </summary>
    public static void SetCurveRenderers()
    {
        SetOriginalPathsVisible(!Variable.DoPathCurve);
        if (Variable.DoPathCurve && Variable.PathGraphical.Count > 0)
        {
            for (var i = 0; i < Variable.PathGraphical.Count; i++)
            {
                var points = Variable.PathGraphical[i].Item1;
                var isTp = Variable.PathGraphical[i].Item2;
                var material = isTp ? Variable.DashedMaterial : Variable.SolidMaterial;
                var color = Variable.ColorPaths[i % 2].Value;
                if (Variable.PathCurves.Count > i)
                    Variable.PathCurves[i].SetPositions(points);
                else
                {
                    InitLineRenderer(material, color, 2, out var curve);
                    curve.SetPositions(points);
                    Variable.PathCurves.Add(curve);
                }
            }

            while (Variable.PathCurves.Count > Variable.PathGraphical.Count)
            {
                Object.Destroy(Variable.PathCurves.Last().gameObject);
                Variable.PathCurves.RemoveAt(Variable.PathCurves.Count - 1);
            }
        }
        else
        {
            foreach (var curve in Variable.PathCurves)
                Object.Destroy(curve.gameObject);
            Variable.PathCurves.Clear();
        }

        if (Variable.DoVortexCurve && Variable.SpiralGraphical.Length >= 2)
        {
            if (Variable.VortexCurve == null)
                InitLineRenderer(Variable.SolidMaterial, Variable.ColorSpiral.Value, 2, out Variable.VortexCurve);
            Variable.VortexCurve.SetPositions(Variable.SpiralGraphical);
        }
        else if (Variable.VortexCurve != null)
            Object.Destroy(Variable.VortexCurve.gameObject);
    }

    /// <summary>
    /// 渲染范围圈
    /// </summary>
    public static void SetRangeRenderers()
    {
        var mapIndex = Variable.MapId[Managers.RecipeMap.currentMap.potionBase.name];
        if (Variable.DoVortexRange && mapIndex != 2 && Variable.VortexIndex[mapIndex] >= 0)
        {
            var selVortex = Variable.Vortices[mapIndex][Variable.VortexIndex[mapIndex]];
            InitRange(selVortex.Rad - Variable.LineWidth.Value * 0.5, selVortex.Pos, out var points);
            if (Variable.VortexRange == null)
                InitLineRenderer(Variable.SolidMaterial, Variable.ColorRange.Value, 1, out Variable.VortexRange);
            Variable.VortexRange.SetPositions(points);
        }
        else if (Variable.VortexRange != null)
            Object.Destroy(Variable.VortexRange.gameObject);

        if (Variable.DoEffectRange && Variable.TargetEffect != null)
        {
            var effectPos = Variable.Vec2D.From(Variable.TargetEffect.transform.localPosition);
            var effectRot = Variable.TargetEffect.transform.localEulerAngles.z;
            var indRot = Managers.RecipeMap.indicatorRotation.Value;
            var devRot = Math.Abs(Algebra.DeltaAngleDeg(indRot, effectRot));
            var cen = effectPos + Variable.Offset;
            var mapTrans = Managers.RecipeMap.currentMap.referencesContainer.transform;
            var posDev = mapTrans.TransformPoint(cen.ToVec3());
            double[] rads = [1.53, 1.0 / 3.0 - devRot / 216.0, 1.0 / 18.0 - devRot / 216.0];
            double[] radl = [.. rads.Select(x => x - Variable.LineWidth.Value * 0.5)];
            InitRange(radl[0], effectPos, out var pointsOut);
            if (Variable.EffectRangeOuter == null)
                InitLineRenderer(Variable.SolidMaterial, Variable.ColorRange.Value, 1, out Variable.EffectRangeOuter);
            Variable.EffectRangeOuter.SetPositions(pointsOut);
            if (rads[1] > Variable.LineWidth.Value)
            {
                if (Variable.EffectDiskMiddle != null)
                    Object.Destroy(Variable.EffectDiskMiddle.gameObject);
                InitRange(radl[1], cen, out var pointsMid);
                if (Variable.EffectRangeMiddle == null)
                    InitLineRenderer(Variable.SolidMaterial, Variable.ColorRange.Value, 1,
                        out Variable.EffectRangeMiddle);
                Variable.EffectRangeMiddle.SetPositions(pointsMid);
            }
            else if (rads[1] > 0)
            {
                if (Variable.EffectRangeMiddle != null)
                    Object.Destroy(Variable.EffectRangeMiddle.gameObject);
                var scale = (float)rads[1] / Variable.LineWidth.Value;
                if (Variable.EffectDiskMiddle == null)
                    InitSpriteRenderer(Variable.RoundSprite, Variable.ColorRange.Value, 1,
                        out Variable.EffectDiskMiddle);
                Variable.EffectDiskMiddle.transform.position = posDev;
                Variable.EffectDiskMiddle.transform.localScale = new(scale, scale, 1);
            }
            else
            {
                if (Variable.EffectRangeMiddle != null)
                    Object.Destroy(Variable.EffectRangeMiddle.gameObject);
                if (Variable.EffectDiskMiddle != null)
                    Object.Destroy(Variable.EffectDiskMiddle.gameObject);
            }

            if (rads[2] > 0)
            {
                var scale = (float)rads[2] / Variable.LineWidth.Value;
                if (Variable.EffectDiskInner == null)
                    InitSpriteRenderer(Variable.RoundSprite, Variable.ColorRange.Value, 1,
                        out Variable.EffectDiskInner);
                Variable.EffectDiskInner.transform.position = posDev;
                Variable.EffectDiskInner.transform.localScale = new(scale, scale, 1);
            }
            else if (Variable.EffectDiskInner != null)
                Object.Destroy(Variable.EffectDiskInner.gameObject);
        }
        else
        {
            if (Variable.EffectRangeOuter != null)
                Object.Destroy(Variable.EffectRangeOuter.gameObject);
            if (Variable.EffectRangeMiddle != null)
                Object.Destroy(Variable.EffectRangeMiddle.gameObject);
            if (Variable.EffectDiskMiddle != null)
                Object.Destroy(Variable.EffectDiskMiddle.gameObject);
            if (Variable.EffectDiskInner != null)
                Object.Destroy(Variable.EffectDiskInner.gameObject);
        }
    }

    /// <summary>
    /// 渲染指示器
    /// </summary>
    public static void SetIndicatorRenderers()
    {
        SetOriginalIndicatorVisible(!Variable.DoTransparency);
        if (Variable.DoTransparency)
        {
            var indPos = Variable.Vec2D.From(Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition) + Variable.Offset;
            var indRot = -Managers.RecipeMap.indicatorRotation.Value * Math.PI / 180.0;
            var mapTrans = Managers.RecipeMap.currentMap.referencesContainer.transform;
            var delta = new Variable.Vec2D(Variable.IndicatorRad * Math.Sin(indRot), Variable.IndicatorRad * Math.Cos(indRot));
            Vector3[] linePoints = [mapTrans.TransformPoint(indPos.ToVec3()), mapTrans.TransformPoint((indPos + delta).ToVec3())];
            InitRange(Variable.IndicatorRad - Variable.LineWidth.Value * 0.5, indPos, out var rangePoints);
            if (Variable.IndicatorRange == null)
            {
                InitLineRenderer(Variable.SolidMaterial, Variable.ColorRange.Value, 1, out Variable.IndicatorRange);
                InitLineRenderer(Variable.SolidMaterial, Variable.ColorRange.Value, 1, out Variable.IndicatorDirection);
            }

            Variable.IndicatorRange.SetPositions(rangePoints);
            Variable.IndicatorDirection.SetPositions(linePoints);
        }
        else if (Variable.IndicatorRange != null)
        {
            Object.Destroy(Variable.IndicatorRange.gameObject);
            Object.Destroy(Variable.IndicatorDirection.gameObject);
        }
    }

    /// <summary>
    /// 渲染自定义直线
    /// </summary>
    public static void SetCustomLineRenderers()
    {
        if (Variable.DoCustomLine && Variable.CustomLineDirections.Count > 0)
        {
            for (var i = 0; i < Variable.CustomLineDirections.Count; i++)
            {
                InitLine(Variable.CustomLineDirections[i], out var points);
                var color = Variable.CustomLineHovers[i] || Variable.TargetLineIndex == i ? Variable.ColorCustomLineHover.Value : Variable.ColorCustomLineNormal.Value;
                var order = Variable.CustomLineHovers[i] || Variable.TargetLineIndex == i ? 6 : 5;
                if (Variable.CustomLines.Count > i)
                    Variable.CustomLines[i].SetPositions(points);
                else
                {
                    InitLineRenderer(Variable.SolidMaterial, color, order, out var line);
                    line.SetPositions(points);
                    Variable.CustomLines.Add(line);
                }
            }

            while (Variable.CustomLines.Count > Variable.CustomLineDirections.Count)
            {
                Object.Destroy(Variable.CustomLines.Last().gameObject);
                Variable.CustomLines.RemoveAt(Variable.CustomLines.Count - 1);
            }
        }
        else
        {
            foreach (var line in Variable.CustomLines)
                Object.Destroy(line.gameObject);
            Variable.CustomLines.Clear();
        }
    }

    /// <summary>
    /// 渲染自定义标记点
    /// </summary>
    public static void SetCustomMarkerRenderers()
    {
        if (Variable.DoCustomMarker && Variable.CustomMarkerPositions.Count > 0)
        {
            var mapTrans = Managers.RecipeMap.currentMap.referencesContainer.transform;
            var arm = Variable.LineWidth.Value * 2.5f;
            for (var i = 0; i < Variable.CustomMarkerPositions.Count; i++)
            {
                var pos = (Variable.DisplayPolar ? Algebra.FromPolar(Variable.CustomMarkerPositions[i]) : Variable.CustomMarkerPositions[i]).ToVec2();
                Vector3[] horizontal = [mapTrans.TransformPoint(new(pos.x - arm, pos.y)), mapTrans.TransformPoint(new(pos.x + arm, pos.y))];
                Vector3[] vertical = [mapTrans.TransformPoint(new(pos.x, pos.y - arm)), mapTrans.TransformPoint(new(pos.x, pos.y + arm))];
                var color = Variable.CustomMarkerHovers[i] || Variable.TargetMarkerIndex == i ? Variable.ColorCustomMarkerHover.Value : Variable.ColorCustomMarkerNormal.Value;
                var order = Variable.CustomMarkerHovers[i] || Variable.TargetMarkerIndex == i ? 6 : 5;
                if (Variable.CustomMarkers.Count > i)
                {
                    Variable.CustomMarkers[i].Item1.SetPositions(horizontal);
                    Variable.CustomMarkers[i].Item2.SetPositions(vertical);
                }
                else
                {
                    InitLineRenderer(Variable.SolidMaterial, color, order, out var lineH);
                    InitLineRenderer(Variable.SolidMaterial, color, order, out var lineV);
                    lineH.SetPositions(horizontal);
                    lineV.SetPositions(vertical);
                    Variable.CustomMarkers.Add((lineH, lineV));
                }
            }

            while (Variable.CustomMarkers.Count > Variable.CustomMarkerPositions.Count)
            {
                var (lineH, lineV) = Variable.CustomMarkers.Last();
                Object.Destroy(lineH.gameObject);
                Object.Destroy(lineV.gameObject);
                Variable.CustomMarkers.RemoveAt(Variable.CustomMarkers.Count - 1);
            }
        }
        else
        {
            foreach (var (lineH, lineV) in Variable.CustomMarkers)
            {
                Object.Destroy(lineH.gameObject);
                Object.Destroy(lineV.gameObject);
            }

            Variable.CustomMarkers.Clear();
        }
    }

    #endregion

    #region 渲染辅助

    /// <summary>
    /// 生成直线
    /// </summary>
    private static void InitLine(double? theta, out Vector3[] points)
    {
        points = [];
        if (!theta.HasValue) return;
        var rad = theta.Value * Math.PI / 180;
        var dx = Math.Cos(rad);
        var dy = Math.Sin(rad);
        var mapTrans = Managers.RecipeMap.currentMap.referencesContainer.transform;
        var indPos = Variable.Vec2D.From(Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition) + Variable.Offset;
        List<Vector3> pts = [];
        if (Math.Abs(dx) > 1e-10)
        {
            var t = (-80 - indPos.X) / dx;
            var y = indPos.Y + t * dy;
            if (y is >= -80 and <= 80)
                pts.Add(mapTrans.TransformPoint(new(-80, (float)y)));
            t = (80 - indPos.X) / dx;
            y = indPos.Y + t * dy;
            if (y is >= -80 and <= 80)
                pts.Add(mapTrans.TransformPoint(new(80, (float)y)));
        }

        if (Math.Abs(dy) > 1e-10)
        {
            var t = (-80 - indPos.Y) / dy;
            var x = indPos.X + t * dx;
            if (x is > -80 and < 80)
                pts.Add(mapTrans.TransformPoint(new((float)x, -80)));
            t = (80 - indPos.Y) / dy;
            x = indPos.X + t * dx;
            if (x is > -80 and < 80)
                pts.Add(mapTrans.TransformPoint(new((float)x, 80)));
        }

        points = [.. pts];
    }

    /// <summary>
    /// 生成圆圈
    /// </summary>
    private static void InitRange(double rad, Variable.Vec2D cen, out Vector3[] points)
    {
        var count = Math.Max(10, (int)(rad * 250));
        points = new Vector3[count];
        var mapTrans = Managers.RecipeMap.currentMap.referencesContainer.transform;
        for (var i = 0; i < count; i++)
        {
            var t = i / (double)(count - 1);
            var angle = t * 2 * Math.PI;
            var x = rad * Math.Cos(angle) + cen.X;
            var y = rad * Math.Sin(angle) + cen.Y;
            points[i] = mapTrans.TransformPoint(new((float)x, (float)y));
        }
    }

    /// <summary>
    /// 隐藏原生药瓶
    /// </summary>
    private static void SetOriginalIndicatorVisible(bool visible)
    {
        var renderers = Managers.RecipeMap.indicator.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
            renderer.enabled = visible;
    }

    /// <summary>
    /// 隐藏原生路径
    /// </summary>
    private static void SetOriginalPathsVisible(bool visible)
    {
        var hints = Managers.RecipeMap.path.fixedPathHints;
        foreach (var r in hints.Where(h => h != null).SelectMany(h => h.GetComponentsInChildren<Renderer>(true)))
            r.enabled = visible;
    }

    #endregion
}