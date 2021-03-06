using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Windows;
using BenchMaestro.Windows;

namespace BenchMaestro
{
    
    //XMRSTAKRX
    //CPUMINER
    static class BenchModule1
    {
        public static void UpdateMonitoring2(Grid ScoreList, Window thiswin)
        {
            try
            {
                var gridLength1 = new GridLength(1.1, GridUnitType.Star);
                var gridLength2 = new GridLength(1, GridUnitType.Star);

                char maxchar = '\u2191';
                char avgchar = '\u2259';
                //char degreesc = '\u2103';
                //char degreesf = '\u2109';
                char degrees = '\u2103';

                int _row = 0;
                string _value = "";

                /// SCORE

                TextBlock _textblock = App.CurrentRun.ScoreBox2;

                if (App.CurrentRun.Score >= 0 && App.CurrentRun.CPUAvgPower > 0)
                {
                    _textblock.Visibility = Visibility.Visible;
                    double _scorepw;
                    string _scorepwscale;
                    App.CurrentRun.ScorePerWatt = App.CurrentRun.ScoreScaled / (float)App.CurrentRun.CPUAvgPower;
                    (_scorepw, _scorepwscale) = HWMonitor.GetScaleValueAndPrefix((float)App.CurrentRun.ScorePerWatt);

                    _textblock.Inlines.Add(new Run { Text = $"{Math.Round((decimal)_scorepw, 2)}", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.scorebrush });
                    _textblock.Inlines.Add(new Run { Text = $" {_scorepwscale}{App.CurrentRun.BenchScoreUnit} per Watt", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                }

                /// TEMP

                _textblock = App.CurrentRun.CPUTempBox;
                Grid _gridblock = App.CurrentRun.CPUTempGrid;
                _gridblock.VerticalAlignment = VerticalAlignment.Center;

                _row = 0;

                if (App.CurrentRun.CPUAvgTemp > 0)
                {

                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUAvgTemp, 1).ToString("0.0")}", FontSize = 17, FontWeight = FontWeights.Bold, Foreground = App.tempbrush });
                    _tb1a.Inlines.Add(new Run { Text = $" {degrees} ", FontSize = 14, FontWeight = FontWeights.Normal, Foreground = App.tempbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CPUMaxTemp, 1).ToString("0.0")} °C", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _row++;
                }

                if (App.CurrentRun.CoresAvgTemp > 0)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $"Cores: {Math.Round(App.CurrentRun.CoresAvgTemp, 1).ToString("0.0")}", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.tempbrush });
                    _tb1a.Inlines.Add(new Run { Text = $" {degrees} ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.tempbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CoresMaxTemp, 1).ToString("0.0")} °C", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _row++;
                }

                ScoreList.Children.Add(_gridblock);

                if (_gridblock.RowDefinitions.Count > 0) _textblock.Text = "";

                /// CLOCK

                _textblock = App.CurrentRun.CPUClockBox;
                _gridblock = App.CurrentRun.CPUClockGrid;
                _gridblock.VerticalAlignment = VerticalAlignment.Center;

                _row = 0;

                if (App.CurrentRun.CPUAvgClock > 0 && App.CurrentRun.CPUAvgEffClock < 0)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $"{App.CurrentRun.CPUAvgClock}", FontSize = 17, FontWeight = FontWeights.Bold, Foreground = App.clockbrush1 });
                    _tb1a.Inlines.Add(new Run { Text = $" MHz", FontSize = 14, FontWeight = FontWeights.Normal, Foreground = App.clockbrush1 });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    if (App.CurrentRun.CPUAvgLoad > 0)
                    {
                        TextBlock _tb1b = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                        _tb1b.Inlines.Add(new Run { Text = $" {avgchar} {Math.Round(App.CurrentRun.CPUAvgLoad, 2)}%", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = App.blackbrush });
                        _tb1b.Inlines.Add(new Run { Text = " Load", FontSize = 9, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                        Grid.SetColumn(_tb1b, 1);
                        Grid.SetRow(_tb1b, _row);
                        _tb1b.TextAlignment = TextAlignment.Left;
                        _gridblock.Children.Add(_tb1b);
                    }

                    _gridblock.RowDefinitions.Add(new RowDefinition { });
                    _row++;

                    TextBlock _tb2a = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb2a.Inlines.Add(new Run { Text = $"{App.CurrentRun.CPUMaxClock}", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = App.clockbrush2 });
                    _tb2a.Inlines.Add(new Run { Text = $" MHz {maxchar}", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.clockbrush2 });
                    Grid.SetColumn(_tb2a, 0);
                    Grid.SetRow(_tb2a, _row);
                    _tb2a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb2a);

                    if (App.CurrentRun.CPUMaxLoad > 0)
                    {
                        TextBlock _tb2b = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                        _tb2b.Inlines.Add(new Run { Text = $" {Math.Round(App.CurrentRun.CPUMaxLoad, 1)} % Load", FontSize = 9, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                        Grid.SetColumn(_tb2b, 1);
                        Grid.SetRow(_tb2b, _row);
                        _tb2b.TextAlignment = TextAlignment.Left;
                        _gridblock.Children.Add(_tb2b);
                    }
                }

                if (App.CurrentRun.CPUAvgEffClock > 0)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $"{App.CurrentRun.CPUAvgEffClock} / {App.CurrentRun.CPUAvgClock}", FontSize = 17, FontWeight = FontWeights.Bold, Foreground = App.clockbrush1 });
                    _tb1a.Inlines.Add(new Run { Text = $" MHz", FontSize = 14, FontWeight = FontWeights.Normal, Foreground = App.clockbrush1 });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $" {avgchar} Eff/Ref", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.clockbrush1 });
                    if (App.CurrentRun.CPUAvgLoad > 0)
                    {
                        _tb1b.Inlines.Add(new Run { Text = $" @ ", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                        _tb1b.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUAvgLoad, 0)} %", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = App.blackbrush });
                        _tb1b.Inlines.Add(new Run { Text = $" Load", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                    }
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _gridblock.RowDefinitions.Add(new RowDefinition { });
                    _row++;

                    TextBlock _tb2a = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb2a.Inlines.Add(new Run { Text = $"{App.CurrentRun.CPUMaxEffClock} / {App.CurrentRun.CPUMaxClock}", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = App.clockbrush2 });
                    _tb2a.Inlines.Add(new Run { Text = $" MHz", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.clockbrush2 });
                    Grid.SetColumn(_tb2a, 0);
                    Grid.SetRow(_tb2a, _row);
                    _tb2a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb2a);

                    TextBlock _tb2b = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb2b.Inlines.Add(new Run { Text = $" {maxchar} Eff/Ref", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.clockbrush1 });
                    if (App.CurrentRun.CPUMaxLoad > 0)
                    {
                        _tb2b.Inlines.Add(new Run { Text = $" @ ", FontSize = 9, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                        _tb2b.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUMaxLoad, 0)} %", FontSize = 9, FontWeight = FontWeights.Bold, Foreground = App.blackbrush });
                        _tb2b.Inlines.Add(new Run { Text = $" Load", FontSize = 9, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                    }
                    Grid.SetColumn(_tb2b, 1);
                    Grid.SetRow(_tb2b, _row);
                    _tb2b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb2b);
                    

                    _gridblock.RowDefinitions.Add(new RowDefinition { });
                    _row++;

                    _value = (App.CurrentRun.CPUAvgStretch > 1) ? App.CurrentRun.CPUAvgStretch.ToString() : App.CurrentRun.CPUAvgStretch > -99999 ? "None" : "N/D";

                    TextBlock _tb3a = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb3a.Inlines.Add(new Run { Text = $"{avgchar} Stretch: ", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                    _tb3a.Inlines.Add(new Run { Text = $"{_value}", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = App.blackbrush });
                    if (_value != "None" && _value != "N/D") _tb3a.Inlines.Add(new Run { Text = " MHz", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = App.blackbrush });
                    Grid.SetColumn(_tb3a, 0);
                    Grid.SetRow(_tb3a, _row);
                    _tb3a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb3a);

                    _value = (App.CurrentRun.CPUMaxStretch > 1) ? App.CurrentRun.CPUMaxStretch.ToString() : App.CurrentRun.CPUMaxStretch > -99999 ? "None" : "N/D";

                    TextBlock _tb3b = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb3b.Inlines.Add(new Run { Text = $" {maxchar} Stretch: ", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                    _tb3b.Inlines.Add(new Run { Text = $"{_value}", FontSize = 9, FontWeight = FontWeights.Bold, Foreground = App.blackbrush });
                    if (_value != "None" && _value != "N/D") _tb3b.Inlines.Add(new Run { Text = " MHz", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = App.blackbrush });
                    Grid.SetColumn(_tb3b, 1);
                    Grid.SetRow(_tb3b, _row);
                    _tb3b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb3b);
                }

                ScoreList.Children.Add(_gridblock);

                if (_gridblock.RowDefinitions.Count > 0) _textblock.Text = "";

                /// POWER

                _textblock = App.CurrentRun.CPUPowerBox;
                _gridblock = App.CurrentRun.CPUPowerGrid;
                _gridblock.VerticalAlignment = VerticalAlignment.Center;

                _row = 0;

                if (App.CurrentRun.CPUAvgPower > 0)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUAvgPower, 1).ToString("0.0")}", FontSize = 17, FontWeight = FontWeights.Bold, Foreground = App.powerbrush });
                    _tb1a.Inlines.Add(new Run { Text = " W ", FontSize = 14, FontWeight = FontWeights.Normal, Foreground = App.powerbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    if (App.CurrentRun.CPUMaxPower > 0)
                    {
                        TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                        _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CPUMaxPower, 1).ToString("0.0")} W", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
                        Grid.SetColumn(_tb1b, 1);
                        Grid.SetRow(_tb1b, _row);
                        _tb1b.TextAlignment = TextAlignment.Left;
                        _gridblock.Children.Add(_tb1b);
                    }
                    _row++;
                }

                if (App.CurrentRun.CoresAvgPower > 0)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $"Cores: ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.powerbrush });
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CoresAvgPower, 1).ToString("0.0")}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = App.powerbrush });
                    _tb1a.Inlines.Add(new Run { Text = " W ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.powerbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CoresMaxPower, 1).ToString("0.0")} W", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _row++;
                }

                if (App.CurrentRun.CPUAvgVoltage > 0)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $"vCore: ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.voltbrush });
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUAvgVoltage, 3).ToString("0.000")}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = App.voltbrush });
                    _tb1a.Inlines.Add(new Run { Text = " V ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.voltbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CPUMaxVoltage, 3).ToString("0.000")} V", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _row++;
                }

                if (App.CurrentRun.CoresAvgVoltage > 0)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $"VIDs: ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.voltbrush });
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CoresAvgVoltage, 3).ToString("0.000")}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = App.voltbrush });
                    _tb1a.Inlines.Add(new Run { Text = " V ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.voltbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CoresMaxVoltage, 3).ToString("0.000")} V", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _row++;
                }

                if (App.CurrentRun.SOCAvgVoltage > 0)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $"SoC: ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.voltbrush });
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.SOCAvgVoltage, 3).ToString("0.000")}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = App.voltbrush });
                    _tb1a.Inlines.Add(new Run { Text = " V ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.voltbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.SOCMaxVoltage, 3).ToString("0.000")} V", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _row++;
                }

                ScoreList.Children.Add(_gridblock);

                if (_gridblock.RowDefinitions.Count > 0) _textblock.Text = "";

                /// ADDITIONAL

                _textblock = App.CurrentRun.AdditionalBox;
                _gridblock = App.CurrentRun.AdditionalGrid;
                _gridblock.VerticalAlignment = VerticalAlignment.Center;

                _row = 0;

                if (App.CurrentRun.CPUSAAvgVoltage > 0)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $"CPU SA: ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.voltbrush });
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUSAAvgVoltage, 3).ToString("0.000")}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = App.voltbrush });
                    _tb1a.Inlines.Add(new Run { Text = " V ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.voltbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CPUSAMaxVoltage, 3).ToString("0.000")} V", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _row++;
                }

                if (App.CurrentRun.CPUIOAvgVoltage > 0)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $"CPU I/O: ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.voltbrush });
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUIOAvgVoltage, 3).ToString("0.000")}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = App.voltbrush });
                    _tb1a.Inlines.Add(new Run { Text = " V ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.voltbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CPUIOMaxVoltage, 3).ToString("0.000")} V", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _row++;
                }

                if (App.CurrentRun.CCDSAvgTemp > 0)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $"CCDs Avg: ", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CCDSAvgTemp, 1).ToString("0.0")} {degrees}", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _row++;

                }

                if (App.CurrentRun.CCD1AvgTemp > 0)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $"CCD1: {Math.Round(App.CurrentRun.CCD1AvgTemp, 1).ToString("0.0")}", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    _tb1a.Inlines.Add(new Run { Text = $" {degrees} ", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CCD1MaxTemp, 1).ToString("0.0")} {degrees}", FontSize = 11, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _row++;
                }

                if (App.CurrentRun.CCD2AvgTemp > 0)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $"CCD2: {Math.Round(App.CurrentRun.CCD2AvgTemp, 1).ToString("0.0")}", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    _tb1a.Inlines.Add(new Run { Text = $" {degrees} ", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CCD2MaxTemp, 1).ToString("0.0")} {degrees}", FontSize = 11, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _row++;
                }

                if (App.CurrentRun.L3AvgTemp > -999)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $" L3: {Math.Round(App.CurrentRun.L3AvgTemp, 1).ToString("0.0")}", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    _tb1a.Inlines.Add(new Run { Text = $" {degrees} ", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.L3MaxTemp, 1).ToString("0.0")} {degrees}", FontSize = 11, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _row++;
                }

                if (App.CurrentRun.CPUPPTAvg > 0)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $" PPT: ", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUPPTAvg, 0)}W ", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    if (App.CurrentRun.CPUPPTAvgLimit > 0)
                    {
                        _tb1a.Inlines.Add(new Run { Text = $" ({App.CurrentRun.CPUPPTAvgLimit}%)", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    }
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $" {maxchar} {Math.Round(App.CurrentRun.CPUPPTMax, 0)}W", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
                    if (App.CurrentRun.CPUPPTMaxLimit > 0)
                    {
                        _tb1b.Inlines.Add(new Run { Text = $" ({App.CurrentRun.CPUPPTMaxLimit}%)", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    }
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _row++;
                }

                if (App.CurrentRun.CPUTDCAvg > 0)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $" TDC: ", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUTDCAvg, 0)}A ", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    if (App.CurrentRun.CPUTDCAvgLimit > 0)
                    {
                        _tb1a.Inlines.Add(new Run { Text = $" ({App.CurrentRun.CPUTDCAvgLimit}%)", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    }
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $" {maxchar} {Math.Round(App.CurrentRun.CPUTDCMax, 0)}A", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
                    if (App.CurrentRun.CPUTDCMaxLimit > 0)
                    {
                        _tb1b.Inlines.Add(new Run { Text = $" ({App.CurrentRun.CPUTDCMaxLimit}%)", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    }
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _row++;
                }

                if (App.CurrentRun.CPUEDCAvg > 0)
                {
                    if (_gridblock.ColumnDefinitions.Count == 0)
                    {
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                        _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                    }
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    TextBlock _tb1a = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1a.Inlines.Add(new Run { Text = $" EDC: ", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUEDCAvg, 0)}A ", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    if (App.CurrentRun.CPUEDCAvgLimit > 0)
                    {
                        _tb1a.Inlines.Add(new Run { Text = $" ({App.CurrentRun.CPUEDCAvgLimit}%)", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    }
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $" {maxchar} {Math.Round(App.CurrentRun.CPUEDCMax, 0)}A", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
                    if (App.CurrentRun.CPUEDCMaxLimit > 0)
                    {
                        _tb1b.Inlines.Add(new Run { Text = $" ({App.CurrentRun.CPUEDCMaxLimit}%)", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    }
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _row++;
                }


                ScoreList.Children.Add(_gridblock);

                if (_gridblock.RowDefinitions.Count > 0) _textblock.Text = "";

                /// DETAILS

                StackPanel _stackpanel = App.CurrentRun.DetailsPanel;
                StackPanel _stackppanel = App.CurrentRun.DetailsPPanel;
                ScrollViewer _scroller = App.CurrentRun.DetailsScroller;
                _textblock = App.CurrentRun.DetailsBox;
                _gridblock = App.CurrentRun.DetailsGrid;
                _gridblock.MinHeight = 200;

                _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });

                _gridblock.ShowGridLines = false;

                _row = 0;

                void AddDetails(List<DetailsGrid> _thislist, string _header)
                {
                    Trace.WriteLine($"Start {_header}");

                    Thickness dthickness = new Thickness(4, 3, 4, 3);
                    GridLength _rowheigth = new GridLength(1, GridUnitType.Star);

                    bool anyprinted = false;

                    if (_thislist == null) return;

                    if (_thislist.Any())
                    {
                        _stackpanel.Visibility = Visibility.Visible;
                        _scroller.Visibility = Visibility.Visible;
                        _gridblock.Visibility = Visibility.Visible;
                        _textblock.Visibility = Visibility.Collapsed;

                        _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });

                        TextBlock _tbh = new TextBlock { Margin = dthickness, Background = App.boxbrush1, HorizontalAlignment = HorizontalAlignment.Stretch };
                        _tbh.Inlines.Add(new Run { Text = _header, FontSize = 9, FontWeight = FontWeights.Bold, Foreground = App.clockbrush1 });
                        Grid.SetColumn(_tbh, 0);
                        Grid.SetRow(_tbh, _row);
                        Grid.SetColumnSpan(_tbh, 6);
                        _tbh.TextAlignment = TextAlignment.Center;
                        _gridblock.Children.Add(_tbh);

                        _row++;

                        int _colspan = 1;
                        int _colspan1 = 1;
                        int _colspan2 = 1;
                        int _colspan3 = 1;
                        int _index = 1;
                        int _col = 0;
                        _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                        int _fcount = _thislist.Where(x => x.Val1 > -99999 || x.Val2 > -99999).Count();

                        if (_fcount == 1)
                        {
                            _col = 1;
                            _colspan = 1;
                            _colspan2 = 2;
                            _colspan3 = 1;
                        }

                        foreach (DetailsGrid _item in _thislist.Where(x => x.Val1 > -99999 || x.Val2 > -99999))
                        {
                            anyprinted = true;
                            string Label = _item.Label.ToString();
                            string Unit = _item.Unit.ToString();

                            Trace.WriteLine($"{_header} {Label} v1={_item.Val1} v2={_item.Val2} v1s={_item.Val1Scale} v2s={_item.Val2Scale} iu={Unit}");

                            if (_item.Val2 == -99998 && _fcount == 1)
                                _colspan2 = 1;
                            else if (_item.Val2 == -99998)
                                _colspan2 = 2;
                            else if(_item.Val2 > -99998 && _item.Unit.Length > 0 && _item.Val2 > 0)
                            {
                            }
                            if (_item.Val1 > -99998)
                            {
                            }

                            string Val1 = String.Format("{0:" + _item.Format + "}", _item.Val1.ToString());
                            string Val2 = String.Format("{0:" + _item.Format + "}", _item.Val2.ToString());

                            if (Val1 == "-99999") Val1 = "N/A";
                            if (Val2 == "-99999") Val2 = "N/A";
                            FontWeight _weight = FontWeights.Normal;
                            if (_item.Bold) _weight = FontWeights.Bold;
                            FontWeight _wnormal = FontWeights.Normal;
                            Trace.WriteLine($"{Label} {Val1} {Val2}");

                            TextBlock _tb1a = new TextBlock { Margin = dthickness, Background = App.boxbrush1, HorizontalAlignment = HorizontalAlignment.Stretch };
                            _tb1a.Inlines.Add(new Run { Text = Label, FontSize = 9, FontWeight = _weight, Foreground = App.blackbrush });
                            Grid.SetColumn(_tb1a, _col);
                            Grid.SetRow(_tb1a, _row);
                            Grid.SetColumnSpan(_tb1a, _colspan1);
                            _tb1a.TextAlignment = TextAlignment.Left;
                            _gridblock.Children.Add(_tb1a);

                            _col++;
                            if (_fcount == 1 && _item.Val2 == -99998) _col++;

                            TextBlock _tb1b = new TextBlock { Margin = dthickness, Background = App.boxbrush1, HorizontalAlignment = HorizontalAlignment.Stretch };
                            _tb1b.Inlines.Add(new Run { Text = $"{Val1}", FontSize = 9, FontWeight = _weight, Foreground = App.blackbrush });
                            if (_item.Val1Scale.Length > 0 || Unit.Length > 0)
                                _tb1b.Inlines.Add(new Run { Text = $" {_item.Val1Scale}{Unit}", FontSize = 9, FontWeight = _wnormal, Foreground = App.blackbrush });
                            Grid.SetColumn(_tb1b, _col);
                            Grid.SetRow(_tb1b, _row);
                            Grid.SetColumnSpan(_tb1b, _colspan2);
                            _tb1b.TextAlignment = TextAlignment.Right;
                            _gridblock.Children.Add(_tb1b);

                            _col++;
                            if (_fcount == 1) _col++;

                            if (_item.Val2 != -99998)
                            {
                                TextBlock _tb1c = new TextBlock { Margin = dthickness, Background = App.boxbrush1, HorizontalAlignment = HorizontalAlignment.Stretch };
                                _tb1c.Inlines.Add(new Run { Text = $"{Val2}", FontSize = 9, FontWeight = _weight, Foreground = App.blackbrush });
                                if (_item.Val2Scale.Length > 0 || Unit.Length > 0)
                                    _tb1c.Inlines.Add(new Run { Text = $" {_item.Val1Scale}{Unit}", FontSize = 9, FontWeight = _wnormal, Foreground = App.blackbrush });
                                Grid.SetColumn(_tb1c, _col);
                                Grid.SetRow(_tb1c, _row);
                                Grid.SetColumnSpan(_tb1c, _colspan3);
                                _tb1c.TextAlignment = TextAlignment.Right;
                                _gridblock.Children.Add(_tb1c);
                            }

                            _col++;
                            ++_index;

                            if (_index % 2 == 1 || _fcount == 1)
                            {
                                _col = 0;
                                _row++;
                                _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                                Trace.WriteLine($"Add Row {_item.Label}");
                            }
                            Trace.WriteLine($"Finish {_header}");
                        }
                        if (!anyprinted) _tbh.Visibility = Visibility.Collapsed;
                    }
                }

                void AddDetailAvgMax(double _avg, double _max, string _header, string _unit, string _format)
                {

                    Trace.WriteLine($"Start {_header}");

                    Thickness dthickness = new Thickness(4, 3, 4, 3);
                    GridLength _rowheigth = new GridLength(1, GridUnitType.Star);
                    _stackpanel.Visibility = Visibility.Visible;
                    _scroller.Visibility = Visibility.Visible;
                    _gridblock.Visibility = Visibility.Visible;
                    _textblock.Visibility = Visibility.Collapsed;

                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });

                    int _colspan = 6;
                    int _col = 0;

                    string Label = _header.ToString();

                    string Val1 = String.Format(CultureInfo.InvariantCulture, "{0:" + _format + "}", _avg);
                    string Val2 = String.Format(CultureInfo.InvariantCulture, "{0:" + _format + "}", _max);
                    if (Val1 == "-999") Val1 = "N/A";
                    if (Val2 == "-999") Val2 = "N/A";


                    TextBlock _tbh = new TextBlock { Margin = dthickness, Background = App.boxbrush1, HorizontalAlignment = HorizontalAlignment.Stretch };
                    _tbh.Inlines.Add(new Run { Text = _header, FontSize = 9, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                    _tbh.Inlines.Add(new Run { Text = $" Avg: ", FontSize = 9, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                    _tbh.Inlines.Add(new Run { Text = Val1, FontSize = 9, FontWeight = FontWeights.Bold, Foreground = App.blackbrush });
                    _tbh.Inlines.Add(new Run { Text = $"{_unit}", FontSize = 9, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                    _tbh.Inlines.Add(new Run { Text = $" Max: ", FontSize = 9, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                    _tbh.Inlines.Add(new Run { Text = Val2, FontSize = 9, FontWeight = FontWeights.Bold, Foreground = App.blackbrush });
                    _tbh.Inlines.Add(new Run { Text = $"{_unit}", FontSize = 9, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                    Grid.SetColumn(_tbh, _col);
                    Grid.SetRow(_tbh, _row);
                    Grid.SetColumnSpan(_tbh, _colspan);
                    _tbh.TextAlignment = TextAlignment.Center;
                    _gridblock.Children.Add(_tbh);

                    _row++;

                    Trace.WriteLine($"Finish {_header}");

                }

                void AddTimings( string _header)
                {

                    Trace.WriteLine($"Start {_header}");

                    Thickness dthickness = new Thickness(4, 3, 4, 3);
                    GridLength _rowheigth = new GridLength(1, GridUnitType.Star);

                    _stackpanel.Visibility = Visibility.Visible;
                    _scroller.Visibility = Visibility.Visible;
                    _gridblock.Visibility = Visibility.Visible;
                    _textblock.Visibility = Visibility.Collapsed;

                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });

                    TextBlock _tbh = new TextBlock { Margin = dthickness, Background = App.boxbrush1, HorizontalAlignment = HorizontalAlignment.Stretch };
                    _tbh.Inlines.Add(new Run { Text = _header, FontSize = 9, FontWeight = FontWeights.Bold, Foreground = App.clockbrush1 });
                    Grid.SetColumn(_tbh, 0);
                    Grid.SetRow(_tbh, _row);
                    Grid.SetColumnSpan(_tbh, 6);
                    _tbh.TextAlignment = TextAlignment.Center;
                    _gridblock.Children.Add(_tbh);

                    _row++;

                    FontWeight _wbold = FontWeights.Bold;
                    FontWeight _wnormal = FontWeights.Normal;

                    int _colspan = 1;
                    int _col = 0;
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });

                    string name = "";
                    string value = "";

                    void AddTiming(string name, string value)
                    {

                        TextBlock _tb1a = new TextBlock { Margin = dthickness, Background = App.boxbrush1, HorizontalAlignment = HorizontalAlignment.Stretch };
                        _tb1a.Inlines.Add(new Run { Text = $"{name}", FontSize = 9, FontWeight = _wnormal, Foreground = App.blackbrush });
                        Grid.SetColumn(_tb1a, _col);
                        Grid.SetRow(_tb1a, _row);
                        Grid.SetColumnSpan(_tb1a, _colspan);
                        _tb1a.TextAlignment = TextAlignment.Left;
                        _gridblock.Children.Add(_tb1a);

                        TextBlock _tb1b = new TextBlock { Margin = dthickness, Background = App.boxbrush1, HorizontalAlignment = HorizontalAlignment.Stretch };
                        _tb1b.Inlines.Add(new Run { Text = $"{value}", FontSize = 9, FontWeight = _wbold, Foreground = App.blackbrush });
                        Grid.SetColumn(_tb1b, _col+1);
                        Grid.SetRow(_tb1b, _row);
                        Grid.SetColumnSpan(_tb1b, _colspan);
                        _tb1b.TextAlignment = TextAlignment.Right;
                        _gridblock.Children.Add(_tb1b);

                    }

                    int row0 = _row;

                    AddTiming("tCL", $"{App.systemInfo.MEMCFG.CL}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tRCDWR", $"{App.systemInfo.MEMCFG.RCDWR}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tRCDRD", $"{App.systemInfo.MEMCFG.RCDRD}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tRP", $"{App.systemInfo.MEMCFG.RP}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tRAS", $"{App.systemInfo.MEMCFG.RAS}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tRC", $"{App.systemInfo.MEMCFG.RC}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tRRDS", $"{App.systemInfo.MEMCFG.RRDS}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tRRDL", $"{App.systemInfo.MEMCFG.RRDL}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tFAW", $"{App.systemInfo.MEMCFG.FAW}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tWTRS", $"{App.systemInfo.MEMCFG.WTRS}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tWTRL", $"{App.systemInfo.MEMCFG.WTRL}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tWR", $"{App.systemInfo.MEMCFG.WR}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tRFC(ns)", $"{App.systemInfo.MEMCFG.RFCns}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tRFC", $"{App.systemInfo.MEMCFG.RFC}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tRFC2", $"{App.systemInfo.MEMCFG.RFC2}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tRFC4", $"{App.systemInfo.MEMCFG.RFC4}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tMOD", $"{App.systemInfo.MEMCFG.MOD}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tMODPDA", $"{App.systemInfo.MEMCFG.MODPDA}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tPHYWRD", $"{App.systemInfo.MEMCFG.PHYWRD}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;
                    AddTiming("tPHYWRL", $"{App.systemInfo.MEMCFG.PHYWRL}");
                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;

                    _row = row0;
                    _col = 2;
                    AddTiming("tRDRDSCL", $"{App.systemInfo.MEMCFG.RDRDSCL}");
                    _row++;
                    AddTiming("tWRWRSCL", $"{App.systemInfo.MEMCFG.WRWRSCL}");
                    _row++;
                    AddTiming("tCWL", $"{App.systemInfo.MEMCFG.CWL}");
                    _row++;
                    AddTiming("tRTP", $"{App.systemInfo.MEMCFG.RTP}");
                    _row++;
                    AddTiming("tRDWR", $"{App.systemInfo.MEMCFG.RDWR}");
                    _row++;
                    AddTiming("tWRRD", $"{App.systemInfo.MEMCFG.WRRD}");
                    _row++;
                    AddTiming("tRDRDSC", $"{App.systemInfo.MEMCFG.RDRDSC}");
                    _row++;
                    AddTiming("tRDRDSD", $"{App.systemInfo.MEMCFG.RDRDSD}");
                    _row++;
                    AddTiming("tRDRDDD", $"{App.systemInfo.MEMCFG.RDRDDD}");
                    _row++;
                    AddTiming("tWRWRSC", $"{App.systemInfo.MEMCFG.WRWRSC}");
                    _row++;
                    AddTiming("tWRWRSD", $"{App.systemInfo.MEMCFG.WRWRSD}");
                    _row++;
                    AddTiming("tWRWRDD", $"{App.systemInfo.MEMCFG.WRWRDD}");
                    _row++;
                    AddTiming("tCKE", $"{App.systemInfo.MEMCFG.CKE}");
                    _row++;
                    AddTiming("tREFI", $"{App.systemInfo.MEMCFG.REFI}");
                    _row++;
                    AddTiming("tREFI(ns)", $"{App.systemInfo.MEMCFG.REFIns}");
                    _row++;
                    AddTiming("tSTAG", $"{App.systemInfo.MEMCFG.STAG}");
                    _row++;
                    AddTiming("tMRD", $"{App.systemInfo.MEMCFG.MRD}");
                    _row++;
                    AddTiming("tMRDPDA", $"{App.systemInfo.MEMCFG.MRDPDA}");
                    _row++;
                    AddTiming("tPHYRDL", $"{App.systemInfo.MEMCFG.PHYRDL}");
                    _row++;
                    AddTiming("PowerDown", $"{App.systemInfo.MEMCFG.PowerDown}");

                    _row = row0;
                    _col = 4;

                    AddTiming("Freq.", $"{App.systemInfo.MEMCFG.Frequency}");
                    _row++;
                    AddTiming("BGS", $"{App.systemInfo.MEMCFG.BGS}");
                    _row++;
                    AddTiming("BGS Alt.", $"{App.systemInfo.MEMCFG.BGSAlt}");
                    _row++;
                    AddTiming("GDM", $"{App.systemInfo.MEMCFG.GDM}");
                    _row++;
                    AddTiming("Cmd2T", $"{App.systemInfo.MEMCFG.Cmd2T}");
                    _row++;
                    AddTiming("Capacity", $"{App.systemInfo.MEMCFG.TotalCapacity}");
                    _row++;
                    AddTiming("ProcODT", $"{App.systemInfo.MemProcODT}");
                    _row++;
                    AddTiming("RttNom", $"{App.systemInfo.MemRttNom}");
                    _row++;
                    AddTiming("RttWr", $"{App.systemInfo.MemRttWr}");
                    _row++;
                    AddTiming("RttPark", $"{App.systemInfo.MemRttPark}");
                    _row++;
                    AddTiming("ClkDrvStr", $"{App.systemInfo.MemClkDrvStren}");
                    _row++;
                    AddTiming("AddrCmdDrvStr", $"{App.systemInfo.MemAddrCmdDrvStren}");
                    _row++;
                    AddTiming("CsOdtDrvStr", $"{App.systemInfo.MemCsOdtCmdDrvStren}");
                    _row++;
                    AddTiming("CkeDrvStr", $"{App.systemInfo.MemCkeDrvStren}");
                    _row++;
                    AddTiming("AddrCmdSetup", $"{App.systemInfo.MemAddrCmdSetup}");
                    _row++;
                    AddTiming("CsOdtSetup", $"{App.systemInfo.MemCsOdtSetup}");
                    _row++;
                    AddTiming("CkeSetup", $"{App.systemInfo.MemCkeSetup}");

                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                    _row++;

                    Trace.WriteLine($"Finish {_header}");

                }

                if (App.CurrentRun.CPUFSBAvg > -999) AddDetailAvgMax(App.CurrentRun.CPUFSBAvg, App.CurrentRun.CPUFSBMax, $"CPU FSB Clock", " MHz", "0.00");
                if (App.CurrentRun.CPUCoresClocks.Any()) AddDetails(App.CurrentRun.CPUCoresClocks, $"Cores Clocks MHz [ Core - Average - Max ]");
                if (App.CurrentRun.CPUCoresEffClocks.Any()) AddDetails(App.CurrentRun.CPUCoresEffClocks, $"Cores Effective Clocks MHz [ Core - Average - Max ]");
                if (App.CurrentRun.CPUCoresStretch.Any()) AddDetails(App.CurrentRun.CPUCoresStretch, $"Cores Clock Stretching MHz [ Core - Average - Max ]");
                if (App.CurrentRun.CPUCoresTemps.Any()) AddDetails(App.CurrentRun.CPUCoresTemps, $"Cores Temps {degrees} [ Core - Average - Max ]");
                if (App.CurrentRun.CPUCoresPower.Any()) AddDetails(App.CurrentRun.CPUCoresPower, $"Cores Power Watt [ Core - Average - Max ]");
                if (App.CurrentRun.CPUCoresVoltages.Any()) AddDetails(App.CurrentRun.CPUCoresVoltages, $"Cores VIDs Volt [ Core - Average - Max ]");
                if (App.CurrentRun.CPULogicalsLoad.Any()) AddDetails(App.CurrentRun.CPULogicalsLoad, $"CPU Load % [ Thread - Average - Max ]");
                if (App.CurrentRun.CPUCoresC0.Any()) AddDetails(App.CurrentRun.CPUCoresC0, $"CPU C0 Time % [ Core - Average - Max ]");
                if (App.CurrentRun.CPUCoresScores.Any()) AddDetails(App.CurrentRun.CPUCoresScores, $"Cores Scores [ Core - Average - Max ]");
                if (App.CurrentRun.CPULogicalsScores.Any())
                {
                    string _header = $"Threads Scores [ Thread - Score ]";
                    if (App.CurrentRun.CPULogicalsScores.First().Val2 > -99998)
                        _header = $"Threads Scores [ Thread - Avg - Max ]";
                    AddDetails(App.CurrentRun.CPULogicalsScores, _header);
                }

                App.CurrentRun.MEMCFG = App.systemInfo.MEMCFG;
                App.CurrentRun.MemPartNumbers = App.systemInfo.MemPartNumbers;
                App.CurrentRun.modules = App.systemInfo.modules;

                if (App.CurrentRun.modules.Count > 0)
                {
                    string _header = $"Memory Modules Timings";
                    AddTimings(_header);
                }

                thiswin.SizeToContent = SizeToContent.WidthAndHeight;
                thiswin.UpdateLayout();

            }
            catch (Exception e)
            {
                Trace.WriteLine($"UpdateMonitoring2 Exception: {e}");
            }
        }
        public static BenchScore GetRunForThreads(List<BenchScore> _scoreRun, int _thrds, string _Benchname)
        {
            foreach (BenchScore _run in _scoreRun)
            {
                if (_run.Threads == _thrds) return _run;
            }
            return new BenchScore(_thrds, _Benchname);
        }

        public static void ScoresLayout(Grid ScoreList, List<BenchScore> scoreRun, int[] threads, string Benchname, string ConfigTag, string BenchScoreUnit) 
        {
            int _column = 0;

            Thickness dpadding = new Thickness(8, 0, 8, 0);

            Thickness smargin = new Thickness(2, 2, 2, 2);

            App.BenchScoreUnit = BenchScoreUnit;

            foreach (int _threads in threads)
            {
                BenchScore _run = GetRunForThreads(scoreRun, _threads, Benchname);
                _run.BenchScoreUnit = BenchScoreUnit;
                _run.Threads = _threads;
                _run.Benchname = Benchname;
                _run.ConfigTag = ConfigTag;
                scoreRun.Add(_run);
                int _row = 0;

                TextBlock _header = new TextBlock { FontSize = 16, Background = App.thrbgbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, Padding = dpadding };
                Grid.SetColumn(_header, _column);
                Grid.SetRow(_header, _row);
                _header.TextAlignment = TextAlignment.Center;
                _header.Inlines.Add(new Run { Text = $"{_threads}", FontWeight = FontWeights.Bold, Foreground = App.thrbrush1 });
                _header.Inlines.Add(new Run { Text = "t", FontWeight = FontWeights.Normal, Foreground = App.thrbrush2 });
                _header.Margin = App.thickness;
                ScoreList.Children.Add(_header);
                _row++;

                // SCORE

                StackPanel _pscorestack = new StackPanel { Margin = App.thickness, Background = App.whitebrush };
                _pscorestack.HorizontalAlignment = HorizontalAlignment.Stretch;
                _pscorestack.VerticalAlignment = VerticalAlignment.Stretch;
                Grid.SetColumn(_pscorestack, _column);
                Grid.SetRow(_pscorestack, _row);

                StackPanel _scorestack = new StackPanel { Background = App.whitebrush };
                _scorestack.HorizontalAlignment = HorizontalAlignment.Stretch;
                _scorestack.VerticalAlignment = VerticalAlignment.Stretch;
                _scorestack.Margin = smargin;
                _pscorestack.Children.Add(_scorestack);

                TextBlock _score = new TextBlock { FontSize = 20, Background = App.whitebrush, Foreground = App.scorebrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                _score.TextAlignment = TextAlignment.Center;
                _score.Inlines.Add(new Run { Text = "Queued", FontSize = 20, FontWeight = FontWeights.Bold });
                _scorestack.Children.Add(_score);
                _run.ScoreBox = _score;

                TextBlock _score2 = new TextBlock { FontSize = 14, Background = App.whitebrush, Foreground = App.scorebrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                _score2.TextAlignment = TextAlignment.Center;
                _scorestack.Children.Add(_score2);
                _score2.Visibility = Visibility.Collapsed;
                _run.ScoreBox2 = _score2;

                ScoreList.Children.Add(_pscorestack);

                _row++;

                /// CPU TEMP

                StackPanel _cputempstack = new StackPanel { Margin = App.thickness, Background = App.boxbrush1 };
                _cputempstack.HorizontalAlignment = HorizontalAlignment.Stretch;
                _cputempstack.VerticalAlignment = VerticalAlignment.Stretch;
                Grid.SetColumn(_cputempstack, _column);
                Grid.SetRow(_cputempstack, _row);

                Grid _cputempgrid = new Grid { Margin = App.thickness };
                _cputempgrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                _cputempgrid.VerticalAlignment = VerticalAlignment.Stretch;
                _run.CPUTempGrid = _cputempgrid;
                Grid.SetColumn(_cputempgrid, _column);
                Grid.SetRow(_cputempgrid, _row);

                TextBlock _cputemp = new TextBlock { FontSize = 14, Background = App.boxbrush1, Foreground = App.blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, Padding = dpadding };
                Grid.SetColumn(_cputemp, _column);
                Grid.SetRow(_cputemp, _row);
                _cputemp.TextAlignment = TextAlignment.Center;
                _cputemp.Margin = App.thickness;
                _cputemp.Text = "N/A";
                _cputempstack.Children.Add(_cputemp);
                _run.CPUTempBox = _cputemp;
                ScoreList.Children.Add(_cputempstack);
                _row++;

                /// CPU CLOCK

                StackPanel _cpuclockstack = new StackPanel { Margin = App.thickness, Background = App.boxbrush2 };
                _cpuclockstack.HorizontalAlignment = HorizontalAlignment.Stretch;
                _cpuclockstack.VerticalAlignment = VerticalAlignment.Stretch;
                Grid.SetColumn(_cpuclockstack, _column);
                Grid.SetRow(_cpuclockstack, _row);

                Grid _cpuclockgrid = new Grid { Margin = App.thickness };
                _cpuclockgrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                _cpuclockgrid.VerticalAlignment = VerticalAlignment.Stretch;
                _run.CPUClockGrid = _cpuclockgrid;
                Grid.SetColumn(_cpuclockgrid, _column);
                Grid.SetRow(_cpuclockgrid, _row);

                TextBlock _cpuclock = new TextBlock { FontSize = 14, Background = App.boxbrush2, Foreground = App.blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, Padding = dpadding };
                Grid.SetColumn(_cpuclock, _column);
                Grid.SetRow(_cpuclock, _row);
                _cpuclock.TextAlignment = TextAlignment.Center;
                _cpuclock.Margin = App.thickness;
                _cpuclock.Text = "N/A";
                _cpuclockstack.Children.Add(_cpuclock);
                _run.CPUClockBox = _cpuclock;
                ScoreList.Children.Add(_cpuclockstack);
                _row++;

                ///CPU POWER

                StackPanel _cpupowerstack = new StackPanel { Margin = App.thickness, Background = App.boxbrush1 };
                _cpupowerstack.HorizontalAlignment = HorizontalAlignment.Stretch;
                _cpupowerstack.VerticalAlignment = VerticalAlignment.Stretch;
                Grid.SetColumn(_cpupowerstack, _column);
                Grid.SetRow(_cpupowerstack, _row);

                Grid _cpupowergrid = new Grid { Margin = App.thickness };
                _cpupowergrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                _cpupowergrid.VerticalAlignment = VerticalAlignment.Stretch;
                _run.CPUPowerGrid = _cpupowergrid;
                Grid.SetColumn(_cpupowergrid, _column);
                Grid.SetRow(_cpupowergrid, _row);

                TextBlock _cpupowerblock = new TextBlock { FontSize = 14, Background = App.boxbrush1, Foreground = App.blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, Padding = dpadding };
                Grid.SetColumn(_cpupowerblock, _column);
                Grid.SetRow(_cpupowerblock, _row);
                _cpupowerblock.TextAlignment = TextAlignment.Center;
                _cpupowerblock.Margin = App.thickness;
                _cpupowerblock.Text = "N/A";
                _cpupowerstack.Children.Add(_cpupowerblock);
                _run.CPUPowerBox = _cpupowerblock;
                ScoreList.Children.Add(_cpupowerstack);
                _row++;

                /// ADDITIONAL

                StackPanel _additionalstack = new StackPanel { Margin = App.thickness, Background = App.boxbrush2 };
                _additionalstack.HorizontalAlignment = HorizontalAlignment.Stretch;
                _additionalstack.VerticalAlignment = VerticalAlignment.Stretch;
                Grid.SetColumn(_additionalstack, _column);
                Grid.SetRow(_additionalstack, _row);

                Grid _additionalgrid = new Grid { Margin = App.thickness };
                _additionalgrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                _additionalgrid.VerticalAlignment = VerticalAlignment.Stretch;
                _run.AdditionalGrid = _additionalgrid;
                Grid.SetColumn(_additionalgrid, _column);
                Grid.SetRow(_additionalgrid, _row);

                TextBlock _additional = new TextBlock { FontSize = 14, Background = App.boxbrush2, Foreground = App.blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, Padding = dpadding };
                Grid.SetColumn(_additional, _column);
                Grid.SetRow(_additional, _row);
                _additional.TextAlignment = TextAlignment.Center;
                _additional.Margin = App.thickness;
                _additional.Text = "N/A";
                _additionalstack.Children.Add(_additional);
                _run.AdditionalBox = _additional;
                ScoreList.Children.Add(_additionalstack);
                _row++;

                /// STARTED

                TextBlock _started = new TextBlock { FontSize = 14, Background = App.boxbrush1, Foreground = App.blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, Padding = dpadding };
                Grid.SetColumn(_started, _column);
                Grid.SetRow(_started, _row);
                _started.TextAlignment = TextAlignment.Center;
                _started.Margin = App.thickness;
                _started.Text = "N/A";
                ScoreList.Children.Add(_started);
                _run.StartedBox = _started;
                _row++;

                /// FINISHED

                TextBlock _finished = new TextBlock { FontSize = 14, Background = App.boxbrush2, Foreground = App.blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, Padding = dpadding };
                Grid.SetColumn(_finished, _column);
                Grid.SetRow(_finished, _row);
                _finished.TextAlignment = TextAlignment.Center;
                _finished.Margin = App.thickness;
                _finished.Text = "N/A";
                _finished.Visibility = Visibility.Visible;
                ScoreList.Children.Add(_finished);
                _run.FinishedBox = _finished;

                TextBlock _livefinished = new TextBlock { FontSize = 14, Background = App.boxbrush2, Foreground = App.blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, Padding = dpadding };
                Grid.SetColumn(_livefinished, _column);
                Grid.SetRow(_livefinished, _row);
                _livefinished.TextAlignment = TextAlignment.Center;
                _livefinished.Margin = App.thickness;
                _livefinished.Text = "N/A";
                _livefinished.Visibility = Visibility.Collapsed;
                ScoreList.Children.Add(_livefinished);
                _run.LiveFinishedBox = _livefinished;
                _row++;

                /// DETAILS

                Expander _detailsexp = new Expander { Header = "Details", IsExpanded = false, FontSize = 14, Foreground = App.detailsbrush, Padding = dpadding };
                Grid.SetColumn(_detailsexp, _column);
                Grid.SetRow(_detailsexp, _row);
                _detailsexp.Collapsed += new RoutedEventHandler(ScoreExpanderHasCollapsed);
                _detailsexp.Expanded += new RoutedEventHandler(ScoreExpanderHasExpanded);
                _detailsexp.HorizontalAlignment = HorizontalAlignment.Stretch;
                _detailsexp.VerticalAlignment = VerticalAlignment.Stretch;
                _detailsexp.Margin = App.thickness;
                _detailsexp.MinHeight = 30;
                _detailsexp.SetBinding(FrameworkElement.HeightProperty, "{Binding RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type Window}}, Path=ActualHeight}");

                StackPanel _detailspstack = new StackPanel { Margin = App.thickness, Background = App.boxbrush1 };
                _detailspstack.HorizontalAlignment = HorizontalAlignment.Stretch;
                _detailspstack.Visibility = Visibility.Visible;

                StackPanel _detailsstack = new StackPanel { Margin = App.thickness, Background = App.boxbrush1 };
                _detailsstack.HorizontalAlignment = HorizontalAlignment.Center;
                _detailsstack.Visibility = Visibility.Collapsed;
                _detailsstack.SetBinding(FrameworkElement.HeightProperty, "{Binding RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type Expander}}, Path=ActualHeight}");

                Grid _detailsgrid = new Grid { Margin = App.thickness };
                _detailsgrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                _detailsgrid.VerticalAlignment = VerticalAlignment.Stretch;
                _detailsgrid.Visibility = Visibility.Collapsed;
                //_detailsgrid.SetBinding(FrameworkElement.HeightProperty, "{Binding RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type StackPanel}}, Path=ActualHeight}");
                _detailsgrid.ShowGridLines = false;

                ScrollViewer _scroller = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden, MinHeight = 100 };
                _scroller.Visibility = Visibility.Collapsed;
                _scroller.Tag = "Details";
                _scroller.SetBinding(FrameworkElement.HeightProperty, "{Binding RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type StackPanel}}, Path=ActualHeight}");
                /*
                Binding bindW = new Binding("MinWidth");
                bindW.Mode = BindingMode.OneWay;
                bindW.Source = App.scoreMinWidth;
                BindingOperations.SetBinding(_score, TextBlock.MinWidthProperty, bindW);
                */
                TextBlock _details = new TextBlock { FontSize = 14, Background = App.boxbrush1, Foreground = App.blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, Padding = dpadding };
                _details.TextAlignment = TextAlignment.Center;
                _details.Text = "N/A";
                _details.TextWrapping = TextWrapping.Wrap;
                _run.DetailsBox = _details;

                _run.DetailsExpander = _detailsexp;
                _run.DetailsPanel = _detailsstack;
                _run.DetailsPPanel = _detailspstack;
                _run.DetailsGrid = _detailsgrid;
                _run.DetailsScroller = _scroller;
                _scroller.Content = _detailsstack;
                _detailsstack.Children.Add(_detailsgrid);
                _detailspstack.Children.Add(_details);
                _detailspstack.Children.Add(_scroller);
                _detailsexp.Content = _detailspstack;
                ScoreList.Children.Add(_detailsexp);

                _row++;

                _column++;
            }
        }
        private static void ScoreExpanderHasCollapsed(object sender, RoutedEventArgs args)
        {
            Expander _sender = sender as Expander;
            IEnumerable<Window> pwins = MainWindow.FindVisualParent<Window>(_sender);
            Window pwin = pwins.FirstOrDefault();
            IEnumerable<Expander> expanders = MainWindow.FindVisualChildren<Expander>(pwin);
            foreach (Expander exp in expanders)
            {
                exp.IsExpanded = false;
                /*
                IEnumerable<ScrollViewer> svs = MainWindow.FindVisualChildren<ScrollViewer>(exp);
                foreach (ScrollViewer sv in svs)
                {
                    sv.Visibility = Visibility.Collapsed;
                }
                */
            }
            pwin.SizeToContent = SizeToContent.WidthAndHeight;
            pwin.UpdateLayout();
        }
        private static void ScoreExpanderHasExpanded(object sender, RoutedEventArgs args)
        {
            Expander _sender = sender as Expander;
            IEnumerable<Window> pwins = MainWindow.FindVisualParent<Window>(_sender);
            Window pwin = pwins.FirstOrDefault();
            IEnumerable<Expander> expanders = MainWindow.FindVisualChildren<Expander>(pwin);
            pwin.Height = pwin.MaxHeight;
            foreach (Expander exp in expanders)
            {
                exp.IsExpanded = true;
            }
            IEnumerable<ScrollViewer> elements = MainWindow.FindVisualChildren<ScrollViewer>(_sender).Where(x => x.Tag != null && x.Tag.ToString().StartsWith("Details"));
            if (elements.Any())
            {
                ScrollViewer sv = elements.FirstOrDefault();
                double _scrollmh = pwin.MaxHeight - sv.TranslatePoint(new System.Windows.Point(0, 0), null).Y;
                double _tsh = sv.ExtentHeight;
                double svHeight = 0;
                if (sv.Visibility == Visibility.Visible && _tsh < _scrollmh )
                {
                    svHeight = _tsh - 8;
                }
                else
                {
                    svHeight = _scrollmh - 8;
                }
                sv.Height = svHeight > 0 ? svHeight : 0;
                sv.ClearValue(ScrollViewer.MinWidthProperty);
                sv.ClearValue(ScrollViewer.MaxWidthProperty);
                sv.ClearValue(ScrollViewer.WidthProperty);
                //sv.MinWidth = sv.ExtentWidth + 16;
                //sv.Width = sv.ExtentWidth + 48;
                //Trace.WriteLine($"exp_scroller aH={sv.ActualHeight} eH={sv.ExtentHeight} vH={sv.ViewportHeight} sH={sv.ScrollableHeight}");
                //Trace.WriteLine($"ScVis {sv.ComputedVerticalScrollBarVisibility}");
            }
            pwin.SizeToContent = SizeToContent.WidthAndHeight;
            pwin.UpdateLayout();
        }
        public static void UpdateScore2(string _score)
        {
            try
            {
                if (App.CurrentRun.ScoreBox != null)
                {
                    TextBlock _textblock = App.CurrentRun.ScoreBox;
                    _textblock.Text = "";
                    if (App.CurrentRun.ScoreUnit.Length > 0)
                    {
                        _score = $"{Math.Round((decimal)App.CurrentRun.Score, 2)}";
                        _textblock.Inlines.Add(new Run { Text = $"{_score}", FontSize = 20, FontWeight = FontWeights.Bold, Foreground = App.scorebrush });
                        _textblock.Inlines.Add(new Run { Text = $"  {App.CurrentRun.ScoreUnit}", FontSize = 14, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                    }
                    else
                    {
                        _textblock.Inlines.Add(new Run { Text = $"{_score}", FontSize = 20, FontWeight = FontWeights.Bold, Foreground = App.scorebrush });
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"UpdateScore2 Exception: {ex}");
            }
        }
        public static void UpdateFinished2(string _exitstatus)
        {
            try
            {
                if (App.CurrentRun.FinishedBox != null)
                {
                    TextBlock _textblock = App.CurrentRun.FinishedBox;
                    _textblock.FontSize = 12;
                    if (App.CurrentRun.Finished == DateTime.MinValue) App.CurrentRun.Finished = DateTime.Now;
                    _textblock.Text = $"Finish: {App.CurrentRun.Finished}";
                    if (_exitstatus.Length > 0)
                    {
                        _textblock.Inlines.Add(new LineBreak { });
                        _textblock.Inlines.Add(new Run { Text = _exitstatus, FontSize = 15, FontWeight = FontWeights.Bold, Foreground = App.maxbrush });
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"UpdateFinished2 Exception: {ex}");
            }
        }
        public static void UpdateHeadersWidth2(Window thisWindow, List<BenchScore> scoreRun)
        {
            try
            {
                double _minwidth = 0;
                foreach (BenchScore score in scoreRun)
                {
                    if (score.ScoreBox.ActualWidth > _minwidth) _minwidth = score.ScoreBox.ActualWidth;
                }
                foreach (BenchScore score in scoreRun)
                {
                    if (score.ScoreBox.ActualWidth < _minwidth)
                    {
                        thisWindow.Dispatcher.BeginInvoke(new Action(() => { score.ScoreBox.MinWidth = _minwidth; score.ScoreBox.Width = _minwidth; thisWindow.UpdateLayout(); }), DispatcherPriority.Normal);                      
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"UpdateHeadersWidth2 Exception: {ex}");
            }
        }

        public static void UpdateStarted2()
        {
            try
            {
                if (App.CurrentRun.StartedBox != null)
                {
                    TextBlock _textblock = App.CurrentRun.StartedBox;
                    _textblock.FontSize = 12;
                    _textblock.Text = $"Started: {App.CurrentRun.Started}";
                    if (App.CurrentRun.StartedTemp > -999) _textblock.Text += $" @ {App.CurrentRun.StartedTemp}°C";
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"UpdateFinished2 Exception: {ex}");
            }
        }
        public static void SetLiveBindings2(BenchScore _scoreRun, bool enabled)
        {
            try
            {
                if (enabled)
                {
                    //CPU TEMP
                    Binding bindLive = new Binding("LiveCPUTemp");
                    bindLive.Mode = BindingMode.OneWay;
                    bindLive.Source = App.systemInfo;
                    BindingOperations.SetBinding(_scoreRun.CPUTempBox, TextBlock.TextProperty, bindLive);
                    //CPU CLOCK
                    bindLive = new Binding("LiveCPUClock");
                    bindLive.Mode = BindingMode.OneWay;
                    bindLive.Source = App.systemInfo;
                    BindingOperations.SetBinding(_scoreRun.CPUClockBox, TextBlock.TextProperty, bindLive);
                    //CPU vCORE Power
                    bindLive = new Binding("LiveCPUPower");
                    bindLive.Mode = BindingMode.OneWay;
                    bindLive.Source = App.systemInfo;
                    BindingOperations.SetBinding(_scoreRun.CPUPowerBox, TextBlock.TextProperty, bindLive);
                    //CPU Additional
                    bindLive = new Binding("LiveCPUAdditional");
                    bindLive.Mode = BindingMode.OneWay;
                    bindLive.Source = App.systemInfo;
                    BindingOperations.SetBinding(_scoreRun.AdditionalBox, TextBlock.TextProperty, bindLive);
                    //Finished
                    bindLive = new Binding("LiveFinished");
                    bindLive.Mode = BindingMode.OneWay;
                    bindLive.Source = App.systemInfo;
                    BindingOperations.SetBinding(_scoreRun.LiveFinishedBox, TextBlock.TextProperty, bindLive);
                    _scoreRun.CPUTempBox.FontWeight = FontWeights.SemiBold;
                    _scoreRun.CPUClockBox.FontWeight = FontWeights.SemiBold;
                    _scoreRun.CPUPowerBox.FontWeight = FontWeights.SemiBold;
                    _scoreRun.AdditionalBox.FontWeight = FontWeights.SemiBold;
                    _scoreRun.CPUTempBox.Foreground = App.tempbrush;
                    _scoreRun.CPUClockBox.Foreground = App.clockbrush1;
                    _scoreRun.CPUPowerBox.Foreground = App.powerbrush;
                    _scoreRun.AdditionalBox.Foreground = App.voltbrush;
                    _scoreRun.FinishedBox.Foreground = App.blackbrush;
                    _scoreRun.FinishedBox.Visibility = Visibility.Collapsed;
                    _scoreRun.LiveFinishedBox.Visibility = Visibility.Visible;
                }
                else
                {
                    Trace.WriteLine("CLEAR BINDINGS");
                    BindingOperations.ClearBinding(_scoreRun.CPUTempBox, TextBlock.TextProperty);
                    BindingOperations.ClearBinding(_scoreRun.CPUClockBox, TextBlock.TextProperty);
                    BindingOperations.ClearBinding(_scoreRun.CPUPowerBox, TextBlock.TextProperty);
                    BindingOperations.ClearBinding(_scoreRun.AdditionalBox, TextBlock.TextProperty);
                    BindingOperations.ClearBinding(_scoreRun.LiveFinishedBox, TextBlock.TextProperty);
                    _scoreRun.CPUTempBox.FontWeight = FontWeights.Normal;
                    _scoreRun.CPUClockBox.FontWeight = FontWeights.Normal;
                    _scoreRun.CPUPowerBox.FontWeight = FontWeights.Normal;
                    _scoreRun.AdditionalBox.FontWeight = FontWeights.Normal;
                    _scoreRun.CPUTempBox.Foreground = App.blackbrush;
                    _scoreRun.CPUClockBox.Foreground = App.blackbrush;
                    _scoreRun.CPUPowerBox.Foreground = App.blackbrush;
                    _scoreRun.AdditionalBox.Foreground = App.blackbrush;
                    _scoreRun.FinishedBox.Foreground = App.blackbrush;
                    _scoreRun.CPUTempBox.Text = "N/A";
                    _scoreRun.CPUClockBox.Text = "N/A";
                    _scoreRun.CPUPowerBox.Text = "N/A";
                    _scoreRun.AdditionalBox.Text = "N/A";
                    _scoreRun.FinishedBox.Visibility = Visibility.Visible;
                    _scoreRun.LiveFinishedBox.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SetLiveBindings[{enabled}] Exception: {ex}");
            }
        }

        public static void ButtonScreenshot_Click2(object sender, RoutedEventArgs e, Window thisWindow, string WinTitle)
        {

            App.bscreenshot = true;
            App.bscreenshotdetails = false;

            IEnumerable<ScrollViewer> elements = MainWindow.FindVisualChildren<ScrollViewer>(thisWindow).Where(x => x.Tag != null && x.Tag.ToString().StartsWith("Details"));
            double curheigth = thisWindow.ActualHeight;
            double curmaxheigth = thisWindow.MaxHeight;
            double curminheigth = thisWindow.MinHeight;
            double curwidth = thisWindow.ActualWidth;
            double curmaxwidth = thisWindow.MaxWidth;
            double curminwidth = thisWindow.MinWidth;
            double maxscrollableheight = 0;

            IEnumerable<Expander> expanders = MainWindow.FindVisualChildren<Expander>(thisWindow);

            DispatcherOperation operation;
            double _scrollbase = 0;

            if (expanders.Any())
            {
                foreach (Expander exp in expanders)
                {
                    if (exp.IsExpanded) App.bscreenshotdetails = true;
                }
            }

            if (elements.Any() && App.bscreenshotdetails)
            {
                if (thisWindow.WindowState == WindowState.Maximized) thisWindow.WindowState = WindowState.Normal;

                foreach (var sv in elements)
                {
                    _scrollbase = sv.TranslatePoint(new System.Windows.Point(0, 0), null).Y;
                    double svHeight = sv.ExtentHeight + 16;
                    maxscrollableheight = svHeight > maxscrollableheight ? svHeight : maxscrollableheight;
                    //sv.Height = svHeight;
                    //sv.Width = svWidth;
                    //sv.Height = svHeight > 0 ? svHeight : 0;
                    Trace.WriteLine($"_svH {svHeight} svaH {sv.ActualHeight} max {maxscrollableheight}");
                    Trace.WriteLine($"aH={sv.ActualHeight} eH={sv.ExtentHeight} vH={sv.ViewportHeight} sH={sv.ScrollableHeight}");
                    Trace.WriteLine($"exp_scroller aH={sv.ActualHeight} eH={sv.ExtentHeight} vH={sv.ViewportHeight} sH={sv.ScrollableHeight}");
                    thisWindow.SizeToContent = SizeToContent.WidthAndHeight;
                }

                //Trace.WriteLine($"Sshot 0 ah={thisWindow.ActualHeight} aMh={thisWindow.MaxHeight}");

                thisWindow.MaxHeight = maxscrollableheight + _scrollbase + 256;
                thisWindow.MinHeight = maxscrollableheight + _scrollbase;
                thisWindow.Height = maxscrollableheight + _scrollbase;
                App.screenshotminheigth = maxscrollableheight + _scrollbase;
                
                foreach (var sv in elements)
                {
                    _scrollbase = sv.TranslatePoint(new System.Windows.Point(0, 0), null).Y;
                    double svWidth = sv.ExtentWidth + 16;
                    double svHeight = sv.ExtentHeight + 16;
                    maxscrollableheight = svHeight > maxscrollableheight ? svHeight : maxscrollableheight;
                    sv.Height = svHeight;
                    sv.Width = svWidth;
                    sv.MinHeight = svHeight;
                    sv.MinWidth = svWidth;
                    //sv.InvalidateVisual();
                    //sv.Height = svHeight > 0 ? svHeight : 0;
                    Trace.WriteLine($"_svH {svHeight} svaH {sv.ActualHeight} max {maxscrollableheight}");
                    Trace.WriteLine($"aH={sv.ActualHeight} eH={sv.ExtentHeight} vH={sv.ViewportHeight} sH={sv.ScrollableHeight}");
                    Trace.WriteLine($"exp_scroller aH={sv.ActualHeight} eH={sv.ExtentHeight} vH={sv.ViewportHeight} sH={sv.ScrollableHeight}");
                }
                thisWindow.SizeToContent = SizeToContent.WidthAndHeight;

                thisWindow.MaxWidth = thisWindow.ActualWidth + 256;
                thisWindow.MinWidth = thisWindow.ActualWidth;


                //Trace.WriteLine($"Sshot 1 ah={thisWindow.ActualHeight} rH={maxscrollableheight + _scrollbase} aMh={thisWindow.MaxHeight}");


                thisWindow.Dispatcher.Invoke(new Action(() => { thisWindow.UpdateLayout(); }), DispatcherPriority.Send);

                //Trace.WriteLine($"Sshot 2 ah={thisWindow.ActualHeight} rH={maxscrollableheight + _scrollbase} aMh={thisWindow.MaxHeight}");

                
                App.screenshotwin = thisWindow;

            }
            thisWindow.Focusable = false;

            if (App.bscreenshotdetails)
            {
                UIntPtr hWnd = App.screenshot.GetActiveWindow();
                using (PleaseWait waitWnd = new PleaseWait(curheigth))
                {
                    Interlocked.CompareExchange(ref App.bscreenshotrendered, 0, 1);
                    waitWnd.Owner = thisWindow;
                    thisWindow.Dispatcher.BeginInvoke(new Action(() => { App.screenshot = new Screenshot(); App.bitmap = App.screenshot.CaptureThisWindow(hWnd); }), DispatcherPriority.Normal);
                    waitWnd.ShowDialog();
                    waitWnd.Focus();
                }
            }
            else
            {
                thisWindow.Dispatcher.Invoke(new Action(() => { App.screenshot = new Screenshot(); App.bitmap = App.screenshot.CaptureActiveWindow(); }), DispatcherPriority.Normal);
            }
            thisWindow.Focusable = true;

            App.ss_filename = DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss_") + WinTitle + ".png";

            if (App.bscreenshotdetails)
            {
                thisWindow.MinHeight = curminheigth;
                thisWindow.MaxHeight = curmaxheigth;
                thisWindow.Height = curheigth;

                thisWindow.MaxWidth = curmaxwidth;
                thisWindow.MinWidth = curminwidth;
                thisWindow.Width = curwidth;

                foreach (var sv in elements)
                {
                    double _scrollh = curheigth - sv.TranslatePoint(new Point(0, 0), null).Y;
                    double _scrollmh = curmaxheigth - sv.TranslatePoint(new Point(0, 0), null).Y;
                    double _scrollth = sv.TranslatePoint(new Point(0, 0), null).Y;
                    double _tsh = sv.ExtentHeight;
                    double svHeight = 0;
                    if (_scrollh >= _tsh - 8)
                    {
                        svHeight = _tsh - 8;
                    }
                    else if (_scrollh >= _scrollmh - 8)
                    {
                        svHeight = _scrollmh - 8;
                    }
                    else
                    {
                        svHeight = _scrollh - 8;
                    }
                    Trace.WriteLine($" svHeight={svHeight} _scrollh={_scrollh} _scrollmh={_scrollmh} _scrollth={_scrollth}");
                    sv.Height = svHeight > 0 ? svHeight : 0;
                    sv.MinHeight = 32;
                    sv.MinWidth = 32;
                    sv.InvalidateVisual();
                    sv.InvalidateScrollInfo();


                    Trace.WriteLine($" aH={sv.ActualHeight} sH={_scrollh} tH={_scrollth}");
                    Trace.WriteLine($" tsh={_tsh} eH={sv.ExtentHeight} scH={sv.ViewportHeight}");

                    operation = thisWindow.Dispatcher.BeginInvoke(new Action(() => { thisWindow.UpdateLayout(); }), DispatcherPriority.Normal);

                    operation.Wait();

                }

                thisWindow.SizeToContent = SizeToContent.WidthAndHeight;
            }

            operation = thisWindow.Dispatcher.BeginInvoke(new Action(() => { thisWindow.UpdateLayout(); }), DispatcherPriority.Normal);

            operation.Wait();

            if (operation.Status != DispatcherOperationStatus.Completed) Trace.WriteLine($"Operation not completed: {operation.Status}");
            

            using (var saveWnd = new SaveWindow(App.bitmap))
            {
                saveWnd.Owner = thisWindow;
                saveWnd.ShowDialog();
                App.screenshot.Dispose();
            }

            App.bscreenshot = false;
            App.bscreenshotdetails = false;
            App.screenshotwin = null;

        }

    }
}
