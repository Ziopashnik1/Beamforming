using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using MathService;
using MathService.Annotations;
using MathService.Extentions.Expressions;
using MathService.Vectors;

namespace Antennas
{
    /// <summary>Симметричный вибратор расположенный вдоль оси OZ</summary>
    public class Vibrator : Antenna
    {
        private const double k = Consts.pi2 / Consts.SpeedOfLigth;

        private static double GetK_f(double f) => k * f;

        //private static double GetK_l(double l) => Consts.pi2 / l;

        public static double GetRadiatingImpedance(double Length, double f0)
        {
            var kl = GetK_f(f0) * Length;
            var coskl = Math.Cos(kl);

            Func<double, double> F = thetta => Math.Cos(kl * Math.Cos(thetta)) - coskl;
            Func<double, double, double> core = (f, thetta) => thetta.Equals(0) ? 1 : f * f / Math.Sin(thetta);
            return 60 * F.GetIntegralValue(core, 0, Consts.pi, Consts.pi / 10000);
        }

        public static Complex GetInputImpedance(double Length, double rho, double f0)
        {
            var k = GetK_f(f0);
            var l2_4 = 4 * Length * Length;
            var cos2kl = Math.Cos(2 * k * Length);
            var m = l2_4 * cos2kl - cos2kl + l2_4 + 1;
            var re = 4 * rho * Length / m;
            var im = rho * Math.Sin(2 * k * Length) * (l2_4 - 1) / m;
            return new Complex(re, im);
        }

        public static double CurentDestribution(double f0, double Length, double z) => Math.Sin(GetK_f(f0) * (Length - Math.Abs(z)));

        public static double GetWaveImpedance(double d, double D) => 120 * (Math.Log(D / d) - 1);

        private double f_Length;

        public double Length
        {
            get => f_Length;
            set
            {
                if(f_Length.Equals(value)) return;
                f_Length = value;
                OnPropertyChanged();
            }
        }

        public Vibrator() : this(1) { }

        public Vibrator(double Length) => f_Length = Length;

        [Pure]
        public override Complex Pattern(SpaceAngle Direction, double f)
        {
            var th = Direction.ThettaRad;
            var kl = k * f * f_Length;
            Func<double, double> cos1 = Math.Cos;
            var cos_kl = cos1(kl);
            return th.Equals(0) ? 0 : (cos1(kl * cos1(th)) - cos_kl) / /*(1 - cos_kl) /*/ Math.Sin(th);
        }

        public override Expression GetPatternExpressionBody(Expression a, Expression f)
        {
            var th = a.GetProperty(nameof(SpaceAngle.ThettaRad));
            var kl = k.ToExpression().Multiply(f).Multiply(this.ToExpression().GetField(nameof(f_Length)));
            var cos_kl = MathExpression.Cos(kl);
            return MathExpression.Cos(kl.Multiply(MathExpression.Cos(th))).Subtract(cos_kl)
                .Divide(1d.ToExpression().Subtract(cos_kl).Multiply(MathExpression.Sin(th)));
        }

        [Pure]
        public double GetActiveLength(double f)
        {
            const double k = Consts.pi / Consts.SpeedOfLigth;
            var k05 = k * f;
            return Math.Tan(k05 * f_Length) / k05;
        }

        [Pure]
        public double CurentDestribution(double f0, double z) => CurentDestribution(f0, f_Length, z);

        [Pure, NotNull]
        public Func<double, double> CurentDestribution(double f0) => z => CurentDestribution(f0, z);

        [Pure]
        public override string ToString() => $"Вибратор L={Length}";
    }
}