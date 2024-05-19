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
                new ScoreSimulatorInfo(2069969, countOk: 37, combo: 1477, mods: "HDDTHR", targetPP: 1380, name:"Rat Race 3mod"),
                new ScoreSimulatorInfo(4415584, mods:"HDDTHR", targetPP: 1280, name:"Kuki brazil 3mod SS"),
                new ScoreSimulatorInfo(111680, countOk: 46, combo: 1106, mods:"HDDTHR", targetPP: 1400, name:"ath 3mod"),
            };
            Constants constants = new Constants();
            IEnumerable<PropertyInfo> consts = typeof(Constants).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            double initialDeviation = DifferenceDev(plays);
            for (int i = 0; i < NUM_REGRESSIONS; i++)
            {
                foreach (var property in consts)
                {
                    Constant current = Constant.GetFromProperty(property, constants);
                    current.Value /= STEP_MULT;
                    RunPlays(plays);
                    Constants.performanceMultiplier *= plays.Average(x => x.targetPP) / plays.Average(x => x.ppValue);
                    double decreaseDeviation = DifferenceDev(plays);
                    current.Value *= STEP_MULT * STEP_MULT;
                    RunPlays(plays);
                    Constants.performanceMultiplier *= plays.Average(x => x.targetPP) / plays.Average(x => x.ppValue);
                    double increaseDeviation = DifferenceDev(plays);
                    current.Value /= STEP_MULT;

                    if (decreaseDeviation > initialDeviation && increaseDeviation > decreaseDeviation);
                    else if (decreaseDeviation < increaseDeviation)
                    {
                        current.Value /= STEP_MULT;
                        initialDeviation = decreaseDeviation;
                    }
                    else
                    {
                        current.Value *= STEP_MULT;
                        initialDeviation = increaseDeviation;
                    }
                }
                Console.WriteLine($"Deviation at {i}: {initialDeviation}");
                Console.WriteLine(constants);
            }
            RunPlays(plays, true);
            Console.WriteLine(constants);
        }
        private static void RunPlays(List<ScoreSimulatorInfo> plays, bool showOutput=false)
        {
            if (!showOutput)
            {
                RunPlaysParallel(plays);
                return;
            }
            foreach (var play in plays)
            {
                play.SetAttribs();
                if (showOutput) Console.WriteLine($"{play.name.PadLeft(plays.Max(p => p.name.Length))}: PP - {play.ppValue:F2}. Target - {play.targetPP:F2}. Diff - {play.difference:F2}");
            }
        }
        public static void RunPlaysParallel(List<ScoreSimulatorInfo> plays)
        {
            Parallel.ForEach(plays, play => 
            {
                play.SetAttribs();
            });
        }

        public static double DifferenceDev(List<ScoreSimulatorInfo> plays)
        {
            IEnumerable<double> ppDiff = plays.Select(p => p.difference);
            double mean = ppDiff.Average();
            return Math.Sqrt(ppDiff.Sum(x => Math.Pow(x - mean, 2)) / ppDiff.Count());
        }
        public static double DifferenceMean(List<ScoreSimulatorInfo> plays)
        {
            IEnumerable<double> ppDiff = plays.Select(p => p.difference);
            return ppDiff.Average();
        }
        public static double RatioDev(List<ScoreSimulatorInfo> plays)
        {
            IEnumerable<double> ppDiff = plays.Select(p => p.ratio);
            double mean = ppDiff.Average();
            return Math.Sqrt(ppDiff.Sum(x => Math.Pow(x - mean, 2)) / ppDiff.Count());
        }
        public static double RatioMean(List<ScoreSimulatorInfo> plays)
        {
            IEnumerable<double> ppDiff = plays.Select(p => p.ratio);
            return ppDiff.Average();
        }
    }
}