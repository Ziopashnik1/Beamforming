using System;
using System.Collections.Generic;
using MathService;

namespace BeamService
{
    public static class Pattern
    {
        private const double toRad = Math.PI / 180;

        /// <summary>
        /// Попытка нахождения максимума
        /// </summary>
        /// <param name="F"></param>
        /// <param name="th0"></param>
        /// <param name="th1"></param>
        /// <param name="th2"></param>
        /// <param name="dth"></param>
        /// <returns></returns>
        public static double GetMaximum(this Func<double, Complex> F, out double th0, double th1 = -90 * toRad, double th2 = 90 * toRad, double dth = 0.1 * toRad)
        {
            var th = th0 = th1;
            var F_max = double.NegativeInfinity;
            while (th <= th2)
            {
                var f = F(th).Abs;
                if (f > F_max)
                {
                    F_max = f;
                    th0 = th;
                }
                th += dth;
            }
            return F_max;
        }

        public static void GetPatternWidth(this Func<double, Complex> F, double th0, out double Left07, out double Right07, out double Left0, out double Right0, double dth = 0.1 * toRad)
        {
            var max = F(th0).Abs;
            var max07 = max / 2;

            var th = th0;
            var f = max;
            while (f > max07) f = F(th -= dth).Abs;
            Left07 = th;

            var f1 = f;
            while (f1 <= f)
            {
                th -= dth;
                f = f1;
                f1 = F(th).Abs;
            }
            Left0 = th + dth * 0.5;

            th = th0;
            f = max;
            while (f >= max07) f = F(th += dth).Abs;
            Right07 = th;

            f1 = Math.Abs(f);
            while (f1 <= f)
            {
                th += dth;
                f = f1;
                f1 = F(th).Abs;
            }
            Right0 = th - dth * 0.5;
        }

        /// <summary>
        /// Попытка нахождения максимумов (УБЛ)
        /// </summary>
        /// <param name="F"></param>
        /// <param name="f0"></param>
        /// <param name="pattern"></param>
        /// <param name="th1"></param>
        /// <param name="th2"></param>
        /// <param name="dth"></param>
        /// <returns></returns>
        public static IEnumerable<PatternValue> GetMaximums(this Func<double, Complex> F, double th1 = -90 * toRad, double th2 = 90 * toRad, double dth = 0.5 * toRad)
        {
            var th = th1;
            if (F(th).Abs > F(th + dth).Abs)
                yield return new PatternValue { Angle = th1, Value = F(th1).Abs };

            while (th <= th2)
            {
                th += dth;
                var F1 = F(th - dth).Abs;
                var F0 = F(th).Abs;
                var F2 = F(th + dth).Abs;

                if (F0 > F1 && F0 > F2)
                    yield return new PatternValue { Angle = th, Value = F0 };
            }

            if (F(th2).Abs > F(th2 - dth).Abs)
                yield return new PatternValue { Angle = th2, Value = F(th2).Abs };
        }

    }
}
