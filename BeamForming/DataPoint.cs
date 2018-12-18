using System;

namespace BeamForming
{
    /// <summary>Точка данных</summary>
    public class DataPoint
    {
        /// <summary>Значение аргумента</summary>
        public double X { get; set; }
        /// <summary>Значение функции</summary>
        public double Y { get; set; }
        /// <summary>Модуль значения функции</summary>
        public double Yabs => Math.Abs(Y);
        /// <summary>Значение функции в дБ (по амплитуде)</summary>
        public double Ydb => 20 * Math.Log10(Yabs);
        /// <summary>Значение функции в дБВт (по мощности)</summary>
        public double YdbP => 10 * Math.Log10(Yabs);
    }
}