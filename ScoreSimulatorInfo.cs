using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using PerformanceCalculator.Simulate;
using osu.Game.Rulesets.Osu.Difficulty;

namespace MassBalancer
{
    public class ScoreSimulatorInfo
    {
        public int mapID { get; set; }
        public int countOk { get; set; }
        public int countMeh { get; set; }
        public int countMiss { get; set; }
        public int combo { get; set; }
        public string[] mods { get; set; }
        public int targetPP { get; set; }
        public OsuPerformanceAttributes ppAttribs { get; set;}
        public string name { get; set; }
        public double ppValue 
            => ppAttribs is null ? 0 : ppAttribs.Total;
        public double difference
            => ppValue - targetPP;
        public double ratio
            => ppValue / targetPP;
        public ScoreSimulatorInfo(int mapID, int countOk=0, int countMeh=0, int countMiss=0, int combo=0, string mods="", int targetPP=0, string name="")
        {
            this.mapID = mapID;
            this.countOk = countOk;
            this.countMeh = countMeh;
            this.countMiss = countMiss;
            this.combo = combo;
            this.targetPP = targetPP;
            this.name = name;
            
            this.mods = new string[mods.Length / 2];
            for (int i = 0; i < mods.Length / 2; i++)
                this.mods[i] = mods[2*i].ToString() + mods[2*i+1].ToString();

            SetAttribs();
        }


        public OsuSimulateCommand GetScoreSimulator()
            => new OsuSimulateCommand(mapID.ToString(), Combo: combo, Mods: mods, Misses: countMiss, Mehs: countMeh, Goods: countOk);

        public OsuPerformanceAttributes SetAttribs()
            => ppAttribs = GetScoreSimulator().CalculatePerformance();

    }
}