using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using MathService;
using MathService.Annotations;
using MathService.Vectors;

namespace Antennas
{
    public sealed class UniformAntenna : Antenna
    {
        public override Complex Pattern(SpaceAngle Direction, double f)
        {
            Contract.Ensures(Contract.Result<Complex>() == 1);
            return Complex.Real;
        }

        public override Expression GetPatternExpressionBody(Expression a, Expression f) => Complex.Real.ToExpression();

        /// <summary>Возвращает строку, которая представляет текущий объект</summary>
        /// <returns>Строка, представляющая текущий объект</returns>
        [NotNull]
        public override string ToString()
        {
            Contract.Ensures(Contract.Result<string>() != null);
            return "Всенаправленная антенна";
        }
    }
}