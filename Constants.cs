using System;
using System.Collections.Generic;
using System.Reflection;
using osu.Game.Rulesets.Osu.Difficulty;

namespace MassBalancer
{
    public class Constant
    {
        private readonly FieldInfo field;
        public double value
        {
            get { return (field.GetValue(null) as double?) ?? 0}
            set { field.SetValue(null, value); }
        }
        public Constant(Type type, string name)
        {
            field = type.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
    public class Constants
    {
        Constant performanceBaseMultiplier;
        Constant aimMultiplier;
        Constant speedMultiplier;

        public Constants()
        {
            performanceBaseMultiplier = new Constant(typeof(OsuPerformanceCalculator), "performanceBaseMultiplier");
            aimMultiplier = new Constant(typeof(Aim), "skillMultiplier");
            speedMultiplier = new Constant(typeof(Speed), "skillMultiplier");
        }
    }
}