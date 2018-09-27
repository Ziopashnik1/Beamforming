namespace BeamService
{
    /// <summary>
    /// Отсчёт цифрового сигнала
    /// </summary>
    public struct Sample
    {
        /// <summary>
        /// Время
        /// </summary>
        public readonly double t;
        /// <summary>
        /// Амплитуда
        /// </summary>
        public readonly double V;
        /// <summary>
        /// Время
        /// </summary>
        public double Time => t;
        /// <summary>
        /// Время в нс
        /// </summary>
        public double Time_ns => t * 1e9;
        /// <summary>
        /// Амплитуда
        /// </summary>
        public double Value => V;
        /// <summary>
        /// Новое значение отсчта цифрового сигнала
        /// </summary>
        /// <param name="t">Время</param>
        /// <param name="v">Амплитуда</param>
        public Sample(double t, double v)
        {
            this.t = t;
            V = v;
        }

        public override string ToString() => $"{t}:{V}";
    }
}