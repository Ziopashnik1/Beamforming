using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BeamService.Functions;
using MathCore;
using MathCore.Vectors;

namespace BeamService
{
    public class RadioScene : ObservableCollection<SpaceSignal>
    {
        public RadioScene() { }
        public RadioScene(IEnumerable<SpaceSignal> Signals) : base(Signals) { }

        public SpaceSignal Add(SpaceAngle angle, SignalFunction signal)
        {
            var space_signal = new SpaceSignal { Angle = angle, Signal = signal };
            Add(space_signal);
            return space_signal;
        }

        public SpaceSignal Add(double Theta, double Phi, SignalFunction signal)
        {
            var space_signal = new SpaceSignal { Theta = Theta, Phi = Phi, Signal = signal };
            Add(space_signal);
            return space_signal;
        }

        public RadioScene Rotate(double Theta, double Phi = 0, AngleType type = AngleType.Rad) => Rotate(new SpaceAngle(Theta, Phi, type));

        public RadioScene Rotate(SpaceAngle Angle)
        {
            return new RadioScene(this.Select(signal => new SpaceSignal { Signal = signal.Signal, Angle = signal.Angle.RotatePhiTheta(Angle) }));
        }
    }
}