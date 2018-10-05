namespace BeamService
{
    public abstract class SignalFunction : ViewModel
    {
        public abstract double Value(double t);

        public static SummSignalFunction operator +(SignalFunction s1, SignalFunction s2) => new SummSignalFunction(s1,s2);
        public static DiffSignalFunction operator -(SignalFunction s1, SignalFunction s2) => new DiffSignalFunction(s1,s2);
        public static MuliyplySignalFunction operator *(SignalFunction s1, SignalFunction s2) => new MuliyplySignalFunction(s1,s2);
    }

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

    public class SummSignalFunction : CombyneSignalFunction
    {
        public SummSignalFunction(SignalFunction s1, SignalFunction s2) : base(s1, s2) {  }

        public override double Value(double t) => S1.Value(t) + S2.Value(t);
    }

    public class DiffSignalFunction : CombyneSignalFunction
    {
        public DiffSignalFunction(SignalFunction s1, SignalFunction s2) : base(s1, s2) { }

        public override double Value(double t) => S1.Value(t) - S2.Value(t);
    }

    public class MuliyplySignalFunction : CombyneSignalFunction
    {
        public MuliyplySignalFunction(SignalFunction s1, SignalFunction s2) : base(s1, s2) { }

        public override double Value(double t) => S1.Value(t) * S2.Value(t);
    }
}