using System.Collections;
using System.Collections.Generic;
using MathService;

namespace BeamService
{
    public class DigitalSpectrum : IEnumerable<SpectrumSample>
    {
        private readonly SpectrumSample[] f_Samples;

        public DigitalSpectrum(Complex[] samples, double df, double f0 = 0)
        {
            df /= 1e9;
            f_Samples = new SpectrumSample[samples.Length];
            for (var i = 0; i < f_Samples.Length; i++)
                f_Samples[i] = new SpectrumSample(df * i + f0, samples[i]);
        }

        public IEnumerator<SpectrumSample> GetEnumerator() => ((IEnumerable<SpectrumSample>)f_Samples).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<SpectrumSample>)f_Samples).GetEnumerator();
    }
}