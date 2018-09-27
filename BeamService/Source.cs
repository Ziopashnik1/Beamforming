using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamService
{
    /// <summary>Источник сигнала для АЦП</summary>
    public class Source
    {
        protected readonly Func<double, double> f_F;

        public double this[double t] => f_F(t);

        public Source(Func<double, double> f) => f_F = f;

        public static Source operator +(Source a, Source b) => a is null ? b : (b is null ? a : new Source(t => a.f_F(t) + b.f_F(t)));
    }
}
