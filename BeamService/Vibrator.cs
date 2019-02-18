using System;
using System.Numerics;
using MathService;

namespace BeamService
{
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
}