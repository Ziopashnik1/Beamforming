using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using MathCore;
using MathCore.Annotations;
using MathCore.Extensions.Expressions;
using MathCore.Vectors;

namespace Antennas
{
    public class LamdaAntenna : Antenna
    {
        [NotNull]
        private readonly Func<double, double, double, double> f_Beam;
        public LamdaAntenna([NotNull]Func<double, double, double, double> Beam) { f_Beam = Beam; }

        /// <summary>Диаграмма направленности</summary>
        /// <param name="Direction">пространственное направление</param>
        /// <param name="f">Частота</param>
        /// <returns>Значение диаграммы направленности в указанном направлении</returns>
        public override Complex Pattern(SpaceAngle Direction, double f)
        {
            Contract.Requires(f > 0);
            return f_Beam(Direction.ThetaRad, Direction.PhiRad, f);
        }

        public override Expression GetPatternExpressionBody(Expression a, Expression f) =>
            f_Beam.GetCallExpression(a.GetProperty(nameof(SpaceAngle.ThetaRad)), a.GetProperty(nameof(SpaceAngle.PhiRad)), f);

        [ContractInvariantMethod]
        private void ObjectInvariant() => Contract.Invariant(f_Beam != null);
    }
}