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
    }
    public class Constants
    {
        public Constant performanceBaseMultiplier { get; set; }
        public Constant aimMultiplier { get; set; }
        public Constant speedMultiplier { get; set; }

        public Constants()
        {
            performanceBaseMultiplier = new Constant(typeof(OsuPerformanceCalculator), "performanceBaseMultiplier");
            aimMultiplier = new Constant(typeof(Aim), "skillMultiplier");
            speedMultiplier = new Constant(typeof(Speed), "skillMultiplier");
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