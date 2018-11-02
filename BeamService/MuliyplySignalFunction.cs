namespace BeamService
{
    public class MuliyplySignalFunction : CombyneSignalFunction
    {
        public MuliyplySignalFunction(SignalFunction s1, SignalFunction s2) : base(s1, s2) { }

        public override double Value(double t) => S1.Value(t) * S2.Value(t);
    }
}