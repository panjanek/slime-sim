using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Effects;

namespace SlimeSim.Models
{
    public class FitnessFunctionConfig
    {
        public double blueMealAward = 4;

        public double blueSurvivalAward = 0.2;

        public double blueDeathPenalty = 3;

        public double blueEnergySpentPenalty = 0.003;

        public double redMealsAward = 3;

        public double redNearPreyAward = 0.015;

        public double redEnergySpentPenalty = 0.005;

        public double decayGenerationsCount = 2.0;
    }
}
