using System.Numerics;
using MathService;

namespace BeamService
{
    public class Uniform : Antenna
    {
        public override Complex Pattern(double th) => 1;
    }
}