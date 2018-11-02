namespace BeamService
{
    public abstract class CombyneSignalFunction : SignalFunction
    {
        public SignalFunction S1 { get; }
        public SignalFunction S2 { get; }

        public CombyneSignalFunction(SignalFunction s1, SignalFunction s2)
        {
            S1 = s1;
            S2 = s2;
        }
    }
}