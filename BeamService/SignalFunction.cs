using MathService.ViewModels;

namespace BeamService
{
    public abstract class SignalFunction : ViewModel
    {
        public abstract double Value(double t);

        public static SummSignalFunction operator +(SignalFunction s1, SignalFunction s2) => new SummSignalFunction(s1,s2);
        public static DiffSignalFunction operator -(SignalFunction s1, SignalFunction s2) => new DiffSignalFunction(s1,s2);
        public static MuliyplySignalFunction operator *(SignalFunction s1, SignalFunction s2) => new MuliyplySignalFunction(s1,s2);
    }
}