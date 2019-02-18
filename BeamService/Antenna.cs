using System.Numerics;
using MathService;

namespace BeamService
{
    public abstract class Antenna : ViewModel, IAntenna
    {
        public abstract Complex Pattern(double th);

        #region Overrides of Object

        public override string ToString() => GetType().Name;

        #endregion
    }
}
