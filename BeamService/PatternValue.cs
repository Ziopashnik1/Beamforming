using System;

namespace BeamService
{
    public class PatternValue
    {
        public double Angle { get; set; }
        public double Angle_deg => Angle / Math.PI * 180;
        public double Value { get; set; }
        public double Value_db => 20 * Math.Log10(Math.Abs(Value));
        public double Value_dbP => 10 * Math.Log10(Math.Abs(Value));

        public override string ToString() => $"{Angle_deg}:{Value}({Value_db}db)";
    }
}
