namespace BeamService.Functions
{
    public abstract class AmplitudeSignalFunction : SignalFunction
    {
        protected double _Amplitude = 1;

        public double Amplitude
        {
            get => _Amplitude;
            set => Set(ref _Amplitude, value);
        }

        protected AmplitudeSignalFunction() { }
        protected AmplitudeSignalFunction(double Amplitude) => _Amplitude = Amplitude;
    }
}