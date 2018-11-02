namespace BeamService
{
    public abstract class AmplitudeSignalFunction : SignalFunction
    {
        private double f_Amplitude = 1;

        public double Amplitude
        {
            get => f_Amplitude;
            set => Set(ref f_Amplitude, value);
        }

        protected AmplitudeSignalFunction() { }
        protected AmplitudeSignalFunction(double Amplitude) => f_Amplitude = Amplitude;
    }
}