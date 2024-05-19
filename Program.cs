using System;
using System.Collections.Generic;
using PerformanceCalculator;
using System.Text;
using McMaster.Extensions.CommandLineUtils;
using osu.Framework.Logging;
using osu.Game.Beatmaps.Formats;
using osu.Game.Online;
using PerformanceCalculator.Simulate;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using System.Reflection;

namespace MassBalancer
{
    public class Program
    {
        public static readonly EndpointConfiguration ENDPOINT_CONFIGURATION = new ProductionEndpointConfiguration();
        const int NUM_REGRESSIONS = 20;
        const double STEP_MULT = 1.05;

        public static void Main(string[] args)
        {
            List<ScoreSimulatorInfo> plays = new List<ScoreSimulatorInfo>()
            {
                new ScoreSimulatorInfo(1754777, countOk: 84, combo: 2260, mods: "HDDT", targetPP: 1550, name: "Sidetracked Day"),
                new ScoreSimulatorInfo(3675267, countOk: 14, mods: "DT", targetPP: 1400, name: "Accolibed Azul Remix"),
                new ScoreSimulatorInfo(1711326, countOk: 45, countMeh: 1, mods: "DT", combo: 1577, targetPP: 1500, name: "Owari"),
                new ScoreSimulatorInfo(1811527, countOk: 62, countMeh: 2, countMiss: 2, combo: 4088, mods: "DT", targetPP: 1400, name: "mrekk Save me [Nightmare]"),
                new ScoreSimulatorInfo(2069969, countOk: 37, combo: 1477, mods: "HDDTHR", targetPP: 1400, name:"Rat Race 3mod"),
                new ScoreSimulatorInfo(4415584, mods:"HDDTHR", targetPP: 1280, name:"Kuki brazil 3mod SS"),
            };
            Constants constants = new Constants();
            PropertyInfo[] consts = typeof(Constants).GetType().GetProperties();
            for (int i = 0; i < NUM_REGRESSIONS; i++)
            {
                double initialDeviation = RatioDev(plays);
                List<(double dev, PropertyInfo property)> constantDeviationPairs = new List<(double, (PropertyInfo, double));
                foreach (var property in consts)
                {
                    double val = property.GetValue(constants);
                    property.SetValue(constants, val * STEP_MULT);
                    RunPlays(plays, true);
                    constantDeviationPairs.Add(RatioDev(plays), property);
                    property.SetValue(constants, val / STEP_MULT);
                }
                if (initialDeviation > constantDeviationPairs.Max(x => x.dev))
                    break;

            }
            constants.performanceBaseMultiplier.Value /= RatioMean(plays);
            RunPlays(plays, true);
            Console.WriteLine(constants);
        }
        private static void RunPlays(List<ScoreSimulatorInfo> plays, bool showOutput=false)
        {
            foreach (var play in plays)
            {
                if (showOutput) 
                    Console.WriteLine($"Calculating {play.name}");
                play.SetAttribs();
                OsuPerformanceAttributes attribs = play.ppAttribs;
            }

            if (showOutput)
            {
                Console.WriteLine("");
                foreach (var play in plays.OrderBy(x => -Math.Abs(x.difference)))
                {
                    Console.WriteLine($"{play.name.PadLeft(plays.Max(p => p.name.Length))}: PP - {play.ppValue:F2}. Target - {play.targetPP:F2}. Diff - {play.difference:F2}");
                }
            }
        }

        public static double DifferenceDev(List<ScoreSimulatorInfo> plays)
        {
            IEnumerable<double> ppDiff = plays.Where(p => p.targetPP != 0).Select(p => p.difference);
            double mean = ppDiff.Average();
            return Math.Sqrt(ppDiff.Sum(x => Math.Pow(x - mean, 2)) / ppDiff.Count());
        }
        public static double DifferenceMean(List<ScoreSimulatorInfo> plays)
        {
            IEnumerable<double> ppDiff = plays.Where(p => p.targetPP != 0).Select(p => p.difference);
            return ppDiff.Average();
        }
        public static double RatioDev(List<ScoreSimulatorInfo> plays)
        {
            IEnumerable<double> ppDiff = plays.Where(p => p.targetPP != 0).Select(p => p.ratio);
            double mean = ppDiff.Average();
            return Math.Sqrt(ppDiff.Sum(x => Math.Pow(x - mean, 2)) / ppDiff.Count());
        }
        public static double RatioMean(List<ScoreSimulatorInfo> plays)
        {
            IEnumerable<double> ppDiff = plays.Where(p => p.targetPP != 0).Select(p => p.ratio);
            return ppDiff.Average();
        }
    }
}