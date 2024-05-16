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

namespace MassBalancer
{
    public class Program
    {
        public static readonly EndpointConfiguration ENDPOINT_CONFIGURATION = new ProductionEndpointConfiguration();

        public static void Main(string[] args)
        {
            List<ScoreSimulatorInfo> plays = new List<ScoreSimulatorInfo>()
            {
                new ScoreSimulatorInfo(1754777, countOk: 84, combo: 2260, mods: "HDDT", targetPP: 1550, name: "Sidetracked Day"),
                new ScoreSimulatorInfo(3675267, countOk: 14, mods: "DT", targetPP: 1400, name: "Accolibed Azul Remix"),
                new ScoreSimulatorInfo(1711326, countOk: 45, countMeh: 1, mods: "DT", combo: 1577, targetPP: 1500, name: "Owari"),
                new ScoreSimulatorInfo(1811527, countOk: 62, countMeh: 2, countMiss: 2, combo: 4088, mods: "DT", targetPP: 1400, name: "mrekk Save me [Nightmare]"),
                new ScoreSimulatorInfo(2069969, countOk: 37, combo: 1477, mods: "HDDTHR", targetPP: 1400, name:"Rat Race 3mod")
            };

            
            foreach (var play in plays)
            {
                Console.WriteLine($"Calculating {play.name}");
                var attribs = play.GetScoreSimulator().CalculatePerformance();
                play.ppValue = attribs.Total;
            }
            Console.WriteLine("");

            foreach (var play in plays.OrderBy(x => -Math.Abs(x.difference)))
            {
                Console.WriteLine($"{play.name.PadLeft(plays.Max(p => p.name.Length))}: PP - {play.ppValue:F2}. Target - {play.targetPP:F2}. Diff - {play.difference:F2}");
            }

            Console.WriteLine($"Std. Dev: {StdDev(plays):F2}");

        }

        public static double StdDev(List<ScoreSimulatorInfo> plays)
        {
            List<double> ppDiff = plays.Where(p => p.targetPP != 0).Select(p => p.difference).ToList();
            double mean = ppDiff.Average();
            return Math.Sqrt(ppDiff.Sum(x => Math.Pow(x - mean, 2)) / ppDiff.Count());
        }
    }
}