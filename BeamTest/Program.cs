using BeamService;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BeamTest
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var N = 8;
            var f = 1e9;
            var d = 15e-2;
            const double toRad = Math.PI / 180;
            var th = 15 * toRad;
            const double c = 3e8;
            var dt = d / c * Math.Sin(th);

            var sources = new Source[N];
            for (int i = 0; i < N; i++)
            {
                var i0 = i;
                sources[i] = new Source(t => Math.Cos(2 * Math.PI * f * (t - i0 * dt)));
            }

            var test_source = new Source(t => Math.Cos(2 * Math.PI * f * t));

            var Nt = 8;

            var s_data = new double[N, Nt];

            var adc = new ADC(20, 5e9, 2);
            for (int i = 0; i < N; i++)
            {
                var signal = adc.GetDiscretSignal(sources[i], Nt);
                for (int n = 0; n < Nt; n++)
                    s_data[i, n] = signal[n];
            }

            var test_signal = adc.GetDiscretSignal(test_source, Nt);

            var test_spectrum = GetSpectrum(test_signal);

            var ss = new Matrix(s_data);
            
            var W = MatrixComplex.Create(8, 8, (i, j) => Complex.Exp(-2 * Math.PI * Complex.ImaginaryOne * i * j / 8));

            var SS = ss * W;

            Console.WriteLine(SS.ToString("f3"));

            double[] Th = { 0, 5, 10 ,15, 20 };

            var Uth = Th.Select(t => Math.Sin(t * toRad)).ToArray();

            

            //var Wth = MatrixComplex.Create(8, 8 , (i, j) => { throw new NotImplementedException(); });

            //var QQ = SS * Wth;

            //var qq = QQ * W.GetComplexConj();

            Console.ReadLine();
        }

        private static double Function(double t)
        {
            return Math.Sin(2 * Math.PI * 1 * t);
        }

        private static Complex[] GetSpectrum(double[] s)
        {
            var N = 8;
            var spektr = new Complex[N];
            for (int k = 0; k < N; k++)
                for (int n = 0; n < N; n++)
                    spektr[k] += s[n] * Complex.Exp(-2 * Math.PI * Complex.ImaginaryOne * n * k / N);
            return spektr;
        }
    }
}
