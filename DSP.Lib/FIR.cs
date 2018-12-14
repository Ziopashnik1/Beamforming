using System;
using JetBrains.Annotations;

namespace DSP.Lib
{
    public class FIR : DigitalFilter
    {
        private readonly int _Order;
        [NotNull] private readonly double[] _Pulse;
        [NotNull] private readonly double[] _State;

        public FIR([NotNull] double[] Pulse)
        {
            _Pulse = Pulse ?? throw new ArgumentNullException(nameof(Pulse));
            _State = new double[_Order = _Pulse.Length];
        }

        #region Overrides of DigitalFilter

        public override void Reset()
        {
            for (var i = 0; i < _Order; i++)
                _State[i] = 0;
        }

        private int _Index;
        public override double GetSample(double sample)
        {
            _State[_Index++] = sample;
            if (_Index >= _Order) _Index = 0;

            var result = 0d;

            for (var i = 0; i < _Order; i++)
                result += _Pulse[_Order - i - 1] * _State[(i + _Index) % _Order];
            return result / _Order;
        }

        #endregion
    }
}