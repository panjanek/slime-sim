using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SlimeSim.Models
{
    public class StartNewSimulationParameters
    {
        public StartNewSimulationParameters() { }

        [JsonIgnore]
        public int width = 1920;

        [JsonIgnore]
        public int height = 1080;

        public double plantsRatio = 0.6;

        public double predatorsRatio = 0.1;

        public int agentsCount = 5000;

        public float decayGreen = 0.99f;

        public float decayBlue = 0.994f;

        public float decayRed = 0.990f;

        public float blueMaxVelocity = 0.3f;

        public float redMaxVelocity = 0.5f;

        [JsonIgnore]
        public bool fixedSeed = true;

        [JsonIgnore]
        public bool useExistingAgents = false;

        [JsonIgnore]
        public bool loadAgentsFromFiles = false;

        [JsonIgnore]
        public List<string> fileNames = new List<string>();

        [JsonIgnore]
        public List<Simulation> sources = new List<Simulation>();
    }
}
