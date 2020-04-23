using System.Diagnostics.Contracts;
using MathCore;
using MathCore.Vectors;

namespace Antennas
{
    /// <summary>Объект по свойствам похожий на антенну</summary>
    [ContractClass(typeof(AntennaContract))]
    public interface IAntenna
    {
        /// <summary>Диаграмма направленности антенны</summary>
        /// <param name="Direction">Пространственный угол</param>
        /// <param name="f">Частота</param>
        /// <returns>Комплексное значение диаграммы направленности</returns>
        Complex Pattern(SpaceAngle Direction, double f);
    }

    /// <summary>Класс-контракт для интерфейса антенны</summary>
    [ContractClassFor(typeof(IAntenna))]
    abstract class AntennaContract : IAntenna
    {
        /// <summary>Диаграмма направленности может быть расчитана лишь для положительных значений частоты</summary>
        /// <param name="Direction">Пространственный угол может принимать любые значения</param>
        /// <param name="f">Значения частоты должны быть больше нуля, не равны NAN и бесконечности</param>
        /// <returns></returns>
        public Complex Pattern(SpaceAngle Direction, double f)
        {
            Contract.Requires(!double.IsNaN(f));
            Contract.Requires(f > 0);
            Contract.Requires(!double.IsInfinity(f));
            return default(Complex);
        }
    }
}