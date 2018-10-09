using System;

namespace BeamService
{
    public class PatternValue
    {
        public double Angle { get; set; }
        public double Angle_deg => Angle / Math.PI * 180;
        public double Angle_rad => Angle / 180 * Math.PI;
        public double Value { get; set; }
        public double Value_db => 20 * Math.Log10(Math.Abs(Value));
        public double Value_dbP => 10 * Math.Log10(Math.Abs(Value));

        public PatternValue() { }

        public PatternValue(double Angle, double Value)
        {
            this.Angle = Angle;
            this.Value = Value;
        }

        public override string ToString() => $"{Angle_deg}:{Value}({Value_db}db)";

        public static PatternValue operator *(PatternValue v, double k) => new PatternValue(v.Angle, v.Value * k);
        public static PatternValue operator /(PatternValue v, double k) => new PatternValue(v.Angle, v.Value / k);
        public static PatternValue operator +(PatternValue v, double k) => new PatternValue(v.Angle, v.Value + k);
        public static PatternValue operator -(PatternValue v, double k) => new PatternValue(v.Angle, v.Value - k);
    }
}
