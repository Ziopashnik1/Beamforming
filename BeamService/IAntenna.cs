using System.Numerics;
using MathService;

namespace BeamService
{
    public interface IAntenna
    {
        Complex Pattern(double th);
    }
}