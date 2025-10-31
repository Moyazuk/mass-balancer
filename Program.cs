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
                new ScoreSimulatorInfo(2850905, countOk: 0, combo: 1615, mods: "HDDT", targetPP: 600, name: "Relief"),
                new ScoreSimulatorInfo(40017, countOk: 23, mods: "HDDTHR", targetPP: 1600, name: "Made of Fire 3mod"),
                new ScoreSimulatorInfo(1528842, countOk: 27, mods: "HDDT", combo: 3886, targetPP: 1600, name: "Alt comp"),
                new ScoreSimulatorInfo(3960019, countOk: 22, combo: 1468, mods: "HDDT", targetPP: 1100, name: "Hikari"),
                new ScoreSimulatorInfo(4894158, countOk: 28, combo: 3480, mods: "HD", targetPP: 720, name:"River Styx"),
                new ScoreSimulatorInfo(3881559, mods:"HD", combo: 2900, countOk: 5, targetPP: 760, name:"Epitaph"),
                new ScoreSimulatorInfo(2022718, mods:"HDHR", combo: 617, targetPP: 460, name:"Ai no Sukima"),
                new ScoreSimulatorInfo(4808501, mods:"HDDT", combo: 590, targetPP: 1600, name:"Zetsubou"),
                new ScoreSimulatorInfo(4776435, mods:"HD", combo: 1530, targetPP: 680, name:"Uncontrollable"),
                new ScoreSimulatorInfo(3745820, mods:"HDDT", combo: 1530, targetPP: 1680, name:"Brazil HDDT"),
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