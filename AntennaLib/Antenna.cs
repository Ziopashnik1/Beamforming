using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using MathService;
using MathService.Annotations;
using MathService.Extentions.Expressions;
using MathService.Vectors;
using MathService.ViewModels;

namespace Antennas
{
    /// <summary>Антенна</summary>
    public abstract class Antenna : ViewModel, IAntenna
    {
        //public PatternManager GetBeamPattern(double f)
        //{
        //    return new PatternManager(this);
        //}

        /// <summary>Диаграмма направленности</summary>
        /// <param name="Thetta">Угол места</param>
        /// <param name="Phi">Угол азимута</param>
        /// <param name="f">Частота</param>
        /// <returns>Значение диаграммы направленности в указанном направлении</returns>
        public Complex Pattern(double Thetta, double Phi, double f)
        {
            Contract.Requires(f > 0);
            return Pattern(new SpaceAngle(Thetta, Phi), f);
        }

        /// <summary>Диаграмма направленности</summary>
        /// <param name="Direction">пространственное направление</param>
        /// <param name="f">Частота</param>
        /// <returns>Значение диаграммы направленности в указанном направлении</returns>
        public abstract Complex Pattern(SpaceAngle Direction, double f);

        /// <summary>Получение диаграммы направленности на указанной частоте</summary>
        /// <param name="f">Частота</param>
        /// <returns>Функция диаграммы направленности от пространственного угла</returns>
        [NotNull]
        public Func<SpaceAngle, Complex> GetPattern(double f)
        {
            Contract.Requires(f > 0);
            Contract.Ensures(Contract.Result<Func<SpaceAngle, Complex>>() != null);
            return a => Pattern(a, f);
        }

        /// <summary>Получение функции диаграммы направленности в зависимости от мериадиального угла</summary>
        /// <param name="Phi">Фиксируемый азимутальный</param>
        /// <returns>Значение диаграммы направленности в мериадиальных углах</returns>
        [NotNull]
        public Func<double, double, Complex> GetPatternOfThetta(double Phi = 0)
        {
            Contract.Ensures(Contract.Result<Func<double, double, Complex>>() != null);
            return (Thetta, f) => Pattern(new SpaceAngle(Thetta, Phi), f);
        }

        /// <summary>Получение функции диаграммы направленности в зависимости от мериадиального угла на частоте</summary>
        /// <param name="f">Частота</param>
        /// <param name="Phi">Азимутальный угол</param>
        /// <returns>Значение диаграммы направленности в мериадиальных углах на частоте f</returns>
        [NotNull]
        public Func<double, Complex> GetPatternOfThettaOnFreq(double f, double Phi = 0)
        {
            Contract.Requires(f > 0);
            Contract.Ensures(Contract.Result<Func<double, Complex>>() != null);
            return Thetta => Pattern(new SpaceAngle(Thetta, Phi), f);
        }


        /// <summary>Получение функции диаграммы направленности в зависимости от угломестного угла на частоте</summary>
        /// <param name="Thetta">Угломестный угол</param>
        /// <returns>Значение диаграммы направленности в мериадиальных углах на частоте f</returns>
        [NotNull]
        public Func<double, double, Complex> GetPatternOfPhi(double Thetta = 0)
        {
            Contract.Ensures(Contract.Result<Func<double, double, Complex>>() != null);
            return (Phi, f) => Pattern(new SpaceAngle(Thetta, Phi), f);
        }

        /// <summary>Получение функции диаграммы направленности в зависимости от угломестного угла на частоте</summary>
        /// <param name="f">Частота</param>
        /// <param name="Thetta">Угломестный угол</param>
        /// <returns>Значение диаграммы направленности в мериадиальных углах на частоте f</returns>
        [NotNull]
        public Func<double, Complex> GetPatternOfPhiOnFreq(double f, double Thetta = 0)
        {
            Contract.Ensures(Contract.Result<Func<double, Complex>>() != null);
            return Phi => Pattern(new SpaceAngle(Thetta, Phi), f);
        }

        /// <summary>Метод получения тела выражения функции диаграммы направленности для переопределения в классах-наследниках</summary>
        /// <param name="a">Выражение параметра угла</param>
        /// <param name="f">Выражение параметра частоты</param>
        /// <returns>Тело выражения функции диаграммы направленности зависящее от пространственного угла и частоты</returns>
        public virtual Expression GetPatternExpressionBody(Expression a, Expression f) =>
            ((Func<SpaceAngle, double, Complex>)Pattern).GetCallExpression(a, f);

        [NotNull]
        public Expression<Func<SpaceAngle, double, Complex>> GetPatternExpression()
        {
            Contract.Ensures(Contract.Result<LambdaExpression>() != null);
            var a = "Angle".ParameterOf(typeof(SpaceAngle));
            var f = "f".ParameterOf(typeof(double));
            var body = GetPatternExpressionBody(a, f);
            return body.CreateLambda<Func<SpaceAngle, double, Complex>>(a, f);
        }

        [NotNull]
        public override string ToString()
        {
            Contract.Ensures(Contract.Result<string>() != null);
            return GetType().Name;
        }    
    }
}
