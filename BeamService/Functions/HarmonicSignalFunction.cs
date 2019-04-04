namespace BeamService.Functions
{
    public abstract class HarmonicSignalFunction : AmplitudeSignalFunction
    {
        protected double _Frequency = 1;
        protected double _Phase;

        public double Frequency
        {
            get => _Frequency;
            set => Set(ref _Frequency, value);
        }

        public double Phase
        {
            get => _Phase;
            set => Set(ref _Phase, value);
        }

        protected HarmonicSignalFunction() { }

        protected HarmonicSignalFunction(double Amplitude, double Frequency, double Phase) : base(Amplitude)
        {
            _Frequency = Frequency;
            _Phase = Phase;
        }
    }
}