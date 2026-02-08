using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimeSim.Models.NN
{
    public class NetworkConfig
    {
        public int inputs;

        public int hidden;

        public int outputs;

        public int[] memoryInputs;

        public int[] memoryOutputs;
    }
}
