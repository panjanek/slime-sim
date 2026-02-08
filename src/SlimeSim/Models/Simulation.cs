using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using OpenTK.Mathematics;
using SlimeSim.Gui;
using SlimeSim.Utils;

namespace SlimeSim.Models
{
    public class Simulation
    {
        public ShaderConfig shaderConfig;

        public Agent[] agents;

        public float[] network;

        public float[] kernelRed;

        public float[] kernelGreen;

        public float[] kernelBlue;

        public float decayRed = 0.99f;

        public float decayGreen = 0.99f;

        public float decayBlue = 0.994f;

        public int step;

        public int generation;

        public double mutationMagnitude = 4.0;

        public double mutationFrequency = 4.0;

        public double crossingOverFrequency = 0.2;

        public List<int> topBlueIds = new List<int>();

        public List<int> topRedIds = new List<int>();

        public StartNewSimulationParameters startedWith;

        [JsonIgnore]
        private Random rnd = new Random(1);

        public Simulation()
        {
            shaderConfig = new ShaderConfig();
            agents = new Agent[shaderConfig.agentsCount];
            InitRandomly(0.6, 0.1);
            kernelRed = MathUtil.Normalize(Blurs.AvailableKernels["Strong"], decayRed);
            kernelGreen = MathUtil.Normalize(Blurs.AvailableKernels["Strong"], decayGreen);
            kernelBlue = MathUtil.Normalize(Blurs.AvailableKernels["Strong"], decayBlue);
        }

        public Simulation(StartNewSimulationParameters parameters)
        {
            InitWithParameters(parameters);
        }

        private void InitWithParameters(StartNewSimulationParameters parameters)
        {
            shaderConfig = new ShaderConfig();
            shaderConfig.width = parameters.width;
            shaderConfig.height = parameters.height;
            shaderConfig.agentsCount = parameters.agentsCount;
            decayGreen = parameters.decayGreen;
            decayBlue = parameters.decayBlue;
            decayRed = parameters.decayRed;
            shaderConfig.blueMaxVelocity = parameters.blueMaxVelocity;
            shaderConfig.redMaxVelocity = parameters.redMaxVelocity;

            startedWith = parameters;
            agents = new Agent[shaderConfig.agentsCount];
            rnd = parameters.fixedSeed ? new Random(1) : new Random();
            InitRandomly(parameters.plantsRatio, parameters.predatorsRatio);
            kernelRed = MathUtil.Normalize(Blurs.AvailableKernels["Strong"], decayRed);
            kernelGreen = MathUtil.Normalize(Blurs.AvailableKernels["Strong"], decayGreen);
            kernelBlue = MathUtil.Normalize(Blurs.AvailableKernels["Strong"], decayBlue);
        }

        private void InitRandomly(double plants, double predators)
        {
            int networksCount = 0;
            for (int i = 0; i < agents.Length; i++)
            {
                var r = rnd.NextDouble();

                agents[i] = new Agent();
                agents[i].flag = 1;
                agents[i].type = rnd.Next(3);
                agents[i].angle = (float)(2 * Math.PI * rnd.NextDouble());
                agents[i].SetPosition(new Vector2((float)(shaderConfig.width * rnd.NextDouble()), (float)(shaderConfig.height * rnd.NextDouble())));
            }

            network = new float[9];
        }

        public void SetFlags()
        {
            for (int i = 0; i < agents.Length; i++)
            {
                int flag = 1;
                agents[i].flag = flag;
            }
        }
    }
}
