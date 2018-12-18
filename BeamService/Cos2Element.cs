using System;
using System.Numerics;

namespace BeamService
{
    public class Cos2Element : Antenna
    {
        public override Complex Pattern(double th)
        {
            var v = Math.Cos(th);
            return v * v;
        }
    }
}