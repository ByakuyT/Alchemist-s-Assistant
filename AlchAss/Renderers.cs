using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.SolventDirectionHint;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlchAss
{
    public static class Renderers
    {
        #region 主要显示
        public static void HandleDirectionLineDisplay(SolventDirectionHint instance, SpriteRenderer spriteRenderer)
        {
            if (Variables.currentMapName != Managers.RecipeMap.currentMap.potionBase.name)
            {
                Variables.allVortexData.Clear();
                Variables.currentMapName = null;
                Variables.hoveredItemName = null;
                Depends.LoadOrScanVortexData();
            }
            if (Variables.directionLine)
            {
                Functions.UpdateLineDirections();
                if (Keyboard.current.slashKey.wasPressedThisFrame)
                {
                    spriteRenderer.enabled = false;
                    Variables.lastPathHintCount = Managers.RecipeMap.path.fixedPathHints.Count;
                    if (Variables.enablePathRendering)
                        Depends.HideOriginalPaths();
                    Depends.SetPotionTranslucent(true);
                }
                var instancePosition = instance.transform.position;
                var indicatorLocalPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
                for (int i = 0; i < 4; i++)
                {
                    Variables.lineRenderer[i].transform.position = instancePosition;
                    Variables.lineRenderer[i].transform.localEulerAngles = new Vector3(0f, 0f, Variables.lineDirection[i]);
                }
                for (int i = 4; i < 14; i++)
                {
                    Vector3? pointPosition = null;
                    if (i < 8)
                    {
                        var zoneIndex = i - 4;
                        if (Variables.zonePoints[zoneIndex, 0] != null)
                            pointPosition = (Vector3)Variables.zonePoints[zoneIndex, 0];
                    }
                    else if (i < 10)
                    {
                        var closestIndex = i - 8;
                        if (Variables.closestPoints[closestIndex].HasValue)
                            pointPosition = (Vector3)Variables.closestPoints[closestIndex].Value;
                    }
                    else
                    {
                        var zoneIndex = i - 10;
                        if (Variables.zonePoints[zoneIndex, 4] != null)
                            pointPosition = (Vector3)Variables.zonePoints[zoneIndex, 4];
                    }
                    Variables.lineRenderer[i].enabled = pointPosition.HasValue;
                    if (pointPosition.HasValue)
                    {
                        var adjustedPosition = pointPosition.Value;
                        adjustedPosition.y = adjustedPosition.y + instancePosition.y - indicatorLocalPosition.y;
                        Variables.lineRenderer[i].transform.position = adjustedPosition;
                    }
                }
                HandleVortexCircleDisplay(instancePosition, indicatorLocalPosition);
                if (Variables.enablePathRendering)
                    HandleIngredientPathDisplay(instancePosition, indicatorLocalPosition);
                HandleTargetEffectCircleDisplay(instancePosition, indicatorLocalPosition);
            }
            else if (Keyboard.current.slashKey.wasPressedThisFrame)
            {
                spriteRenderer.enabled = true;
                for (int i = 0; i < 14; i++)
                    Variables.lineRenderer[i].enabled = false;
                for (int i = 0; i < 3; i++)
                    if (Variables.vortexCircleLineRenderer[i] != null)
                        Variables.vortexCircleLineRenderer[i].enabled = false;
                for (int i = 0; i < Variables.vortexIntersectionRenderer.Length; i++)
                    Variables.vortexIntersectionRenderer[i].enabled = false;
                for (int i = 0; i < Variables.pathLineRenderer.Length; i++)
                    Variables.pathLineRenderer[i].enabled = false;
                if (Variables.pathLineRenderers != null)
                    foreach (var lineRenderer in Variables.pathLineRenderers)
                        if (lineRenderer != null)
                            lineRenderer.enabled = false;
                for (int i = 0; i < Variables.targetEffectCircleRenderer.Length; i++)
                    if (Variables.targetEffectCircleRenderer[i] != null)
                        Variables.targetEffectCircleRenderer[i].enabled = false;
                for (int i = 0; i < Variables.targetEffectCircleLineRenderer.Length; i++)
                    if (Variables.targetEffectCircleLineRenderer[i] != null)
                        Variables.targetEffectCircleLineRenderer[i].enabled = false;
                Variables.lastPathHintCount = 0;
                Depends.ShowOriginalPaths();
                Depends.SetPotionTranslucent(false);
            }
        }
        public static void HandleVortexCircleDisplay(Vector3 instancePosition, Vector2 indicatorLocalPosition)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Variables.vortexCircleLineRenderer[i] == null)
                {
                    var layer = Variables.solventDirectionHint?.gameObject.layer ?? 0;
                    var vortexCircleLine = new GameObject($"VortexCircleLine_{i}") { layer = layer };
                    var lineRenderer = vortexCircleLine.AddComponent<LineRenderer>();
                    SetupCircleLineRenderer(lineRenderer, Variables.lineRenderer[0], Variables.LineColor[11], Variables.lineWidth.Value);
                    Variables.vortexCircleLineRenderer[i] = lineRenderer;
                    lineRenderer.enabled = false;
                }
            }
            for (int i = 0; i < 3; i++)
                Variables.vortexCircleLineRenderer[i].enabled = false;
            if (Variables.selectedVortexIndex >= 0 && Variables.selectedVortexIndex < Variables.allVortexData.Count)
            {
                var vortexData = Variables.allVortexData[Variables.selectedVortexIndex];
                int rendererIndex = GetVortexRendererIndex(vortexData.radius);
                Vector3 position = new Vector3(
                    vortexData.center.x,
                    vortexData.center.y + instancePosition.y - indicatorLocalPosition.y,
                    instancePosition.z);
                var lineRenderer = Variables.vortexCircleLineRenderer[rendererIndex];
                lineRenderer.enabled = true;
                float circleRadius = vortexData.radius - Variables.lineWidth.Value / 2f;
                Vector3[] circlePoints = CreateCirclePoints(position, circleRadius, 64);
                lineRenderer.positionCount = circlePoints.Length;
                lineRenderer.SetPositions(circlePoints);
                lineRenderer.material.color = Variables.LineColor[11];
                lineRenderer.startWidth = Variables.lineWidth.Value;
                lineRenderer.endWidth = Variables.lineWidth.Value;
            }
            DisplayVortexIntersectionPoints(instancePosition, indicatorLocalPosition);
        }
        public static void HandleIngredientPathDisplay(Vector3 instancePosition, Vector2 indicatorLocalPosition)
        {
            var currentPathHintCount = Managers.RecipeMap.path.fixedPathHints.Count;
            if (currentPathHintCount != Variables.lastPathHintCount)
            {
                if (Variables.enablePathRendering)
                    Depends.HideOriginalPaths();
                Variables.lastPathHintCount = currentPathHintCount;
            }
            if (!Variables.enablePathRendering)
            {
                if (Variables.pathLineRenderers != null)
                    foreach (var lineRenderer in Variables.pathLineRenderers)
                        if (lineRenderer != null)
                            lineRenderer.enabled = false;
                for (int i = 0; i < Variables.pathLineRenderer.Length; i++)
                    Variables.pathLineRenderer[i].enabled = false;
                return;
            }
            if (Variables.ingredientPathGroups.Count == 0)
            {
                if (Variables.pathLineRenderers != null)
                    foreach (var lineRenderer in Variables.pathLineRenderers)
                        if (lineRenderer != null)
                            lineRenderer.enabled = false;
                for (int i = 0; i < Variables.pathLineRenderer.Length; i++)
                    Variables.pathLineRenderer[i].enabled = false;
                return;
            }
            if (Variables.pathLineRenderers == null || Variables.pathLineRenderers.Length < Variables.ingredientPathGroups.Count)
            {
                var oldRenderers = Variables.pathLineRenderers ?? new LineRenderer[0];
                Variables.pathLineRenderers = new LineRenderer[Variables.ingredientPathGroups.Count];
                Array.Copy(oldRenderers, Variables.pathLineRenderers, Math.Min(oldRenderers.Length, Variables.pathLineRenderers.Length));
                for (int i = oldRenderers.Length; i < Variables.pathLineRenderers.Length; i++)
                {
                    var layer = Variables.solventDirectionHint?.gameObject.layer ?? 0;
                    var pathLine = new GameObject($"IngredientPathLineRenderer_{i}") { layer = layer };
                    var lineRenderer = pathLine.AddComponent<LineRenderer>();
                    SetupLineRenderer(lineRenderer, Variables.lineRenderer[0]);
                    Variables.pathLineRenderers[i] = lineRenderer;
                    lineRenderer.enabled = false;
                }
            }
            var mapTransform = Managers.RecipeMap.currentMap.referencesContainer.transform;
            var pathTransform = Managers.RecipeMap.path.thisTransform;
            for (int groupIndex = 0; groupIndex < Variables.ingredientPathGroups.Count; groupIndex++)
            {
                var group = Variables.ingredientPathGroups[groupIndex];
                var lineRenderer = Variables.pathLineRenderers[groupIndex];
                if (group.normalSegments.Count == 0)
                    lineRenderer.enabled = false;
                else
                {
                    lineRenderer.enabled = true;
                    lineRenderer.material.color = group.pathColor;
                    var allPoints = new List<Vector3>();
                    foreach (var segment in group.normalSegments)
                        for (int pointIdx = 0; pointIdx < segment.Length; pointIdx++)
                        {
                            var worldPoint = mapTransform.InverseTransformPoint(pathTransform.TransformPoint(segment[pointIdx]));
                            var adjustedPoint = new Vector3(
                                worldPoint.x,
                                worldPoint.y + instancePosition.y - indicatorLocalPosition.y,
                                instancePosition.z);
                            allPoints.Add(adjustedPoint);
                        }
                    lineRenderer.positionCount = allPoints.Count;
                    lineRenderer.SetPositions(allPoints.ToArray());
                }
            }
            for (int i = Variables.ingredientPathGroups.Count; i < Variables.pathLineRenderers.Length; i++)
                Variables.pathLineRenderers[i].enabled = false;
            int totalTeleportationSegments = 0;
            foreach (var group in Variables.ingredientPathGroups)
                totalTeleportationSegments += group.teleportationSegments.Count;
            if (Variables.pathLineRenderer.Length < totalTeleportationSegments)
            {
                var oldRenderers = Variables.pathLineRenderer;
                Variables.pathLineRenderer = new SpriteRenderer[totalTeleportationSegments];
                Array.Copy(oldRenderers, Variables.pathLineRenderer, oldRenderers.Length);
                for (int i = oldRenderers.Length; i < totalTeleportationSegments; i++)
                {
                    var layer = Variables.solventDirectionHint?.gameObject.layer ?? 0;
                    var pathLine = new GameObject("TeleportationPathLine") { layer = layer };
                    var renderer = pathLine.AddComponent<SpriteRenderer>();
                    SetupSpriteRenderer(renderer, Variables.lineRenderer[0], Color.white, new Vector2(1f, Variables.lineWidth.Value));
                    renderer.sortingOrder = Variables.lineRenderer[0].sortingOrder + 1;
                    Variables.pathLineRenderer[i] = renderer;
                    renderer.enabled = false;
                }
            }
            int rendererIndex = 0;
            foreach (var group in Variables.ingredientPathGroups)
            {
                foreach (var segment in group.teleportationSegments)
                    if (segment.Length >= 2 && rendererIndex < Variables.pathLineRenderer.Length)
                    {
                        var renderer = Variables.pathLineRenderer[rendererIndex];
                        var startPoint = mapTransform.InverseTransformPoint(pathTransform.TransformPoint(segment[0]));
                        var endPoint = mapTransform.InverseTransformPoint(pathTransform.TransformPoint(segment[1]));
                        renderer.enabled = true;
                        renderer.color = group.pathColor;
                        var midPoint = (startPoint + endPoint) * 0.5f;
                        var direction = endPoint - startPoint;
                        renderer.transform.position = new Vector3(
                            midPoint.x,
                            midPoint.y + instancePosition.y - indicatorLocalPosition.y,
                            instancePosition.z);
                        renderer.transform.localEulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, direction));
                        renderer.size = new Vector2(direction.magnitude, Variables.lineWidth.Value);
                        SetupDashedLine(renderer, direction.magnitude);
                        rendererIndex++;
                    }
            }
            for (int i = rendererIndex; i < Variables.pathLineRenderer.Length; i++)
                Variables.pathLineRenderer[i].enabled = false;
        }
        public static void HandleTargetEffectCircleDisplay(Vector3 instancePosition, Vector2 indicatorLocalPosition)
        {
            Variables.SharedCache.UpdateCache();
            if (!Variables.SharedCache.isValid)
            {
                for (int i = 0; i < Variables.targetEffectCircleRenderer.Length; i++)
                    if (Variables.targetEffectCircleRenderer[i] != null)
                        Variables.targetEffectCircleRenderer[i].enabled = false;
                for (int i = 0; i < Variables.targetEffectCircleLineRenderer.Length; i++)
                    if (Variables.targetEffectCircleLineRenderer[i] != null)
                        Variables.targetEffectCircleLineRenderer[i].enabled = false;
                return;
            }
            float[] baseRadiusValues = [1.53f, 1f / 3f, 1f / 18f];
            float[] actualRadiusValues;
            if (Variables.useAngleAdjustedRadius)
            {
                var angleDelta = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, Variables.SharedCache.targetEffect.transform.localEulerAngles.z));
                actualRadiusValues = [1.53f, 1f / 3f - angleDelta / 216f, 1f / 18f - angleDelta / 216f];
            }
            else
                actualRadiusValues = baseRadiusValues;
            for (int i = 0; i < Variables.targetEffectCircleRenderer.Length; i++)
            {
                if (Variables.targetEffectCircleRenderer[i] == null)
                {
                    var layer = Variables.solventDirectionHint?.gameObject.layer ?? 0;
                    var targetCircle = new GameObject($"TargetEffectCircle_{i}") { layer = layer };
                    var renderer = targetCircle.AddComponent<SpriteRenderer>();
                    renderer.sortingLayerName = Variables.lineRenderer[0].sortingLayerName;
                    renderer.sortingOrder = Variables.lineRenderer[0].sortingOrder + 1;
                    renderer.color = Variables.LineColor[11];
                    Variables.targetEffectCircleRenderer[i] = renderer;
                    renderer.enabled = false;
                }
                if (Variables.targetEffectCircleSprite[i] == null)
                    Variables.targetEffectCircleSprite[i] = CreateCircleSpriteForRadius(baseRadiusValues[i]);
            }
            for (int i = 0; i < Variables.targetEffectCircleLineRenderer.Length; i++)
            {
                if (Variables.targetEffectCircleLineRenderer[i] == null)
                {
                    var layer = Variables.solventDirectionHint?.gameObject.layer ?? 0;
                    var targetCircleLine = new GameObject($"TargetEffectCircleLine_{i}") { layer = layer };
                    var lineRenderer = targetCircleLine.AddComponent<LineRenderer>();
                    SetupCircleLineRenderer(lineRenderer, Variables.lineRenderer[0], Variables.LineColor[11], Variables.lineWidth.Value);
                    Variables.targetEffectCircleLineRenderer[i] = lineRenderer;
                    lineRenderer.enabled = false;
                }
            }
            for (int i = 0; i < Variables.targetEffectCircleRenderer.Length; i++)
            {
                var targetSpriteRenderer = Variables.targetEffectCircleRenderer[i];
                var targetLineRenderer = Variables.targetEffectCircleLineRenderer[i];
                if (actualRadiusValues[i] <= 0f)
                {
                    targetSpriteRenderer.enabled = false;
                    targetLineRenderer.enabled = false;
                    continue;
                }
                Vector3 position = new Vector3(
                    Variables.SharedCache.targetPosition.x,
                    Variables.SharedCache.targetPosition.y + instancePosition.y - indicatorLocalPosition.y,
                    instancePosition.z);
                if (actualRadiusValues[i] <= Variables.lineWidth.Value)
                {
                    targetSpriteRenderer.enabled = true;
                    targetLineRenderer.enabled = false;
                    targetSpriteRenderer.transform.position = position;
                    targetSpriteRenderer.sprite = Variables.targetEffectCircleSprite[i];
                    float scaleRatio = actualRadiusValues[i] / baseRadiusValues[i];
                    targetSpriteRenderer.transform.localScale = Vector3.one * scaleRatio;
                    targetSpriteRenderer.color = Variables.LineColor[11];
                }
                else
                {
                    targetSpriteRenderer.enabled = false;
                    targetLineRenderer.enabled = true;
                    float circleRadius = actualRadiusValues[i] - Variables.lineWidth.Value / 2f;
                    Vector3[] circlePoints = CreateCirclePoints(position, circleRadius, 64);
                    targetLineRenderer.positionCount = circlePoints.Length;
                    targetLineRenderer.SetPositions(circlePoints);
                    targetLineRenderer.material.color = Variables.LineColor[11];
                    targetLineRenderer.startWidth = Variables.lineWidth.Value;
                    targetLineRenderer.endWidth = Variables.lineWidth.Value;
                }
            }
        }
        public static void DisplayVortexIntersectionPoints(Vector3 instancePosition, Vector2 indicatorLocalPosition)
        {
            int requiredCount = Variables.vortexIntersectionPoints.Count;
            if (Variables.vortexIntersectionRenderer.Length < requiredCount)
            {
                var oldRenderers = Variables.vortexIntersectionRenderer;
                Variables.vortexIntersectionRenderer = new SpriteRenderer[requiredCount];
                for (int i = 0; i < oldRenderers.Length; i++)
                    Variables.vortexIntersectionRenderer[i] = oldRenderers[i];
                for (int i = oldRenderers.Length; i < requiredCount; i++)
                {
                    var layer = Variables.solventDirectionHint?.gameObject.layer ?? 0;
                    var intersectionNode = new GameObject("VortexIntersection") { layer = layer };
                    var renderer = intersectionNode.AddComponent<SpriteRenderer>();
                    SetupSpriteRenderer(renderer, Variables.lineRenderer[3], Variables.LineColor[10], new Vector2(Variables.pointSize.Value, Variables.pointSize.Value));
                    renderer.sortingOrder = Variables.lineRenderer[3].sortingOrder + 2;
                    renderer.transform.localEulerAngles = Vector3.zero;
                    Variables.vortexIntersectionRenderer[i] = renderer;
                    renderer.enabled = false;
                }
            }
            for (int i = 0; i < Variables.vortexIntersectionRenderer.Length; i++)
            {
                var renderer = Variables.vortexIntersectionRenderer[i];
                if (i < Variables.vortexIntersectionPoints.Count)
                {
                    var intersectionPoint = Variables.vortexIntersectionPoints[i];
                    renderer.enabled = true;
                    renderer.transform.position = new Vector3(
                        intersectionPoint.x,
                        intersectionPoint.y + instancePosition.y - indicatorLocalPosition.y,
                        instancePosition.z);
                }
                else
                    renderer.enabled = false;
            }
        }
        #endregion

        #region 渲染设置
        public static void InitializeLineRenderers(SolventDirectionHint instance, SpriteRenderer spriteRenderer)
        {
            for (int i = 0; i < 4; i++)
                if (Variables.lineRenderer[i] == null)
                {
                    var newLine = new GameObject("Line") { layer = instance.gameObject.layer };
                    Variables.lineRenderer[i] = newLine.AddComponent<SpriteRenderer>();
                    SetupSpriteRenderer(Variables.lineRenderer[i], spriteRenderer, Variables.LineColor[i], new Vector2(200f, Variables.lineWidth.Value));
                }
            for (int i = 4; i < 10; i++)
                if (Variables.lineRenderer[i] == null)
                {
                    var newNode = new GameObject("Node") { layer = instance.gameObject.layer };
                    Variables.lineRenderer[i] = newNode.AddComponent<SpriteRenderer>();
                    SetupSpriteRenderer(Variables.lineRenderer[i], spriteRenderer, Variables.LineColor[i], new Vector2(Variables.pointSize.Value, Variables.pointSize.Value));
                    Variables.lineRenderer[i].sortingOrder += 3;
                }
            for (int i = 10; i < 14; i++)
                if (Variables.lineRenderer[i] == null)
                {
                    var newNode = new GameObject("Node") { layer = instance.gameObject.layer };
                    Variables.lineRenderer[i] = newNode.AddComponent<SpriteRenderer>();
                    SetupSpriteRenderer(Variables.lineRenderer[i], spriteRenderer, Variables.LineColor[i - 6], new Vector2(Variables.pointSize.Value, Variables.pointSize.Value));
                    Variables.lineRenderer[i].sortingOrder += 3;
                }
        }
        public static void SetupLineRenderer(LineRenderer lineRenderer, SpriteRenderer referenceRenderer)
        {
            lineRenderer.sortingLayerName = referenceRenderer.sortingLayerName;
            lineRenderer.sortingOrder = referenceRenderer.sortingOrder + 1;
            lineRenderer.startWidth = Variables.lineWidth.Value;
            lineRenderer.endWidth = Variables.lineWidth.Value;
            lineRenderer.useWorldSpace = true;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        public static void SetupSpriteRenderer(SpriteRenderer renderer, SpriteRenderer referenceRenderer, Color color, Vector2 size)
        {
            renderer.sortingLayerName = referenceRenderer.sortingLayerName;
            renderer.sortingOrder = referenceRenderer.sortingOrder;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.sprite = Sprite.Create(Variables.texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1.0f, 0, SpriteMeshType.FullRect);
            renderer.color = color;
            renderer.size = size;
        }
        public static void SetupSolidLine(SpriteRenderer renderer)
        {
            if (renderer.sprite == null || renderer.sprite.texture != Variables.texture)
                renderer.sprite = Sprite.Create(Variables.texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1.0f, 0, SpriteMeshType.FullRect);
            renderer.drawMode = SpriteDrawMode.Tiled;
        }
        public static void SetupDashedLine(SpriteRenderer renderer, float lineLength)
        {
            if (Variables.dashedTexture == null)
                CreateDashedTexture();
            renderer.sprite = Variables.dashedSprite;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(lineLength, Variables.lineWidth.Value);
        }
        #endregion

        #region 圆环生成
        public static Vector3[] CreateCirclePoints(Vector3 center, float radius, int segments = 64)
        {
            Vector3[] points = new Vector3[segments + 1];
            float angleStep = 360f / segments;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                points[i] = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0f
                );
            }
            return points;
        }
        public static void SetupCircleLineRenderer(LineRenderer lineRenderer, SpriteRenderer referenceRenderer, Color color, float lineWidth)
        {
            lineRenderer.sortingLayerName = referenceRenderer.sortingLayerName;
            lineRenderer.sortingOrder = referenceRenderer.sortingOrder + 1;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.useWorldSpace = true;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.material.color = color;
            lineRenderer.loop = true;
        }
        #endregion

        #region 材质创建
        public static int GetVortexRendererIndex(float radius)
        {
            if (Mathf.Abs(radius - 1.74f) < 0.1f)
                return 0;
            else if (Mathf.Abs(radius - 1.99f) < 0.1f)
                return 1;
            else
                return 2;
        }
        public static Sprite CreateCircleSpriteForRadius(float radius)
        {
            float diameter = radius * 2f;
            int textureSize = 500;
            Texture2D circleTexture = new(textureSize, textureSize);
            Color[] pixels = new Color[textureSize * textureSize];
            Vector2 center = new(textureSize * 0.5f, textureSize * 0.5f);
            float outerRadius = textureSize * 0.5f;
            for (int y = 0; y < textureSize; y++)
                for (int x = 0; x < textureSize; x++)
                {
                    Vector2 pos = new(x, y);
                    float distance = Vector2.Distance(pos, center);
                    if (distance <= outerRadius)
                    {
                        float alpha = 1.0f;
                        if (distance > outerRadius - 1f)
                            alpha = outerRadius - distance;
                        alpha = Mathf.Clamp01(alpha);
                        pixels[y * textureSize + x] = new Color(1f, 1f, 1f, alpha);
                    }
                    else
                        pixels[y * textureSize + x] = Color.clear;
                }
            circleTexture.SetPixels(pixels);
            circleTexture.Apply();
            circleTexture.filterMode = FilterMode.Bilinear;
            float pixelsPerUnit = textureSize / diameter;
            return Sprite.Create(circleTexture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
        public static void CreateDashedTexture()
        {
            int textureWidth = 12;
            int textureHeight = 1;
            Variables.dashedTexture = new Texture2D(textureWidth, textureHeight);
            Color[] pixels = new Color[textureWidth * textureHeight];
            for (int x = 0; x < textureWidth; x++)
                pixels[x] = x < 6 ? Color.white : Color.clear;
            Variables.dashedTexture.SetPixels(pixels);
            Variables.dashedTexture.Apply();
            Variables.dashedTexture.filterMode = FilterMode.Point;
            Variables.dashedTexture.wrapMode = TextureWrapMode.Repeat;
            Variables.dashedSprite = Sprite.Create(Variables.dashedTexture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f), textureWidth / 0.3f, 0, SpriteMeshType.FullRect);
        }
        #endregion
    }
}
