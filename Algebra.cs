using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AlchAssV4;

public static class Algebra
{
    #region 通用求根

    /// <summary>
    /// Brent算法
    /// </summary>
    public static double Brent(Func<double, double> f, double a, double b)
    {
        var fa = f(a);
        var fb = f(b);
        var fTol = 1e-12 * Math.Max(1.0, Math.Max(Math.Abs(fa), Math.Abs(fb)));
        var xTol = 1e-12 * Math.Max(1.0, Math.Max(Math.Abs(a), Math.Abs(b)));
        if (Math.Abs(fa) < fTol) return a;
        if (Math.Abs(fb) < fTol) return b;
        if (Math.Abs(fa) < Math.Abs(fb))
        {
            (a, b) = (b, a);
            (fa, fb) = (fb, fa);
        }

        var c = a;
        var fc = fa;
        var d = c;
        var mf = true;
        while (Math.Abs(fb) > fTol && Math.Abs(b - a) > xTol)
        {
            double s;
            if (fa != fc && fb != fc && fa != fb)
                s = a * fb * fc / ((fa - fb) * (fa - fc)) + b * fa * fc / ((fb - fa) * (fb - fc)) +
                    c * fa * fb / ((fc - fa) * (fc - fb));
            else
                s = b - fb * (b - a) / (fb - fa);
            var co1 = s < (3 * a + b) / 4 || s > b;
            var co2 = mf && Math.Abs(s - b) >= Math.Abs(b - c) / 2;
            var co3 = !mf && Math.Abs(s - b) >= Math.Abs(c - d) / 2;
            var co4 = mf && Math.Abs(b - c) < xTol;
            var co5 = !mf && Math.Abs(c - d) < xTol;
            if (co1 || co2 || co3 || co4 || co5)
            {
                s = (a + b) / 2;
                mf = true;
            }
            else
                mf = false;

            var fs = f(s);
            if (double.IsNaN(fs)) return double.NaN;
            d = c;
            c = b;
            fc = fb;
            if (Math.Sign(fa) == Math.Sign(fs))
            {
                a = s;
                fa = fs;
            }
            else
            {
                b = s;
                fb = fs;
            }

            if (Math.Abs(fa) < Math.Abs(fb))
            {
                (a, b) = (b, a);
                (fa, fb) = (fb, fa);
            }
        }

        return b;
    }

    /// <summary>
    /// 求解多项式在区间内的根
    /// </summary>
    public static void FindAllRoots(Variable.Poly p, double l, double r, out List<double> roots)
    {
        roots = [];
        var poly = p.NormalizedInf();
        switch (poly.Degree)
        {
            case < 1:
                return;
            case 1:
            {
                var x = -p.Coef(0) / p.Coef(1);
                if (x > l + 1e-10 && x < r - 1e-10)
                    roots.Add(x);
                return;
            }
            default:
                FindAllRoots(poly.Derivative(), l, r, out var crits);
                crits.Insert(0, l);
                crits.Add(r);
                foreach (var x in from x in crits let y = poly.Evaluate(x) where Math.Abs(y) < 1e-10 select x)
                    AddUniqueRoot(roots, x);
                for (var i = 0; i < crits.Count - 1; i++)
                {
                    var x0 = crits[i];
                    var x1 = crits[i + 1];
                    if (x1 - x0 < 1e-10) continue;
                    var y0 = poly.Evaluate(x0);
                    var y1 = poly.Evaluate(x1);
                    if (Math.Abs(y0) < 1e-10 || Math.Abs(y1) < 1e-10) continue;
                    if (Math.Sign(y0) == Math.Sign(y1)) continue;
                    var root = Brent(poly.Evaluate, x0, x1);
                    if (root > l + 1e-10 && root < r - 1e-10) AddUniqueRoot(roots, root);
                }

                roots.Sort();
                break;
        }
    }

    #endregion

    #region 三次方程求根

    /// <summary>
    /// 求解一般三次方程的全部不同实根
    /// </summary>
    public static List<double> SolveCubic(double A, double B, double C, double D)
    {
        List<double> roots = [];
        var p = B / A;
        var q = C / A;
        var r = D / A;
        var shift = -p / 3.0;
        var P = q - p * p / 3.0;
        var Q = 2.0 * p * p * p / 27.0 - p * q / 3.0 + r;
        var disc = Q * Q / 4.0 + P * P * P / 27.0;
        if (P > 0.0)
        {
            var arg = 1.5 * Q / P * Math.Sqrt(3.0 / P);
            var t = -2.0 * Math.Sqrt(P / 3.0) * Math.Sinh(Asinh(arg) / 3.0);
            AddUniqueRoot(roots, t + shift);
        }
        else if (P < 0.0)
        {
            if (disc > 0.0)
            {
                var sqrtDisc = Math.Sqrt(disc);
                var u = Cbrt(-Q / 2.0 + sqrtDisc);
                var v = Cbrt(-Q / 2.0 - sqrtDisc);
                var denom = u * u - u * v + v * v;
                var t = denom > 0.0 ? -Q / denom : u + v;
                AddUniqueRoot(roots, t + shift);
            }
            else
            {
                var m = 2.0 * Math.Sqrt(-P / 3.0);
                var denom = Math.Sqrt(-P * P * P / 27.0);
                var arg = -Q / 2.0 / denom;
                arg = arg < -1.0 ? -1.0 : arg > 1.0 ? 1.0 : arg;
                var theta = Math.Acos(arg) / 3.0;
                for (var i = 0; i < 3; i++)
                {
                    var t = m * Math.Cos(theta + 2.0 * Math.PI * i / 3.0);
                    AddUniqueRoot(roots, t + shift);
                }
            }
        }
        else
            AddUniqueRoot(roots, Cbrt(-Q) + shift);

        return roots;
    }

