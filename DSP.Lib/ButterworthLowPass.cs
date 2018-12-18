using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace DSP.Lib
{
    class ButterworthLowPass : IIR
    {
        public ButterworthLowPass([NotNull] double[] a, [NotNull] double[] b) : base(a, b)
        {
        }
    }
}
