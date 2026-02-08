using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimeSim.Utils
{
    public class DebugUtil
    {
        public static bool Debug = true;

        public static string LogFile = "log.txt";

        public static void Log(string message)
        {
            File.AppendAllText(LogFile, $"{message}\n");
        }

        public static void TestBeta()
        {
            var rng = new Random(1);
            List<double> sample = new List<double>();
            for (int i = 0; i < 50000; i++)
                sample.Add(MathUtil.NextBeta(rng, 0.8, 1.5));

            int[] histogram = new int[50];
            foreach(var x in sample)
            {
                int h = (int)Math.Floor(x * histogram.Length);
                if (h > histogram.Length - 1)
                    h = histogram.Length - 1;
                histogram[h]++;
            }

            string txt = "";
            for(int h=0; h<histogram.Length; h++)
            {
                txt += $"{(h * 1.0 / histogram.Length).ToString("0.000")}\t{histogram[h]}\n";
            }

            txt = txt.Replace(".", ",");
            Console.WriteLine(txt);
        }
    }
}
