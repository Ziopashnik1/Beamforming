using System;
using System.Collections.Generic;
using System.Linq;
using MathService.Vectors;

namespace Antennas
{
    public class LinearAntennaArray : AntennaArray
    {
        public LinearAntennaArray(IEnumerable<Antenna> elements, double d)
            : base(elements.Select((a, i) => new AntennaItem(a, new Vector3D(i * d), new SpaceAngle(), 1)))
        {
            var L2 = L_x / 2;
            for(var i = 0; i < Count; i++)
                this[i].Location = this[i].Location.Dec_X(L2);
        }

        public LinearAntennaArray(int N, double d) : this(new Antenna[N].Initialize(i => new UniformAntenna()), d) { }
    }
}