    #endregion

    #region 数值处理

    /// <summary>
    /// 添加不重复的根
    /// </summary>
    public static void AddUniqueRoot(List<double> roots, double value)
    {
        if (roots.Any(root => Math.Abs(value - root) < 1e-10))
            return;
        roots.Add(value);
    }

    /// <summary>
    /// 添加不重复的弧度根
    /// </summary>
    public static void AddUniqueAngleRoot(List<double> roots, double angle)
    {
        angle = DeltaAngleRad(0, angle);
        if (roots.Any(root => Math.Abs(DeltaAngleRad(angle, root)) < 1e-10))
            return;
        roots.Add(angle);
    }

    /// <summary>
    /// 转换为极坐标
    /// </summary>
    public static Variable.Vec2D ToPolar(Variable.Vec2D v)
    {
        return new(v.Magnitude, Variable.Vec2D.Right.SignedAngle(v));
    }

    /// <summary>
    /// 转换为直角坐标
    /// </summary>
    public static Variable.Vec2D FromPolar(Variable.Vec2D v)
    {
        var theta = v.Y / 180 * Math.PI;
        return new(v.X * Math.Cos(theta), v.X * Math.Sin(theta));
    }

    /// <summary>
    /// 判断浮点数是否为有限值
    /// </summary>
    public static bool IsFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    /// <summary>
    /// Math.BitIncrement(double)
    /// </summary>
    public static double Up(double x)
    {
        var bits = BitConverter.DoubleToInt64Bits(x);
        if ((bits & 0x7FF0000000000000L) == 0x7FF0000000000000L)
        {
            if ((bits & 0x000FFFFFFFFFFFFFL) != 0 || bits > 0) return x;
            return -double.MaxValue;
        }

        if (bits == long.MinValue) return double.Epsilon;
        bits += bits < 0 ? -1 : 1;
        return BitConverter.Int64BitsToDouble(bits);
    }

    /// <summary>
    /// Math.BitDecrement(double)
    /// </summary>
    public static double Down(double x)
    {
        var bits = BitConverter.DoubleToInt64Bits(x);
        if ((bits & 0x7FF0000000000000L) == 0x7FF0000000000000L)
        {
            if ((bits & 0x000FFFFFFFFFFFFFL) != 0 || bits < 0) return x;
            return double.MaxValue;
        }

        if (bits == 0L) return -double.Epsilon;
        bits += bits < 0 ? 1 : -1;
        return BitConverter.Int64BitsToDouble(bits);
    }

    /// <summary>
    /// Math.Clamp(double)
    /// </summary>
    public static double Clamp(double value, double min, double max)
    {
        return value < min ? min : value > max ? max : value;
    }

    /// <summary>
    /// Math.DeltaAngle(double, deg)
    /// </summary>
    public static double DeltaAngleDeg(double current, double target)
    {
        var delta = (target - current) % 360.0;
        return delta > 180.0 ? delta - 360.0 : delta <= -180.0 ? delta + 360.0 : delta;
    }

    /// <summary>
    /// Math.DeltaAngle(double, rad)
    /// </summary>
    public static double DeltaAngleRad(double current, double target)
    {
        var delta = (target - current) % (2.0 * Math.PI);
        return delta > Math.PI ? delta - 2.0 * Math.PI : delta <= -Math.PI ? delta + 2.0 * Math.PI : delta;
    }

    /// <summary>
    /// Math.Asinh(double)
    /// </summary>
    public static double Asinh(double x)
    {
        return Math.Sign(x) * Math.Log(Math.Abs(x) + Math.Sqrt(x * x + 1.0));
    }

    /// <summary>
    /// Math.Cbrt(double)
    /// </summary>
    public static double Cbrt(double x)
    {
        return x >= 0.0 ? Math.Pow(x, 1.0 / 3.0) : -Math.Pow(-x, 1.0 / 3.0);
    }

    #endregion
}