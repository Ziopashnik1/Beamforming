﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSP.Lib
{
    public abstract class DigitalFilter
    {
        public abstract double GetSample(double sample);

        public IEnumerable<double> Filter(IEnumerable<double> samples)
        {
            foreach (var sample in samples)
                yield return GetSample(sample);
        }

        public DigitalSignal Filter(DigitalSignal signal) => new DigitalSignal(signal.dt, Filter(signal.Samples));

        public abstract void Reset();
    }
}