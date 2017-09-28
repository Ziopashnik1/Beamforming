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
        private static readonly Complex i1 = Complex.ImaginaryOne;
        private const double pi2 = Math.PI * 2;

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
            var th = 45 * toRad;  // угол падения волны 
            const double c = 3e8;
            var delta_t = d / c * Math.Sin(th);
            Console.WriteLine("Временная задержка фронта между излучателями {0} нс", delta_t * 1e9);

            var sources = new Source[N];
            for (var i = 0; i < N; i++)
            {
                var i0 = i;
                sources[i] = new Source(t => Math.Cos(pi2 * f * (t - i0 * delta_t)));
            }

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
            var df = 1 / (dt * Nt);
            Console.WriteLine("Дискрет частоты в матрице W {0}", df);
            var W = MatrixComplex.Create(Nt, Nf, (i, j) => Complex.Exp(-pi2 * i1 * i * j / Nf) / N);

            const string number_format = " 000.0;-000.0";
            Console.Write("W[deg]= ");
            Console.WriteLine((W.GetArg() / toRad).ToString(number_format));

            var SS = ss * W;

            Console.Write("|SS| = |ss*W| = ");
            Console.WriteLine(SS.GetMod().ToString("0.##"));

            Console.Write("arg(SS) = ");
            Console.WriteLine((SS.GetArg() / toRad).ToString(number_format));

            var th0 = 45 * toRad;
            Func<int, int, int> m_correction = (m, M) => m <= M / 2 ? m : m - M; 
            Func<int, int, Complex> w_funct = (i, m) =>
            {
                var fm = m_correction(m, Nf) * df;
                return Complex.Exp(i1 * pi2 / c * fm * i * d * Math.Sin(th0));
            };
            var Wth0 = MatrixComplex.Create(8, 8, w_funct);
            Console.Write("Wth0({0})[deg] = ", th0 / toRad);
            Console.WriteLine((Wth0.GetArg() / toRad).ToString(number_format));

            var QQ = ElementMultiply(SS, Wth0);

            Console.Write("|QQ| = |ss*W| = ");
            Console.WriteLine(QQ.GetMod().ToString("0.##"));

            Console.Write("arg(QQ) = ");
            Console.WriteLine((QQ.GetArg() / toRad).ToString(number_format));

            var Q = SumRows(QQ);

            Console.Write("|Q| = ??? = ");
            Console.WriteLine(Q.GetMod().ToString("0.##"));

            Console.Write("arg(Q) = ");
            Console.WriteLine((Q.GetArg() / toRad).ToString(number_format));

            var W_inv = MatrixComplex.Create(Nt, Nf, (i, j) => Complex.Exp(pi2 * i1 * i * j / Nf));
            //var W_inv = W * N;

            Console.Write("W_inv[deg]= ");
            Console.WriteLine((W_inv.GetArg() / toRad).ToString(number_format));

            var q = Q * W_inv;

            Console.Write("Re(q) = Re(q*W_inv) = ");
            Console.WriteLine(q.GetReal().ToString("0.##"));

            Console.Write("Im(q) = Im(q*W_inv) = ");
            Console.WriteLine(q.GetIm().ToString("0.##"));

            Console.ReadLine();
        }

        private static MatrixComplex ElementMultiply(MatrixComplex A, MatrixComplex B)
        {
            if(A.M != B.M) throw new InvalidOperationException("Число столбцов матриц не совпадает"); 
            if(A.N != B.N) throw new InvalidOperationException("Число строк матриц не совпадает");
            
            var result = new MatrixComplex(A.N, A.M);
            for(var i = 0; i < A.N; i++)
            for (var j = 0; j < B.M; j++)
                result[i, j] = A[i, j] * B[i, j];
            return result;
        }

        private static MatrixComplex SumRows(MatrixComplex A)
        {
            var result = new MatrixComplex(1, A.M);
            for (var j = 0; j < A.M; j++)
            {
                var summ = new Complex();
                for (var i = 0; i < A.N; i++) summ += A[i, j];
                result[0, j] = summ;
            }
            return result;
        }

        private static Complex[] GetSpectrum(double[] s)
        {
            var N = 8;
            var spektr = new Complex[N];
            for (var k = 0; k < N; k++)
                for (var n = 0; n < N; n++)
                    spektr[k] += s[n] * Complex.Exp(-pi2 * i1 * n * k / N);
            return spektr;
        }
    }
}
