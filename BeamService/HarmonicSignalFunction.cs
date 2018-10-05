namespace BeamService
{
    public abstract class HarmonicSignalFunction : AmplitudeSignalFunction
    {
        private double f_Frequency = 1;
        private double f_Phase;

        public double Frequency
        {
            get => f_Frequency;
            set => Set(ref f_Frequency, value);
        }

        public double Phase
        {
            get => f_Phase;
            set => Set(ref f_Phase, value);
        }

        protected HarmonicSignalFunction() { }

        protected HarmonicSignalFunction(double Amplitude, double Frequency, double Phase) : base(Amplitude)
        {
            f_Frequency = Frequency;
            f_Phase = Phase;
        }
    }
}