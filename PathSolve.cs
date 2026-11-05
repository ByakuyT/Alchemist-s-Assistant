using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using PotionCraft.ManagersSystem;

namespace AlchAssV4;

public static class PathSolve
{
    #region 计算核心

    /// <summary>
    /// 计算路径 → 加水
    /// </summary>
    private static Variable.Vec2D? PathToLadle(List<(Variable.Vec2D, bool)> path, Variable.Vec2D O,
        Variable.Vec2D C,
        double rad)
    {
        if (path.Count < 2) return null;
        var gate = new object();
        var bestP = (double.PositiveInfinity, -1, 0.0);
        Parallel.For(0, path.Count, () => (double.PositiveInfinity, -1, 0.0),
            (i, _, local) =>
            {
                var B = path[i].Item1;
                Geometry.LineVsCircle(B, O, C, rad, out var ti);
                var cur = (ti.HasValue ? ti.Value * O.Distance(B) : double.PositiveInfinity, i, 0.0);
                local = Better(cur, local);
                if (i == path.Count - 1 || path[i + 1].Item2) return local;
                var D = path[i + 1].Item1 - B;
                var U = B - C;
                var W = O - B;
                var d2 = D.SqrMagnitude;
                var u2 = U.SqrMagnitude;
                var w2 = W.SqrMagnitude;
                var ud = U.Dot(D);
                var wd = W.Dot(D);
                var uw = U.Dot(W);
                var a = new Variable.Poly(w2, -2.0 * wd, d2);
                var b = new Variable.Poly(uw, wd - ud, -d2);
                var c = new Variable.Poly(u2 - rad * rad, 2.0 * ud, d2);
                var e = new Variable.Poly(ud, d2);
                var v = new Variable.Poly(wd, -d2);
                var p = a * e;
                var k = v * (a + b) - p;
                var r = a * p * p - 2.0 * b * p * k + c * k * k;
                Algebra.FindAllRoots(r, 0.0, 1.0, out var rs);
                foreach (var ri in rs)
                {
                    double t;
                    var ar = a.Evaluate(ri);
                    var br = b.Evaluate(ri);
                    var cr = c.Evaluate(ri);
                    var pr = p.Evaluate(ri);
                    var kr = k.Evaluate(ri);
                    if (Math.Abs(kr) > 1e-10)
                    {
                        t = -pr / kr;
                        var t2 = -2.0 * br / ar - t;
                        if (t < 0.0 || t > t2 || t2 > 1.0) continue;
                    }
                    else
                    {
                        var delta = br * br - ar * cr;
                        if (delta < 0.0) continue;
                        t = (-br - Math.Sqrt(delta)) / ar;
                        if (t is < 0.0 or > 1.0) continue;
                        var hs = Math.Abs(pr) + Math.Abs(kr * t);
                        if (Math.Abs(pr + kr * t) > 1e-10 * hs) continue;
                    }

                    var cu2 = (t * Math.Sqrt(ar), i, ri);
                    local = Better(cu2, local);
                }

                return local;
            },
            local =>
            {
                lock (gate)
                    bestP = Better(local, bestP);
            });
        if (bestP.Item2 < 0) return null;
        if (bestP.Item2 == path.Count - 1)
            return path[bestP.Item2].Item1;
        var B = path[bestP.Item2].Item1;
        var D = path[bestP.Item2 + 1].Item1 - B;
        return B + D * bestP.Item3;
    }

