using System;
using JetBrains.Annotations;

namespace DSP.Lib
{
    public static class DigitalFilterExtensions
    {
        public static double[] GetImpulseResponse([NotNull] this DigitalFilter Filter, int SamplesCount)
        {
            if (Filter is null) throw new ArgumentNullException(nameof(Filter));
            if(SamplesCount < 1) throw new ArgumentOutOfRangeException(nameof(SamplesCount), "Число отсчётов импульсной характеристики должно быть больше 2");

            var impulse_response = new double[SamplesCount];

            Filter.Reset();
            impulse_response[0] = Filter.GetSample(1);
            for (var i = 1; i < SamplesCount; i++)
                impulse_response[i] = Filter.GetSample(0);
            return impulse_response;
        }
    }
}
