using System;
using System.Linq;
using System.Numerics;
using DSP.Lib;
using DSP.Lib.Service;

namespace DSP.TestConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const double fp = 500;
            const double fs = 1500;
            const double DeltaF = 10;
            const double fd = 5000;
            const double dt = 1 / fd;
            const int SamplesCount = 5000;

            const double Rp = 1;
            const double Rs = 30;

            var filter = new ButterworthLowPass(fp, fs, dt);
        
            var s0 = new DigitalSignal(dt, Enumerable.Repeat(1d, SamplesCount));
            var A0 = Math.Sqrt(2);
            var sp = new DigitalSignal(dt, SamplesCount, t => A0 * Math.Cos(2 * Math.PI * fp * t));
            var ss = new DigitalSignal(dt, SamplesCount, t => A0 * Math.Cos(2 * Math.PI * fs * t));

            var H0 = filter.GetTransmissionCoefficient(0, dt).Magnitude;
            var H0P = H0 * H0;
            var y0 = filter.Filter(s0);
            filter.Reset();
            var Hsp = filter.GetTransmissionCoefficient(fp, dt).Magnitude;
            var HspP = Hsp * Hsp;
            var yp = filter.Filter(sp);
            filter.Reset();
            var Hss = filter.GetTransmissionCoefficient(fs, dt).Magnitude;
            var HssP = Hss * Hss;
            var ys = filter.Filter(ss);
            filter.Reset();

            var s = s0 + sp + ss;
            var S = s.GetSpectrum();

            var absS = S.Select(v => v.Magnitude).ToArray();
            var argS = S.Select(v => v.Phase).ToArray();

            var y = filter.Filter(s);
            filter.Reset();


            var Y = y.GetSpectrum();

            var absY = Y.Select(v => v.Magnitude).ToArray();
            var argY = Y.Select(v => v.Phase).ToArray();

            

            Console.ReadLine();
        }

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
    }
}
