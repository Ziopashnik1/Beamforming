using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using MathService;
using MathService.Annotations;
using MathService.Vectors;
using Task = System.Threading.Tasks.Task;

namespace Antennas
{
    public static class BeamPatternExtentions
    {
        public class BeamAnalyseResult
        {
            private readonly FuncExtentions.SamplingResult<Complex>.Result[] f_Samples;
            private readonly int f_Left07Index;
            private readonly int f_Right07Index;
            private readonly int f_LeftSllIndex;
            private readonly int f_RightSllIndex;
            public int MaximumIndex { get; }
            public double AngleOfMaximum => f_Samples[MaximumIndex].Argument;
            public Complex ValueOfMaxMaximum => f_Samples[MaximumIndex].Value;
            public Tuple<double, Complex> Maximum => f_Samples[MaximumIndex];

            public double BeamWidth => f_Samples[f_Right07Index].Argument - f_Samples[f_Left07Index].Argument;
            public double BeamCenterAngle => (f_Samples[f_Right07Index].Argument + f_Samples[f_Left07Index].Argument) / 2;

            public double Simetry
            {
                get
                {
                    var result = (Maximum.Item1 - BeamCenterAngle) / BeamWidth;
                    return result * result;
                }
            }

            public Tuple<double, Complex> LeftSideLobe => f_Samples[f_LeftSllIndex];
            public Tuple<double, Complex> RightSideLobe => f_Samples[f_RightSllIndex];
            public double LeftAverageSideLobLevel { get; }
            public double RightAverageSideLobLevel { get; }
            public double Directivity { get; }
            public double Accuracy { get; }

            public BeamAnalyseResult
            (
                FuncExtentions.SamplingResult<Complex>.Result[] Samples,
                int MaximumIndex,
                int Left07Index,
                int Right07Index,
                int LeftSLLIndex, int RightSLLIndex,
                double LeftAverageSLL, double RightAwerageSLL,
                double Directivity,
                double Accuracy
            )
            {
                this.f_Samples = Samples;
                this.MaximumIndex = MaximumIndex;
                f_Left07Index = Left07Index;
                f_Right07Index = Right07Index;
                f_LeftSllIndex = LeftSLLIndex;
                f_RightSllIndex = RightSLLIndex;
                LeftAverageSideLobLevel = LeftAverageSLL;
                RightAverageSideLobLevel = RightAwerageSLL;
                this.Directivity = Directivity;
                this.Accuracy = Accuracy;
            }
        }

        public static BeamAnalyseResult Analyse
        (
            this Func<double, Complex> f,
            double AngleMin = -Consts.pi, double AngleMax = Consts.pi,
            double Accuracy = 1e-2
        )
        {
            Contract.Requires(f != null);

            var ff = f.SamplingAdaptive_OneWay(fv => fv.Power, AngleMin, AngleMax, Accuracy).ToArray();

            var max_pos = ff.GetMaxIndex(fv => fv.Value.Power);

            var max2 = ff[max_pos].Value.Power;
            var ff_2 = Array.ConvertAll(ff, v => v.Value.Power / max2);
            var ff_db = Array.ConvertAll(ff_2, v => v.In_dB_byPower());

            var index_07_left = 0;
            var index_07_right = 0;
            var length = ff.Length;
            for(int i = 0, i_left = max_pos, i_right = max_pos; i < length && (index_07_left == 0 || index_07_right == 0); i++)
            {
                if(index_07_left == 0)
                {
                    i_left--;
                    if(i_left <= 0) index_07_left = 0;
                    else
                    {
                        var left = ff_db[i_left];
                        if(left < -3)
                            index_07_left = -3 - left < ff_db[i_left + 1] + 3 ? i_left : i_left + 1;
                    }
                }
                if(index_07_right == 0)
                {
                    i_right++;
                    if(i_right >= length - 1) index_07_right = length - 1;
                    else
                    {
                        var right = ff_db[i_right];
                        if(right < -3)
                            index_07_right = -3 - right < ff_db[i_right - 1] + 3 ? i_right : i_right + 1;
                    }
                }
            }

            var max_angle = ff[max_pos].Argument;
            var beam_width = ff[index_07_right].Argument - ff[index_07_left].Argument;


            var sll_left_average = 0d;
            var sll_left_max_index = 0;
            var sll_left_max = double.NegativeInfinity;
            var index = index_07_left;
            while(ff[index].Argument > max_angle - beam_width && index >= 0) index--;
            for(var i = index; i >= 0; i--)
            {
                var fv = ff_2[i];
                sll_left_average += fv;
                if(fv <= sll_left_max) continue;
                sll_left_max = fv;
                sll_left_max_index = i;
            }
            sll_left_average /= index;

            var sll_right_average = 0d;
            var sll_right_max_index = 0;
            var sll_right_max = double.NegativeInfinity;
            index = index_07_right;
            while(ff[index].Argument > max_angle + beam_width && index < length) index++;
            for(var i = index; i < length; i++)
            {
                var fv = ff_2[i];
                sll_right_average += fv;
                if(fv <= sll_right_max) continue;
                sll_right_max = fv;
                sll_right_max_index = i;
            }
            sll_right_average /= length - index;

            return new BeamAnalyseResult
            (
                ff, max_pos,
                index_07_left, index_07_right,
                sll_left_max_index, sll_right_max_index,
                sll_left_average, sll_right_average,
                GetDirectivity(ff, max_pos),
                Accuracy
            );


        }

