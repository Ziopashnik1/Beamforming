using System;
using System.Numerics;
using MathService;

namespace BeamService
{
    public class CosElement : Antenna
    {
        public override Complex Pattern(double th) => Math.Cos(th);
    }
}