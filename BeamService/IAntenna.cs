using MathService;

namespace BeamService
{
    public interface IAntenna
    {
        Complex Pattern(double th);
    }
}