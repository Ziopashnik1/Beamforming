using System.Numerics;

namespace BeamService
{
    public interface IAntenna
    {
        Complex Pattern(double th);
    }
}