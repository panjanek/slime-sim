using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimeSim.Models
{
    public class RankedAgent
    {
        public int index;

        public double fitness;

        public Agent agent;
    }

    public class RankedAgentWithDistance
    {
        public RankedAgent ranked;

        public double distance;
    }
}
