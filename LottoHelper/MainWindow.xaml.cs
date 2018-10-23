using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LottoHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameType gameType;
        public MainWindow()
        {
            InitializeComponent();
            this.RadioButton51.IsChecked = true;
        }

        private void AnyTextChanged(object sender, TextChangedEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            (var winningNumbers, var winningNumberError) = NumberSet.Parse(tbWinningNumbers.Text, gameType);
            var entries = new List<NumberSet>();
            foreach (var line in tbEntries.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                var set = NumberSet.Parse(line, gameType).Set;
                if (set != null)
                    entries.Add(set);
            }
            var strs = new List<string>();
            if (winningNumbers != null)
            {
                foreach (var entry in entries)
                {
                    var prize = entry.CalcPrize(winningNumbers, gameType);
                    if (prize > 0)
                        strs.Add(string.Format("${0} for {1} {2}", prize.ToString("N"), string.Join(" ", entry.Numbers), entry.SpecialNumber));
                }
                tbWinners.Text = string.Format("Winners ({0} of {1}):{3}{2}", strs.Count, entries.Count, string.Join(Environment.NewLine, strs), Environment.NewLine);
            }
            else
                tbWinners.Text = $"(unable to compare to winning numbers: {winningNumberError})";
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
}
