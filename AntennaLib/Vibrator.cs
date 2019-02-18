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

        private static readonly Func<double, double> ln = Math.Log;
        private static readonly Func<double, double> abs = Math.Abs;
        private static readonly Func<double, double> sin = Math.Sin;
        private static readonly Func<double, double> cos = Math.Cos;
        private static readonly Func<double, double> tg = Math.Tan;
        //private static readonly Func<Expression, MethodCallExpression> ln_exp = MathExpression.Log;
        //private static readonly Func<Expression, MethodCallExpression> abs_exp = MathExpression.Abs;
        private static readonly Func<Expression, MethodCallExpression> sin_exp = MathExpression.Sin;
        private static readonly Func<Expression, MethodCallExpression> cos_exp = MathExpression.Cos;
        //private static readonly Func<Expression, MethodCallExpression> tg_exp = MathExpression.Tan;

        private static double GetK_f(double f)
        {
            Contract.Requires(f > 0);
            Contract.Ensures(Contract.Result<double>() > 0);
            Contract.Ensures(Contract.Result<double>().Equals(f * k));
            return k * f;
        }

        private static double GetK_l(double l)
        {
            Contract.Requires(l > 0);
            Contract.Ensures(Contract.Result<double>() > 0);
            Contract.Ensures(Contract.Result<double>().Equals(Consts.pi2 / l));

            return Consts.pi2 / l;
        }

        public static double GetRadiatingImpedance(double Length, double f0)
        {
            Contract.Requires(Length > 0);
            Contract.Requires(f0 > 0);
            Contract.Ensures(Contract.Result<double>() > 0);

            var kl = GetK_f(f0) * Length;
            var coskl = cos(kl);

            Func<double, double> F = thetta => cos(kl * cos(thetta)) - coskl;
            Func<double, double, double> core = (f, thetta) => thetta.Equals(0) ? 1 : f * f / sin(thetta);
            return 60 * F.GetIntegralValue(core, 0, Consts.pi, Consts.pi / 10000);
        }

        public static Complex GetInputImpedance(double Length, double rho, double f0)
        {
            Contract.Requires(Length > 0);
            Contract.Requires(rho > 0);
            Contract.Requires(f0 > 0);
            Contract.Ensures(Contract.Result<Complex>().Re >= 0);

            var k = GetK_f(f0);
            var l2_4 = 4 * Length * Length;
            var cos2kl = cos(2 * k * Length);
            var m = l2_4 * cos2kl - cos2kl + l2_4 + 1;
            var re = 4 * rho * Length / m;
            var im = rho * sin(2 * k * Length) * (l2_4 - 1) / m;
            return new Complex(re, im);
        }

        public static double CurentDestribution(double f0, double Length, double z)
        {
            Contract.Requires(Length > 0);
            Contract.Requires(f0 > 0);
            Contract.Ensures(Math.Abs(Contract.Result<double>()) <= 1);
            return sin(GetK_f(f0) * (Length - abs(z)));
        }

        public static double GetWaveImpedance(double d, double D)
        {
            Contract.Requires(d > 0);
            Contract.Requires(D > 0);
            Contract.Ensures(Contract.Result<double>() >= 0);
            return 120 * (ln(D / d) - 1);
        }

        private double f_Length;

        public double Length
        {
            [System.Diagnostics.Contracts.Pure]
            get
            {
                Contract.Ensures(Contract.Result<double>() >= 0);
                Contract.Ensures(Contract.Result<double>().Equals(f_Length));
                return f_Length;
            }
            set
            {
                Contract.Requires(value >= 0);
                Contract.Ensures(f_Length >= 0);
                Contract.Ensures(f_Length.Equals(value));
                if(f_Length.Equals(value)) return;
                f_Length = value;
                OnPropertyChanged();
            }
        }

        public Vibrator() : this(1) { }

        public Vibrator(double Length)
        {
            Contract.Requires(Length > 0);
            Contract.Ensures(f_Length > 0);
            Contract.Ensures(f_Length.Equals(Length));
            f_Length = Length;
        }

        [System.Diagnostics.Contracts.Pure]
        public override Complex Pattern(SpaceAngle Direction, double f)
        {
            Contract.Requires(f > 0);

            var th = Direction.ThettaRad;
            var kl = k * f * f_Length;
            var cos_kl = cos(kl);
            return th.Equals(0) ? 0 : (cos(kl * cos(th)) - cos_kl) / /*(1 - cos_kl) /*/ sin(th);
        }

        public override Expression GetPatternExpressionBody(Expression a, Expression f)
        {
            var th = a.GetProperty(nameof(SpaceAngle.ThettaRad));
            var kl = k.ToExpression().Multiply(f).Multiply(this.ToExpression().GetField(nameof(f_Length)));
            var cos_kl = cos_exp(kl);
            return cos_exp(kl.Multiply(cos_exp(th))).Subtract(cos_kl)
                .Divide(1d.ToExpression().Subtract(cos_kl).Multiply(sin_exp(th)));
        }

        [System.Diagnostics.Contracts.Pure]
        public double GetActiveLength(double f)
        {
            Contract.Requires(f > 0);
            Contract.Ensures(Contract.Result<double>() > 0);

            const double k = Consts.pi / Consts.SpeedOfLigth;
            var k05 = k * f;
            return tg(k05 * f_Length) / k05;
        }

        [System.Diagnostics.Contracts.Pure]
        public double CurentDestribution(double f0, double z)
        {
            Contract.Requires(f0 > 0);
            return CurentDestribution(f0, f_Length, z);
        }

        [System.Diagnostics.Contracts.Pure, NotNull]
        public Func<double, double> CurentDestribution(double f0)
        {
            Contract.Requires(f0 > 0);
            Contract.Ensures(Contract.Result<Func<double, double>>() != null);
            return z => CurentDestribution(f0, z);
        }

        [System.Diagnostics.Contracts.Pure]
        public override string ToString()
        {
            Contract.Ensures(Contract.Result<string>() != null);
            return $"Вибратор L={Length}";
        }
    }
}