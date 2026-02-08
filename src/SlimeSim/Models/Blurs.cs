using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimeSim.Models
{
    public static class Blurs
    {
        public static Dictionary<string, float[]> AvailableKernels { get; set; } = new()
        {
            ["Default"] = [
  0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                  0.0f, 1.0f, 2.0f, 1.0f, 0.0f,
                  0.1f, 2.0f, 30.0f, 2.0f, 0.1f,
                  0.0f, 1.0f, 2.0f, 1.0f, 0.0f,
                  0.0f, 0.0f, 0.0f, 0.0f, 0.0f
],
            ["Mild"] = [
  0,  0,  1,  0,  0,
                  0,  2,  4,  2,  0,
                  1,  4, 20,  4,  1,
                  0,  2,  4,  2,  0,
                  0,  0,  1,  0,  0
],
            ["Moderate"] = [
  1,  2,  3,  2,  1,
                  2,  4,  6,  4,  2,
                  3,  6, 10,  6,  3,
                  2,  4,  6,  4,  2,
                  1,  2,  3,  2,  1
],
            ["Strong"] = [
  1,  2,  4,  2,  1,
                  2,  4,  8,  4,  2,
                  4,  8,  4,  8,  4,
                  2,  4,  8,  4,  2,
                  1,  2,  4,  2,  1
],
            ["Edge-biased"] = [
  0,  1,  2,  1,  0,
                  1,  2,  4,  2,  1,
                  2,  4,  2,  4,  2,
                  1,  2,  4,  2,  1,
                  0,  1,  2,  1,  0
],
            ["Uniform"] = [
  1,  1,  1,  1,  1,
                  1,  1,  1,  1,  1,
                  1,  1,  1,  1,  1,
                  1,  1,  1,  1,  1,
                  1,  1,  1,  1,  1
],
            ["Anisotropic"] = [
  0,  0,  1,  0,  0,
                  0,  1,  2,  1,  0,
                  0,  2,  6,  2,  0,
                  0,  1,  2,  1,  0,
                  0,  0,  1,  0,  0
]
        };
    }
}
