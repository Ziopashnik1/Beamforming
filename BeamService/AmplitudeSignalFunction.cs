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

        public AmplitudeSignalFunction() { }
        public AmplitudeSignalFunction(double Amplitude) => f_Amplitude = Amplitude;
    }

    public class RectSignalFunction : AmplitudeSignalFunction
    {
        private double f_Period = 5e-9;
        private double f_Tau = 2.5e-9;

        public double Period
        {
            get => f_Period;
            set => Set(ref f_Period, value);
        }

        public double Tau
        {
            get => f_Tau;
            set => Set(ref f_Tau, value);
        }

        public override double Value(double t)
        {
            t = t % Period + (t < 0 ? Period : 0);
            return Amplitude * (t.Equals(Tau) || t.Equals(0d) ? 0.5 : (0 < t && t < Tau ? 1 : 0));
        }
    }
}