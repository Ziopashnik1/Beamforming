using System;
using JetBrains.Annotations;

namespace DSP.Lib
{
    public static class DoubleFuncExtensions
    {
        public static double[] Sample([NotNull] this Func<double, double> f, double dt, int SamplesCount)
        {
            if (f is null) throw new ArgumentNullException(nameof(f));
            if(dt <= 0) throw new ArgumentOutOfRangeException(nameof(dt), "Период дискретизации должен быть больше 0");
            if(SamplesCount <= 0) throw new ArgumentOutOfRangeException(nameof(SamplesCount), "Число отсчётов должно быть больше 0");

            var samples = new double[SamplesCount];
            for (var i = 0; i < SamplesCount; i++)
                samples[i] = f(i * dt);

            return samples;
        }
    }
}
