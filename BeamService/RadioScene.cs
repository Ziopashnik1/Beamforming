using System.Collections.Generic;
using System.Collections.ObjectModel;
using MathService.Vectors;

namespace BeamService
{
    public class RadioScene : ObservableCollection<SpaceSignal>
    {
        public SpaceSignal Add(SpaceAngle angle, SignalFunction signal)
        {
            var space_signal = new SpaceSignal {Angle = angle, Signal = signal};
            Add(space_signal);
            return space_signal;
        }

        public SpaceSignal Add(double Thetta, double Phi, SignalFunction signal)
        {
            var space_signal = new SpaceSignal { Thetta = Thetta, Phi = Phi, Signal = signal };
            Add(space_signal);
            return space_signal;
        }
    }
}