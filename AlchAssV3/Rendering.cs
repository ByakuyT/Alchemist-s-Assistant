using PotionCraft.ManagersSystem;
using System.Linq;
using UnityEngine;

namespace AlchAssV3
{
    public static class Rendering
    {
        #region 生成材质
        /// <summary>
        /// 生成纯色材质
        /// </summary>
        public static Material CreateSolidMaterial()
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixels([Color.white]);
            texture.Apply();
            return new Material(Shader.Find("Sprites/Default")) { mainTexture = texture };
        }

        /// <summary>
        /// 生成虚线材质
        /// </summary>
        public static Material CreateDashedMaterial()
        {
            var texture = new Texture2D(10, 1);
            Color[] pixels = new Color[10];
            for (int x = 0; x < 10; x++)
                pixels[x] = x < 5 ? Color.white : Color.clear;
            texture.SetPixels(pixels);
            texture.Apply();
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Point;
            return new Material(Shader.Find("Sprites/Default")) { mainTexture = texture };
        }

        /// <summary>
        /// 生成矩形精灵
        /// </summary>
        public static Sprite CreateSquareSprite()
        {
            var squareTexture = new Texture2D(1, 1);
            var pixels = new Color[1] { Color.white };
            squareTexture.SetPixels(pixels);
            squareTexture.Apply();
            squareTexture.filterMode = FilterMode.Point;
            return Sprite.Create(squareTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        }

        /// <summary>
        /// 生成圆形精灵
        /// </summary>
        public static Sprite CreateRoundSprite()
        {
            var textureSize = 500;
            var circleTexture = new Texture2D(textureSize, textureSize);
            var pixels = new Color[textureSize * textureSize];
            var center = new Vector2(textureSize * 0.5f, textureSize * 0.5f);
            var radius = textureSize * 0.5;
            for (var y = 0; y < textureSize; y++)
                for (var x = 0; x < textureSize; x++)
                {
                    var pos = new Vector2(x, y);
                    var distance = Vector2.Distance(pos, center);
                    if (distance <= radius)
                        pixels[y * textureSize + x] = Color.white;
                    else
                        pixels[y * textureSize + x] = Color.clear;
                }
            circleTexture.SetPixels(pixels);
            circleTexture.Apply();
            circleTexture.filterMode = FilterMode.Bilinear;
            var pixelsPerUnit = textureSize / (Variable.LineWidth.Value * 2);
            return Sprite.Create(circleTexture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        /// <summary>
        /// 生成材质和精灵
        /// </summary>
        public static void CreateMaterialAndSprites()
        {
            Variable.SolidMaterial = CreateSolidMaterial();
            Variable.DashedMaterial = CreateDashedMaterial();
            Variable.RoundSprite = CreateRoundSprite();
            Variable.SquareSprite = CreateSquareSprite();
        }

        /// <summary>
        /// 生成曲线渲染器
        /// </summary>
        public static void InitLineRenderer(ref LineRenderer curve)
        {
            var obj = new GameObject("Renderer") { layer = 8 };
            curve = obj.AddComponent<LineRenderer>();

            curve.textureMode = LineTextureMode.Tile;
            curve.useWorldSpace = true;
            curve.startWidth = Variable.LineWidth.Value;
            curve.endWidth = Variable.LineWidth.Value;
            curve.sortingLayerName = "RecipeMapIndicator";
            curve.positionCount = 0;
            curve.enabled = false;
        }

        /// <summary>
        /// 生成精灵渲染器
        /// </summary>
        public static void InitSpriteRenderer(ref SpriteRenderer sprite)
        {
            var obj = new GameObject("Renderer") { layer = 8 };
            sprite = obj.AddComponent<SpriteRenderer>();

            sprite.drawMode = SpriteDrawMode.Simple;
            sprite.maskInteraction = SpriteMaskInteraction.None;
            sprite.spriteSortPoint = SpriteSortPoint.Center;
            sprite.sortingLayerName = "RecipeMapIndicator";
            sprite.enabled = false;
        }

        /// <summary>
        /// 更新曲线渲染器
        /// </summary>
        public static void UpdateLineRenderer(Material material, Color color, ref LineRenderer curve, Vector3[] points, bool loop, int order)
        {
            curve.sortingOrder = order;
            curve.loop = loop;
            curve.material = material;
            curve.startColor = color;
            curve.endColor = color;
            curve.positionCount = points.Length;
            curve.SetPositions(points);
            curve.enabled = true;
        }

        /// <summary>
        /// 更新精灵渲染器
        /// </summary>
        public static void UpdateSpriteRenderer(Sprite material, Color color, ref SpriteRenderer sprite, Vector3 pos, float scale, int order)
        {
            sprite.sortingOrder = order;
            sprite.sprite = material;
            sprite.transform.position = pos;
            sprite.transform.localScale = new Vector3(scale, scale, 1);
            sprite.color = color;
            sprite.enabled = true;
        }
        #endregion

        #region 渲染对象
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
                if (ClosestEnables[i] && !float.IsNaN(Variable.ClosestPositions[i].x))
                {
                    var posDev = mapTrans.TransformPoint(Variable.ClosestPositions[i]);
                    if (Variable.ClosestPoints[i] == null)
                        InitSpriteRenderer(ref Variable.ClosestPoints[i]);
                    UpdateSpriteRenderer(Variable.SquareSprite, Variable.ColorClosest.Value, ref Variable.ClosestPoints[i], posDev, (float)Variable.NodeSize.Value, 4);
                }
                else if (Variable.ClosestPoints[i] != null)
                    Object.Destroy(Variable.ClosestPoints[i].gameObject);
            }

            for (var i = 0; i < 4; i++)
            {
                if (IntersectionEnables[i] && Variable.IntersectionPositions[i].Count > 0)
                {
                    for (var j = 0; j < Variable.IntersectionPositions[i].Count; j++)
                    {
                        var posDev = mapTrans.TransformPoint(Variable.IntersectionPositions[i][j]);

                        if (Variable.IntersectionPoints[i].Count <= j)
                        {
                            var point = new SpriteRenderer();
                            InitSpriteRenderer(ref point);
                            UpdateSpriteRenderer(Variable.SquareSprite, Variable.ColorIntersection.Value, ref point, posDev, (float)Variable.NodeSize.Value, 4);
                            Variable.IntersectionPoints[i].Add(point);
                        }
                        else
                        {
                            var point = Variable.IntersectionPoints[i][j];
                            UpdateSpriteRenderer(Variable.SquareSprite, Variable.ColorIntersection.Value, ref point, posDev, (float)Variable.NodeSize.Value, 4);
                            Variable.IntersectionPoints[i][j] = point;
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
                if (DangerEnables[i] && !float.IsNaN(Variable.DefeatPositions[i].x))
                {
                    var posDev = mapTrans.TransformPoint(Variable.DefeatPositions[i]);
                    if (Variable.DefeatPoints[i] == null)
                        InitSpriteRenderer(ref Variable.DefeatPoints[i]);
                    UpdateSpriteRenderer(Variable.SquareSprite, Variable.ColorDefeat.Value, ref Variable.DefeatPoints[i], posDev, (float)Variable.NodeSize.Value, 4);
                }
                else if (Variable.DefeatPoints[i] != null)
                    Object.Destroy(Variable.DefeatPoints[i].gameObject);

                if (DangerEnables[i] && Variable.DangerPositions[i].Count > 0)
                {
                    for (var j = 0; j < Variable.DangerPositions[i].Count; j++)
                    {
                        var posDev = mapTrans.TransformPoint(Variable.DangerPositions[i][j]);

                        if (Variable.DangerPoints[i].Count <= j)
                        {
                            var point = new SpriteRenderer();
                            InitSpriteRenderer(ref point);
                            UpdateSpriteRenderer(Variable.SquareSprite, Variable.ColorIntersection.Value, ref point, posDev, (float)Variable.NodeSize.Value, 4);
                            Variable.DangerPoints[i].Add(point);
                        }
                        else
                        {
                            var point = Variable.DangerPoints[i][j];
                            UpdateSpriteRenderer(Variable.SquareSprite, Variable.ColorIntersection.Value, ref point, posDev, (float)Variable.NodeSize.Value, 4);
                            Variable.DangerPoints[i][j] = point;
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
                    var posDev = mapTrans.TransformPoint(Variable.SwampPositions[i]);

                    if (Variable.SwampPoints.Count <= i)
                    {
                        var point = new SpriteRenderer();
                        InitSpriteRenderer(ref point);
                        UpdateSpriteRenderer(Variable.SquareSprite, Variable.ColorIntersection.Value, ref point, posDev, (float)Variable.NodeSize.Value, 4);
                        Variable.SwampPoints.Add(point);
                    }
                    else
                    {
                        var point = Variable.SwampPoints[i];
                        UpdateSpriteRenderer(Variable.SquareSprite, Variable.ColorIntersection.Value, ref point, posDev, (float)Variable.NodeSize.Value, 4);
                        Variable.SwampPoints[i] = point;
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
                    Calculation.InitLine(Variable.LineDirections[i], out var points);
                    if (points.Length == 2)
                    {
                        if (Variable.Lines[i] == null)
                            InitLineRenderer(ref Variable.Lines[i]);
                        UpdateLineRenderer(Variable.SolidMaterial, Variable.ColorLines[i].Value, ref Variable.Lines[i], points, false, 3);
                    }
                    else if (Variable.Lines[i] != null)
                        Object.Destroy(Variable.Lines[i].gameObject);
                }
                else if (Variable.Lines[i] != null)
                    Object.Destroy(Variable.Lines[i].gameObject);
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
                    Calculation.InitLine(Variable.CustomLineDirections[i], out var points);
                    var color = Variable.CustomLineHovers[i] || Variable.TargetLineIndex == i ? Variable.ColorCustomHover.Value : Variable.ColorCustomNormal.Value;
                    var order = Variable.CustomLineHovers[i] || Variable.TargetLineIndex == i ? 6 : 5;

                    if (Variable.CustomLines.Count <= i)
                    {
                        var line = new LineRenderer();
                        InitLineRenderer(ref line);
                        UpdateLineRenderer(Variable.SolidMaterial, color, ref line, points, false, order);
                        Variable.CustomLines.Add(line);
                    }
                    else
                    {
                        var line = Variable.CustomLines[i];
                        UpdateLineRenderer(Variable.SolidMaterial, color, ref line, points, false, order);
                        Variable.CustomLines[i] = line;
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
        /// 渲染曲线
        /// </summary>
        public static void SetCurveRenderers()
        {
            HideOriginalPaths(!Variable.DoPathCurve);

            if (Variable.DoPathCurve && Variable.PathGraphical.Count > 0)
            {
                for (var i = 0; i < Variable.PathGraphical.Count; i++)
                {
                    var points = Variable.PathGraphical[i].Item1;
                    var isTp = Variable.PathGraphical[i].Item2;
                    var material = isTp ? Variable.DashedMaterial : Variable.SolidMaterial;
                    var color = Variable.ColorPaths[i % 2].Value;

                    if (Variable.PathCurves.Count <= i)
                    {
                        var curve = new LineRenderer();
                        InitLineRenderer(ref curve);
                        UpdateLineRenderer(material, color, ref curve, points, false, 2);
                        Variable.PathCurves.Add(curve);
                    }
                    else
                    {
                        var curve = Variable.PathCurves[i];
                        UpdateLineRenderer(material, color, ref curve, points, false, 2);
                        Variable.PathCurves[i] = curve;
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

            if (Variable.DoVortexCurve && Variable.VortexGraphical.Length >= 2)
            {
                if (Variable.VortexCurve == null)
                    InitLineRenderer(ref Variable.VortexCurve);
                UpdateLineRenderer(Variable.SolidMaterial, Variable.ColorVortex.Value, ref Variable.VortexCurve, Variable.VortexGraphical, false, 2);
            }
            else if (Variable.VortexCurve != null)
                Object.Destroy(Variable.VortexCurve.gameObject);
        }

        /// <summary>
        /// 渲染范围圈
        /// </summary>
        public static void SetRangeRenderers()
        {
            if (Variable.DoEffectRange && Variable.TargetEffect != null)
            {
                Vector2 effectPos = Variable.TargetEffect.transform.localPosition;
                var effectRot = Variable.TargetEffect.transform.localEulerAngles.z;
                var indRot = Managers.RecipeMap.indicatorRotation.Value;
                var devRot = Mathf.Abs(Mathf.DeltaAngle(indRot, effectRot));
                var mapTrans = Managers.RecipeMap.currentMap.referencesContainer.transform;
                var posDev = mapTrans.TransformPoint(effectPos);
                double[] rads = [1.53, 1.0 / 3.0 - devRot / 216.0, 1.0 / 18.0 - devRot / 216.0];

                Calculation.InitRange(rads[0], effectPos.x, effectPos.y, out var pointsOut);
                if (Variable.EffectRangeOuter == null)
                    InitLineRenderer(ref Variable.EffectRangeOuter);
                UpdateLineRenderer(Variable.SolidMaterial, Variable.ColorRange.Value, ref Variable.EffectRangeOuter, pointsOut, true, 1);

                if (rads[1] > Variable.LineWidth.Value)
                {
                    if (Variable.EffectDiskMiddle != null)
                        Object.Destroy(Variable.EffectDiskMiddle.gameObject);
                    Calculation.InitRange(rads[1], effectPos.x, effectPos.y, out var pointsMid);
                    if (Variable.EffectRangeMiddle == null)
                        InitLineRenderer(ref Variable.EffectRangeMiddle);
                    UpdateLineRenderer(Variable.SolidMaterial, Variable.ColorRange.Value, ref Variable.EffectRangeMiddle, pointsMid, true, 1);
                }
                else if (rads[1] > 0)
                {
                    var scale = rads[1] / Variable.LineWidth.Value;
                    if (Variable.EffectRangeMiddle != null)
                        Object.Destroy(Variable.EffectRangeMiddle.gameObject);
                    if (Variable.EffectDiskMiddle == null)
                        InitSpriteRenderer(ref Variable.EffectDiskMiddle);
                    UpdateSpriteRenderer(Variable.RoundSprite, Variable.ColorRange.Value, ref Variable.EffectDiskMiddle, posDev, (float)scale, 1);
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
                    var scale = rads[2] / Variable.LineWidth.Value;
                    if (Variable.EffectDiskInner == null)
                        InitSpriteRenderer(ref Variable.EffectDiskInner);
                    UpdateSpriteRenderer(Variable.RoundSprite, Variable.ColorRange.Value, ref Variable.EffectDiskInner, posDev, (float)scale, 1);
                }
                else if (Variable.EffectDiskInner != null)
                    Object.Destroy(Variable.EffectDiskInner.gameObject);
            }
            else
            {
                if (Variable.EffectRangeOuter != null) Object.Destroy(Variable.EffectRangeOuter.gameObject);
                if (Variable.EffectRangeMiddle != null) Object.Destroy(Variable.EffectRangeMiddle.gameObject);
                if (Variable.EffectDiskMiddle != null) Object.Destroy(Variable.EffectDiskMiddle.gameObject);
                if (Variable.EffectDiskInner != null) Object.Destroy(Variable.EffectDiskInner.gameObject);
            }

            var mapindex = Variable.MapId[Managers.RecipeMap.currentMap.potionBase.name];
            if (Variable.DoVortexRange && mapindex != 2 && Variable.VortexIndex[mapindex] >= 0)
            {
                var selVortex = Variable.Vortexs[mapindex][Variable.VortexIndex[mapindex]];
                Calculation.InitRange(selVortex.r, selVortex.x, selVortex.y, out var points);
                if (Variable.VortexRange == null)
                    InitLineRenderer(ref Variable.VortexRange);
                UpdateLineRenderer(Variable.SolidMaterial, Variable.ColorRange.Value, ref Variable.VortexRange, points, true, 1);
            }
            else if (Variable.VortexRange != null)
                Object.Destroy(Variable.VortexRange.gameObject);
        }

        public static void SetIndicatorRenderers()
        {
            HideOriginalIndicator(!Variable.DoTransparency);

            if (Variable.DoTransparency)
            {
                var indPos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
                var indRot = -Managers.RecipeMap.indicatorRotation.Value * Mathf.Deg2Rad;
                var mapTrans = Managers.RecipeMap.currentMap.referencesContainer.transform;
                Vector3 delta = new(0.74f * Mathf.Sin(indRot), 0.74f * Mathf.Cos(indRot));
                Vector3[] linePoints = [mapTrans.TransformPoint(indPos), mapTrans.TransformPoint(indPos + delta)];
                Calculation.InitRange(0.74, indPos.x, indPos.y, out var points);
                if (Variable.IndicatorRange == null)
                {
                    InitLineRenderer(ref Variable.IndicatorRange);
                    InitLineRenderer(ref Variable.IndicatorDirection);
                }
                UpdateLineRenderer(Variable.SolidMaterial, Variable.ColorRange.Value, ref Variable.IndicatorRange, points, true, 1);
                UpdateLineRenderer(Variable.SolidMaterial, Variable.ColorRange.Value, ref Variable.IndicatorDirection, linePoints, false, 1);
            }
            else if (Variable.IndicatorRange != null)
            {
                Object.Destroy(Variable.IndicatorRange.gameObject);
                Object.Destroy(Variable.IndicatorDirection.gameObject);
            }
        }

        /// <summary>
        /// 隐藏原生路径
        /// </summary>
        public static void HideOriginalPaths(bool hide)
        {
            var hints = Managers.RecipeMap.path.fixedPathHints;
            foreach (var hint in hints)
                if (hint != null)
                {
                    var renderers = hint.GetComponentsInChildren<Renderer>(true);
                    foreach (var renderer in renderers)
                        renderer.enabled = hide;
                }
        }

        /// <summary>
        /// 隐藏原生药瓶
        /// </summary>
        public static void HideOriginalIndicator(bool hide)
        {
            var renderers = Managers.RecipeMap.indicator.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
                renderer.enabled = hide;
        }
        #endregion
    }
}
