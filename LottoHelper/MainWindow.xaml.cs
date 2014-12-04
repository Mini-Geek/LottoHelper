using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace LottoHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NumberSet WinningNumbers;
        IList<NumberSet> Entries;
        GameType gameType;
        public MainWindow()
        {
            InitializeComponent();
            this.RadioButton6.IsChecked = true;
        }

        private void tbWinningNumbers_TextChanged(object sender, TextChangedEventArgs e)
        {
            Update();
        }

        private void tbEntries_TextChanged(object sender, TextChangedEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            WinningNumbers = NumberSet.Parse(tbWinningNumbers.Text, gameType);
            Entries = new List<NumberSet>();
            foreach (var line in tbEntries.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                var set = NumberSet.Parse(line, gameType);
                if (set != null)
                    Entries.Add(set);
            }
            var strs = new List<string>();
            if (WinningNumbers != null)
            {
                foreach (var entry in Entries)
                {
                    var prize = entry.CalcPrize(WinningNumbers, gameType);
                    if (prize > 0)
                        strs.Add(string.Format("${0} for {1} {2}", prize.ToString("N"), string.Join(" ", entry.Numbers), entry.SpecialNumber));
                }
                tbWinners.Text = string.Format("Winners ({0} of {1}):{3}{2}", strs.Count, Entries.Count, string.Join(Environment.NewLine, strs), Environment.NewLine);
            }
            else
                tbWinners.Text = "(no winning numbers)";
        }

        private void RadioButton51_Click(object sender, RoutedEventArgs e)
        {
            this.gameType = GameType.MegaMillions;
            Update();
        }

        private void RadioButton6_Click(object sender, RoutedEventArgs e)
        {
            this.gameType = GameType.TexasLotto;
            Update();
        }
    }

    public enum GameType
    {
        MegaMillions,
        TexasLotto
    }

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
                        default: return 1;
                        case 1: return 2;
                        case 2: return 5;
                        case 3: return 50;
                        case 4: return 5000;
                        case 5: return 137000000;
                    }
                }
                else
                {
                    switch (matches)
                    {
                        default: return 0;
                        case 3: return 5;
                        case 4: return 500;
                        case 5: return 1000000;
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

        public static NumberSet Parse(string s, GameType gameType)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            var split = regex.Match(s).Groups[2].Captures.Cast<Capture>().Select(x => x.Value).ToArray();
            if (split.Length != 6)
                return null;
            var resp = new NumberSet();
            int last = 0;
            int max = gameType == GameType.MegaMillions ? 75 : 54;
            foreach (var num in gameType == GameType.MegaMillions ? split.Take(5) : split)
            {
                int i;
                if (int.TryParse(num, out i))
                    resp.Numbers.Add(i);
                else
                    return null;
                if (i <= last)
                {
                    Debug.WriteLine("Line out of order");
                    return null;
                }
                if (i > max)
                {
                    Debug.WriteLine("Line over max");
                    return null;
                }
                last = i;
            }
            if (gameType == GameType.MegaMillions)
            {
                int p;
                if (int.TryParse(split[5], out p))
                    resp.SpecialNumber = p;
                else
                    return null;
            }
            return resp;
        }
    }
}