        private static double GetDirectivity(FuncExtentions.SamplingResult<Complex>.Result[] ff, double max_pos)
        {
            var d = 0d;
            var f_last = ff[0].Value.Power * Math.Cos(ff[0].Argument - max_pos);
            for(var i = 1; i < ff.Length; i++)
            {
                var f = ff[i].Value.Power * Math.Cos(ff[i].Argument - max_pos);
                d += (ff[i].Argument - ff[i - 1].Argument) * (f + f_last) / 2;
                f_last = f;
            }
            var a = ff[ff.Length - 1].Argument - ff[0].Argument;
            return (a * a) / d;
        }



        public static Func<double, Complex> ToThettaPattern(Func<SpaceAngle, Complex> f, double Phi) => Thetta => f(new SpaceAngle(Thetta, Phi));

        public static Func<double, Complex> ToPhiPattern(Func<SpaceAngle, Complex> f, double Thetta) => Phi => f(new SpaceAngle(Thetta, Phi));

        public static double GetDerectivity(this Func<SpaceAngle, Complex> F, Action<double> Complite = null) =>
            GetDerectivity((th, phi) => F(new SpaceAngle(th, phi)), Complite);

        public static double GetDerectivity(this Func<double, double, Complex> F, Action<double> Complite = null)
        {
            Contract.Requires(F != null);

            var i_p = 0;
            var N_p = 360;
            var N_t = 360;

            var f = Complite != null
                ? (Func<double, double>)(p =>
               {
                   i_p++;
                   var I1 = FuncExtentions.GetIntegralValue(t =>
                   {
                       var I0 = F(t, p).Power * Math.Sin(t);
                       return I0;
                   }, 0, Consts.pi, Consts.pi / N_t);
                   Complite.Invoke((double)i_p / (N_p + 1));
                   return I1;
               })
                : (p => FuncExtentions.GetIntegralValue(t => F(t, p).Power * Math.Sin(t), 0, Consts.pi, Consts.pi / N_t));

            var I = f.GetIntegralValue(0, Consts.pi2, Consts.pi2 / N_p);

            return 2 * Consts.pi2 / I;
        }

        public static double GetDerectivityBuffered(this Func<SpaceAngle, Complex> F, Action<double> Complite = null) =>
            GetDerectivityBuffered((th, phi) => F(new SpaceAngle(th, phi)), Complite);
        public static double GetDerectivityBuffered(this Func<double, double, Complex> F, Action<double> Complite = null)
        {
            Contract.Requires(F != null);

            var i_p = 0;
            var N_p = 360;
            var N_t = 360;
            var buffer = new Dictionary<double, Dictionary<double, double>>(360);

            var f = Complite != null
                ? (Func<double, double>)(p =>
                {
                    var b = buffer.GetValueOrAddNew(p, () => new Dictionary<double, double>(360));
                    i_p++;
                    Func<double, double> f0 = t => b.GetValueOrAddNew(t, th => F(th, p).Power * Math.Sin(th));
                    var I1 = f0.GetIntegralValue(0, Consts.pi, Consts.pi / N_t);
                    Complite.Invoke((double)i_p / (N_p + 1));
                    return I1;
                })
                : (p =>
                {
                    var b = buffer.GetValueOrAddNew(p, () => new Dictionary<double, double>(360));
                    Func<double, double> f0 = t => b.GetValueOrAddNew(t, th => F(th, p).Power * Math.Sin(th));
                    return f0.GetIntegralValue(0, Consts.pi, Consts.pi / N_t);
                });

            var I = f.GetIntegralValue(0, Consts.pi2, Consts.pi2 / N_p);

            return 2 * Consts.pi2 / I;
        }

        public static double GetDirectivity([NotNull] this Func<double, Complex> F)
        {
            Contract.Requires(F != null);

            var buffer = new Dictionary<double, Complex>();
            Func<double, Complex> f = A => buffer.GetValueOrAddNew(A, a => F(a));

            var I = FuncExtentions.GetIntegralValue_Adaptive(th => f(th).Power * Math.Cos(th), -Consts.pi, Consts.pi);
            return 2 / I;
        }

        public static Task<double> GetDirectivityAsync([NotNull] this Func<double, Complex> F)
        {
            Contract.Requires(F != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);
            return Task.Run(() => F.GetDirectivity());
        }

        public static double GetDirectivityBuffered([NotNull] this Func<double, Complex> F)
        {
            Contract.Requires(F != null);
            var I = FuncExtentions.GetIntegralValue_Adaptive(th => F(th).Power * Math.Cos(th), -Consts.pi, Consts.pi);
            return 2 / I;
        }

        public static Task<double> GetDirectivityBufferedAsync([NotNull] this Func<double, Complex> F)
        {
            Contract.Requires(F != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);
            return Task.Run(() => F.GetDirectivityBuffered());
        }
    }
}
