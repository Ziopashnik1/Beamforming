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
            Console.WriteLine("Число элементов решётки {0}", N);
            var f = 1e9;
            Console.WriteLine("Частота сигнала {0} ГГц", f / 1e9);
            var T = 1 / f;
            Console.WriteLine("Период сигнала {0} нс", T * 1e9);
            var d = 15e-2;
            Console.WriteLine("Шаг между излучателями в решётке {0} см", d * 100);
            const double toRad = Math.PI / 180;
            var th = 15 * toRad;
            const double c = 3e8;
            var delta_t = d / c * Math.Sin(th);
            Console.WriteLine("Временная задержка фронта между излучателями {0} нс", delta_t * 1e9);

            var sources = new Source[N];
            for (var i = 0; i < N; i++)
            {
                var i0 = i;
                sources[i] = new Source(t => Math.Cos(2 * Math.PI * f * (t - i0 * delta_t)));
            }

            var test_source = new Source(t => Math.Cos(2 * Math.PI * f * t));

            var Nt = 8;

            var s_data = new double[N, Nt];


            var fd = 8e9;
            Console.WriteLine("Частота дискретизации {0} ГГц ({1:0.##}f)", fd / 1e9, fd / f);
            var dt = 1 / fd;
            Console.WriteLine("Период дискретизации {0} нс ({1:0.##}T)", dt * 1e9, dt / T);

            var adc = new ADC(20, fd, 10);
            for (var i = 0; i < N; i++)
            {
                var signal = adc.GetDiscretSignal(sources[i], Nt);
                for (var n = 0; n < Nt; n++)
                    s_data[i, n] = signal[n];
            }

            var ss = new Matrix(s_data);
            Console.Write("ss = ");
            Console.WriteLine(ss.ToString("f3"));

            var Nf = Nt;
            Console.WriteLine("Число спектральных компонент {0}", Nf);
            var df = 1 / dt;
            Console.WriteLine("Дискрет частоты в матрице W {0}", df);
            var W = MatrixComplex.Create(Nt, Nf, (i, j) => Complex.Exp(-2 * Math.PI * Complex.ImaginaryOne * i * j / Nf));

            Console.Write("W = ");
            Console.WriteLine(W.ToString("f3"));

            var SS = ss * W;

            Console.Write("|SS| = |ss*W| = ");
            Console.WriteLine(SS.GetMod().ToString("0.##"));

            Console.Write("arg(SS) = ");
            Console.WriteLine((SS.GetArg() / toRad).ToString("000.0"));

            //double[] Th = { 0, 5, 10, 15, 20, 25, 30, 35 };

            //var uth = Th.Select(t => Math.Sin(t * toRad)).ToArray();

            var th0 = 30 * toRad;
            var Wth0 = MatrixComplex.Create(8, 8, (i, j) => Complex.Exp(-Complex.ImaginaryOne * 2 * Math.PI * df / c * Math.Sin(th0) * i * j));
            Console.Write("Wth0({0}) = ", th0 / toRad);
            Console.WriteLine((Wth0.GetArg() / toRad).ToString("000.0"));

            var QQ = SS * Wth0;
            Console.Write("QQ = SS * Wth0 =");
            Console.WriteLine(Wth0.ToString("f3"));

            Console.ReadLine();
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
            for (var k = 0; k < N; k++)
                for (var n = 0; n < N; n++)
                    spektr[k] += s[n] * Complex.Exp(-2 * Math.PI * Complex.ImaginaryOne * n * k / N);
            return spektr;
        }
    }
}
