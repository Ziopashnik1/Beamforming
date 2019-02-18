using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathService;
using MathService.Vectors;

namespace Antennas
{
    public struct PatternValue
    {
        public double Angle { get; }
        public Complex Value { get; }

        public double AngleDeg => Angle * Consts.ToDeg;
        public double AngleRad => Angle * Consts.ToRad;
        public double ValueIndB => Value.Abs.In_dB();
        public double ValueIndBP => Value.Abs.In_dB_byPower();
        public double ValueAbs => Value.Abs;

        public PatternValue(double angle, Complex v)
        {
            Angle = angle;
            Value = v;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{AngleDeg:0.00}:{ValueIndB:0.##}db";
        }
    }
}
