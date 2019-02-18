using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antennas;

namespace BeamService
{
    public class DigitalAntennaArray2 : AntennaArray
    {
        public DigitalAntennaArray2(IEnumerable<AntennaItem> items) : base(items) { }
    }
}
