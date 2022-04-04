using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace BenchMaestro
{
    
    //XMRSTAKRX
    //CPUMINER
    static class BenchModule1
    {
        public static void UpdateMonitoring2(Grid ScoreList)
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

                /// TEMP

                TextBlock _textblock = App.CurrentRun.CPUTempBox;
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
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUAvgTemp, 1)}", FontSize = 17, FontWeight = FontWeights.Bold, Foreground = App.tempbrush });
                    _tb1a.Inlines.Add(new Run { Text = $" {degrees} ", FontSize = 14, FontWeight = FontWeights.Normal, Foreground = App.tempbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CPUMaxTemp, 1)} °C", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
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
                    _tb1a.Inlines.Add(new Run { Text = $"Cores: ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.tempbrush });
                    _tb1a.Inlines.Add(new Run { Text = $" {degrees} ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.tempbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CoresMaxTemp, 1)} °C", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
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

                    _value = App.CurrentRun.CPUAvgStretch > -99999 ? App.CurrentRun.CPUAvgStretch.ToString() : "N/D";
                    _value = (App.CurrentRun.CPUAvgStretch > 1)? App.CurrentRun.CPUAvgStretch.ToString() : "None";

                    TextBlock _tb3a = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb3a.Inlines.Add(new Run { Text = $"{avgchar} Stretch: ", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                    _tb3a.Inlines.Add(new Run { Text = $"{_value}", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = App.blackbrush });
                    if (_value != "None") _tb3a.Inlines.Add(new Run { Text = " MHz", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = App.blackbrush });
                    Grid.SetColumn(_tb3a, 0);
                    Grid.SetRow(_tb3a, _row);
                    _tb3a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb3a);

                    _value = App.CurrentRun.CPUMaxStretch > -99999 ? App.CurrentRun.CPUMaxStretch.ToString() : "N/D";
                    _value = (App.CurrentRun.CPUMaxStretch > 1) ? App.CurrentRun.CPUMaxStretch.ToString() : "None";

                    TextBlock _tb3b = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb3b.Inlines.Add(new Run { Text = $" {maxchar} Stretch: ", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.blackbrush });
                    _tb3b.Inlines.Add(new Run { Text = $"{_value}", FontSize = 9, FontWeight = FontWeights.Bold, Foreground = App.blackbrush });
                    if (_value != "None") _tb3b.Inlines.Add(new Run { Text = " MHz", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = App.blackbrush });
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
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUAvgPower, 1)}", FontSize = 17, FontWeight = FontWeights.Bold, Foreground = App.powerbrush });
                    _tb1a.Inlines.Add(new Run { Text = " W ", FontSize = 14, FontWeight = FontWeights.Normal, Foreground = App.powerbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    if (App.CurrentRun.CPUMaxPower > 0)
                    {
                        TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                        _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CPUMaxPower, 1)} W", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
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
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CoresAvgPower, 1)}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = App.powerbrush });
                    _tb1a.Inlines.Add(new Run { Text = " W ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.powerbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CoresMaxPower, 1)} W", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
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
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUAvgVoltage, 3)}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = App.voltbrush });
                    _tb1a.Inlines.Add(new Run { Text = " V ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.voltbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CPUMaxVoltage, 3)} V", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
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
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CoresAvgVoltage, 3)}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = App.voltbrush });
                    _tb1a.Inlines.Add(new Run { Text = " V ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.voltbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CoresMaxVoltage, 3)} V", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
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
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.SOCAvgVoltage, 3)}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = App.voltbrush });
                    _tb1a.Inlines.Add(new Run { Text = " V ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.voltbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.SOCMaxVoltage, 3)} V", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
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
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUSAAvgVoltage, 3)}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = App.voltbrush });
                    _tb1a.Inlines.Add(new Run { Text = " V ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.voltbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CPUSAMaxVoltage, 3)} V", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
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
                    _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUIOAvgVoltage, 3)}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = App.voltbrush });
                    _tb1a.Inlines.Add(new Run { Text = " V ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = App.voltbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CPUIOMaxVoltage, 3)} V", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
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
                    _tb1b.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CCDSAvgTemp, 1)} {degrees}", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
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
                    _tb1a.Inlines.Add(new Run { Text = $"CCD1: {Math.Round(App.CurrentRun.CCD1AvgTemp, 1)}", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    _tb1a.Inlines.Add(new Run { Text = $" {degrees} ", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CCD1MaxTemp, 1)} {degrees}", FontSize = 11, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
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
                    _tb1a.Inlines.Add(new Run { Text = $"CCD2: {Math.Round(App.CurrentRun.CCD2AvgTemp, 1)}", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    _tb1a.Inlines.Add(new Run { Text = $" {degrees} ", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CCD2MaxTemp, 1)} {degrees}", FontSize = 11, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
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
                    _tb1a.Inlines.Add(new Run { Text = $" L3: {Math.Round(App.CurrentRun.L3AvgTemp, 1)}", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    _tb1a.Inlines.Add(new Run { Text = $" {degrees} ", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
                    Grid.SetColumn(_tb1a, 0);
                    Grid.SetRow(_tb1a, _row);
                    _tb1a.TextAlignment = TextAlignment.Right;
                    _gridblock.Children.Add(_tb1a);

                    TextBlock _tb1b = new TextBlock { Background = App.boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                    _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.L3MaxTemp, 1)} {degrees}", FontSize = 11, FontWeight = FontWeights.Normal, Foreground = App.maxbrush });
                    Grid.SetColumn(_tb1b, 1);
                    Grid.SetRow(_tb1b, _row);
                    _tb1b.TextAlignment = TextAlignment.Left;
                    _gridblock.Children.Add(_tb1b);

                    _row++;
                }

                if (App.CurrentRun.CPUPPTAvg > -999)
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
                    _tb1b.Inlines.Add(new Run { Text = $" {maxchar} {Math.Round(App.CurrentRun.CPUPPTMax, 0)}W", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
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

                if (App.CurrentRun.CPUTDCAvg > -999)
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
                    _tb1b.Inlines.Add(new Run { Text = $" {maxchar} {Math.Round(App.CurrentRun.CPUTDCMax, 0)}A", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
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

                if (App.CurrentRun.CPUEDCAvg > -999)
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
                    _tb1b.Inlines.Add(new Run { Text = $" {maxchar} {Math.Round(App.CurrentRun.CPUEDCMax, 0)}A", FontSize = 10, FontWeight = FontWeights.Normal, Foreground = App.additionbrush });
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

                _row = 0;

                void AddDetails(List<DetailsGrid> _thislist, string _header)
                {

                    Trace.WriteLine($"Start {_header}");

                    Thickness dthickness = new Thickness(4, 3, 4, 3);
                    GridLength _rowheigth = new GridLength(1, GridUnitType.Star);

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
                        int _index = 1;
                        int _col = 0;
                        _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                        int _fcount = _thislist.Where(x => x.Val1 > -99999 || x.Val2 > -99999).Count();

                        if (_fcount == 1) _colspan = 2;

                        foreach (DetailsGrid _item in _thislist.Where(x => x.Val1 > -99999 || x.Val2 > -99999))
                        {
                            Trace.WriteLine($"{_header} {_item.Label} {_item.Val1} {_item.Val2}");

                            string Label = _item.Label.ToString();
                            string Val1 = String.Format("{0:" + _item.Format + "}", _item.Val1.ToString());
                            string Val2 = String.Format("{0:" + _item.Format + "}", _item.Val2.ToString());
                            if (Val1 == "-99999") Val1 = "N/A";
                            if (Val2 == "-99999") Val2 = "N/A";
                            FontWeight _weight = FontWeights.Normal;
                            if (_item.Bold) _weight = FontWeights.Bold;
                            Trace.WriteLine($"{Label} {Val1} {Val2}");

                            TextBlock _tb1a = new TextBlock { Margin = dthickness, Background = App.boxbrush1, HorizontalAlignment = HorizontalAlignment.Stretch };
                            _tb1a.Inlines.Add(new Run { Text = Label, FontSize = 9, FontWeight = _weight, Foreground = App.blackbrush });
                            Grid.SetColumn(_tb1a, _col);
                            Grid.SetRow(_tb1a, _row);
                            Grid.SetColumnSpan(_tb1a, _colspan);
                            _tb1a.TextAlignment = TextAlignment.Left;
                            _gridblock.Children.Add(_tb1a);

                            _col++;
                            if (_fcount == 1) _col++;

                            TextBlock _tb1b = new TextBlock { Margin = dthickness, Background = App.boxbrush1, HorizontalAlignment = HorizontalAlignment.Stretch };
                            _tb1b.Inlines.Add(new Run { Text = Val1, FontSize = 9, FontWeight = _weight, Foreground = App.blackbrush });
                            Grid.SetColumn(_tb1b, _col);
                            Grid.SetRow(_tb1b, _row);
                            Grid.SetColumnSpan(_tb1b, _colspan);
                            _tb1b.TextAlignment = TextAlignment.Right;
                            _gridblock.Children.Add(_tb1b);

                            _col++;
                            if (_fcount == 1) _col++;

                            TextBlock _tb1c = new TextBlock { Margin = dthickness, Background = App.boxbrush1, HorizontalAlignment = HorizontalAlignment.Stretch };
                            _tb1c.Inlines.Add(new Run { Text = Val2, FontSize = 9, FontWeight = _weight, Foreground = App.blackbrush });
                            Grid.SetColumn(_tb1c, _col);
                            Grid.SetRow(_tb1c, _row);
                            Grid.SetColumnSpan(_tb1c, _colspan);
                            _tb1c.TextAlignment = TextAlignment.Right;
                            _gridblock.Children.Add(_tb1c);

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
                        if (_index == 0) _tbh.Visibility = Visibility.Collapsed;
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

        public static void ScoresLayout(Grid ScoreList, List<BenchScore> scoreRun, int[] threads, string Benchname, string ConfigTag) {

            int _column = 0;

            foreach (int _threads in threads)
            {
                BenchScore _run = GetRunForThreads(scoreRun, _threads, Benchname);
                _run.Threads = _threads;
                _run.Benchname = Benchname;
                _run.ConfigTag = ConfigTag;
                scoreRun.Add(_run);
                int _row = 0;

                TextBlock _header = new TextBlock { FontSize = 16, Background = App.thrbgbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                Grid.SetColumn(_header, _column);
                Grid.SetRow(_header, _row);
                _header.TextAlignment = TextAlignment.Center;
                _header.Inlines.Add(new Run { Text = $"{_threads}", FontWeight = FontWeights.Bold, Foreground = App.thrbrush1 });
                _header.Inlines.Add(new Run { Text = "t", FontWeight = FontWeights.Normal, Foreground = App.thrbrush2 });
                _header.Margin = App.thickness;
                ScoreList.Children.Add(_header);
                _row++;

                TextBlock _score = new TextBlock { FontSize = 20, Background = App.whitebrush, Foreground = App.scorebrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                Grid.SetColumn(_score, _column);
                Grid.SetRow(_score, _row);
                _score.TextAlignment = TextAlignment.Center;
                _score.Inlines.Add(new Run { Text = "Queued", FontSize = 20, FontWeight = FontWeights.Bold });
                _score.Margin = App.thickness;
                ScoreList.Children.Add(_score);
                _run.ScoreBox = _score;
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

                TextBlock _cputemp = new TextBlock { FontSize = 14, Background = App.boxbrush1, Foreground = App.blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
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

                TextBlock _cpuclock = new TextBlock { FontSize = 14, Background = App.boxbrush2, Foreground = App.blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
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

                TextBlock _cpupowerblock = new TextBlock { FontSize = 14, Background = App.boxbrush1, Foreground = App.blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
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

                TextBlock _additional = new TextBlock { FontSize = 14, Background = App.boxbrush2, Foreground = App.blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
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

                TextBlock _started = new TextBlock { FontSize = 14, Background = App.boxbrush1, Foreground = App.blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                Grid.SetColumn(_started, _column);
                Grid.SetRow(_started, _row);
                _started.TextAlignment = TextAlignment.Center;
                _started.Margin = App.thickness;
                _started.Text = "N/A";
                ScoreList.Children.Add(_started);
                _run.StartedBox = _started;
                _row++;

                /// FINISHED

                TextBlock _finished = new TextBlock { FontSize = 14, Background = App.boxbrush2, Foreground = App.blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                Grid.SetColumn(_finished, _column);
                Grid.SetRow(_finished, _row);
                _finished.TextAlignment = TextAlignment.Center;
                _finished.Margin = App.thickness;
                _finished.Text = "N/A";
                ScoreList.Children.Add(_finished);
                _run.FinishedBox = _finished;
                _row++;

                /// DETAILS

                Expander _detailsexp = new Expander { Header = "Details", IsExpanded = false, FontSize = 14, Foreground = App.detailsbrush };
                Grid.SetColumn(_detailsexp, _column);
                Grid.SetRow(_detailsexp, _row);
                _detailsexp.HorizontalAlignment = HorizontalAlignment.Stretch;
                _detailsexp.VerticalAlignment = VerticalAlignment.Stretch;
                _detailsexp.Margin = App.thickness;
                _detailsexp.MinHeight = 30;

                StackPanel _detailspstack = new StackPanel { Margin = App.thickness, Background = App.boxbrush1 };
                _detailspstack.HorizontalAlignment = HorizontalAlignment.Stretch;
                _detailspstack.Visibility = Visibility.Visible;

                StackPanel _detailsstack = new StackPanel { Margin = App.thickness, Background = App.boxbrush1 };
                _detailsstack.HorizontalAlignment = HorizontalAlignment.Center;
                _detailsstack.Visibility = Visibility.Collapsed;
                _detailsstack.SetBinding(FrameworkElement.HeightProperty, "{Binding RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type ScrollViewer}}, Path=ActualHeight}");

                Grid _detailsgrid = new Grid { Margin = App.thickness };
                _detailsgrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                _detailsgrid.VerticalAlignment = VerticalAlignment.Stretch;
                _detailsgrid.Visibility = Visibility.Collapsed;
                _detailsgrid.SetBinding(FrameworkElement.HeightProperty, "{Binding RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type StackPanel}}, Path=ActualHeight}");
                _detailsgrid.ShowGridLines = false;

                ScrollViewer _scroller = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Visible, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden, MinHeight = 100 };
                _scroller.Visibility = Visibility.Collapsed;
                _scroller.Tag = "Details";
                _scroller.SetBinding(FrameworkElement.HeightProperty, "{Binding RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type StackPanel}}, Path=ActualHeight}");

                TextBlock _details = new TextBlock { FontSize = 14, Background = App.boxbrush1, Foreground = App.blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch };
                _details.TextAlignment = TextAlignment.Center;
                _details.Text = "N/A";
                _details.TextWrapping = TextWrapping.Wrap;
                _run.DetailsBox = _details;

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
    }
}
