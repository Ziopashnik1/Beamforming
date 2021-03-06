﻿using System;
using System.Linq;
using System.Numerics;
using DSP.Lib.Service;

namespace DSP.Lib
{
    public class ButterworthLowPass : IIR
    {
        private class ButterworthLowPassOptions
        {
            private static double Fd(double f, double dt) => Math.Tan(Math.PI * f * dt) / (Math.PI * dt);

            private static Complex[] Poluses(int N, double alpha)
            {
                var L = N / 2;
                var r = N % 2;

                var result = new Complex[N];

                if (r > 0) result[0] = new Complex(-alpha, 0);

                var w = Math.PI / (2 * N);
                var i = r;
                for (var n = 1; n <= L; n++)
                {
                    var sin = Math.Sin(w * (2 * n - 1));
                    var cos = Math.Cos(w * (2 * n - 1));
                    result[i] = new Complex(-alpha * sin, alpha * cos);
                    result[i + 1] = new Complex(-alpha * sin, -alpha * cos);
                    i += 2;
                }

                return result;
            }

            private static Complex ZTransform(in Complex p, double dt) => (2 / dt + p) / (2 / dt - p);

            private static double Kz(Complex[] p, double dt)
            {
                var result = new Complex(1, 0);

                for (var i = 0; i < p.Length; i++)
                    result *= 2 / dt - p[i];

                if (Math.Abs(result.Phase) > 1e-16)
                    throw new InvalidOperationException("Комплексный результат");

                return 1 / result.Real;
            }

            public readonly double[] A;
            public readonly double[] B;

            public ButterworthLowPassOptions(double fp, double fs, double dt, double Rp = 1, double Rs = 30)
            {
                var Fp = Fd(fp, dt);
                var Fs = Fd(fs, dt);
                var k_F = Fs / Fp;

                var eps_p = Math.Sqrt(Math.Pow(10, Rp / 10) - 1);
                var eps_s = Math.Sqrt(Math.Pow(10, Rs / 10) - 1);
                var k_eps = eps_s / eps_p;

                var double_N = Math.Log(k_eps) / Math.Log(k_F);
                var N = (int)double_N;
                if (double_N > N) N++;

                var alpha = Math.Pow(eps_p, -1d / N);

                var p1 = Poluses(N, alpha * 2 * Math.PI * Fp);

                var z1 = p1.Select(p => ZTransform(p, dt)).ToArray();

                var Kz1 = Kz(p1, dt);

                var K = Kz1 * Math.Pow(2 * Math.PI * Fp, N) / eps_p;

                B = Polynom.GetCoefficients(Enumerable.Repeat(-1d, N).ToArray()).Select(b => b * K).ToArray();

                A = Polynom.GetCoefficients(z1).Select(a => a.Real).Reverse().ToArray();
            }
        }

        public ButterworthLowPass(double fp, double fs, double dt, double Rp = 1, double Rs = 30)
            : this(new ButterworthLowPassOptions(fp,fs,dt,Rp,Rs)) { }

        private ButterworthLowPass(ButterworthLowPassOptions Options) : base(Options.A, Options.B) { }
    }
}
