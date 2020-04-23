using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathCore.ViewModels;

namespace BeamService.AmplitudeDestributions
{
    public abstract class AmplitudeDestribution : ViewModel
    {
        /// <summary>Значение амплитуды в точке апертуры</summary>
        /// <param name="x">Относительное положение точки в апертуры (на краях апертуры = 0.5)</param>
        /// <returns></returns>
        public abstract double Value(double x);

        public Func<double, double> GetDestribution(double ApertureCenter, double ApertureLength) => x => Value((x - ApertureCenter) / ApertureLength);
    }

    public class Uniform : AmplitudeDestribution
    {
        public override double Value(double x) => 1;

        #region Overrides of Object

        public override string ToString() => "Равномерное";

        #endregion
    }

    public class CosOnPedestal : AmplitudeDestribution
    {
        private double _Delta = 0.5;

        public double Delta
        {
            get => _Delta;
            set => Set(ref _Delta, value);
        }

        public override double Value(double x) => (1 - _Delta) + _Delta * Math.Cos(Math.PI * x);

        #region Overrides of Object

        public override string ToString() => $"cos на пьед. {_Delta}";

        #endregion
    }
}
