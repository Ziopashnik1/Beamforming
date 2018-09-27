namespace BeamService
{
    public abstract class AmplitudeSignalFunction : SignalFunction
    {
        public double Amplitude { get; set; } = 1;
    }

    public class RectSignalFunction : AmplitudeSignalFunction
    {
        public double Period { get; set; } = 5e-9;

        public double Tau { get; set; } = 2.5e-9;

        public override double Value(double t)
        {
            t = t % Period + (t < 0 ? Period : 0);
            return Amplitude * (t.Equals(Tau) || t.Equals(0d) ? 0.5 : (0 < t && t < Tau ? 1 : 0));
        }
    }
}