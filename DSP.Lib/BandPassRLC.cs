namespace DSP.Lib
{
    public class BandPassRLC : IIR
    {
        public BandPassRLC(double f0, double DeltaF, double dt)
            :base(
                a: new[] { 1d, },
                b: new[] { 0, 0d  }
            )
        {

        }
    }
}