    /// <summary>
    /// 计算漩涡 → 加水
    /// </summary>
    private static Variable.Vec2D? VortexToLadle(Variable.Spiral? spiral, Variable.Vec2D o, Variable.Vec2D c,
        double rad)
    {
        if (spiral == null) return null;
        var sp = spiral.Value;
        var bestP = (double.PositiveInfinity, 0.0);
        bestP = Better(SpiralPoint(sp, sp.MinAngle, o, c, rad), bestP);
        var width = sp.MaxAngle - sp.MinAngle;
        if (width > 1e-10)
        {
            var n = Math.Max(1, (int)Math.Ceiling(width / Math.PI * 16.0));
            var span = width / n;
            var partitions = new (double, double)[n];
            for (var i = 0; i < n; i++)
            {
                var a = sp.MinAngle + i * span;
                var b = i == n - 1 ? sp.MaxAngle : sp.MinAngle + (i + 1) * span;
                partitions[i] = (a, b);
            }

            var locals = new List<double>[n];
            Parallel.For(0, n,
                i => locals[i] =
                    Algebra.GetEnclosureRoots(PhiPoint, EnclosePhi, partitions[i].Item1, partitions[i].Item2));
            var rootl = locals.SelectMany(l => l).OrderBy(x => x).ToList();
            var roots = new List<double>();
            foreach (var r in rootl)
                if (roots.Count == 0 || Math.Abs(roots.Last() - r) > 1e-10)
                    roots.Add(r);
            foreach (var r in roots)
                bestP = Better(SpiralPoint(sp, r, o, c, rad), bestP);
            bestP = Better(SpiralPoint(sp, sp.MaxAngle, o, c, rad), bestP);
        }

        return new(sp.Pos.X + Variable.Spiral.A * bestP.Item2 * Math.Cos(bestP.Item2 + sp.Rot),
            sp.Pos.Y + Variable.Spiral.A * bestP.Item2 * Math.Sin(bestP.Item2 + sp.Rot));

        double PhiPoint(double th)
        {
            var phi = th + sp.Rot;
            var sinPhi = Math.Sin(phi);
            var cosPhi = Math.Cos(phi);
            var dir = new Variable.Vec2D(cosPhi, sinPhi);
            var tan = new Variable.Vec2D(-sinPhi, cosPhi);
            var z = c - o;
            var d = sp.Pos + th * Variable.Spiral.A * dir - o;
            var dp = Variable.Spiral.A * (dir + th * tan);
            var q2 = d.SqrMagnitude;
            var zd = d.Dot(z);
            var h = zd * zd - (z.SqrMagnitude - rad * rad) * q2;
            if (h <= 0.0) return double.NaN;
            var d2 = d.Dot(dp);
            var zdp = dp.Dot(z);
            var t = zd * d2 - zdp * q2;
            return 2.0 * (Math.Sqrt(h) * (d2 * q2 + t) + zd * t);
        }

        Variable.Interval? EnclosePhi(Variable.Interval th)
        {
            var phi = th + Variable.Interval.Point(sp.Rot);
            var sinPhi = phi.Sin();
            var cosPhi = phi.Cos();
            var at = th * Variable.Spiral.A;
            var dx = Variable.Interval.Point(sp.Pos.X - o.X) + at * cosPhi;
            var dy = Variable.Interval.Point(sp.Pos.Y - o.Y) + at * sinPhi;
            var dxp = (cosPhi - th * sinPhi) * Variable.Spiral.A;
            var dyp = (sinPhi + th * cosPhi) * Variable.Spiral.A;
            var z = c - o;
            var k2 = z.SqrMagnitude - rad * rad;
            var q2 = dx * dx + dy * dy;
            var zd = dx * z.X + dy * z.Y;
            var h = zd * zd - q2 * k2;
            var d2 = dx * dxp + dy * dyp;
            var zdp = dxp * z.X + dyp * z.Y;
            var t = zd * d2 - zdp * q2;
            return 2.0 * (h.Sqrt() * (d2 * q2 + t) + zd * t);
        }
    }

    #endregion

    #region 计算入口

    /// <summary>
    /// 路径 → 加水计算入口
    /// </summary>
    public static void StartPathToLadle()
    {
        if (Variable.HandoffRunning >= 0) return;
        Variable.HandoffRunning = 1;
        if (!Variable.DoPathCurve || !Variable.DoLines[1])
        {
            Variable.HandoffRunning = -1;
            return;
        }

        var path = new List<(Variable.Vec2D, bool)>(Variable.PathPhysical);
        var offset = Variable.Offset;
        GetEffectAndVortex(out var doEffect, out var doVortex, out var effectPos, out var vortexPos,
            out var vortexRad);
        ThreadingHelper.Instance.StartAsyncInvoke(() =>
        {
            var effectHandoff = doEffect ? PathToLadle(path, offset, effectPos, Variable.EffectRad) : null;
            var vortexHandoff = doVortex ? PathToLadle(path, offset, vortexPos, vortexRad) : null;
            return () =>
            {
                Variable.HandoffPositions[0] = effectHandoff;
                Variable.HandoffPositions[1] = vortexHandoff;
                Variable.HandoffRunning = -1;
            };
        });
    }

