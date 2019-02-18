using System;
using System.Linq.Expressions;
using MathService;
using MathService.Extentions.Expressions;
using MathService.Vectors;

namespace Antennas
{
    public class Dipole : Antenna
    {
        /// <inheritdoc />
        public override Complex Pattern(SpaceAngle Direction, double f) => Math.Cos(Direction.ThettaRad);

        /// <inheritdoc />
        public override Expression GetPatternExpressionBody(Expression a, Expression f) => MathExpression.Cos(a.GetProperty(nameof(SpaceAngle.ThettaRad)));

        /// <inheritdoc />
        public override string ToString() => "Диполь";
    }

    public class Guigens : Antenna
    {
        /// <inheritdoc />
        public override Complex Pattern(SpaceAngle Direction, double f)
        {
            var v = Math.Cos(Direction.ThettaRad);
            return v * v;
        }

        /// <inheritdoc />
        public override Expression GetPatternExpressionBody(Expression a, Expression f) => 
            MathExpression.Cos(a.GetProperty(nameof(SpaceAngle.ThettaRad)).Divide(2.ToExpression())).Power(2.ToExpression());

        /// <inheritdoc />
        public override string ToString() => "Эллемент Гюйгенса";
    }
}