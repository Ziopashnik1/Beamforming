using System;
using System.Numerics;

namespace BeamService
{
    public interface IAntenna
    {
        Complex Pattern(double th);
    }

    public abstract class Antenna : ViewModel, IAntenna
    {
        public abstract Complex Pattern(double th);

        #region Overrides of Object

        public override string ToString() => GetType().Name;

        #endregion
    }

    public class Uniform : Antenna
    {
        public override Complex Pattern(double th) => 1;
    }

    public class Vibrator : Antenna
    {
        private double f_Length = 0.5;

        public double Length
        {
            get => f_Length;
            set => Set(ref f_Length, value);
        }

        public override Complex Pattern(double th)
        {
            var l = f_Length * Math.PI * 2;
            return (Math.Cos(l * Math.Sin(th)) - Math.Cos(l)) / Math.Cos(th);
        }
    }

    public class Cos2Element : Antenna
    {
        public override Complex Pattern(double th)
        {
            var v = Math.Cos(th);
            return v * v;
        }
    }

    public class CosElement : Antenna
    {
        public override Complex Pattern(double th) => Math.Cos(th);
    }

    public class GuigensElement : Antenna
    {
        public override Complex Pattern(double th) => Math.Abs(1 - Math.Cos(th - Math.PI)) / 2;
    }
}
