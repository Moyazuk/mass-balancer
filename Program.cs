using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ValueSet
{
    public class ScoreInfo
    {
        public int mapID { get; set; }
        public int countOk { get; set; }
        public int countMeh { get; set; }
        public int countMiss { get; set; }
        public int combo { get; set; }
        public string[] mods { get; set; }
        public int targetPP { get; set; }
        public double ppValue { get; set;}
        public string name { get; set; }
        public double difference
            => ppValue - targetPP;
        public ScoreInfo(int mapID, int countOk=0, int countMeh=0, int countMiss=0, int combo=0, string mods="", int targetPP=0, string name="")
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
        }
        private string ModArguments()
        {
            string ret = "";
            foreach (string mod in mods)
                ret += $"-m {mod} ";
                return ret;
        }
        private string ComboArgument()
            => combo == 0 ? "" : $"-c {combo}";
        public string CommandArguments()
            => $"simulate osu {mapID} {ModArguments()} -X {countMiss} {ComboArgument()} -M {countMeh} -G {countOk} -nc -j";
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            List<ScoreInfo> plays = new List<ScoreInfo>()
            {
                new ScoreInfo(1754777, countOk: 84, combo: 2260, mods: "HDDT", targetPP: 1550, name: "Sidetracked Day"),
                new ScoreInfo(3675267, countOk: 14, mods: "DT", targetPP: 1400, name: "Accolibed Azul Remix"),
                new ScoreInfo(1711326, countOk: 45, countMeh: 1, mods: "DT", combo: 1577, targetPP: 1500, name: "Owari"),
                new ScoreInfo(1811527, countOk: 62, countMeh: 2, countMiss: 2, combo: 4088, mods: "DT", targetPP: 1400, name: "mrekk Save me [Nightmare]"),
                new ScoreInfo(2069969, countOk: 37, combo: 1477, mods: "HDDTHR", targetPP: 1400, name:"Rat Race 3mod")
            };
            foreach (var play in plays)
            {
                Console.WriteLine($"Calculating {play.name}");
                string json = RunCommandWithBash($"calc.sh {play.CommandArguments()}");
                double pp = double.Parse(ReadJson(json, "pp"));
                play.ppValue = pp;
            }
            Console.WriteLine("");

            foreach (var play in plays.OrderBy(x => -Math.Abs(x.difference)))
            {
                Console.WriteLine($"{play.name.PadLeft(plays.Max(p => p.name.Length))}: PP - {play.ppValue:F2}. Target - {play.targetPP:F2}. Diff - {play.difference:F2}");
            }

            Console.WriteLine($"Std. Dev: {StdDev(plays):F2}");

        }

        public static double StdDev(List<ScoreInfo> plays)
        {
            List<double> ppDiff = plays.Where(p => p.targetPP != 0).Select(p => p.difference).ToList();
            double mean = ppDiff.Average();
            return Math.Sqrt(ppDiff.Sum(x => Math.Pow(x - mean, 2)) / ppDiff.Count());
        }

        public static string ReadJson(string json, string name)
        {
            string[] lines = json.Split('\n');
            string relevantLine = lines.First(l => l.Contains(name));
            return relevantLine.Split(':')[1].Replace(" ", "");
        }


        public static string RunCommandWithBash(string command)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = "/bin/bash";
            psi.Arguments = command;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            using var process = Process.Start(psi);

            if (process is null)
                throw new Exception("process was null");

            process.WaitForExit();

            var output = process.StandardOutput.ReadToEnd();

            return output;
        }
    }
}