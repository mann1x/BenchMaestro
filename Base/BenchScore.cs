using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BenchMaestro
{
    public class BenchScore
    {
        public int Threads { get; set; }
        public string Benchname { get; set; }
        public string ProgressBar { get; set; }
        public string FinishString { get; set; }
        public Window RunWindow { get; set; }
        public TextBlock ScoreBox { get; set; }
        public TextBlock ScoreBox2 { get; set; }
        public float? Score { get; set; }
        public float? ScoreMax { get; set; }
        public float? ScoreScaled { get; set; }
        public float? ScorePerWatt { get; set; }
        public string BenchScoreUnit { get; set; }
        public float?[] Scores { get; set; }
        public string ScoreUnit { get; set; }
        public Grid CPUClockGrid { get; set; }
        public TextBlock CPUClockBox { get; set; }
        public double CPUMaxClock { get; set; }
        public double CPUAvgClock { get; set; }
        public double CPUMaxEffClock { get; set; }
        public double CPUAvgEffClock { get; set; }
        public double CPUMaxLoad { get; set; }
        public double CPUAvgLoad { get; set; }
        public double CPUMaxStretch { get; set; }
        public double CPUAvgStretch { get; set; }
        public Grid CPUPowerGrid { get; set; }
        public TextBlock CPUPowerBox { get; set; }
        public double CPUMaxPower { get; set; }
        public double CPUAvgPower { get; set; }
        public double CPUMaxVoltage { get; set; }
        public double CPUAvgVoltage { get; set; }
        public double CPUSAMaxVoltage { get; set; }
        public double CPUSAAvgVoltage { get; set; }
        public double CPUIOMaxVoltage { get; set; }
        public double CPUIOAvgVoltage { get; set; }
        public double CoresAvgVoltage { get; set; }
        public double CoresMaxVoltage { get; set; }
        public double CoresMaxPower { get; set; }
        public double CoresAvgPower { get; set; }
        public Grid CPUTempGrid { get; set; }
        public TextBlock CPUTempBox { get; set; }
        public double CPUMaxTemp { get; set; }
        public double CPUAvgTemp { get; set; }
        public Grid DetailsGrid { get; set; }
        public Expander DetailsExpander { get; set; }
        public StackPanel DetailsPanel { get; set; }
        public StackPanel DetailsPPanel { get; set; }
        public TextBlock DetailsBox { get; set; }
        public ScrollViewer DetailsScroller { get; set; }
        public double CoresMaxTemp { get; set; }
        public double CoresAvgTemp { get; set; }
        public double CPUFSBAvg { get; set; }
        public double CPUFSBMax { get; set; }
        public double SOCAvgVoltage { get; set; }
        public double SOCMaxVoltage { get; set; }
        public double L3AvgTemp { get; set; }
        public double L3MaxTemp { get; set; }
        public double CCD1AvgTemp { get; set; }
        public double CCD1MaxTemp { get; set; }
        public double CCD2AvgTemp { get; set; }
        public double CCD2MaxTemp { get; set; }
        public double CCDSAvgTemp { get; set; }
        public double CPUPPTMax { get; set; }
        public double CPUPPTAvg { get; set; }
        public int CPUPPTMaxLimit { get; set; }
        public int CPUPPTAvgLimit { get; set; }
        public double CPUTDCMax { get; set; }
        public double CPUTDCAvg { get; set; }
        public int CPUTDCMaxLimit { get; set; }
        public int CPUTDCAvgLimit { get; set; }
        public double CPUEDCMax { get; set; }
        public double CPUEDCAvg { get; set; }
        public int CPUEDCMaxLimit { get; set; }
        public int CPUEDCAvgLimit { get; set; }
        public string CPUCores { get; set; }
        public Grid AdditionalGrid { get; set; }
        public TextBlock AdditionalBox { get; set; }
        public int Runtime { get; set; }
        public List<DetailsGrid> CPUCoresVoltages { get; set; }
        public List<DetailsGrid> CPUCoresClocks { get; set; }
        public List<DetailsGrid> CPUCoresEffClocks { get; set; }
        public List<DetailsGrid> CPUCoresStretch { get; set; }
        public List<DetailsGrid> CPUCoresPower { get; set; }
        public List<DetailsGrid> CPUCoresScores { get; set; }
        public List<DetailsGrid> CPUCoresTemps { get; set; }
        public List<DetailsGrid> CPUCoresC0 { get; set; }
        public List<DetailsGrid> CPULogicalsLoad { get; set; }
        public List<DetailsGrid> CPULogicalsScores { get; set; }
        public TextBlock StartedBox { get; set; }
        public DateTime Started { get; set; }
        public int StartedTemp { get; set; }
        public TextBlock FinishedBox { get; set; }
        public DateTime Finished { get; set; }
        public string ExitStatus { get; set; }
        public string ConfigTag { get; set; }
        public string OutputLog { get; set; }
        public string RunLog { get; set; }
        public string Features { get; set; }
        public string CPUFeatures { get; set; }
        public string SWFeatures { get; set; }
        public string AlgoFeatures { get; set; }
        public string Algo { get; set; }

        public List<int> RunCores { get; set; }
        public List<int> RunLogicals { get; set; }

        public BenchScore(int threads, string benchname)
        {
            Threads = threads;
            Benchname = benchname;
            ClearRun();
        }

        public void ClearRun()
        {
            ProgressBar = "";
            Score = -999;
            ScoreMax = -999;
            Scores = new float?[] { };
            ScoreUnit = "";
            CPUMaxClock = -999;
            CPUAvgClock = -999;
            CPUMaxClock = -999;
            CPUMaxEffClock = -999;
            CPUAvgEffClock = -999;
            CPUMaxStretch = -99999;
            CPUAvgStretch = -99999;
            CPUMaxLoad = -999;
            CPUAvgLoad = -999;
            CPUMaxPower = -999;
            CPUAvgPower = -999;
            CPUMaxVoltage = -999;
            CPUAvgVoltage = -999;
            CPUSAMaxVoltage = -999;
            CPUSAAvgVoltage = -999;
            CPUIOMaxVoltage = -999;
            CPUIOAvgVoltage = -999;
            CoresAvgVoltage = -999;
            CoresMaxVoltage = -999;
            CoresMaxPower = -999;
            CoresAvgPower = -999;
            CPUMaxTemp = -999;
            CPUAvgTemp = -999;
            CoresMaxTemp = -999;
            CoresAvgTemp = -999;
            CPUFSBAvg = -999;
            CPUFSBMax = -999;
            SOCAvgVoltage = -999;
            SOCMaxVoltage = -999;
            L3AvgTemp = -999;
            L3MaxTemp = -999;
            CCD1AvgTemp = -999;
            CCD1MaxTemp = -999;
            CCD2AvgTemp = -999;
            CCD2MaxTemp = -999;
            CCDSAvgTemp = -999;
            CPUPPTMax = -999;
            CPUPPTAvg = -999;
            CPUPPTMaxLimit = -999;
            CPUPPTAvgLimit = -999;
            CPUTDCMax = -999;
            CPUTDCAvg = -999;
            CPUTDCMaxLimit = -999;
            CPUTDCAvgLimit = -999;
            CPUEDCMax = -999;
            CPUEDCAvg = -999;
            CPUEDCMaxLimit = -999;
            CPUEDCAvgLimit = -999;
            CPUCores = "";
            CPUCores = "";
            ExitStatus = "";
            ConfigTag = "";
            OutputLog = "";
            Features = "";
            CPUFeatures = "";
            SWFeatures = "";
            AlgoFeatures = "";
            Algo = "";
            FinishString = "";
            CPUCoresVoltages = new List<DetailsGrid>();
            CPUCoresClocks = new List<DetailsGrid>();
            CPUCoresEffClocks = new List<DetailsGrid>();
            CPUCoresStretch = new List<DetailsGrid>();
            CPUCoresPower = new List<DetailsGrid>();
            CPUCoresScores = new List<DetailsGrid>();
            CPUCoresTemps = new List<DetailsGrid>();
            CPUCoresC0 = new List<DetailsGrid>();
            CPULogicalsLoad = new List<DetailsGrid>();
            CPULogicalsScores = new List<DetailsGrid>();
            BenchScoreUnit = "";
            ExitStatus = "";
            OutputLog = "";
            RunLog = "";
            StartedTemp = -999;
            RunCores = new List<int>();
            RunLogicals = new List<int>();
            Runtime = App.GetRuntime("default");

        }
    }

}