using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace LottoHelper
{
    public class NumberSet
    {
        static Regex regex = new Regex(@"^((\d{2})\s?){6}$");
        public ISet<int> Numbers { get; private set; }
        public int? SpecialNumber { get; private set; }

        private NumberSet()
        {
            this.Numbers = new HashSet<int>();
        }

        public int CalcPrize(NumberSet other, GameType gameType)
        {
            int matches = this.Numbers.Intersect(other.Numbers).Count();
            if (gameType == GameType.MegaMillions)
            {
                if (matches < 0 || matches > 5)
                    throw new InvalidOperationException();
                if (this.SpecialNumber == other.SpecialNumber)
                {
                    switch (matches)
                    {
                        default: return 2;
                        case 1: return 4;
                        case 2: return 10;
                        case 3: return 200;
                        case 4: return 10000;
                        case 5: return 913_000_000;
                    }
                }
                else
                {
                    switch (matches)
                    {
                        default: return 0;
                        case 3: return 10;
                        case 4: return 500;
                        case 5: return 1_000_000;
                    }
                }
            }
            else if (gameType == GameType.TexasLotto)
            {
                if (matches < 0 || matches > 6)
                    throw new InvalidOperationException();
                switch (matches)
                {
                    default: return 0;
                    case 3: return 3;
                    case 4: return 57;
                    case 5: return 3377;
                    case 6: return 6250000;
                }
            }
            else
                throw new ArgumentException();
        }

        public static (NumberSet Set, string ParseError) Parse(string s, GameType gameType)
        {
            if (string.IsNullOrEmpty(s))
                return (null, "Blank");
            var split = regex.Match(s).Groups[2].Captures.Cast<Capture>().Select(x => x.Value).ToArray();
            if (split.Length != 6)
                return (null, "Didn't get six numbers");
            var resp = new NumberSet();
            int last = 0;
            int max = gameType == GameType.MegaMillions ? 75 : 54;
            foreach (var num in gameType == GameType.MegaMillions ? split.Take(5) : split)
            {
                if (int.TryParse(num, out int i))
                    resp.Numbers.Add(i);
                else
                    return (null, "Found non-number value " + num);
                if (i <= last)
                {
                    Debug.WriteLine("Line out of order");
                    return (null, "Line out of order");
                }
                if (i > max)
                {
                    Debug.WriteLine("Line over max");
                    return (null, "Line over max");
                }
                last = i;
            }
            if (gameType == GameType.MegaMillions)
            {
                if (int.TryParse(split[5], out int p))
                    resp.SpecialNumber = p;
                else
                    return (null, "Found non-number value " + split[5]);
                if (resp.SpecialNumber > 25)
                {
                    Debug.WriteLine("Special over max");
                    return (null, "Special over max");
                }
            }
            return (resp, "");
        }
    }
}
