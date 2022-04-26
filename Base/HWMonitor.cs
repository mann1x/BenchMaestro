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
using System.Text.RegularExpressions;
using System.ComponentModel;

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
        public static DateTime MonitoringBenchStartedTS = DateTime.MinValue;
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
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUSAVoltage, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Voltage));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUIOVoltage, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Voltage));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD2Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CCDSTemp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CoresTemp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresVoltages, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Voltage));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresPower, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresTemps, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPULogicalsLoad, HWSensorValues.MultiLogical, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load));
            App.hwsensors.Add(new HWSensorItem(HWSensorName.CPULogicalsScores, HWSensorValues.MultiLogical, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load));
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

        public static void ReInit(bool _board = true, bool _gpu = false)
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

        public static void UpdateZenSensors()
        {
            try
            {
                bool _refreshpt = App.systemInfo.ZenRefreshPowerTable();

                float _livetdc = 0;
                float _liveedc = 0;
                float _livevcore = 0;

                if (!_refreshpt)
                {
                    Trace.WriteLine($"ZenRefreshPowerTable Error, skip to next cycle");
                    return;
                }

                float _cpuVcore, _cpuVsoc;
                (_cpuVcore, _cpuVsoc) = App.systemInfo.Zen.GetCpuVcc();
                //Trace.WriteLine($"_cpuVcore: {_cpuVcore} _cpuVsoc: {_cpuVsoc}");

                float _cpuBusClock;
                _cpuBusClock = App.systemInfo.Zen.GetCpuBusClock();
                //Trace.WriteLine($"_cpuBusClock: {_cpuBusClock}");

                if (_cpuVcore > 0)
                {
                    App.hwsensors.UpdateZenSensor(HWSensorName.CPUVoltage, _cpuVcore);
                    _livevcore = _cpuVcore;
                }
                if (_cpuVsoc > 0)
                {
                    App.hwsensors.UpdateZenSensor(HWSensorName.SOCVoltage, _cpuVsoc);
                }
                if (_cpuBusClock > 0)
                {
                    App.hwsensors.UpdateZenSensor(HWSensorName.CPUFSB, _cpuBusClock);
                }

                foreach (HWSensorItem _sensor in App.hwsensors.Where(sensorItem => sensorItem.Source == HWSensorSource.Zen && sensorItem.Enabled && sensorItem.ZenPTOffset >= 0))
                {
                        if (_sensor.SensorValues == HWSensorValues.Single)
                        {
                            //Trace.WriteLine($"Zen Sensor update {_sensor.Name} PTOffset={_sensor.ZenPTOffset}");
                            float _value = App.systemInfo.Zen.powerTable.Table[_sensor.ZenPTOffset] * _sensor.ZenMulti;
                            _sensor.Values.Add(_value);
                            if (_sensor.Name == HWSensorName.CPUTDC) _livetdc = _value;
                            if (_sensor.Name == HWSensorName.CPUEDC) _liveedc = _value;
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
                        //Trace.Write($"_GetCpuLoad [{cpuLoad.GetCpuCount()}] read #{i} ");
                        float _value = cpuLoad.GetCpuLoad(i);
                        //Trace.WriteLine($" {_value}%");
                        _sensor.MultiValues[i].Values.Add(_value);
                        if (_value > 98)
                        {
                            int _core = ProcessorInfo.PhysicalCore(i);
                            float? _c0 = App.hwsensors.GetValue(HWSensorName.CPUCoresC0, _core);
                            //Trace.WriteLine($"Test Stretch CPU={i} [{_core}] C0={_c0}");
                            if (App.hwsensors.IsAny(HWSensorName.CPUCoresClocks,_core) && App.hwsensors.IsAny(HWSensorName.CPUCoresEffClocks, _core) && _c0 >= 99)
                            {
                                float? _stretch = App.hwsensors.GetValue(HWSensorName.CPUCoresClocks, _core) - App.hwsensors.GetValue(HWSensorName.CPUCoresEffClocks, _core);
                                //Trace.WriteLine($"Stretch {_stretch} MHz");
                                if (_stretch > 0)
                                {
                                    App.hwsensors.UpdateZenSensor(HWSensorName.CPUCoresStretch, _stretch, _core);
                                    //Trace.WriteLine($"Add Stretch [{_core}] {_stretch} MHz");
                                }
                            }
                        }
                    }
                }

                float? _cputemp = App.systemInfo.Zen.GetCpuTemperature();
                float? _cpuload = cpuLoad.GetTotalLoad();
                App.hwsensors.UpdateZenSensor(HWSensorName.CPULoad, _cpuload);
                App.hwsensors.UpdateZenSensor(HWSensorName.CPUTemp, _cputemp);
                //Trace.WriteLine($"Zen CPU Temp {App.systemInfo.Zen.GetCpuTemperature()}");

                if (App.systemInfo.ZenPerCCDTemp)
                {
                    {
                        float? ccd1temp = App.systemInfo.Zen.GetSingleCcdTemperature(0);
                        //Trace.WriteLine($"CCD1T={ccd1temp}");
                        if (ccd1temp != null) App.hwsensors.UpdateZenSensor(HWSensorName.CCD1Temp, ccd1temp);
                        float? ccd2temp = App.systemInfo.Zen.GetSingleCcdTemperature(1);
                        //Trace.WriteLine($"CCD2T={ccd2temp}");
                        if (ccd2temp != null) App.hwsensors.UpdateZenSensor(HWSensorName.CCD2Temp, ccd2temp);
                        if (ccd1temp != null && ccd2temp != null) App.hwsensors.UpdateZenSensor(HWSensorName.CCDSTemp, (ccd1temp + ccd2temp) / 2);
                    }
                }

                float? _cpupower = App.hwsensors.GetValue(HWSensorName.CPUPower);
                App.systemInfo.UpdateLiveCPUClock($"{Math.Round((double)_cpuload, 0)}% CPU Load");
                App.systemInfo.UpdateLiveCPUTemp($"{Math.Round((double)_cputemp, 1).ToString("0.0")}°C");
                App.systemInfo.UpdateLiveCPUPower($"{Math.Round((double)_cpupower, 0)}W");
                string _liveadditional = "";
                if (_livevcore > 0) _liveadditional += $"vCore: {Math.Round((double)_livevcore, 4).ToString("0.000")}V\n";
                if (_livetdc > 0) _liveadditional += $"TDC: {Math.Round((double)_livetdc, 0)}A ";
                if (_liveedc > 0) _liveadditional += $"EDC: {Math.Round((double)_liveedc, 0)}A ";
                if (_liveadditional.Length > 0)
                    App.systemInfo.UpdateLiveCPUAdditional(_liveadditional);
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

            if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("Core #"))
            {
                foreach (HWSensorItem _sensor in App.hwsensors.Where(sensorItem => sensorItem.Name == HWSensorName.CPUCoresClocks && sensorItem.Source == HWSensorSource.Libre))
                {
                    foreach (HWSensorMultiValues _sensorValues in _sensor.MultiValues.Where(sensorValue => sensorValue.LibreIdentifier == sensor.Identifier.ToString()))
                    {
                        _sensorValues.Values.Add(sensor.Value);
                    }
                }
            }

            else if (sensor.SensorType == SensorType.Power && sensor.Name.Contains("Core #"))
            {
                foreach (HWSensorItem _sensor in App.hwsensors.Where(sensorItem => sensorItem.Name == HWSensorName.CPUCoresPower && sensorItem.Source == HWSensorSource.Libre))
                {
                    foreach (HWSensorMultiValues _sensorValues in _sensor.MultiValues.Where(sensorValue => sensorValue.LibreIdentifier == sensor.Identifier.ToString()))
                    {
                        _sensorValues.Values.Add(sensor.Value);
                    }
                }
            }

            else if (sensor.SensorType == SensorType.Voltage && sensor.Name.Contains("Core #"))
            {
                foreach (HWSensorItem _sensor in App.hwsensors.Where(sensorItem => sensorItem.Name == HWSensorName.CPUCoresVoltages && sensorItem.Source == HWSensorSource.Libre))
                {
                    foreach (HWSensorMultiValues _sensorValues in _sensor.MultiValues.Where(sensorValue => sensorValue.LibreIdentifier == sensor.Identifier.ToString()))
                    {
                        _sensorValues.Values.Add(sensor.Value);
                    }
                }
            }

            else if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #"))
            {
                foreach (HWSensorItem _sensor in App.hwsensors.Where(sensorItem => sensorItem.Name == HWSensorName.CPULogicalsLoad && sensorItem.Source == HWSensorSource.Libre))
                {
                    List<HWSensorMultiValues> _sensorValues2 = _sensor.MultiValues;
                    //Trace.WriteLine($"1 = Sensor update {_sensor.Name} c={_sensorValues2.Count} {sensor.Identifier}={sensor.Value}");
                    foreach (HWSensorMultiValues _sensorValues in _sensorValues2)
                    {
                        if (_sensorValues.LibreIdentifier == sensor.Identifier.ToString()) { 
                            //Trace.WriteLine($"Sensor update {sensor.Identifier}={sensor.Value}");
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

            if (CPUSource == HWSensorSource.Libre)
            {
                float? _cputemp = App.hwsensors.GetValue(HWSensorName.CPUTemp);
                float? _cpupower = App.hwsensors.GetValue(HWSensorName.CPUPower);
                float? _cpuclock = App.hwsensors.GetValue(HWSensorName.CPUClock);
                float? _cpuload = App.hwsensors.GetValue(HWSensorName.CPULoad);
                if (_cputemp > 0) App.systemInfo.UpdateLiveCPUTemp($"{Math.Round((double)_cputemp, 1)}°C");
                string _strclock = "";
                if (_cpuclock > 0) _strclock += $"{Math.Round((double)_cpuclock, 0)} MHz";
                if (_cpuclock > 0 && _cpuload > 0) _strclock += " @ ";
                if (_cpuload > 0) _strclock += $"{Math.Round((double)_cpuload, 0)}% CPU Load";
                if (_strclock.Length < 1) _strclock = "N/A";
                App.systemInfo.UpdateLiveCPUClock(_strclock);
                if (_cpupower > 0) App.systemInfo.UpdateLiveCPUPower($"{Math.Round((double)_cpupower, 0)}W");
            }
        }
        public static void CheckSensor(IHardware hardware, ISensor sensor)
        {

            try
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

                if (hardware.HardwareType == HardwareType.Cpu && sensor.SensorType == SensorType.Temperature && (sensor.Name.StartsWith("Core (Tctl/Tdie)") || sensor.Name.Contains("CPU Package")) && CPUSource == HWSensorSource.Libre)
                {
                    App.hwsensors.InitLibre(HWSensorName.CPUTemp, sensor.Identifier.ToString(), sensor.Name);
                    Trace.WriteLine($"Libre CPU Temp Sensor added {sensor.Identifier} HW={hardware.Identifier}");
                }

                if (hardware.HardwareType == HardwareType.Cpu && sensor.SensorType == SensorType.Load && sensor.Name == "CPU Total" && CPUSource == HWSensorSource.Libre)
                {
                    App.hwsensors.InitLibre(HWSensorName.CPULoad, sensor.Identifier.ToString(), sensor.Name);
                    Trace.WriteLine($"Libre CPU Load Sensor added {sensor.Identifier} HW={hardware.Identifier}");
                }

                if (hardware.HardwareType == HardwareType.Cpu && sensor.SensorType == SensorType.Power && (sensor.Name == "Package" || sensor.Name == "CPU Package") && CPUSource == HWSensorSource.Libre)
                {
                    App.hwsensors.InitLibre(HWSensorName.CPUPower, sensor.Identifier.ToString(), sensor.Name);
                    Trace.WriteLine($"Libre CPU Power Sensor added {sensor.Identifier} HW={hardware.Identifier}");
                }

                if (hardware.HardwareType == HardwareType.Cpu && sensor.SensorType == SensorType.Voltage && sensor.Name == "Core (SVI2 TFN)" && CPUSource == HWSensorSource.Libre)
                {
                    App.hwsensors.InitLibre(HWSensorName.CPUVoltage, sensor.Identifier.ToString(), sensor.Name);
                    Trace.WriteLine($"Libre AMD CPU Voltage Sensor added {sensor.Identifier} HW={hardware.Identifier}");
                }

                if (hardware.HardwareType == HardwareType.Motherboard && sensor.SensorType == SensorType.Voltage && sensor.Name == "Vcore" && CPUSource == HWSensorSource.Libre)
                {
                    App.hwsensors.InitLibre(HWSensorName.CPUVoltage, sensor.Identifier.ToString(), sensor.Name);
                    Trace.WriteLine($"Libre Intel CPU Voltage Sensor added {sensor.Identifier} HW={hardware.Identifier}");
                }

                if (hardware.HardwareType == HardwareType.Motherboard && sensor.SensorType == SensorType.Voltage && sensor.Name == "CPU SA" && CPUSource == HWSensorSource.Libre)
                {
                    App.hwsensors.InitLibre(HWSensorName.CPUSAVoltage, sensor.Identifier.ToString(), sensor.Name);
                    Trace.WriteLine($"Libre CPUSA Voltage Sensor added {sensor.Identifier} HW={hardware.Identifier}");
                }

                if (hardware.HardwareType == HardwareType.Motherboard && sensor.SensorType == SensorType.Voltage && sensor.Name == "CPU I/O" && CPUSource == HWSensorSource.Libre)
                {
                    App.hwsensors.InitLibre(HWSensorName.CPUIOVoltage, sensor.Identifier.ToString(), sensor.Name);
                    Trace.WriteLine($"Libre CPUIO Voltage Sensor added {sensor.Identifier} HW={hardware.Identifier}");
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

                if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("Core #") && CPUSource == HWSensorSource.Libre)
                {
                    Match match = Regex.Match(sensor.Name, @".*Core #(?<core>\d+).*$", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string _core = match.Groups[1].Value;
                        Trace.WriteLine($"Libre CPU Cores Clocks #{_core}");
                        int _index = Int32.Parse(_core);
                        App.hwsensors.InitLibreMulti(HWSensorName.CPUCoresClocks, hardware.Identifier.ToString(), "Cores Clock", _index, sensor.Identifier.ToString(), sensor.Name);
                        Trace.WriteLine($"Libre CPU Cores Clocks #{_index} Sensor added #{sensor.Index} {sensor.Identifier} HW={hardware.Identifier}");
                    }
                }

                if (sensor.SensorType == SensorType.Power && sensor.Name.Contains("Core #") && CPUSource == HWSensorSource.Libre)
                {
                    Match match = Regex.Match(sensor.Name, @".*Core #(?<core>\d+).*$", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string _core = match.Groups[1].Value;
                        Trace.WriteLine($"Libre CPU Cores Power #{_core}");
                        int _index = Int32.Parse(_core);
                        App.hwsensors.InitLibreMulti(HWSensorName.CPUCoresPower, hardware.Identifier.ToString(), "CPU Cores Power", _index, sensor.Identifier.ToString(), sensor.Name);
                        Trace.WriteLine($"Libre CPU Cores Power #{_index} Sensor added #{sensor.Index} {sensor.Identifier} HW={hardware.Identifier}");
                    }
                }

                if (sensor.SensorType == SensorType.Voltage && sensor.Name.Contains("Core #") && CPUSource == HWSensorSource.Libre)
                {
                    Match match = Regex.Match(sensor.Name, @".*Core #(?<core>\d+).*$", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string _core = match.Groups[1].Value;
                        Trace.WriteLine($"Libre CPU Cores Voltage #{_core}");
                        int _index = Int32.Parse(_core);
                        App.hwsensors.InitLibreMulti(HWSensorName.CPUCoresVoltages, hardware.Identifier.ToString(), "CPU Cores Voltage", _index, sensor.Identifier.ToString(), sensor.Name);
                        Trace.WriteLine($"Libre CPU Cores Voltage #{_index} Sensor added #{sensor.Index} {sensor.Identifier} HW={hardware.Identifier}");
                    }
                }

                if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("Core #") && !sensor.Name.Contains("Distance to TjMax") && CPUSource == HWSensorSource.Libre)
                {
                    Match match = Regex.Match(sensor.Name, @".*Core #(?<core>\d+).*$", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string _core = match.Groups[1].Value;
                        Trace.WriteLine($"Libre CPU Cores Temperature #{_core}");
                        int _index = Int32.Parse(_core);
                        App.hwsensors.InitLibreMulti(HWSensorName.CPUCoresTemps, hardware.Identifier.ToString(), "CPU Cores Temperature", _index, sensor.Identifier.ToString(), sensor.Name);
                        Trace.WriteLine($"Libre CPU Cores Temperature #{_index} Sensor added #{sensor.Index} {sensor.Identifier} HW={hardware.Identifier}");
                    }
                }

                if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #") && CPUSource == HWSensorSource.Libre)
                {
                    Match match = Regex.Match(sensor.Name, @".*Core #(?<core>\d+).*$", RegexOptions.IgnoreCase);
                    Match matcht = Regex.Match(sensor.Name, @".*Core #(?<cpu>\d+) Thread #(?<thread>\d+).*$", RegexOptions.IgnoreCase);
                    if (matcht.Success || match.Success)
                    {
                        Match matchid = Regex.Match(sensor.Identifier.ToString(), @".*cpu/\d+/load/(?<cpu>\d+)$", RegexOptions.IgnoreCase);
                        if (matchid.Success)
                        {
                            string _cpu = matchid.Groups[1].Value;
                            string _thread = matchid.Groups[2].Value;
                            int _index = Int32.Parse(_cpu);
                            App.hwsensors.InitLibreMulti(HWSensorName.CPULogicalsLoad, hardware.Identifier.ToString(), "CPU Cores Load", _index, sensor.Identifier.ToString(), sensor.Name);
                            if (matcht.Success)
                            {
                                Trace.WriteLine($"Libre CPU Cores w/Threads Load #{_cpu} #{_thread}");
                                Trace.WriteLine($"Libre CPU Cores w/Threads Load #{_index} Sensor added #{sensor.Index} {sensor.Identifier} HW={hardware.Identifier}");
                            }
                            if (match.Success)
                            {
                                Trace.WriteLine($"Libre CPU Cores Load #{_cpu}");
                                Trace.WriteLine($"Libre CPU Cores Load #{_index} Sensor added #{sensor.Index} {sensor.Identifier} HW={hardware.Identifier}");
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Libre CheckSensor Exception: {ex}");
            }

        }

        public static void DumpHWM(bool _debug)
        {
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
                    sb.AppendLine();
                    sb.AppendLine(computer.GetReport()); 
                    string path = @".\Logs\dumphwm.txt";
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
                    string path = @".\Logs\dumphwmidle.txt";
                    if (!File.Exists(path)) File.Delete(path);

                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine(sb.ToString());
                    }
                }
                _dumphwmidle = false;
            }

        }

        public static void ParseMonitoring()
        {
            TimeSpan _deltarun = MonitoringStart - MonitoringEnd;

            Thread.Sleep(250);

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
            double _coresmaxsc = -99999;
            double _coresavgsc = -99999;

            Trace.WriteLine($"GET STATS FOR {App.CurrentRun.RunCores.Count} CORES");

            App.CurrentRun.CPUMaxTemp = (double)(App.hwsensors.GetMax(HWSensorName.CPUTemp) + App.hwsensors.GetOffset(HWSensorName.CPUTemp));
            App.CurrentRun.CPUAvgTemp = (double)(App.hwsensors.GetAvg(HWSensorName.CPUTemp) + App.hwsensors.GetOffset(HWSensorName.CPUTemp));
            Trace.WriteLine($"CPU AVG TEMP {App.CurrentRun.CPUAvgTemp} MAX {App.CurrentRun.CPUMaxTemp}");

            App.CurrentRun.CPUMaxPower = (double)App.hwsensors.GetMax(HWSensorName.CPUPower);
            App.CurrentRun.CPUAvgPower = (double)App.hwsensors.GetAvg(HWSensorName.CPUPower);
            Trace.WriteLine($"CPU AVG POWER {App.CurrentRun.CPUAvgPower} MAX {App.CurrentRun.CPUMaxPower}");

            App.CurrentRun.CPUMaxVoltage = (double)App.hwsensors.GetMax(HWSensorName.CPUVoltage);
            App.CurrentRun.CPUAvgVoltage = (double)App.hwsensors.GetAvg(HWSensorName.CPUVoltage);
            Trace.WriteLine($"CPU AVG VOLTAGE {App.CurrentRun.CPUAvgVoltage} MAX {App.CurrentRun.CPUMaxVoltage}");

            App.CurrentRun.CPUIOMaxVoltage = (double)App.hwsensors.GetMax(HWSensorName.CPUIOVoltage);
            App.CurrentRun.CPUIOAvgVoltage = (double)App.hwsensors.GetAvg(HWSensorName.CPUIOVoltage);
            Trace.WriteLine($"CPUIO AVG VOLTAGE {App.CurrentRun.CPUIOAvgVoltage} MAX {App.CurrentRun.CPUIOMaxVoltage}");

            App.CurrentRun.CPUSAMaxVoltage = (double)App.hwsensors.GetMax(HWSensorName.CPUSAVoltage);
            App.CurrentRun.CPUSAAvgVoltage = (double)App.hwsensors.GetAvg(HWSensorName.CPUSAVoltage);
            Trace.WriteLine($"CPUSA AVG VOLTAGE {App.CurrentRun.CPUSAAvgVoltage} MAX {App.CurrentRun.CPUSAMaxVoltage}");

            App.CurrentRun.SOCMaxVoltage = (double)App.hwsensors.GetMax(HWSensorName.SOCVoltage);
            App.CurrentRun.SOCAvgVoltage = (double)App.hwsensors.GetAvg(HWSensorName.SOCVoltage);
            Trace.WriteLine($"CPU AVG SOC {App.CurrentRun.SOCAvgVoltage} MAX {App.CurrentRun.SOCMaxVoltage}");

            App.CurrentRun.CCD1MaxTemp = (double)(App.hwsensors.GetMax(HWSensorName.CCD1Temp) + App.hwsensors.GetOffset(HWSensorName.CCD1Temp));
            App.CurrentRun.CCD1AvgTemp = (double)(App.hwsensors.GetAvg(HWSensorName.CCD1Temp) + App.hwsensors.GetOffset(HWSensorName.CCD1Temp));
            Trace.WriteLine($"CPU AVG CCD1 {App.CurrentRun.CCD1AvgTemp} MAX {App.CurrentRun.CCD1MaxTemp}");

            App.CurrentRun.CCD2MaxTemp = (double)(App.hwsensors.GetMax(HWSensorName.CCD2Temp) + App.hwsensors.GetOffset(HWSensorName.CCD2Temp));
            App.CurrentRun.CCD2AvgTemp = (double)(App.hwsensors.GetAvg(HWSensorName.CCD2Temp) + App.hwsensors.GetOffset(HWSensorName.CCD2Temp));
            Trace.WriteLine($"CPU AVG CCD2 {App.CurrentRun.CCD2AvgTemp} MAX {App.CurrentRun.CCD2MaxTemp}");

            App.CurrentRun.CCDSAvgTemp = (double)App.hwsensors.GetMax(HWSensorName.CCDSTemp);
            Trace.WriteLine($"CPU AVG CCDS {App.CurrentRun.CCDSAvgTemp}");

            App.CurrentRun.CPUFSBAvg = (double)App.hwsensors.GetAvg(HWSensorName.CPUFSB);
            App.CurrentRun.CPUFSBMax = (double)App.hwsensors.GetMax(HWSensorName.CPUFSB);
            Trace.WriteLine($"CPU AVG FSB {App.CurrentRun.CPUFSBAvg} MAX {App.CurrentRun.CPUFSBMax}");

            App.CurrentRun.CPUPPTMax = (double)App.hwsensors.GetMax(HWSensorName.CPUPPT);
            App.CurrentRun.CPUPPTAvg = (double)App.hwsensors.GetAvg(HWSensorName.CPUPPT);
            Trace.WriteLine($"CPU AVG PPT {App.CurrentRun.CPUPPTAvg} MAX {App.CurrentRun.CPUPPTMax}");

            App.CurrentRun.CPUTDCMax = (double)App.hwsensors.GetMax(HWSensorName.CPUTDC);
            App.CurrentRun.CPUTDCAvg = (double)App.hwsensors.GetAvg(HWSensorName.CPUTDC);
            Trace.WriteLine($"CPU AVG TDC {App.CurrentRun.CPUTDCAvg} MAX {App.CurrentRun.CPUTDCMax}");

            App.CurrentRun.CPUEDCMax = (double)App.hwsensors.GetMax(HWSensorName.CPUEDC);
            App.CurrentRun.CPUEDCAvg = (double)App.hwsensors.GetAvg(HWSensorName.CPUEDC);
            Trace.WriteLine($"CPU AVG EDC {App.CurrentRun.CPUEDCAvg} MAX {App.CurrentRun.CPUEDCMax}");

            double _sensoravg = 0;
            double _sensormax = 0;

            for (int _core1 = 1; _core1 <= App.systemInfo.CPUCores; ++_core1)
            {
                bool _bold = false;
                if (App.CurrentRun.RunCores.Contains(_core1)) _bold = true;

                int _core = _core1 - 1;

                if (App.hwsensors.IsEnabled(HWSensorName.CPUCoresClocks))
                {
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
                        _coresavgv = _coresavgv == -99999 ? _sensoravg : (_coresavgv + _sensoravg) / 2;
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
                    _sensoravg = (double)(App.hwsensors.GetAvg(HWSensorName.CPUCoresTemps, _core) + App.hwsensors.GetOffset(HWSensorName.CPUCoresTemps));
                    _sensormax = (double)(App.hwsensors.GetMax(HWSensorName.CPUCoresTemps, _core) + App.hwsensors.GetOffset(HWSensorName.CPUCoresTemps));
                    if (_bold)
                    {
                        if (_coresmaxt < _sensormax && _sensormax > -99999) _coresmaxt = _sensormax;
                        _coresavgt = _coresavgt == -99999 ? _sensoravg : (_coresavgt + _sensoravg) / 2;
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
                    int _core = ProcessorInfo.PhysicalCore(_cpu - 1);
                    int _thread = ProcessorInfo.ThreadID(_cpu - 1);
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

            if (App.hwsensors.IsEnabled(HWSensorName.CPULogicalsScores))
            {
                string _unit = App.hwsensors.GetUnit(HWSensorName.CPULogicalsScores);
                for (int _cpu = 1; _cpu <= App.systemInfo.CPULogicalProcessors; ++_cpu)
                {
                    _coresavgsc = -99999;
                    _coresmaxsc = -99999;
                    bool _bold = true;
                    int _core = ProcessorInfo.PhysicalCore(_cpu - 1);
                    int _thread = ProcessorInfo.ThreadID(_cpu - 1);
                    _sensoravg = (double)App.hwsensors.GetAvg(HWSensorName.CPULogicalsScores, _cpu - 1);
                    _sensormax = (double)App.hwsensors.GetMax(HWSensorName.CPULogicalsScores, _cpu - 1);
                    if (_bold)
                    {
                        if (_coresmaxsc < _sensormax && _sensormax > -99999) _coresmaxsc = _sensormax;
                        _coresavgsc = _coresavgsc == -99999 ? _sensoravg : (_coresavgsc + _sensoravg) / 2;
                    }
                    double _scoreavg;
                    string _scoreavgscale;
                    double _scoremax;
                    string _scoremaxscale;

                    (_scoreavg, _scoreavgscale) = GetScaleValueAndPrefix(_coresavgsc);
                    (_scoremax, _scoremaxscale) = GetScaleValueAndPrefix(_coresmaxsc);

                    App.CurrentRun.CPULogicalsScores.Add(new DetailsGrid($"#{_core}T{_thread}", (float)Math.Round(_scoreavg, 2), (float)Math.Round(_scoremax, 2), _bold, "2", _unit, _scoreavgscale, _scoremaxscale));
                    Trace.WriteLine($"[CPU {_cpu} #{_core}T{_thread} Score Avg: {Math.Round(_scoreavg, 2)} {_scoreavgscale}{_unit} Max: {Math.Round(_scoremax, 2)} {_scoremaxscale}{_unit}]");
                }
            }

            App.CurrentRun.CoresAvgVoltage = _coresavgv;
            App.CurrentRun.CoresMaxVoltage = _coresmaxv;

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

            if (App.hwsensors.IsAny(HWSensorName.CCD1L3Temp) || App.hwsensors.IsAny(HWSensorName.CCD2L3Temp))
            {
                float _ccd1l3avg = (float)App.hwsensors.GetAvg(HWSensorName.CCD1L3Temp);
                float _ccd2l3avg = (float)App.hwsensors.GetAvg(HWSensorName.CCD2L3Temp);
                float _ccd1l3max = (float)App.hwsensors.GetMax(HWSensorName.CCD1L3Temp);
                float _ccd2l3max = (float)App.hwsensors.GetMax(HWSensorName.CCD2L3Temp);

                if (_ccd1l3avg > 0 && _ccd2l3avg > 0)
                {
                    _ccd1l3avg = _ccd1l3avg + (float)App.hwsensors.GetOffset(HWSensorName.CCD1L3Temp);
                    _ccd2l3avg = _ccd2l3avg + (float)App.hwsensors.GetOffset(HWSensorName.CCD2L3Temp);
                    _ccd1l3max = _ccd1l3max + (float)App.hwsensors.GetOffset(HWSensorName.CCD1L3Temp);
                    _ccd2l3max = _ccd2l3max + (float)App.hwsensors.GetOffset(HWSensorName.CCD2L3Temp);
                    App.CurrentRun.L3AvgTemp = (double)(_ccd1l3avg + _ccd2l3avg) / 2;
                    App.CurrentRun.L3MaxTemp = (double)(_ccd1l3max + _ccd2l3max) / 2;
                } 
                else if (_ccd1l3avg > 0)
                {
                    _ccd1l3avg = _ccd1l3avg + (float)App.hwsensors.GetOffset(HWSensorName.CCD1L3Temp);
                    _ccd1l3max = _ccd1l3max + (float)App.hwsensors.GetOffset(HWSensorName.CCD1L3Temp);
                    App.CurrentRun.L3AvgTemp = (double)_ccd1l3avg;
                    App.CurrentRun.L3MaxTemp = (double)_ccd1l3max;
                }
                else if (_ccd1l3avg > 0)
                {
                    _ccd2l3avg = _ccd2l3avg + (float)App.hwsensors.GetOffset(HWSensorName.CCD2L3Temp);
                    _ccd2l3max = _ccd2l3max + (float)App.hwsensors.GetOffset(HWSensorName.CCD2L3Temp);
                    App.CurrentRun.L3AvgTemp = (double)_ccd2l3avg;
                    App.CurrentRun.L3MaxTemp = (double)_ccd2l3max;
                }
            }

            if (App.systemInfo.ZenPPT > 0)
            {
                App.CurrentRun.CPUPPTAvgLimit = (int)Math.Round((double)(100 * App.CurrentRun.CPUPPTAvg) / App.systemInfo.ZenPPT);
                App.CurrentRun.CPUPPTMaxLimit = (int)Math.Round((double)(100 * App.CurrentRun.CPUPPTMax) / App.systemInfo.ZenPPT);
            }

            if (App.systemInfo.ZenTDC > 0)
            {
                App.CurrentRun.CPUTDCAvgLimit = (int)Math.Round((double)(100 * App.CurrentRun.CPUTDCAvg) / App.systemInfo.ZenTDC);
                App.CurrentRun.CPUTDCMaxLimit = (int)Math.Round((double)(100 * App.CurrentRun.CPUTDCMax) / App.systemInfo.ZenTDC);
            }

            if (App.systemInfo.ZenEDC > 0)
            {
                App.CurrentRun.CPUEDCAvgLimit = (int)Math.Round((double)(100 * App.CurrentRun.CPUEDCAvg) / App.systemInfo.ZenEDC);
                App.CurrentRun.CPUEDCMaxLimit = (int)Math.Round((double)(100 * App.CurrentRun.CPUEDCMax) / App.systemInfo.ZenEDC);
            }

            Trace.WriteLine($"MonitoringParsed");
            MonitoringParsed = true;
            MonitoringPause = false;
            MonitoringPooling = MonitoringPoolingSlow;
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
                    try
                    {
                        ParseMonitoring();
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"HWM ParseMonitoring Exception: {ex}");
                    }

                }

                if (!MonitoringPause)
                {
                    if (CPUSource == HWSensorSource.Zen)
                    {
                        if (cpuLoad.IsAvailable) cpuLoad.Update();
                        UpdateZenSensors();
                    }

                    bool _libre = false;

                    //if (MonitoringIdle) _libre = true;
                    if (MonitoringDevices.Contains(HWSensorDevice.CPU) && CPUSource == HWSensorSource.Libre)
                    {
                        //Trace.WriteLine("Libre CPU");
                        _libre = true;
                    }
                    if (MonitoringDevices.Contains(HWSensorDevice.GPU) && GPUSource == HWSensorSource.Libre)
                    {
                        //Trace.WriteLine("Libre GPU");
                        _libre = true;
                    }
                    if (MonitoringDevices.Contains(HWSensorDevice.MainBoard) && BoardSource == HWSensorSource.Libre)
                    {
                        //Trace.WriteLine("Libre Board");
                        _libre = true;
                    }

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

                            TimeSpan _deltastart = DateTime.Now - MonitoringBenchStartedTS;
                            Trace.WriteLine($"MonitoringStart Check for load on Logical ({_deltastart.TotalSeconds:0}s): {_cpu} {_cpuload}");
                            if (_deltastart.TotalSeconds > 10)
                            {
                                MonitoringStart = DateTime.Now;
                                MonitoringStarted = true;
                                MonitoringPooling = MonitoringPoolingSlow;
                                Trace.WriteLine($"MonitoringStart STARTED on timeout detection: {_deltastart.TotalSeconds:0}s {MonitoringStart}");
                            }
                            else if (_cpuload > 80)
                            {
                                MonitoringStart = DateTime.Now;
                                MonitoringStarted = true;
                                MonitoringPooling = MonitoringPoolingSlow;
                                Trace.WriteLine($"MonitoringStart STARTED on load on Logical: {_cpu} {_cpuload} {MonitoringStart}");
                            }
                            else if (_cpuload == -99999)
                            {
                                MonitoringStart = DateTime.Now;
                                MonitoringStarted = true;
                                MonitoringPooling = MonitoringPoolingSlow;
                                Trace.WriteLine($"MonitoringStart STARTED can't read Load on Logical: {_cpu} {MonitoringStart}");
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
                    int _cputemp = (int)App.hwsensors.GetValue(HWSensorName.CPUMBTemp);
                    if (App.hwsensors.IsEnabled(HWSensorName.CPUMBTemp) && _cputemp > 0)
                    {
                        IdleCurrentCPUTemp = _cputemp;
                    }
                    else
                    {
                        _cputemp = (int)App.hwsensors.GetValue(HWSensorName.CPUTemp);
                        if (_cputemp > 0) IdleCurrentCPUTemp = _cputemp;
                    }
                    IdleCurrentCPULoad = (int)App.hwsensors.GetValue(HWSensorName.CPULoad);
                    Trace.WriteLine($"MonitoringIdle current CPU Temp: {IdleCurrentCPUTemp} Load {IdleCurrentCPULoad}");

                    if (IdleCurrentCPUTemp == -99999) IdleCPUTempSensor = false;

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

                try
                {
                    DumpHWM(_debug);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"HWM Monitoring DumpHWM Exception: {ex}");
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

        public static (double, string) GetScaleValueAndPrefix(double value)
        {
            string prefix;
            if (value < 1e4) prefix = "";
            else if (value < 1e7) { prefix = "k"; value /= 1e3; }
            else if (value < 1e10) { prefix = "M"; value /= 1e6; }
            else if (value < 1e13) { prefix = "G"; value /= 1e9; }
            else if (value < 1e16) { prefix = "T"; value /= 1e12; }
            else if (value < 1e19) { prefix = "P"; value /= 1e15; }
            else if (value < 1e22) { prefix = "E"; value /= 1e18; }
            else if (value < 1e25) { prefix = "Z"; value /= 1e21; }
            else { prefix = "Y"; value /= 1e24; }
            return (value, prefix);
        }
        public static double SetScaleValueFromPrefix(double value, string _prefix)
        {
            string prefix = _prefix.Trim();
            if (prefix == "") return value;
            if (prefix == "Y" || prefix == "y") { value *= 1e24; }
            else if (prefix == "Z" || prefix == "z") { value *= 1e21; }
            else if (prefix == "E" || prefix == "e") { value *= 1e18; }
            else if (prefix == "P" || prefix == "p") { value *= 1e15; }
            else if (prefix == "T" || prefix == "t") { value *= 1e12; }
            else if (prefix == "G" || prefix == "g") { value *= 1e9; }
            else if (prefix == "M" || prefix == "m") { value *= 1e6; }
            else if (prefix == "K" || prefix == "k") { value *= 1e3; }
            return value;
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
