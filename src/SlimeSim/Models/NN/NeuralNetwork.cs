using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimeSim.Utils;

namespace SlimeSim.Models.NN
{
    public class NeuralNetwork : INeuralNetwork
    {
        public int Size => inputs * hidden + hidden + hidden * outputs + outputs;

        private int inputs;

        private int hidden;

        private int outputs;

        private int[] memoryInputs;

        private int[] memoryOutputs;

        bool[] isMemory;

        public NeuralNetwork(NetworkConfig config)
        {
            this.inputs = config.inputs;
            this.hidden = config.hidden;
            this.outputs = config.outputs;
            this.memoryInputs = config.memoryInputs;    // indexes of inputs that serve memory
            this.memoryOutputs = config.memoryOutputs;  // indexes of outputs that serve memory
            isMemory = new bool[Size];
            for (int i = 0; i < Size; i++)
                isMemory[i] = IsMemoryWeight(i);
        }

        private bool IsMemoryWeight(int localIndex)
        {
            if (memoryInputs == null || memoryInputs.Length == 0 || memoryOutputs == null || memoryOutputs.Length == 0)
                return false;

            // check if this is weight from memory input to some hidden layer neuron
            foreach (var inputIdx in memoryInputs)
            {
                for (int h = 0; h < hidden; h++)
                    if (localIndex == h * inputs + inputIdx)
                        return true;
            }

            // check if this is weight from hidden layer to memory output
            int offs2 = hidden * inputs + hidden;
            foreach (var outputIdx in memoryOutputs)
            {
                for (int h = 0; h < hidden; h++)
                    if (localIndex == offs2 + outputIdx * hidden + h)
                        return true;

                if (localIndex == offs2 + outputs * hidden + outputIdx) // memory output neuron bias
                    return true;
            }

            return false;
        }

        public void Init(float[] network, int offset, Random rnd)
        {
            for (int i = 0; i < inputs * hidden; i++) //weights 1
                network[offset + i] = (float)(rnd.NextDouble() * 2 - 1);
            for (int i = inputs * hidden; i < inputs * hidden + hidden; i++) //biases 1
                network[offset + i] = (float)(rnd.NextDouble() * 1 - 0.5);
            int offs2 = hidden * inputs + hidden;
            for (int i = offs2; i < offs2 + hidden * outputs; i++)  //weights 2
                network[offset + i] = (float)(rnd.NextDouble() * 2 - 1);
            for (int i = offs2 + hidden * outputs; i < Size; i++) //biases 2
                network[offset + i] = (float)(rnd.NextDouble() * 1 - 0.5);
        }

        // mutate randomly selected weights
        public void Mutate(float[] network, int offset, Random rnd, double changedWeightsRatio, double stdDev)
        {
            int offs2 = hidden * inputs + hidden;
            for (int i = 0; i < Size; i++)
            {
                bool isMemoryWeight = isMemory[i];
                var probabilityMult = isMemoryWeight ? 0.2 : 1.0;
                if (rnd.NextDouble() <= changedWeightsRatio * probabilityMult)
                {
                    double stdMult = isMemoryWeight ? 0.3 : 1.0;  //mutate memory weight with 30% magnitude
                    if (i >= inputs * hidden && i < inputs * hidden + hidden)
                        stdMult = isMemoryWeight ? 0.3 : 0.5; // if 1st layer bias: 50% magnitude, 30% if memory input bias

                    if (i >= offs2 + hidden * outputs)
                        stdMult = isMemoryWeight ? 0.1 : 0.3; // if 2nd layer bias: 30% magnitude, 10% if memory input bias

                    double delta = MathUtil.NextGaussian(rnd, 0.0, stdDev * stdMult);
                    network[offset + i] += (float)delta;
                }
            }
        }

        // mutate all inputs of one hidden layer neuron
        public void MutateAllIncomming(float[] network, int offset, Random rnd, double stdDev)
        {
            int h = rnd.Next(hidden + outputs);
            if (h < hidden) //1st layer
            {
                for (int i = 0; i < inputs; i++)
                {
                    int index = h * inputs + i;

                    if (isMemory[index] && rnd.NextDouble() > 0.3) //mutate memory weights less often
                        continue;

                    double stdMult = 1.0;
                    if (isMemory[index]) // mutate memory input weight with 30% maginute
                        stdMult = 0.3;

                    double delta = MathUtil.NextGaussian(rnd, 0.0, stdMult * stdDev);
                    network[offset + index] += (float)delta;
                }

                double biasDelta = MathUtil.NextGaussian(rnd, 0.0, stdDev*0.5);
                network[offset + inputs * hidden + h] += (float)biasDelta;
            }
            else //2nd layer
            {
                var o = h - hidden;
                int offs2 = hidden * inputs + hidden;
                for (int j = 0; j < hidden; j++)
                {
                    int index = offs2 + o * hidden + j;

                    if (isMemory[index] && rnd.NextDouble() > 0.3) //mutate memory weights less often
                        continue;

                    double stdMult = 1.0;
                    if (isMemory[index]) // mutate memory input weight with 30% maginute
                        stdMult = 0.3;

                    double delta = MathUtil.NextGaussian(rnd, 0.0, stdMult * stdDev);
                    network[offset + index] += (float)delta;
                }

                int biasIndex = offs2 + hidden * outputs + o;
                var biasStdMult = 1.0;
                if (isMemory[biasIndex]) // mutate memory output bias with 20% maginute
                    biasStdMult = 0.2;
                double biasDelta = MathUtil.NextGaussian(rnd, 0.0, stdDev * 0.5 * biasStdMult);
                network[offset + biasIndex] += (float)biasDelta;
            }
        }

