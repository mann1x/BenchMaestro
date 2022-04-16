using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace BenchMaestro
{
    //XMRSTAKRX
    using Module1 = BenchModule1;
    class BenchModule2_XMRStakRx
    {
        public static void UpdateProgress2(int value, ProgressBar ProgressBar)
        {
            //App.CurrentRun.ProgressBar = $"Running benchmark iteration {BenchCurrentIteration}/{BenchIterations}";

            if (value >= 0)
            {
                ProgressBar.Value = value;
            }
            else
            {
                TimeSpan _runningspan = DateTime.Now - App.TSRunStart;

                int _projectedruntime = (App.IterationPretime + App.IterationRuntime + App.IterationPostime + 2) * App.BenchIterations;

                DateTime _endingts = App.TSRunStart.AddSeconds(_projectedruntime);

                TimeSpan _remaining = _endingts - DateTime.Now;

                int _percentage = (int)_runningspan.TotalSeconds * 95 / _projectedruntime;

                ProgressBar.Value = _percentage > 95 ? 95 : _percentage;

                App.systemInfo.UpdateLiveFinished($"{_runningspan.ToReadableString()} run - {_remaining.ToReadableString()} left");

                //Trace.WriteLine($"PB% {_percentage} REM {(int)_remaining.TotalSeconds - (int)_runningspan.TotalSeconds} ELA {(int)_runningspan.TotalSeconds} PROJ {_projectedruntime} PRE {IterationPretime} RUN {IterationRuntime} POST {IterationPostime} ITER {BenchCurrentIteration}");
            }
        }
        public static void UpdateRunSettings2(TextBlock RunSettingsLabel, TextBlock RunSettingsBlock)
        {
            RunSettingsLabel.Visibility = Visibility.Visible;
            RunSettingsBlock.Inlines.Clear();
            if (App.CurrentRun.Runtime > 0) RunSettingsBlock.Inlines.Add(new Run { Text = $"Runtime {App.CurrentRun.Runtime} seconds" });
        }


        public static (bool, string, float?, string, string) ParseLogLine(string _line)
        {
            Trace.WriteLine($"BENCH LINE=={_line}");
            if (_line.Contains("invalid config"))
            {
                return (false, "CONFIG", 0, "", "");
            }
            
            string erroraff_pattern = @"^\[.*\] : WARNING setting affinity failed.";
            string start_pattern = @"^\[.*\] : Start a (?<seconds>.*) second benchmark...";
            string tscore_pattern = @"^\[.*\] : Benchmark Thread (?<thread>\d+) cpu: (?<score>.*) (?<scoreunit>.*)";
            string end_pattern = @"^\[.*\] : Benchmark Total: (?<score>.*) (?<scoreunit>.*)";

            Regex start_rgx = new Regex(start_pattern, RegexOptions.Multiline);
            Match start_m = start_rgx.Match(_line);
            if (start_m.Success)
            {
                return (true, "START", 0, start_m.Groups[1].Value, "");
            }
            Regex erroraff_rgx = new Regex(erroraff_pattern, RegexOptions.Multiline);
            Match erroraff_m = erroraff_rgx.Match(_line);
            if (erroraff_m.Success)
            {
                return (false, "Affinity error, reboot", 0, start_m.Groups[1].Value, "");
            }
            Regex tscore_rgx = new Regex(tscore_pattern, RegexOptions.Multiline);
            Match tscore_m = tscore_rgx.Match(_line);
            if (tscore_m.Success)
            {
                string[] results = tscore_rgx.GetGroupNames();
                string thread = "";
                string score = "";
                string scoreunit = "";

                foreach (var name in results)
                {
                    Group grp = tscore_m.Groups[name];
                    if (name == "thread" && grp.Value.Length > 0)
                    {
                        thread = grp.Value.TrimEnd('\r', '\n').Trim();
                    }
                    if (name == "score" && grp.Value.Length > 0)
                    {
                        score = grp.Value.TrimEnd('\r', '\n').Trim();
                    }
                    if (name == "scoreunit" && grp.Value.Length > 0)
                    {
                        scoreunit = grp.Value.TrimEnd('\r', '\n').Trim();
                    }
                }

                int int_test = 0;
                if (score.Length > 0 && scoreunit.Length > 0 && thread.Length > 0 && Int32.TryParse(thread.ToCharArray(), out int_test))
                {
                    float? fscore = float.Parse(score, CultureInfo.InvariantCulture.NumberFormat);
                    Trace.WriteLine($"LogLine Thread Result: {thread} {score} {scoreunit} fscore: {fscore} int_test: {int_test}");
                    return (true, "THREAD", fscore, scoreunit, thread);
                }
                else
                {
                    Trace.WriteLine($"LogLine End Result error: {score} {scoreunit}");
                    return (false, "END", 0, "", "");
                }
            }
            Regex end_rgx = new Regex(end_pattern, RegexOptions.Multiline);
            Match end_m = end_rgx.Match(_line);
            if (end_m.Success)
            {
                string[] results = end_rgx.GetGroupNames();
                string score = "";
                string scoreunit = "";

                foreach (var name in results)
                {
                    Group grp = end_m.Groups[name];
                    if (name == "score" && grp.Value.Length > 0)
                    {
                        score = grp.Value.TrimEnd('\r', '\n').Trim();
                    }
                    if (name == "scoreunit" && grp.Value.Length > 0)
                    {
                        scoreunit = grp.Value.TrimEnd('\r', '\n').Trim();
                    }
                }

                if (score.Length > 0 && scoreunit.Length > 0)
                {
                    float? fscore = float.Parse(score, CultureInfo.InvariantCulture.NumberFormat);
                    Trace.WriteLine($"LogLine End Result: {score} {scoreunit} fscore: {fscore}");
                    return (true, "END", fscore, scoreunit, "");
                }
                else
                {
                    Trace.WriteLine($"LogLine End Result error: {score} {scoreunit}");
                    return (false, "END", 0, "", "");
                }
            }
            return (true, "", 0, "", "");
        }

        public static void RunBench(string Benchname, List<BenchScore> scoreRun, string BenchArgs, string BenchBinary, bool BenchArchived, bool EndChecklowLoad,
                Action<int> UpdateProgress,
                Action<string> UpdateScore,
                Action<string> UpdateMainStatus,
                Action<string> UpdateFinished,
                Action SetStart,
                Action SetEnabled,
                Action UpdateRunStop,
                Action UpdateMonitoring,
                Action UpdateRunSettings,
                Action UpdateStarted,
                Action UpdateRunStart,
                Action<BenchScore, bool> SetLiveBindings
            )
        {
            try
            {
                void RunningProcessClear(object sender, System.EventArgs e)
                {
                    Trace.WriteLine(
                        $"Exit time    : {App.BenchProc.ExitTime}\n" +
                        $"Exit code    : {App.BenchProc.ExitCode}\n" +
                        $"Elapsed time : {Math.Round((App.BenchProc.ExitTime - App.BenchProc.StartTime).TotalMilliseconds)}");
                    App.RunningProcess = -1;
                    if (App.BenchProc.ExitCode != 0 && App.CurrentRun.FinishString.Length < 1)
                    {
                        UpdateMainStatus($"Benchmark execution error, exitcode: {App.BenchProc.ExitCode}");
                        UpdateScore("Error");
                        App.CurrentRun.FinishString = $"[{App.BenchProc.ExitCode}] System unstable?";
                        App.benchrunning = false;
                        HWMonitor.MonitoringParsed = true;
                    }
                    UpdateFinished(App.CurrentRun.FinishString);

                }

                int BenchCurrentIteration = 0;

                bool parseStatus = true;
                string parseMsg = "";
                float? parseFloat = 0;
                string parseString1 = "";
                string parseString2 = "";
                string _unit = App.BenchScoreUnit;

                HWMonitor.AddMonDevice(HWSensorDevice.CPU);
                HWMonitor.RemoveMonDevice(HWSensorDevice.MainBoard);
                HWMonitor.RemoveMonDevice(HWSensorDevice.GPU);

                App.hwsensors.SetEnabled(HWSensorName.CPULogicalsScores, false);
                App.hwsensors.SetUnit(HWSensorName.CPULogicalsScores, _unit);

                App.TaskRunning = true;
                App.scoreMinWidth = 0;

                App.benchcts = new();
                CancellationToken benchtoken = new CancellationToken();
                benchtoken = App.benchcts.Token;

                Trace.WriteLine($"RUN BENCH 1 Benchname={Benchname}");

                App.hwmtimer.Interval = HWMonitor.MonitoringPooling;
                HWMonitor.EndCheckLowLoad = EndChecklowLoad;

                int[] threads = App.GetThreads();
                int CPUCores = App.systemInfo.CPUCores;
                int[] CPPC = App.systemInfo.CPPCActiveOrder;

                Trace.WriteLine($"RUN BENCH 1A");

                App.TSRunStart = DateTime.Now;
                App.IterationPretimeTS = DateTime.MinValue;
                App.IterationRuntimeTS = DateTime.MinValue;
                App.IterationPostimeTS = DateTime.MinValue;
                App.BenchIterations = threads.Count();
                App.IterationPretime = App.GetIdleStableTime();
                App.IterationRuntime = App.GetRuntime(Benchname);
                int _runtime = App.GetRuntime(Benchname);
                App.IterationPostime = 5;
                BenchCurrentIteration = 0;

                UpdateRunStart();

                Trace.WriteLine($"RUN BENCH 1B Iterations={threads.Count()}");

                foreach (int _thrds in threads)
                {
                    BenchCurrentIteration++;

                    App.IterationPretimeTS = DateTime.Now;

                    Trace.WriteLine($"RUN BENCH 2 Benchname={Benchname} Threads={_thrds}");

                    BenchScore _scoreRun = Module1.GetRunForThreads(scoreRun, _thrds, Benchname);

                    App.CurrentRun = _scoreRun;

                    _scoreRun.ClearRun();
                    _scoreRun.Runtime = _runtime;
                    _scoreRun.BenchScoreUnit = _unit;

                    UpdateProgress(-1);

                    if (benchtoken.IsCancellationRequested) return;

                    UpdateRunSettings();

                    HWMonitor.MonitoringStart = DateTime.MinValue;
                    HWMonitor.MonitoringEnd = DateTime.MinValue;
                    HWMonitor.MonitoringPause = false;
                    HWMonitor.MonitoringParsed = false;
                    HWMonitor.MonitoringBenchStarted = false;
                    HWMonitor.MonitoringStarted = false;
                    HWMonitor.MonitoringStopped = false;

                    string path = @".\xmrstakrx_cpu.txt";
                    if (!File.Exists(path)) File.Delete(path);

                    string CPPCOrder = String.Join(", ", CPPC);

                    Trace.WriteLine($"[0based] CPPC: {CPPCOrder}");

                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine("\"cpu_threads_conf\" :");
                        sw.WriteLine("[");

                        int bitMask = 0;
                        bitMask = App.GetRunCoresAndLogicals(_scoreRun, _thrds);
                        foreach (int cpu in _scoreRun.RunLogicals)
                        {
                            sw.WriteLine($"{{ \"low_power_mode\" : false, \"affine_to_cpu\" : {cpu-1} }},");
                        }
                        sw.WriteLine("]");
                    }

                    Trace.WriteLine($"[1based] RunCores: {string.Join(", ", _scoreRun.RunCores.ToArray())}");
                    Trace.WriteLine($"[1based] RunLogicals: {string.Join(", ", _scoreRun.RunLogicals.ToArray())}");

                    string _args = BenchArgs.Replace("###runtime###", App.CurrentRun.Runtime.ToString());

                    App.BenchProc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = BenchBinary,
                            Arguments = _args,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                            StandardOutputEncoding = Encoding.GetEncoding(850),
                            StandardErrorEncoding = Encoding.GetEncoding(850)
                        }
                    };

                    if (!File.Exists(BenchBinary))
                    {
                        UpdateScore("Error");
                        UpdateMainStatus($"Benchmark has been deleted, check your AntiVirus!");
                        App.CurrentRun.FinishString = "Bench binary not found";
                        UpdateFinished(App.CurrentRun.FinishString);
                        Trace.WriteLine($"{Benchname} Out of Loop at Threads: {_thrds}");
                        App.benchcts.Cancel();
                        return;
                    }

                    HWMonitor.MonitoringPause = true;
                    HWMonitor.MonitoringIdle = true;

                    App.hwmtimer.Enabled = false;

                    HWMonitor.ReInit(true);

                    App.hwmtimer.Enabled = true;

                    HWMonitor.MonitoringPause = false;

                    Thread.Sleep(500);

                    bool IamIdling = false;

                    DateTime TSIdleStart = DateTime.Now;
                    DateTime TSIdleUpdated = DateTime.Now;

                    int IdleLowestTemp = HWMonitor.IdleCurrentCPUTemp;
                    int IdleAbsoluteCPULimit = App.GetIdleAbsoluteCPULimit();
                    int IdleLowestLoad = App.GetIdleLowestLoad();
                    int IdleStableTime = App.GetIdleStableTime();
                    int IdleStartTimeout = App.GetIdleStartTimeout();

                    while (!IamIdling)
                    {
                        if (benchtoken.IsCancellationRequested)
                        {
                            _scoreRun.Finished = DateTime.Now;
                            UpdateScore("Error");
                            UpdateMainStatus($"Benchmark aborted while running {_thrds}T");
                            if (App.CurrentRun.FinishString.Length < 1)
                                App.CurrentRun.FinishString = "Aborted by user";
                            _scoreRun.ExitStatus = App.CurrentRun.FinishString;
                            UpdateFinished(App.CurrentRun.FinishString);
                            Trace.WriteLine($"{Benchname} Out of Loop at Threads: {_thrds}");
                            return;
                        }
                        UpdateProgress(-1);
                        Thread.Sleep(250);
                        TimeSpan _idlestart = DateTime.Now - TSIdleStart;
                        if (!HWMonitor.IdleCPUTempSensor)
                        {
                            Trace.WriteLine($"CPU Temp Sensor not found waiting static {HWMonitor.IdleStaticWait} ms");
                            UpdateMainStatus($"CPU Temp Sensor not found, waiting {TimeSpan.FromMilliseconds(HWMonitor.IdleStaticWait).TotalSeconds} seconds before start...");
                            int _slices = HWMonitor.IdleStaticWait / 100;
                            for (int i = 1; i <= _slices; i++)
                            {
                                if (benchtoken.IsCancellationRequested)
                                {
                                    _scoreRun.Finished = DateTime.Now;
                                    UpdateScore("Error");
                                    UpdateMainStatus($"Benchmark aborted while running {_thrds}T");
                                    if (App.CurrentRun.FinishString.Length < 1)
                                        App.CurrentRun.FinishString = "Aborted by user";
                                    _scoreRun.ExitStatus = App.CurrentRun.FinishString;
                                    UpdateFinished(App.CurrentRun.FinishString);
                                    Trace.WriteLine($"{Benchname} Out of Loop at Threads: {_thrds}");
                                    return;
                                    //throw new OperationCanceledException();
                                }
                                UpdateProgress(-1);
                                Trace.WriteLine($"Seelping for staticwait");
                                Thread.Sleep(100);
                            }
                            IamIdling = true;
                        }
                        else if (_idlestart.TotalSeconds > IdleStartTimeout)
                        {
                            UpdateMainStatus($"CPU idle scan timeout, starting benchmark");
                            Trace.WriteLine($"Check idling timeout: {_idlestart.TotalSeconds}");
                            _scoreRun.StartedTemp = HWMonitor.IdleCurrentCPUTemp;
                            IamIdling = true;
                        }
                        else
                        {
                            if (HWMonitor.IdleCurrentCPUTemp < IdleLowestTemp)
                            {
                                TSIdleUpdated = DateTime.Now;
                                IdleLowestTemp = HWMonitor.IdleCurrentCPUTemp;
                                Trace.WriteLine($"Check idling temp new lowest: {IdleLowestTemp}");
                            }
                            if (HWMonitor.IdleCurrentCPUTemp > IdleLowestTemp + HWMonitor.IdleHysteresis)
                            {
                                TSIdleUpdated = DateTime.Now;
                                Trace.WriteLine($"Check idling temp higher the hysteresis: {HWMonitor.IdleCurrentCPUTemp}");
                            }
                            if (HWMonitor.IdleCurrentCPULoad > IdleLowestLoad)
                            {
                                TSIdleUpdated = DateTime.Now;
                                Trace.WriteLine($"Check idling load too high: {HWMonitor.IdleCurrentCPULoad}");
                            }
                            if (HWMonitor.IdleCurrentCPUTemp > IdleAbsoluteCPULimit)
                            {
                                TSIdleUpdated = DateTime.Now;
                                Trace.WriteLine($"Check idling temp higher the absolute minimum: {HWMonitor.IdleCurrentCPUTemp}");
                            }
                            TimeSpan _idlestable = DateTime.Now - TSIdleUpdated;
                            TimeSpan _idleruntime = DateTime.Now - TSIdleStart;
                            int _stillseconds = IdleStableTime - (int)_idlestable.TotalSeconds;
                            int _tilltimeoutseconds = IdleStartTimeout - (int)_idleruntime.TotalSeconds;
                            if (_stillseconds > _tilltimeoutseconds) _stillseconds = _tilltimeoutseconds;
                            Trace.WriteLine($"Waiting for idling CPU temperature: {HWMonitor.IdleCurrentCPUTemp}°C, still {_stillseconds} seconds before start till={_tilltimeoutseconds}");
                            UpdateMainStatus($"Waiting for CPU idling : {HWMonitor.IdleCurrentCPUTemp}°C @ {HWMonitor.IdleCurrentCPULoad}% load, {_stillseconds} seconds to start");
                            if (_idlestable.TotalSeconds > IdleStableTime)
                            {
                                _scoreRun.StartedTemp = HWMonitor.IdleCurrentCPUTemp;
                                _idlestart = DateTime.Now - TSIdleStart;
                                Trace.WriteLine($"Check idling success in: {_idlestart.TotalSeconds} stable for: {_idlestable.TotalSeconds} current temp: {HWMonitor.IdleCurrentCPUTemp} lowest temp: {IdleLowestTemp}");
                                IamIdling = true;
                            }

                        }
                    }

                    HWMonitor.MonitoringIdle = false;

                    HWMonitor.MonitoringPause = true;

                    HWMonitor.ReInit(true);

                    HWMonitor.MonitoringPause = false;

                    DateTime _started = DateTime.Now;
                    _scoreRun.Started = _started;
                    UpdateStarted();

                    string _startstr = $"Benchmark {_thrds}T started at {_started}";
                    if (_scoreRun.StartedTemp > -999) _startstr += $" with CPU temperature at {_scoreRun.StartedTemp}°C";
                    Trace.WriteLine(_startstr);
                    UpdateMainStatus(_startstr);

                    App.BenchProc.EnableRaisingEvents = true;
                    App.BenchProc.Exited += new EventHandler(RunningProcessClear);

                    App.BenchProc.Start();

                    if (_thrds < App.systemInfo.CPULogicalProcessors)
                        App.BenchProc.PriorityClass = ProcessPriorityClass.AboveNormal;

                    HWMonitor.MonitoringPooling = HWMonitor.MonitoringPoolingFast;

                    BenchRun.SetRunProcID(App.BenchProc.Id);

                    parseStatus = true;
                    parseMsg = "";
                    parseFloat = 0;
                    parseString1 = "";
                    parseString2 = "";
                    App.benchrunning = true;
                    App.benchclosed = false;

                    App.BenchProc.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                        {
                            string _line = e.Data.ToString();
                            App.CurrentRun.RunLog += _line;
                            Trace.WriteLine($"BENCH ERROR LINE=={_line}");
                        }
                    };

                    List<DetailsGrid> _tscores = new();
                    List<int[]> _logicalsscores = new();

                    App.BenchProc.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data == null)
                        {
                            App.benchclosed = true;
                            Trace.WriteLine("BENCH FINISHED");
                        }
                        else
                        {
                            string _line = e.Data.ToString();
                            App.CurrentRun.RunLog += _line;
                            (parseStatus, parseMsg, parseFloat, parseString1, parseString2) = ParseLogLine(_line);

                            if (!parseStatus)
                            {
                                _scoreRun.Finished = DateTime.Now;
                                UpdateScore("Error");
                                UpdateMainStatus($"Benchmark execution error: {parseMsg}");
                                SetLiveBindings(_scoreRun, false);
                                App.CurrentRun.FinishString = $"{parseMsg}";
                                _scoreRun.ExitStatus = parseMsg;
                                UpdateFinished(App.CurrentRun.FinishString);
                                Trace.WriteLine($"{Benchname} Out of Loop at Threads: {_thrds}");
                                App.benchcts.Cancel();
                            }
                            else
                            {
                                if (parseMsg == "START")
                                {
                                    TimeSpan _pretimespan = DateTime.Now - App.IterationPretimeTS;
                                    App.IterationPretime = (int)_pretimespan.TotalSeconds;
                                    App.IterationRuntimeTS = DateTime.Now;
                                    HWMonitor.MonitoringBenchStarted = true;
                                    HWMonitor.MonitoringBenchStartedTS = DateTime.Now;
                                    UpdateScore("Running...");
                                    UpdateMainStatus($"Benchmark {_thrds}t running...");
                                    Trace.WriteLine("Benchmark START");
                                    parseMsg = "";
                                }
                                if (parseMsg == "THREAD")
                                {
                                    int _cpu = _scoreRun.RunLogicals[Int32.Parse(parseString2)] - 1;
                                    int _core = ProcessorInfo.PhysicalCore(_cpu);
                                    int _thread = ProcessorInfo.ThreadID(_cpu);
                                    _logicalsscores.Add(new int[] { _scoreRun.RunLogicals[Int32.Parse(parseString2)] - 1, Int32.Parse(parseString2) });
                                    _tscores.Add(new DetailsGrid($"#{_core}T{_thread}", parseFloat, -99998, true, "0.00", parseString1));
                                    parseMsg = "";
                                }
                                if (parseMsg == "END")
                                {
                                    TimeSpan _runtimespan = DateTime.Now - App.IterationRuntimeTS;
                                    App.IterationRuntime = (int)_runtimespan.TotalSeconds;
                                    App.IterationPostimeTS = DateTime.Now;
                                    float? _valscore = parseFloat;
                                    string _prefix = parseString1[0..^_unit.Length];
                                    if (_prefix.Length > 0)
                                    {
                                        _valscore = (float?)HWMonitor.SetScaleValueFromPrefix((double)parseFloat, _prefix);
                                    }
                                    _scoreRun.ScoreScaled = (float)Math.Round((decimal)_valscore,0);
                                    Trace.WriteLine($"Score scaled={_scoreRun.ScoreScaled}");
                                    _scoreRun.Score = parseFloat;
                                    _scoreRun.ScoreUnit = parseString1;
                                    UpdateScore("");
                                    UpdateMainStatus("Benchmark finished");
                                    Trace.WriteLine("Benchmark END");
                                    parseMsg = "";
                                    App.benchrunning = false;
                                }

                            }

                        }
                    };

                    App.BenchProc.BeginOutputReadLine();

                    SetLiveBindings(_scoreRun, true);

                    while (App.benchrunning)
                    {
                        if (benchtoken.IsCancellationRequested)
                        {
                            SetLiveBindings(_scoreRun, false);
                            _scoreRun.Finished = DateTime.Now;
                            //hwmcts.Cancel();
                            UpdateScore("Error");
                            UpdateMainStatus($"Benchmark aborted while running {_thrds}T");
                            if (App.CurrentRun.FinishString.Length < 1)
                                App.CurrentRun.FinishString = "Aborted by user";
                            _scoreRun.ExitStatus = App.CurrentRun.FinishString;
                            UpdateFinished(App.CurrentRun.FinishString);
                            Trace.WriteLine($"{Benchname} Out of Loop at Threads: {_thrds}");
                            return;
                        }
                        UpdateProgress(-1);

                        Thread.Sleep(500);
                    }

                    App.BenchProc.WaitForExit();

                    if (HWMonitor.MonitoringEnd == DateTime.MinValue) HWMonitor.MonitoringEnd = DateTime.Now;

                    DateTime _finished = DateTime.Now;
                    _scoreRun.Finished = _finished;
                    UpdateFinished(App.CurrentRun.FinishString);
                    Trace.WriteLine($"Finish: {_finished}");

                    while (!HWMonitor.MonitoringParsed && !benchtoken.IsCancellationRequested)
                    {
                        Thread.Sleep(100);
                    }

                    SetLiveBindings(_scoreRun, false);

                    List<int> _sortrunlogicals = _scoreRun.RunLogicals;
                    _sortrunlogicals.Sort();
                    for (int i = 0; i < _sortrunlogicals.Count; ++i)
                    {
                        foreach (int[] _cpuscore in _logicalsscores)
                        {
                            int _runcpu = _sortrunlogicals[i] - 1;
                            DetailsGrid _item = _tscores[_cpuscore[1]];
                            //Trace.WriteLine($"Check tscores _runcpu={_runcpu} _cpu0=[{_cpuscore[0]}]: {_item}");
                            if (_cpuscore[0] == _runcpu) _scoreRun.CPULogicalsScores.Add(_item);
                        }
                    }
                    _sortrunlogicals = null;

                    UpdateMonitoring();

                    TimeSpan _postimespan = DateTime.Now - App.IterationPostimeTS;
                    App.IterationPostime = (int)_postimespan.TotalSeconds;

                    UpdateProgress(-1);

                    if (benchtoken.IsCancellationRequested)
                    {
                        SetLiveBindings(App.CurrentRun, false);
                        UpdateScore("Error");
                        UpdateMainStatus($"Benchmark aborted while running {_thrds}T");
                        App.CurrentRun.FinishString = "Aborted by user";
                        UpdateFinished(App.CurrentRun.FinishString);
                        Trace.WriteLine($"{Benchname} Out of Loop at Threads: {_thrds}");
                        return;
                    }
                    else
                    {
                        UpdateMainStatus($"Benchmark {_thrds}T finished at {_finished}");
                    }
                    _scoreRun.OutputLog = App.CurrentRun.RunLog;
                }
                UpdateProgress(99);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"{Benchname} Exception: {e}");
                UpdateScore("Error");
                UpdateMainStatus($"Benchmark run exception error: {e}");
            }
            finally
            {
                SetLiveBindings(App.CurrentRun, false);
                HWMonitor.MonitoringPause = false;
                HWMonitor.MonitoringIdle = false;
                HWMonitor.MonitoringParsed = false;
                HWMonitor.MonitoringBenchStarted = false;
                HWMonitor.MonitoringStarted = false;
                HWMonitor.MonitoringStopped = false;
                Trace.WriteLine($"{Benchname} Finally {App.RunningProcess}");
                if (App.RunningProcess != -1) BenchRun.KillProcID(App.RunningProcess);
                App.TaskRunning = false;
                if (BenchArchived)
                {
                    try
                    {
                        File.Delete(BenchBinary);
                    }
                    catch
                    {
                    }
                }
                SetStart();
                SetEnabled();
                UpdateRunStop();
            }

        }
    }
}
