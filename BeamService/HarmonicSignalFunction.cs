namespace BeamService
{
    public abstract class HarmonicSignalFunction : AmplitudeSignalFunction
    {
        public double Frequency { get; set; } = 1;
        public double Phase { get; set; }
    }
}