        public void CrossOver(float[] network, int parent1Offset, int parent2Offset, int childOffset, Random rnd)
        {
            var decision1 = rnd.NextDouble();
            int offs2 = hidden * inputs + hidden;
            if (decision1 < 0.5) // with probability 50%: get 1st laver from parent1, second layer from parent 2 (parents already random)
            {
                for (int i = 0; i < Size; i++)
                    network[childOffset + i] = i < offs2 ? network[parent1Offset + i] : network[parent2Offset + i];
            }
            else if (decision1 < 0.9)  // with probability 40%: combine neurons from parents randomly
            {
                for (int h = 0; h < hidden; h++)
                {
                    var parentOffset = rnd.NextDouble() < 0.5 ? parent1Offset : parent2Offset;
                    network[childOffset + hidden * inputs + h] = network[parentOffset + hidden * inputs + h]; //pick bias
                    for (int i = 0; i < inputs; i++)
                        network[childOffset + h * inputs + i] = network[parentOffset + h * inputs + i]; // pick input weights
                    for (int o = 0; o < outputs; o++)
                        //network[childOffset + offs2 + h * outputs + o] = network[parentOffset + offs2 + h * outputs + o]; // pick output weights
                        network[childOffset + offs2 + o * hidden + h] = network[parentOffset + offs2 + o * hidden + h]; // pick output weights
                }

                var decision2 = rnd.NextDouble();
                for (int o = 0; o < outputs; o++)
                {
                    int outputBiasOffs = offs2 + hidden * outputs + o;
                    if (decision2 < 0.50) // 50% of the times: average output biases:
                        network[childOffset + outputBiasOffs] = 0.5f * (network[parent1Offset + outputBiasOffs] + network[parent2Offset + outputBiasOffs]);
                    else if (decision2 < 0.75) //25% of times: pick output biases from parents randomly
                    {
                        var parentOffset = rnd.NextDouble() < 0.5 ? parent1Offset : parent2Offset;
                        network[childOffset + outputBiasOffs] = network[parentOffset + outputBiasOffs];
                    } // 25% of times: leave output biases unchanged
                }
            }
            else //with probability 10%: combine weights of parents randomly - may be destructive
            {
                for (int i = 0; i < Size; i++)
                    network[childOffset + i] = rnd.NextDouble() < 0.5 ? network[parent1Offset + i] : network[parent2Offset + i];
            }
        }

        private float Activate(float x)
        {
            return (float)Math.Tanh(x);
        }

        public float[] Evaluate(float[] network, int offset, float[] inp)
        {
            // Hidden layer
            float[] hid = new float[hidden];
            for (int h = 0; h < hidden; ++h)
            {
                float sum = network[offset + (inputs * hidden) + h];
                for (int i = 0; i < inputs; ++i)
                    sum += inp[i] * network[offset + h * inputs + i];

                hid[h] = Activate(sum);
            }

            // Output layer
            int offs2 = hidden * inputs + hidden;
            float[] result = new float[outputs];
            for (int o = 0; o < outputs; ++o)
            {
                float sum = network[offset + (offs2 + hidden * outputs) + o];
                for (int h = 0; h < hidden; ++h)
                    sum += hid[h] * network[offset + offs2 + o * hidden + h];

                result[o] = Activate(sum);
            }

            return result;
        }

        public float[] GetInputSample(int seed)
        {
            var rnd = new Random(seed);
            var res = new float[inputs];
            for(int i=0; i<inputs; i++)
            {
                if (i < 15)
                    res[i] = rnd.NextDouble() < 0.1 ? (float)rnd.NextDouble() : (float)MathUtil.NextBeta(rnd, 1.3, 4);
                else if (memoryInputs.Contains(i))
                    res[i] = 1 - 2 * (float)rnd.NextDouble();
            }

            if (rnd.NextDouble() < 0.5)
            {
                res[15] = res[2 * 3 + 0] - res[0 * 3 + 0];
                res[16] = res[4 * 3 + 0] - res[3 * 3 + 0];
            }
            else
            {
                res[15] = res[2 * 3 + 2] - res[0 * 3 + 2];   
                res[16] = res[4 * 3 + 2] - res[3 * 3 + 2];   
            }    

            return res;
        }
    }
}
