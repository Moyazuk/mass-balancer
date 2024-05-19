using System;
using System.Collections.Generic;
using System.Reflection;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Osu.Difficulty.Skills;

namespace MassBalancer
{
    public class Constant
    {
        private readonly FieldInfo field;
        public double Value
        {
            get { return (field.GetValue(null) as double?) ?? 0; }
            set { field.SetValue(null, value); }
        }
        public Constant(Type type, string name)
        {
            field = type.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
        public override string ToString()
            => Value.ToString();
        public static Constant GetFromProperty(PropertyInfo info, Constants owner)
            => info.GetValue(owner) as Constant;
    }
    public class Constants
    {
        public Constant performanceBaseMultiplier { get; set; }
        public Constant aimMultiplier { get; set; }
        public Constant speedMultiplier { get; set; }
        public Constant speedStrainDecayBase { get; set; }
        public Constant aimStrainDecayBase { get; set; }
        public Constant shortLengthBonusMultiplier { get; set; }
        public Constant longLengthBonusMultiplier { get; set; }
        public Constant accuracyLengthBonusMax { get; set; }
        public Constant accuracyLengthBonusDivisor { get; set; }
        public Constant longMapThreshold { get; set; }

        public Constants()
        {
            performanceBaseMultiplier = new Constant(typeof(OsuPerformanceCalculator), "performanceBaseMultiplier");
            aimMultiplier = new Constant(typeof(Aim), "skillMultiplier");
            speedMultiplier = new Constant(typeof(Speed), "skillMultiplier");
            speedStrainDecayBase = new Constant(typeof(Speed), "strainDecayBase");
            aimStrainDecayBase = new Constant(typeof(Aim), "strainDecayBase");
            shortLengthBonusMultiplier = new Constant(typeof(OsuPerformanceCalculator), "shortLengthBonusMultiplier");
            longLengthBonusMultiplier = new Constant(typeof(OsuPerformanceCalculator), "longLengthBonusMultiplier");
            accuracyLengthBonusMax = new Constant(typeof(OsuPerformanceCalculator), "accuracyLengthBonusMax");
            accuracyLengthBonusDivisor = new Constant(typeof(OsuPerformanceCalculator), "accuracyLengthBonusDivisor");
            longMapThreshold = new Constant(typeof(OsuPerformanceCalculator), "longMapThreshold");
        }
        public override string ToString()
        {
            string output = "";
            PropertyInfo[] props = this.GetType().GetProperties();
            foreach (PropertyInfo property in props)
                output += $"{property.Name}: {property.GetValue(this)}\n";
            return output;
        }
    }
}