    /// <summary>
    /// 漩涡 → 加水计算入口
    /// </summary>
    public static void StartVortexToLadle()
    {
        if (Variable.HandoffRunning >= 0) return;
        Variable.HandoffRunning = 3;
        if (!Variable.DoVortexCurve || !Variable.DoLines[1])
        {
            Variable.HandoffRunning = -1;
            return;
        }

        var spiral = Variable.SpiralPhysical;
        var offset = Variable.Offset;
        GetEffectAndVortex(out var doEffect, out var doVortex, out var effectPos, out var vortexPos,
            out var vortexRad);
        ThreadingHelper.Instance.StartAsyncInvoke(() =>
        {
            var effectHandoff = doEffect ? VortexToLadle(spiral, offset, effectPos, Variable.EffectRad) : null;
            var vortexHandoff = doVortex ? VortexToLadle(spiral, offset, vortexPos, vortexRad) : null;
            return () =>
            {
                Variable.HandoffPositions[0] = effectHandoff;
                Variable.HandoffPositions[1] = vortexHandoff;
                Variable.HandoffRunning = -1;
            };
        });
    }

    /// <summary>
    /// 加水 → 路径计算入口
    /// </summary>
    public static void StartLadleToPath()
    {
    }

    /// <summary>
    /// 漩涡 → 路径计算入口
    /// </summary>
    public static void StartVortexToPath()
    {
    }

    #endregion

    #region 辅助计算

    /// <summary>
    /// 比较两个折线候选结果
    /// </summary>
    private static (double, int, double) Better((double, int, double) a, (double, int, double) b)
    {
        if (Math.Abs(a.Item1 - b.Item1) > 1e-10) return a.Item1 < b.Item1 ? a : b;
        if (a.Item2 != b.Item2) return a.Item2 < b.Item2 ? a : b;
        return a.Item3 < b.Item3 ? a : b;
    }

    /// <summary>
    /// 比较两个螺线候选结果
    /// </summary>
    private static (double, double) Better((double, double) a, (double, double) b)
    {
        if (Math.Abs(a.Item1 - b.Item1) > 1e-10) return a.Item1 < b.Item1 ? a : b;
        return a.Item2 > b.Item2 ? a : b;
    }

    /// <summary>
    /// 计算单个螺线点
    /// </summary>
    private static (double, double) SpiralPoint(Variable.Spiral S, double ang, Variable.Vec2D O, Variable.Vec2D C,
        double rad)
    {
        var pos = new Variable.Vec2D(S.Pos.X + Variable.Spiral.A * ang * Math.Cos(ang + S.Rot),
            S.Pos.Y + Variable.Spiral.A * ang * Math.Sin(ang + S.Rot));
        Geometry.LineVsCircle(pos, O, C, rad, out var t);
        return (t.HasValue ? t.Value * O.Distance(pos) : double.PositiveInfinity, ang);
    }

    /// <summary>
    /// 获取目标效果和漩涡参数
    /// </summary>
    private static void GetEffectAndVortex(out bool doEffect, out bool doVortex, out Variable.Vec2D effectPos,
        out Variable.Vec2D vortexPos, out double vortexRad)
    {
        doEffect = Variable.TargetEffect != null;
        var mapId = Variable.MapId[Managers.RecipeMap.currentMap.potionBase.name];
        var vortexId = mapId < 2 ? Variable.VortexIndex[mapId] : -1;
        doVortex = vortexId >= 0;
        effectPos = doEffect ? Variable.Vec2D.From(Variable.TargetEffect.transform.localPosition) : default;
        var selVortex = doVortex ? Variable.Vortices[mapId][vortexId] : default;
        vortexPos = selVortex.Pos;
        vortexRad = selVortex.Rad;
    }

    #endregion
}