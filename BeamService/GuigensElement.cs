using System;
using System.Numerics;
using MathService;

namespace BeamService
{
    public class GuigensElement : Antenna
    {
        public override Complex Pattern(double th) => Math.Abs(1 - Math.Cos(th - Math.PI)) / 2;
    }
}