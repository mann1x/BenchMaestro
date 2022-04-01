﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using LibreHardwareMonitor.Hardware;
using System.Timers;
using System.Diagnostics;
using System.IO;
using ZenStates.Core;

namespace BenchMaestro
{
    public class HWMonitor
    {
        public static Computer computer = new Computer();

        public static DateTime MonitoringStart = DateTime.MinValue;
        public static DateTime MonitoringEnd = DateTime.MinValue;
        public static bool MonitoringStarted = false;
        public static bool MonitoringStopped = false;
        public static bool MonitoringBenchStarted = false;
        public static bool MonitoringParsed = false;
        public static bool MonitoringPause = false;
        public static bool MonitoringIdle = false;
        public static bool IdleCPUTempSensor = true;
        public static bool InitSensor = false;
        public static int MonitoringPooling = 250;
        public static int MonitoringPoolingFast = 250;
        public static int MonitoringPoolingSlow = 1000;
        public static int IdleCurrentCPUTemp = 1000;
        public static int IdleHysteresis = 3;
        public static int IdleCurrentCPULoad = 100;
        public static int IdleStaticWait = 20000;
        public static List<HWSensorDevice> MonitoringDevices;
        public static HWSensorSource CPUSource;
        public static HWSensorSource GPUSource;
        public static HWSensorSource BoardSource;
        public static bool EndCheckLowLoad = false;
        public static CpuLoad cpuLoad;

        public static bool _dumphwm = true;
        public static bool _dumphwmidle = true;     

