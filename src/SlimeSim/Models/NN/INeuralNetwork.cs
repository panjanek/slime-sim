using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimeSim.Models.NN
{
    public interface INeuralNetwork
    {
        int Size { get; }

        void Init(float[] network, int offset, Random rnd);

        void Mutate(float[] network, int offset, Random rnd, double changedWeightsRatio, double stdDev);

        void MutateAllIncomming(float[] network, int offset, Random rnd, double stdDev);

        void CrossOver(float[] network, int parent1Offset, int parent2Offset, int childOffset, Random rnd);

        float[] Evaluate(float[] network, int offset, float[] inp);

        float[] GetInputSample(int seed);
    }
}
