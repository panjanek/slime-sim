using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimeSim.Utils
{
    public static class MathUtil
    {
        public static float[] Normalize(float[] array, float decay)
        {
            float[] result = new float[array.Length];
            int n = (int)Math.Sqrt(array.Length);
            var sum = array.Sum();
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = decay * array[i] / sum;
                int x = i % n;
                int y = i / n;
                if (array[i] != array[y * n + (n - x - 1)] || array[i] != array[(n - y - 1) * n + x] || array[i] != array[(n - y - 1) * n + (n - x - 1)])
                    throw new Exception("Kernel not symmetric!");
            }

            return result;
        }

        /// <summary>
        /// Generate random number with normal distribution
        /// </summary>
        public static double NextGaussian(Random rng, double mean = 0.0, double stdDev = 1.0)
        {
            // Uniform(0,1] random doubles
            double u1 = 1.0 - rng.NextDouble();
            double u2 = 1.0 - rng.NextDouble();

            // Standard normal (0,1)
            double randStdNormal =
                Math.Sqrt(-2.0 * Math.Log(u1)) *
                Math.Cos(2.0 * Math.PI * u2);

            return mean + stdDev * randStdNormal;
        }

        public static double NextGamma(Random rng, double shape)
        {
            // Marsaglia and Tsang method (shape >= 1)
            if (shape < 1.0)
            {
                // Boosting method
                return NextGamma(rng, shape + 1.0) * Math.Pow(rng.NextDouble(), 1.0 / shape);
            }

            double d = shape - 1.0 / 3.0;
            double c = 1.0 / Math.Sqrt(9.0 * d);

            while (true)
            {
                double x = NextGaussian(rng);
                double v = 1.0 + c * x;
                if (v <= 0) continue;

                v = v * v * v;
                double u = rng.NextDouble();

                if (u < 1.0 - 0.0331 * x * x * x * x)
                    return d * v;

                if (Math.Log(u) < 0.5 * x * x + d * (1.0 - v + Math.Log(v)))
                    return d * v;
            }
        }

        /// <summary>
        /// Generates random number between 0 and 1 with beta distribution
        /// </summary>
        public static double NextBeta(Random rng, double alpha, double beta)
        {
            double x = NextGamma(rng, alpha);
            double y = NextGamma(rng, beta);
            return x / (x + y);
        }
    }
}