        public static void NewSensors()
        {

            App.hwsensors = new List<HWSensorItem>();

            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUClock, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEffClock, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUFSB, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPower, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUVoltage, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Voltage));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CoresPower, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTemp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUMBTemp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.MainBoard, HWSensorType.Temperature));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPULoad, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.SOCVoltage, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Voltage));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD2Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CCDSTemp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CoresTemp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresVoltages, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Voltage));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresPower, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresTemps, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPULogicalsLoad, HWSensorValues.MultiLogical, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load));
        }
        public static void AddMonDevice(HWSensorDevice _device)
        {
            if (!MonitoringDevices.Contains(_device))
            {
                MonitoringDevices.Add(_device);
            }
        }
        public static void RemoveMonDevice(HWSensorDevice _device)
        {
            if (MonitoringDevices.Contains(_device))
            {
                MonitoringDevices.Remove(_device);
            }
        }
        public static void Init()
        {

            MonitoringDevices = new();
            MonitoringDevices.Add(HWSensorDevice.CPU);
            MonitoringDevices.Add(HWSensorDevice.MainBoard);

            GPUSource = HWSensorSource.Libre;
            BoardSource = HWSensorSource.Libre;

            computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsNetworkEnabled = false,
                IsStorageEnabled = false
            };
            computer.Open();

            MonitoringPause = true;

            cpuLoad = new CpuLoad(App.systemInfo.CPULogicalProcessors);
            cpuLoad.Update();

        }

        public static void ReInit(bool _board = false, bool _gpu = false)
        {
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = _gpu,
                IsMemoryEnabled = false,
                IsMotherboardEnabled = _board,
                IsControllerEnabled = false,
                IsNetworkEnabled = false,
                IsStorageEnabled = false
            };
            computer.Open();
        }

        public static void InitZenSensors()
        {
            foreach (HWSensorItem _sensor in App.hwsensors.Where(sensorItem => sensorItem.Source == HWSensorSource.Zen && sensorItem.Enabled == true))
            {
                
            }

        }
        public static void UpdateZenSensors()
        {
            try
            {
                SMU.Status status = App.systemInfo.Zen.RefreshPowerTable();

                if (status != SMU.Status.OK)
                {
                    for (int r = 0; r < 5; ++r)
                    {
                        Thread.Sleep(25);
                        status = App.systemInfo.Zen.RefreshPowerTable();
                        if (status == SMU.Status.OK) r = 5;
                    }
                }

                foreach (HWSensorItem _sensor in App.hwsensors.Where(sensorItem => sensorItem.Source == HWSensorSource.Zen && sensorItem.Enabled && sensorItem.ZenPTOffset >= 0))
                {
                    if (_sensor.SensorValues == HWSensorValues.Single)
                    {
                        //Trace.WriteLine($"Zen Sensor update {_sensor.Name} PTOffset={_sensor.ZenPTOffset}");
                        _sensor.Values.Add(App.systemInfo.Zen.powerTable.Table[_sensor.ZenPTOffset] * _sensor.ZenMulti);
                        //Trace.WriteLine($"Zen Sensor update {_sensor.Name}={_sensor.Values.Last()}");
                    }
                    else
                    {
                        foreach (HWSensorMultiValues _sensorValues in _sensor.MultiValues)
                        {
                            //Trace.WriteLine($"Zen Multi Sensor update {_sensor.Name} PTOffset={_sensorValues.ZenPTOffset}");
                            _sensorValues.Values.Add(App.systemInfo.Zen.powerTable.Table[_sensorValues.ZenPTOffset] * _sensor.ZenMulti);
                            //Trace.WriteLine($"Zen Sensor update {_sensor.Name}={_sensorValues.Values.Last()}");

                        }
                    }
                }

                foreach (HWSensorItem _sensor in App.hwsensors.Where(sensorItem => sensorItem.Name == HWSensorName.CPULogicalsLoad))
                {
                    for (int i = 0; i < App.systemInfo.CPULogicalProcessors; i++)
                    {
                        float _value = cpuLoad.GetCpuLoad(i);
                        //Trace.Write($"_GetCpuLoad [{cpuLoad.GetCpuCount()}] read #{i}");
                        _sensor.MultiValues[i].Values.Add(_value);
                        //Trace.WriteLine($" {_value}%");
                        if (_value > 98)
                        {
                            int _core = ProcessorInfo.PhysicalCore(i);
                            Trace.WriteLine($"Test Stretch CPU={i} [{_core}]");
                            if (App.hwsensors.IsAny(HWSensorName.CPUCoresClocks,_core) && App.hwsensors.IsAny(HWSensorName.CPUCoresEffClocks, _core))
                            {
                                float? _stretch = App.hwsensors.GetValue(HWSensorName.CPUCoresClocks, _core) - App.hwsensors.GetValue(HWSensorName.CPUCoresEffClocks, _core);
                                Trace.WriteLine($"Stretch {_stretch} MHz");
                                if (_stretch >= 1)
                                {
                                    App.hwsensors.UpdateZenSensor(HWSensorName.CPUCoresStretch, _stretch, _core);
                                    Trace.WriteLine($"Add Stretch [{_core}] {_stretch} MHz");
                                }
                            }
                        }
                    }
                }

                App.hwsensors.Where(sensorItem => sensorItem.Name == HWSensorName.CPULoad).First().Values.Add(cpuLoad.GetTotalLoad());

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"UpdateZenSensors Exception: {ex}");
            }

        }

        public static void Close()
        {

            App.hwmtimer.Enabled = false;

            App.hwmcts.Cancel();

            try
            {
                App.mreshwm.Wait(App.hwmcts.Token);
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine($"HWM canceled");
            }

            computer.Close();

        }

        public static void UpdateSensor(ISensor sensor)
        {

            if (sensor.SensorType == SensorType.Clock && sensor.Name.StartsWith("Core #"))
            {
                foreach (HWSensorItem _sensor in App.hwsensors.Where(sensorItem => sensorItem.Name == HWSensorName.CPUCoresClocks && sensorItem.Source == HWSensorSource.Libre))
                {
                    foreach (HWSensorMultiValues _sensorValues in _sensor.MultiValues.Where(sensorValue => sensorValue.LibreIdentifier == sensor.Identifier.ToString()))
                    {
                        _sensorValues.Values.Add(sensor.Value);
                    }
                }
            }

            else if (sensor.SensorType == SensorType.Power && sensor.Name.StartsWith("Core #"))
            {
                foreach (HWSensorItem _sensor in App.hwsensors.Where(sensorItem => sensorItem.Name == HWSensorName.CPUCoresPower && sensorItem.Source == HWSensorSource.Libre))
                {
                    foreach (HWSensorMultiValues _sensorValues in _sensor.MultiValues.Where(sensorValue => sensorValue.LibreIdentifier == sensor.Identifier.ToString()))
                    {
                        _sensorValues.Values.Add(sensor.Value);
                    }
                }
            }

            else if (sensor.SensorType == SensorType.Voltage && sensor.Name.StartsWith("Core #"))
            {
                foreach (HWSensorItem _sensor in App.hwsensors.Where(sensorItem => sensorItem.Name == HWSensorName.CPUCoresVoltages && sensorItem.Source == HWSensorSource.Libre))
                {
                    foreach (HWSensorMultiValues _sensorValues in _sensor.MultiValues.Where(sensorValue => sensorValue.LibreIdentifier == sensor.Identifier.ToString()))
                    {
                        _sensorValues.Values.Add(sensor.Value);
                    }
                }
            }

            else if (sensor.SensorType == SensorType.Load && sensor.Name.StartsWith("CPU Core #"))
            {
                foreach (HWSensorItem _sensor in App.hwsensors.Where(sensorItem => sensorItem.Name == HWSensorName.CPULoad && sensorItem.Source == HWSensorSource.Libre))
                {
                    List<HWSensorMultiValues> _sensorValues2 = _sensor.MultiValues;
                    Trace.WriteLine($"1 = Sensor update {_sensor.Name} c={_sensorValues2.Count} {sensor.Identifier}={sensor.Value}");
                    foreach (HWSensorMultiValues _sensorValues in _sensorValues2)
                    {
                        if (_sensorValues.LibreIdentifier == sensor.Identifier.ToString()) { 
                            Trace.WriteLine($"Sensor update {sensor.Identifier}={sensor.Value}");
                            _sensorValues.Values.Add(sensor.Value);
                        }
                    }
                }
            }
 
            else
            {
                foreach (HWSensorItem _sensor in App.hwsensors.Where(sensorItem => sensorItem.SensorValues == HWSensorValues.Single && sensorItem.Source == HWSensorSource.Libre))
                {
                    if (_sensor.LibreIdentifier == sensor.Identifier.ToString())
                    {
                        _sensor.Values.Add(sensor.Value);
                    }
                }
            }

        }

        public static void UpdateSensors(IComputer computer)
        {

            foreach (IHardware hardware in computer.Hardware)
            {
                foreach (IHardware subhardware in hardware.SubHardware)
                {
                    foreach (ISensor sensor in subhardware.Sensors)
                    {
                        App.hwsensors.UpdateSensor(sensor.Identifier.ToString(), sensor.Value);
                    }
                }

                foreach (ISensor sensor in hardware.Sensors)
                {
                    App.hwsensors.UpdateSensor(sensor.Identifier.ToString(), sensor.Value);
                }
            }

        }
        public static void CheckSensor(IHardware hardware, ISensor sensor)
        {

            //string _subhardware = "NULL";
            //if (subhardware != null) _subhardware = subhardware.Identifier.ToString();

            string line = $"Name: {sensor.Name}, value: {sensor.Value}, max: {sensor.Max}, min: {sensor.Min}, index: {sensor.Index}, control: {sensor.Control}, identifier: {sensor.Identifier}, type: {sensor.SensorType}";
            Trace.WriteLine($"\tLibre Checking Sensor {line} HW={hardware.Identifier}");

            if (hardware.HardwareType == HardwareType.Motherboard && sensor.SensorType == SensorType.Temperature && sensor.Name == "CPU" && BoardSource == HWSensorSource.Libre)
            {
                App.hwsensors.InitLibre(HWSensorName.CPUMBTemp, sensor.Identifier.ToString(), sensor.Name);
                Trace.WriteLine($"Libre CPU MB Temp Sensor added {sensor.Identifier} HW={hardware.Identifier}");               
            }

            if (hardware.HardwareType == HardwareType.Cpu && sensor.SensorType == SensorType.Temperature && sensor.Name.StartsWith("Core (Tctl/Tdie)") && CPUSource == HWSensorSource.Libre)
            {
                App.hwsensors.InitLibre(HWSensorName.CPUTemp, sensor.Identifier.ToString(), sensor.Name);
                Trace.WriteLine($"Libre CPU Temp Sensor added {sensor.Identifier} HW={hardware.Identifier}");
            }

            if (hardware.HardwareType == HardwareType.Cpu && sensor.SensorType == SensorType.Load && sensor.Name == "CPU Total" && CPUSource == HWSensorSource.Libre)
            {
                App.hwsensors.InitLibre(HWSensorName.CPULoad, sensor.Identifier.ToString(), sensor.Name);
                Trace.WriteLine($"Libre CPU Load Sensor added {sensor.Identifier} HW={hardware.Identifier}");
            }

            if (hardware.HardwareType == HardwareType.Cpu && sensor.SensorType == SensorType.Power && sensor.Name == "Package" && CPUSource == HWSensorSource.Libre)
            {
                App.hwsensors.InitLibre(HWSensorName.CPUPower, sensor.Identifier.ToString(), sensor.Name);
                Trace.WriteLine($"Libre CPU Power Sensor added {sensor.Identifier} HW={hardware.Identifier}");
            }

            if (hardware.HardwareType == HardwareType.Cpu && sensor.SensorType == SensorType.Voltage && sensor.Name == "Core (SVI2 TFN)" && CPUSource == HWSensorSource.Libre)
            {
                App.hwsensors.InitLibre(HWSensorName.CPUVoltage, sensor.Identifier.ToString(), sensor.Name);
                Trace.WriteLine($"Libre CPU Voltage Sensor added {sensor.Identifier} HW={hardware.Identifier}");
            }

            if (hardware.HardwareType == HardwareType.Cpu && sensor.SensorType == SensorType.Voltage && sensor.Name == "SoC (SVI2 TFN)" && CPUSource == HWSensorSource.Libre)
            {
                App.hwsensors.InitLibre(HWSensorName.SOCVoltage, sensor.Identifier.ToString(), sensor.Name);
                Trace.WriteLine($"Libre CPU SOC Voltage Sensor added {sensor.Identifier} HW={hardware.Identifier}");
            }

            if (hardware.HardwareType == HardwareType.Cpu && sensor.SensorType == SensorType.Temperature && sensor.Name == "CCD1 (Tdie)" && CPUSource == HWSensorSource.Libre)
            {
                App.hwsensors.InitLibre(HWSensorName.CCD1Temp, sensor.Identifier.ToString(), sensor.Name);
                Trace.WriteLine($"Libre CPU CCD1 Temp Sensor added {sensor.Identifier} HW={hardware.Identifier}");
            }

            if (hardware.HardwareType == HardwareType.Cpu && sensor.SensorType == SensorType.Temperature && sensor.Name == "CCD2 (Tdie)" && CPUSource == HWSensorSource.Libre)
            {
                App.hwsensors.InitLibre(HWSensorName.CCD2Temp, sensor.Identifier.ToString(), sensor.Name);
                Trace.WriteLine($"Libre CPU CCD2 Temp Sensor added {sensor.Identifier} HW={hardware.Identifier}");
            }

            if (hardware.HardwareType == HardwareType.Cpu && sensor.SensorType == SensorType.Temperature && sensor.Name == "CCDs Average (Tdie)" && CPUSource == HWSensorSource.Libre)
            {
                App.hwsensors.InitLibre(HWSensorName.CCDSTemp, sensor.Identifier.ToString(), sensor.Name);
                Trace.WriteLine($"Libre CPU CCDs Average Temp Sensor added {sensor.Identifier} HW={hardware.Identifier}");
            }

            if (hardware.HardwareType == HardwareType.Cpu && sensor.SensorType == SensorType.Clock && sensor.Name == "Bus Speed" && CPUSource == HWSensorSource.Libre)
            {
                App.hwsensors.InitLibre(HWSensorName.CPUFSB, sensor.Identifier.ToString(), sensor.Name);
                Trace.WriteLine($"Libre CPU FSB Clock Sensor added {sensor.Identifier} HW={hardware.Identifier}");
            }

            if (sensor.SensorType == SensorType.Clock && sensor.Name.StartsWith("Core #") && CPUSource == HWSensorSource.Libre)
            {
                string _indexstr = sensor.Name[6..];
                Trace.WriteLine($"Libre CPU Cores Clocks #{_indexstr}");
                int _index = Int32.Parse(_indexstr);
                App.hwsensors.InitLibreMulti(HWSensorName.CPUCoresClocks, hardware.Identifier.ToString(), "Cores Clock", _index, sensor.Identifier.ToString(), sensor.Name);
                Trace.WriteLine($"Libre CPU Cores Clocks #{_index} Sensor added #{sensor.Index} {sensor.Identifier} HW={hardware.Identifier}");
            }

            if (sensor.SensorType == SensorType.Power && sensor.Name.StartsWith("Core #") && CPUSource == HWSensorSource.Libre)
            {
                string _indexstr = sensor.Name[6..^5];
                Trace.WriteLine($"Libre CPU Cores Power #{_indexstr}");
                int _index = Int32.Parse(_indexstr);
                App.hwsensors.InitLibreMulti(HWSensorName.CPUCoresPower, hardware.Identifier.ToString(), "CPU Cores Power", _index, sensor.Identifier.ToString(), sensor.Name);
                Trace.WriteLine($"Libre CPU Cores Power #{_index} Sensor added #{sensor.Index} {sensor.Identifier} HW={hardware.Identifier}");
            }

            if (sensor.SensorType == SensorType.Voltage && sensor.Name.StartsWith("Core #") && CPUSource == HWSensorSource.Libre)
            {
                string _indexstr = sensor.Name[6..^3];
                Trace.WriteLine($"Libre CPU Cores Voltage #{_indexstr}");
                int _index = Int32.Parse(_indexstr);
                App.hwsensors.InitLibreMulti(HWSensorName.CPUCoresVoltages, hardware.Identifier.ToString(), "CPU Cores Voltage", _index, sensor.Identifier.ToString(), sensor.Name);
                Trace.WriteLine($"Libre CPU Cores Voltage #{_index} Sensor added #{sensor.Index} {sensor.Identifier} HW={hardware.Identifier}");
            }

            if (sensor.SensorType == SensorType.Load && sensor.Name.StartsWith("CPU Core #") && CPUSource == HWSensorSource.Libre)
            {
                string _indexstr = sensor.Name[10..];
                Trace.WriteLine($"Libre CPU Cores Load #{_indexstr}");                
                int _index = Int32.Parse(_indexstr);
                App.hwsensors.InitLibreMulti(HWSensorName.CPULogicalsLoad, hardware.Identifier.ToString(), "CPU Cores Load", _index, sensor.Identifier.ToString(), sensor.Name);
                Trace.WriteLine($"Libre CPU Cores Load #{_index} Sensor added #{sensor.Index} {sensor.Identifier} HW={hardware.Identifier}");
            }

        }

        public static void OnHWM(object sender, ElapsedEventArgs args)
        {
            try
            {
                CancellationToken hwmtoken = new CancellationToken();
                hwmtoken = (CancellationToken)App.hwmcts.Token;

                //Trace.WriteLine("HWM MONITOR START");
                if (hwmtoken.IsCancellationRequested)
                {
                    Trace.WriteLine("HWM MONITOR CANCELLATION REQUESTED");
                    hwmtoken.ThrowIfCancellationRequested();
                }

                if (!InitSensor)
                {
                    Trace.WriteLine("HWM MONITOR INIT SENSORS");

                    computer.Accept(new UpdateVisitor());

                    foreach (IHardware hardware in computer.Hardware)
                    {
                        foreach (IHardware subhardware in hardware.SubHardware)
                        {
                            foreach (ISensor sensor in subhardware.Sensors)
                            {
                                CheckSensor(hardware, sensor);
                            }
                        }

                        foreach (ISensor sensor in hardware.Sensors)
                        {
                            CheckSensor(hardware, sensor);
                        }
                    }

                    Trace.WriteLine("HWM MONITOR INIT SENSORS DONE");

                    InitSensor = true;
                }

                if (MonitoringStopped && !MonitoringParsed)
                {
                    TimeSpan _deltarun = MonitoringStart - MonitoringEnd;

                    Thread.Sleep(1000);

                    double _coresmaxt = -99999;
                    double _coresavgt = -99999;
                    double _coresmaxc = -99999;
                    double _coresavgc = -99999;
                    double _coresmaxec = -99999;
                    double _coresavgec = -99999;
                    double _coresmaxst = -99999;
                    double _coresavgst = -99999;
                    double _coresmaxv = -99999;
                    double _coresavgv = -99999;
                    double _coresmaxp = -99999;
                    double _coresavgp = -99999;
                    double _coresmaxl = -99999;
                    double _coresavgl = -99999;

                    Trace.WriteLine($"GET STATS FOR {App.CurrentRun.RunCores.Count} CORES");

                    App.CurrentRun.CPUMaxTemp = (double)App.hwsensors.GetMax(HWSensorName.CPUTemp);
                    App.CurrentRun.CPUAvgTemp = (double)App.hwsensors.GetAvg(HWSensorName.CPUTemp);
                    Trace.WriteLine($"CPU AVG TEMP {App.CurrentRun.CPUAvgTemp} MAX {App.CurrentRun.CPUMaxTemp}");

                    App.CurrentRun.CPUMaxPower = (double)App.hwsensors.GetMax(HWSensorName.CPUPower);
                    App.CurrentRun.CPUAvgPower = (double)App.hwsensors.GetAvg(HWSensorName.CPUPower);
                    Trace.WriteLine($"CPU AVG POWER {App.CurrentRun.CPUAvgPower} MAX {App.CurrentRun.CPUMaxPower}");

                    App.CurrentRun.CPUMaxVoltage = (double)App.hwsensors.GetMax(HWSensorName.CPUVoltage);
                    App.CurrentRun.CPUAvgVoltage = (double)App.hwsensors.GetAvg(HWSensorName.CPUVoltage);
                    Trace.WriteLine($"CPU AVG VOLTAGE {App.CurrentRun.CPUAvgVoltage} MAX {App.CurrentRun.CPUMaxVoltage}");

                    App.CurrentRun.SOCMaxVoltage = (double)App.hwsensors.GetMax(HWSensorName.SOCVoltage);
                    App.CurrentRun.SOCAvgVoltage = (double)App.hwsensors.GetAvg(HWSensorName.SOCVoltage);
                    Trace.WriteLine($"CPU AVG SOC {App.CurrentRun.SOCAvgVoltage} MAX {App.CurrentRun.SOCMaxVoltage}");

                    App.CurrentRun.CCD1MaxTemp = (double)App.hwsensors.GetMax(HWSensorName.CCD1Temp);
                    App.CurrentRun.CCD1AvgTemp = (double)App.hwsensors.GetAvg(HWSensorName.CCD1Temp);
                    Trace.WriteLine($"CPU AVG CCD1 {App.CurrentRun.CCD1AvgTemp} MAX {App.CurrentRun.CCD1MaxTemp}");

                    App.CurrentRun.CCD2MaxTemp = (double)App.hwsensors.GetMax(HWSensorName.CCD2Temp);
                    App.CurrentRun.CCD2AvgTemp = (double)App.hwsensors.GetAvg(HWSensorName.CCD2Temp);
                    Trace.WriteLine($"CPU AVG CCD2 {App.CurrentRun.CCD2AvgTemp} MAX {App.CurrentRun.CCD2MaxTemp}");

                    App.CurrentRun.CCDSAvgTemp = (double)App.hwsensors.GetMax(HWSensorName.CCDSTemp);
                    Trace.WriteLine($"CPU AVG CCDS {App.CurrentRun.CCDSAvgTemp}");

                    App.CurrentRun.CPUFSBAvg = (double)App.hwsensors.GetAvg(HWSensorName.CPUFSB);
                    App.CurrentRun.CPUFSBMax = (double)App.hwsensors.GetMax(HWSensorName.CPUFSB);
                    Trace.WriteLine($"CPU AVG FSB {App.CurrentRun.CPUFSBAvg} MAX {App.CurrentRun.CPUFSBMax}");

                    double _sensoravg = 0;
                    double _sensormax = 0;

                    for (int _core1 = 1; _core1 <= App.systemInfo.CPUCores; ++_core1)
                    {
                        bool _bold = false;
                        if (App.CurrentRun.RunCores.Contains(_core1)) _bold = true;

                        int _core = _core1 - 1;

                        if (App.hwsensors.IsEnabled(HWSensorName.CPUCoresClocks)) {
                            _sensoravg = (double)App.hwsensors.GetAvg(HWSensorName.CPUCoresClocks, _core);
                            _sensormax = (double)App.hwsensors.GetMax(HWSensorName.CPUCoresClocks, _core);
                            if (_bold)
                            {
                                if (_coresmaxc < _sensormax && _sensormax > -99999) _coresmaxc = _sensormax;
                                _coresavgc = _coresavgc == -99999 ? _sensoravg : (_coresavgc + _sensoravg) / 2;
                            }
                            App.CurrentRun.CPUCoresClocks.Add(new DetailsGrid($"#{_core}", (float)Math.Round(_sensoravg, 0), (float)Math.Round(_sensormax, 0), _bold, "0"));
                            Trace.WriteLine($"[Core {_core} Clock Avg: {Math.Round(_sensoravg, 0)} Max: {Math.Round(_sensormax, 0)} ]");
                        }

                        if (App.hwsensors.IsEnabled(HWSensorName.CPUCoresEffClocks))
                        {
                            _sensoravg = (double)App.hwsensors.GetAvg(HWSensorName.CPUCoresEffClocks, _core);
                            _sensormax = (double)App.hwsensors.GetMax(HWSensorName.CPUCoresEffClocks, _core);
                            if (_bold)
                            {
                                if (_coresmaxec < _sensormax && _sensormax > -99999) _coresmaxec = _sensormax;
                                _coresavgec = _coresavgec == -99999 ? _sensoravg : (_coresavgec + _sensoravg) / 2;
                            }
                            App.CurrentRun.CPUCoresEffClocks.Add(new DetailsGrid($"#{_core}", (float)Math.Round(_sensoravg, 0), (float)Math.Round(_sensormax, 0), _bold, "0"));
                            Trace.WriteLine($"[Core {_core} Eff Clock Avg: {Math.Round(_sensoravg, 0)} Max: {Math.Round(_sensormax, 0)} ]");

                            _sensoravg = (double)App.hwsensors.GetAvg(HWSensorName.CPUCoresStretch, _core);
                            _sensormax = (double)App.hwsensors.GetMax(HWSensorName.CPUCoresStretch, _core);
                            if (_bold)
                            {
                                if (_coresmaxst < _sensormax && _sensormax > -99999) _coresmaxst = _sensormax;
                                if (_sensoravg > -99999) _coresavgst = _coresavgst == -99999 ? _sensoravg : (_coresavgst + _sensoravg) / 2;
                            }
                            App.CurrentRun.CPUCoresStretch.Add(new DetailsGrid($"#{_core}", (float)Math.Round(_sensoravg, 0), (float)Math.Round(_sensormax, 0), _bold, "0"));
                            Trace.WriteLine($"[Core {_core} Stretch Clock Avg: {Math.Round(_sensoravg, 0)} Max: {Math.Round(_sensormax, 0)} ]");
                        }

                        if (App.hwsensors.IsEnabled(HWSensorName.CPUCoresPower))
                        {
                            _sensoravg = (double)App.hwsensors.GetAvg(HWSensorName.CPUCoresPower, _core);
                            _sensormax = (double)App.hwsensors.GetMax(HWSensorName.CPUCoresPower, _core);
                            if (_bold)
                            {
                                if (_coresmaxp < _sensormax && _sensormax > -99999) _coresmaxp = _sensormax;
                                _coresavgp = _coresavgp == -99999 ? _sensoravg : (_coresavgp + _sensoravg) / 2;
                            }
                            App.CurrentRun.CPUCoresPower.Add(new DetailsGrid($"#{_core}", (float)Math.Round(_sensoravg, 1), (float)Math.Round(_sensormax, 1), _bold, "0.0"));
                            Trace.WriteLine($"[Core {_core} Power Avg: {Math.Round(_sensoravg, 1)} Max: {Math.Round(_sensormax, 1)} ]");
                        }

                        if (App.hwsensors.IsEnabled(HWSensorName.CPUCoresVoltages))
                        {
                            _sensoravg = (double)App.hwsensors.GetAvg(HWSensorName.CPUCoresVoltages, _core);
                            _sensormax = (double)App.hwsensors.GetMax(HWSensorName.CPUCoresVoltages, _core);
                            if (_bold)
                            {
                                if (_coresmaxv < _sensormax && _sensormax > -99999) _coresmaxv = _sensormax;
                                _coresavgv = _coresavgv == -99999 ? _coresavgv : (_coresavgv + _sensoravg) / 2;
                            }
                            App.CurrentRun.CPUCoresVoltages.Add(new DetailsGrid($"#{_core}", (float)Math.Round(_sensoravg, 3), (float)Math.Round(_sensormax, 3), _bold, "0.000"));
                            Trace.WriteLine($"[Core {_core} VID Avg: {Math.Round(_sensoravg, 3)} Max: {Math.Round(_sensormax, 3)}]");
                        }

                        if (App.hwsensors.IsEnabled(HWSensorName.CPUCoresC0))
                        {
                            _sensoravg = (double)App.hwsensors.GetAvg(HWSensorName.CPUCoresC0, _core);
                            _sensormax = (double)App.hwsensors.GetMax(HWSensorName.CPUCoresC0, _core);
                            App.CurrentRun.CPUCoresC0.Add(new DetailsGrid($"#{_core}", (float)Math.Round(_sensoravg, 0), (float)Math.Round(_sensormax, 0), _bold, "0"));
                            Trace.WriteLine($"[Core {_core} Load Avg: {Math.Round(_sensoravg, 0)} Max: {Math.Round(_sensormax, 0)} ]");
                        }

                        if (App.hwsensors.IsEnabled(HWSensorName.CPUCoresTemps))
                        {
                            _sensoravg = (double)App.hwsensors.GetAvg(HWSensorName.CPUCoresTemps, _core);
                            _sensormax = (double)App.hwsensors.GetMax(HWSensorName.CPUCoresTemps, _core);
                            if (_bold)
                            {
                                if (_coresmaxt < _sensormax && _sensormax > -99999) _coresmaxt = _sensormax;
                                _coresavgt = _coresavgt == -99999 ? _coresavgt : (_coresavgt + _sensoravg) / 2;
                            }
                            App.CurrentRun.CPUCoresTemps.Add(new DetailsGrid($"#{_core}", (float)Math.Round(_sensoravg, 0), (float)Math.Round(_sensormax, 0), _bold, "0.0"));
                            Trace.WriteLine($"[Core {_core} Temp Avg: {Math.Round(_sensoravg, 0)} Max: {Math.Round(_sensormax, 0)} ]");
                        }
                    }
                    
                    if (App.hwsensors.IsEnabled(HWSensorName.CPULogicalsLoad)) 
                    { 
                        for (int _cpu = 1; _cpu <= App.systemInfo.CPULogicalProcessors; ++_cpu)
                        {
                            bool _bold = false;
                            if (App.CurrentRun.RunLogicals.Contains(_cpu)) _bold = true;
                            int _core = (int)(_cpu - 1) / 2;
                            int _thread = _cpu % 2 == 0 ? 1 : 0;
                            _sensoravg = (double)App.hwsensors.GetAvg(HWSensorName.CPULogicalsLoad, _cpu - 1);
                            _sensormax = (double)App.hwsensors.GetMax(HWSensorName.CPULogicalsLoad, _cpu - 1);
                            if (_bold)
                            {
                                if (_coresmaxl < _sensormax && _sensormax > -99999) _coresmaxl = _sensormax;
                                _coresavgl = _coresavgl == -99999 ? _sensoravg : (_coresavgl + _sensoravg) / 2;
                            }
                            App.CurrentRun.CPULogicalsLoad.Add(new DetailsGrid($"#{_core}T{_thread}", (float)Math.Round(_sensoravg, 0), (float)Math.Round(_sensormax, 0), _bold, "0"));
                            Trace.WriteLine($"[CPU {_cpu} #{_core}T{_thread} Load Avg: {Math.Round(_sensoravg, 0)} Max: {Math.Round(_sensormax, 0)} ]");
                        }
                    }

                    App.CurrentRun.CoresAvgTemp = Math.Round(_coresavgt, 1);
                    App.CurrentRun.CoresMaxTemp = Math.Round(_coresmaxt, 1);

                    App.CurrentRun.CoresAvgPower = Math.Round(_coresavgp, 1);
                    App.CurrentRun.CoresMaxPower = Math.Round(_coresmaxp, 1);

                    App.CurrentRun.CPUAvgClock = Math.Round(_coresavgc, 0);
                    App.CurrentRun.CPUMaxClock = Math.Round(_coresmaxc, 0);

                    App.CurrentRun.CPUAvgEffClock = Math.Round(_coresavgec, 0);
                    App.CurrentRun.CPUMaxEffClock = Math.Round(_coresmaxec, 0);

                    App.CurrentRun.CPUAvgStretch = Math.Round(_coresavgst, 0);
                    App.CurrentRun.CPUMaxStretch = Math.Round(_coresmaxst, 0);

                    App.CurrentRun.CPUAvgLoad = Math.Round(_coresavgl, 0);
                    App.CurrentRun.CPUMaxLoad = Math.Round(_coresmaxl, 0);

                    App.CurrentRun.CoresAvgVoltage = _coresavgv;
                    App.CurrentRun.CoresMaxVoltage = _coresmaxv;

                    if (App.hwsensors.IsAny(HWSensorName.CCD1L3Temp) && App.hwsensors.IsAny(HWSensorName.CCD2L3Temp))
                    {
                        App.CurrentRun.L3AvgTemp = (double)(App.hwsensors.GetAvg(HWSensorName.CCD1L3Temp) + App.hwsensors.GetAvg(HWSensorName.CCD2L3Temp)) / 2;
                        App.CurrentRun.L3MaxTemp = (double)(App.hwsensors.GetMax(HWSensorName.CCD1L3Temp) + App.hwsensors.GetMax(HWSensorName.CCD2L3Temp)) / 2;
                    }
                    else if (App.hwsensors.IsAny(HWSensorName.CCD1L3Temp))
                    {
                        App.CurrentRun.L3AvgTemp = (double)App.hwsensors.GetAvg(HWSensorName.CCD1L3Temp);
                        App.CurrentRun.L3MaxTemp = (double)App.hwsensors.GetMax(HWSensorName.CCD1L3Temp);
                    }

                    Trace.WriteLine($"MonitoringParsed");
                    MonitoringParsed = true;
                    MonitoringPause = false;
                    MonitoringPooling = MonitoringPoolingSlow;                    
                }

                if (!MonitoringPause)
                {
                    if (CPUSource == HWSensorSource.Zen)
                    {
                        if (cpuLoad.IsAvailable) cpuLoad.Update();
                        UpdateZenSensors();
                    }

                    bool _libre = false;

                    if (MonitoringIdle) _libre = true;
                    if (MonitoringDevices.Contains(HWSensorDevice.CPU) && CPUSource == HWSensorSource.Libre) _libre = true;
                    if (MonitoringDevices.Contains(HWSensorDevice.GPU) && GPUSource == HWSensorSource.Libre) _libre = true;
                    if (MonitoringDevices.Contains(HWSensorDevice.MainBoard) && BoardSource == HWSensorSource.Libre) _libre = true;

                    if (_libre)
                    {
                        computer.Accept(new UpdateVisitor());
                        UpdateSensors(computer);
                    }
                }

                if (!MonitoringPause && !MonitoringStarted && MonitoringBenchStarted)
                {
                    foreach (int _cpu in App.CurrentRun.RunLogicals)
                    {
                        if (!MonitoringStarted)
                        {
                            double _cpuload = (double)App.hwsensors.GetValue(HWSensorName.CPULogicalsLoad, _cpu - 1);

                            Trace.WriteLine($"MonitoringStart Check for load on Logical: {_cpu} {_cpuload}");
                            if (_cpuload > 80)
                            {
                                MonitoringStart = DateTime.Now;
                                MonitoringStarted = true;
                                MonitoringPooling = MonitoringPoolingSlow;
                                Trace.WriteLine($"MonitoringStart STARTED on load on Logical: {_cpu} {_cpuload} {MonitoringStart}");
                            }
                        }
                    }

                    if (MonitoringStarted)
                    {
                        bool _libre = false;

                        if (MonitoringDevices.Contains(HWSensorDevice.CPU) && CPUSource == HWSensorSource.Libre) _libre = true;
                        if (MonitoringDevices.Contains(HWSensorDevice.GPU) && GPUSource == HWSensorSource.Libre) _libre = true;
                        if (MonitoringDevices.Contains(HWSensorDevice.MainBoard) && BoardSource == HWSensorSource.Libre) _libre = true;

                        if (_libre)
                        {
                            computer.Reset();
                            computer.Accept(new UpdateVisitor());
                        }
                        App.hwsensors.Reset();
                        Trace.WriteLine($"MonitoringStart RESET stats");
                    }
                }

                if (MonitoringIdle)
                {
                    Trace.WriteLine($"MonitoringIdle check CPU temp and load");

                    if (App.hwsensors.IsEnabled(HWSensorName.CPUMBTemp))
                    {
                        computer.Accept(new UpdateVisitor());
                        IdleCurrentCPUTemp = (int)App.hwsensors.GetValue(HWSensorName.CPUMBTemp);
                    }
                    else
                    {
                        IdleCurrentCPUTemp = (int)App.hwsensors.GetValue(HWSensorName.CPUTemp);
                    }
                    IdleCurrentCPULoad = (int)App.hwsensors.GetValue(HWSensorName.CPULoad);
                    Trace.WriteLine($"MonitoringIdle current CPU Temp: {IdleCurrentCPUTemp} Load {IdleCurrentCPULoad}");

                    if (IdleCurrentCPUTemp == -999) IdleCPUTempSensor = false;

                }

                TimeSpan _delta = DateTime.Now - MonitoringStart;
                //Trace.WriteLine($"Monitoring pause={MonitoringPause} bstarted={MonitoringBenchStarted} started={MonitoringStarted} stopped={MonitoringStopped} _delta={_delta.TotalSeconds}");

                if (!MonitoringPause && MonitoringStarted && !MonitoringStopped)
                {
                    if (App.RunningProcess == -1)
                    {
                        MonitoringEnd = DateTime.Now;
                        TimeSpan _deltam = MonitoringEnd - MonitoringStart;
                        MonitoringStopped = true;
                        MonitoringPause = true;
                        Trace.WriteLine($"MonitoringEnd STOPPED on Bench exit duration {_deltam.TotalSeconds}");
                    }

                    if (_delta.TotalSeconds > (App.CurrentRun.Runtime - 5) && EndCheckLowLoad)
                    {
                        Trace.WriteLine($"MonitoringEnd Check for load");
                        int _cpulowload = 0;
                        MonitoringPooling = MonitoringPoolingFast;
                        foreach (int _cpu in App.CurrentRun.RunLogicals)
                        {
                            if (!MonitoringStopped)
                            {
                                double _cpuload = (double)App.hwsensors.GetValue(HWSensorName.CPULogicalsLoad, _cpu - 1);

                                Trace.WriteLine($"MonitoringEnd Check for load on Logical: {_cpu} L={_cpuload}% Lowload={_cpulowload}");
                                if (_cpuload < 90 && _cpuload > -999)
                                {
                                    _cpulowload++;
                                }
                            }
                        }

                        if ((_cpulowload > 0 && App.CurrentRun.Threads < App.systemInfo.CPUCores) || (_cpulowload > App.systemInfo.CPUCores / 2 && App.CurrentRun.Threads >= App.systemInfo.CPUCores))
                        {
                            MonitoringEnd = DateTime.Now;
                            TimeSpan _deltam = MonitoringEnd - MonitoringStart;
                            MonitoringStopped = true;
                            MonitoringPause = true;
                            Trace.WriteLine($"MonitoringEnd STOPPED on low load for {_cpulowload} logical cores: {MonitoringEnd} duration {_deltam.TotalSeconds}");
                        }

                    }

                }

                bool _debug = false;

                if (_debug || _dumphwm && !MonitoringIdle)
                {

                    StringBuilder sb = new StringBuilder();
                    string line = "";

                    foreach (IHardware hardware in computer.Hardware)
                    {
                        line = $"Hardware: {hardware.Name} identifier: {hardware.Identifier}";
                        Trace.WriteLine(line);
                        if (_dumphwm) sb.AppendLine(line);

                        foreach (IHardware subhardware in hardware.SubHardware)
                        {
                            line = $"\tSubhardware: {subhardware.Name} identifier: {subhardware.Identifier}";
                            Trace.WriteLine(line);
                            if (_dumphwm) sb.AppendLine(line);

                            foreach (ISensor sensor in subhardware.Sensors)
                            {
                                line = $"\tSensor: {sensor.Name}, value: {sensor.Value}, max: {sensor.Max}, min: {sensor.Min}, index: {sensor.Index}, control: {sensor.Control}, identifier: {sensor.Identifier}, type: {sensor.SensorType}";
                                Trace.WriteLine(line);
                                if (_dumphwm) sb.AppendLine(line);

                                foreach (IParameter parameter in sensor.Parameters)
                                {
                                    line = $"\t\t\tParameter: {parameter.Name}, value: {parameter.Value}, Sensor: {parameter.Sensor}, desc: {parameter.Description}";
                                    Trace.WriteLine(line);
                                    if (_dumphwm) sb.AppendLine(line);
                                }
                            }
                        }

                        foreach (ISensor sensor in hardware.Sensors)
                        {
                            line = $"\tSensor: {sensor.Name}, value: {sensor.Value}, max: {sensor.Max}, min: {sensor.Min}, index: {sensor.Index}, control: {sensor.Control}, identifier: {sensor.Identifier}, type: {sensor.SensorType}";
                            Trace.WriteLine(line);
                            if (_dumphwm) sb.AppendLine(line);

                            foreach (IParameter parameter in sensor.Parameters)
                            {
                                line = $"\t\tParameter: {parameter.Name}, value: {parameter.Value}, Sensor: {parameter.Sensor}, desc: {parameter.Description}";
                                Trace.WriteLine(line);
                                if (_dumphwm) sb.AppendLine(line);
                            }
                        }
                    }
                    if (_dumphwm)
                    {
                        string path = @".\dumphwm.txt";
                        if (!File.Exists(path)) File.Delete(path);

                        using (StreamWriter sw = File.CreateText(path))
                        {
                            sw.WriteLine(sb.ToString());
                        }
                    }
                    _dumphwm = false;
                }

                if ((_debug || _dumphwmidle) && MonitoringIdle)
                {

                    StringBuilder sb = new StringBuilder();
                    string line = "";

                    foreach (IHardware hardware in computer.Hardware)
                    {
                        line = $"Hardware: {hardware.Name} identifier: {hardware.Identifier}";
                        Trace.WriteLine(line);
                        if (_dumphwmidle) sb.AppendLine(line);

                        foreach (IHardware subhardware in hardware.SubHardware)
                        {
                            line = $"\tSubhardware: {subhardware.Name} identifier: {subhardware.Identifier}";
                            Trace.WriteLine(line);
                            if (_dumphwmidle) sb.AppendLine(line);

                            foreach (ISensor sensor in subhardware.Sensors)
                            {
                                line = $"\tSensor: {sensor.Name}, value: {sensor.Value}, max: {sensor.Max}, min: {sensor.Min}, index: {sensor.Index}, control: {sensor.Control}, identifier: {sensor.Identifier}, type: {sensor.SensorType}";
                                Trace.WriteLine(line);
                                if (_dumphwmidle) sb.AppendLine(line);

                                foreach (IParameter parameter in sensor.Parameters)
                                {
                                    line = $"\t\t\tParameter: {parameter.Name}, value: {parameter.Value}, Sensor: {parameter.Sensor}, desc: {parameter.Description}";
                                    Trace.WriteLine(line);
                                    if (_dumphwmidle) sb.AppendLine(line);
                                }
                            }
                        }

                        foreach (ISensor sensor in hardware.Sensors)
                        {
                            line = $"\tSensor: {sensor.Name}, value: {sensor.Value}, max: {sensor.Max}, min: {sensor.Min}, index: {sensor.Index}, control: {sensor.Control}, identifier: {sensor.Identifier}, type: {sensor.SensorType}";
                            Trace.WriteLine(line);
                            if (_dumphwmidle) sb.AppendLine(line);

                            foreach (IParameter parameter in sensor.Parameters)
                            {
                                line = $"\t\tParameter: {parameter.Name}, value: {parameter.Value}, Sensor: {parameter.Sensor}, desc: {parameter.Description}";
                                Trace.WriteLine(line);
                                if (_dumphwmidle) sb.AppendLine(line);
                            }
                        }
                    }
                    if (_dumphwmidle)
                    {
                        string path = @".\dumphwmidle.txt";
                        if (!File.Exists(path)) File.Delete(path);

                        using (StreamWriter sw = File.CreateText(path))
                        {
                            sw.WriteLine(sb.ToString());
                        }
                    }
                    _dumphwmidle = false;
                }


            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("HWM Monitoring cycle exiting due to cancellation");
                throw;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"HWM Monitoring cycle exception: {e}");
            }
            finally
            {
                App.hwmtimer.Interval = MonitoringPooling;
                //Trace.WriteLine($"HWM MONITOR TICK {MonitoringPooling}ms");
            }
        }

        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
    }
}