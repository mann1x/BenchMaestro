using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenchMaestro
{

    public class HWSensors<T> : IList<T>
    {
        private List<T> _innerList;
        private IEnumerable<T> _lazyLoader;
        private void ensureList()
        {
            if (_innerList == null)
                _innerList = new List<T>(_lazyLoader);
        }
        #region IList<T> Members
        public int IndexOf(T item)
        {
            ensureList();
            return _innerList.IndexOf(item);
        }
        public void Insert(int index, T item)
        {
            ensureList();
            _innerList.Insert(index, item);
        }
        public void RemoveAt(int index)
        {
            ensureList();
            _innerList.RemoveAt(index);
        }
        public T this[int index]
        {
            get
            {
                ensureList();
                return _innerList[index];
            }
            set
            {
                ensureList();
                _innerList[index] = value;
            }
        }
        #endregion
        #region ICollection<T> Members
        public void Add(T item)
        {
            ensureList();
            _innerList.Add(item);
        }
        public void Clear()
        {
            ensureList();
            _innerList.Clear();
        }
        public bool Contains(T item)
        {
            ensureList();
            return _innerList.Contains(item);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            ensureList();
            _innerList.CopyTo(array, arrayIndex);
        }
        public int Count
        {
            get { ensureList(); return _innerList.Count; }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }
        public bool Remove(T item)
        {
            ensureList();
            return _innerList.Remove(item);
        }
        #endregion
        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator()
        {
            ensureList();
            return _innerList.GetEnumerator();
        }
        #endregion
        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            ensureList();
            return _innerList.GetEnumerator();
        }
        #endregion
    }
    public static class HWSensors
    {
        public static void InitZen(this List<HWSensorItem> _sensors, HWSensorName _name, int _offset, int _multi = 1, bool _enabled = true)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name)
                    {
                        _sensor.Source = HWSensorSource.Zen;
                        _sensor.Enabled = _enabled;
                        _sensor.ZenMulti = _multi;
                        _sensor.ZenPTOffset = _offset;
                        Trace.WriteLine($"Zen Added {_name} Offset {_offset}");
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"HWSensor InitZen Exception: {ex}");
            }
        }
        public static void InitZenMulti(this List<HWSensorItem> _sensors, HWSensorName _name, int _offset, int _index, int _multi = 1, bool _enabled = true)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name)
                    {
                        _sensor.Source = HWSensorSource.Zen;
                        _sensor.Enabled = _enabled;
                        _sensor.ZenMulti = _multi;
                        _sensor.ZenPTOffset = _offset;

                        if (_sensor.SensorValues != HWSensorValues.Single)
                        {
                            _sensor.MultiValues.Add(new HWSensorMultiValues());
                            _sensor.MultiValues[_index - 1].ZenPTOffset = _offset;
                            Trace.WriteLine($"ZenMulti Added {_name} Index {_index - 1} Offset {_offset}");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"HWSensor InitZenMulti Exception: {ex}");
            }
        }

        public static void InitLibre(this List<HWSensorItem> _sensors, HWSensorName _name, string _libreId, string _libreLabel)
        {
            try
            {
                foreach (var _sensor in _sensors.Where(sensorItem => sensorItem.Source == HWSensorSource.Libre))
                {
                    if (_sensor.Name == _name)
                    {
                        _sensor.Enabled = true;
                        _sensor.LibreIdentifier = _libreId;
                        _sensor.LibreLabel = _libreLabel;
                        Trace.WriteLine($"Libre Added {_name} Label {_libreLabel} Id {_libreId}");
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"HWSensor InitLibre Exception: {ex}");
            }
        }

        public static void InitLibreMulti(this List<HWSensorItem> _sensors, HWSensorName _name, string _libreId, string _libreLabel, int _index, string _libreMultiId, string _libreMultiLabel)
        {
            try
            {
                foreach (var _sensor in _sensors.Where(sensorItem => sensorItem.Source == HWSensorSource.Libre))
                {
                    if (_sensor.Name == _name)
                    {
                        _sensor.Enabled = true;
                        _sensor.LibreIdentifier = _libreId;
                        _sensor.LibreLabel = _libreLabel;

                        if (_sensor.SensorValues != HWSensorValues.Single)
                        {
                            _sensor.MultiValues.Add(new HWSensorMultiValues());
                            _sensor.MultiValues[_index - 1].LibreIdentifier = _libreMultiId;
                            _sensor.MultiValues[_index - 1].LibreLabel = _libreMultiLabel;
                            Trace.WriteLine($"Libre Added {_name} Index {_index - 1} Label {_libreMultiLabel} Id {_libreMultiId}");
                        }


                    }
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"HWSensor InitLibreMulti Exception: {ex}");
            }
        }
        public static bool IsEnabled(this List<HWSensorItem> _sensors, HWSensorName _name)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name)
                    {
                        return _sensor.Enabled;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"HWSensor IsEnabled Exception: {ex}");
                return false;
            }
        }
        public static bool IsAny(this List<HWSensorItem> _sensors, HWSensorName _name, int _cpu = -1)
        {
            int _count = 0;
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name && _sensor.Enabled)
                    {
                        if (_sensor.SensorValues == HWSensorValues.Single)
                        {
                            if (_sensor.Values.Any())
                            {
                                return true;
                            }
                        }
                        else
                        {
                            _count = _sensor.MultiValues.Count;
                            if (_sensor.MultiValues[_cpu].Values.Any())
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"HWSensor IsAny Exception {_name} cpu={_cpu}/{_count}: {ex}");
                return false;
            }
        }
        public static float? GetValue(this List<HWSensorItem> _sensors, HWSensorName _name, int _cpu = -1)
        {
            int _count = 0;
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if(_sensor.Name == _name && _sensor.Enabled)
                    {
                        if (_sensor.SensorValues == HWSensorValues.Single)
                        {
                            if (_sensor.Values.Any())
                            {
                                return _sensor.Values.Last();
                            }
                        }
                        else
                        {
                            _count = _sensor.MultiValues.Count;
                            if (_sensor.MultiValues[_cpu].Values.Any())
                            {
                                return _sensor.MultiValues[_cpu].Values.Last();
                            }
                        }
                    }
                }
                return -99999;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"HWSensor GetValue Exception {_name} cpu={_cpu}/{_count}: {ex}");
                return -99999;
            }
        }
        public static float? GetMax(this List<HWSensorItem> _sensors, HWSensorName _name, int _cpu = -1)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name && _sensor.Enabled)
                    {
                        if (_sensor.SensorValues == HWSensorValues.Single)
                        {
                            if (_sensor.Values.Any())
                            {
                                return _sensor.Values.Max();
                            }
                        }
                            
                        else
                        {
                            if (_sensor.MultiValues[_cpu].Values.Any())
                            {
                                return _sensor.MultiValues[_cpu].Values.Max();
                            }
                        }
                    }
                }
                return -99999;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"HWSensor GetMax Exception: {ex}");
                return -99999;
            }
        }

        public static float? GetAvg(this List<HWSensorItem> _sensors, HWSensorName _name, int _cpu = -1)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name && _sensor.Enabled)
                    {
                        if (_sensor.SensorValues == HWSensorValues.Single)
                        {
                            if (_sensor.Values.Any())
                            {
                                return _sensor.Values.Average();
                            }
                        }
                        else
                        {
                            if (_sensor.MultiValues[_cpu].Values.Any())
                            {
                                return _sensor.MultiValues[_cpu].Values.Average();
                            }
                        }
                    }
                }
                return -99999;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"HWSensor GetAvg Exception: {ex}");
                return -99999;
            }
        }
        public static float? GetLastAvgMulti(this List<HWSensorItem> _sensors, HWSensorName _name)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name && _sensor.Enabled && _sensor.SensorValues != HWSensorValues.Single)
                    {
                        List<float?> _avg = new List<float?>();
                        foreach (var _sensorValues in _sensor.MultiValues)
                        {
                            _avg.Add(_sensorValues.Values.Last());
                        }
                        return _avg.Average();
                    }
                }
                return -99999;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"HWSensor GetLastAvgMulti Exception: {ex}");
                return -99999;
            }
        }

        public static void UpdateSensor(this List<HWSensorItem> _sensors, string _Identifier, float? _value)
        {
            try
            {
                foreach (var _sensor in _sensors.Where(sensorItem => sensorItem.Source == HWSensorSource.Libre))
                {
                    if (_sensor.SensorValues != HWSensorValues.Single)
                    {
                        foreach (var _mvalue in _sensor.MultiValues)
                        {
                            if (_mvalue.LibreIdentifier == _Identifier)
                            {
                                _mvalue.Values.Add(_value);
                            }
                        }
                    } 
                    else
                    {
                        if (_sensor.LibreIdentifier == _Identifier)
                        {
                            //if (_sensor.Name == HWSensorName.CPUPower) Trace.WriteLine($"\t\tPWR {_value}");
                            _sensor.Values.Add(_value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"HWSensor UpdateSensor Exception: {ex}");
            }
        }
        public static void UpdateZenSensor(this List<HWSensorItem> _sensors, HWSensorName _name, float? _value, int _core = -1)
        {
            try
            {
                foreach (var _sensor in _sensors.Where(sensorItem => sensorItem.Source == HWSensorSource.Zen && sensorItem.Name == _name))
                {
                    if (_sensor.SensorValues != HWSensorValues.Single)
                    {
                        _sensor.MultiValues[_core].Values.Add(_value);
                    }
                    else
                    {
                        _sensor.Values.Add(_value);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"HWSensor UpdateZenSensor Exception: {ex}");
            }
        }

        public static void Reset(this List<HWSensorItem> _sensors)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    _sensor.Values.Clear();
                    _sensor.Values.TrimExcess();

                    if (_sensor.SensorValues != HWSensorValues.Single) { 
                        foreach (HWSensorMultiValues _mValues in _sensor.MultiValues)
                        {
                            _mValues.Values.Clear();
                            _mValues.Values.TrimExcess();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"HWSensor Reset Exception: {ex}");
            }
        }

    }

    public class HWSensorMultiValues
    {
        public List<float?> Values { get; set; }
        public string LibreLabel { get; set; }
        public string LibreIdentifier { get; set; }
        public int ZenPTOffset { get; set; }
        public void Reset()
        {
            Values.Clear();
            Values.TrimExcess();

        }
        public HWSensorMultiValues()
        {
            LibreLabel = "";
            LibreIdentifier = "";
            Values = new();
        }
    }

    public class HWSensorItem
    {
        public HWSensorSource Source { get; set; }
        public List<float?> Values { get; set; }
        public List<HWSensorMultiValues> MultiValues { get; set; }
        public bool Enabled { get; set; }
        public string LibreLabel { get; set; }
        public string LibreIdentifier { get; set; }
        public string ManualLibreLabel { get; set; }
        public string ManialLibreIdentifier { get; set; }
        
        public int ZenPTOffset { get; set; }
        public int ZenMulti { get; set; }

        public HWSensorConfig ConfigType { get; set; }
        public HWSensorValues SensorValues { get; set; }
        public HWSensorType SensorType { get; set; }
        public HWSensorName Name { get; set; }
        public HWSensorDevice Device { get; set; }

        public HWSensorItem(HWSensorName _name, HWSensorValues _values, HWSensorConfig _config, HWSensorDevice _device, HWSensorType _type, HWSensorSource _source = HWSensorSource.Libre)
        {
            Name = _name;
            SensorValues = _values;
            ConfigType = _config;
            Device = _device;
            Source = _source;
            SensorType = _type;
            Enabled = false;
            LibreLabel = "";
            LibreIdentifier = "";
            ZenPTOffset = 0;
            ZenMulti = 1;
            Values = new();

            if (_values != HWSensorValues.Single)
            {
                MultiValues = new();
            }

            Trace.WriteLine($"Added {_name}");
        }
    }


    public enum HWSensorSource
    {
        Libre,
        Zen

    }
    public enum HWSensorConfig
    {
        Auto,
        Manual

    }

    public enum HWSensorValues
    {
        Single,
        MultiCore,
        MultiLogical

    }
    public enum HWSensorType
    {
        Clock,
        Voltage,
        Power,
        Amperage,
        Load,
        Temperature

    }
    public enum HWSensorDevice
    {
        CPU,
        MainBoard,
        GPU
    }
    public enum HWSensorName
    {
        CPUClock,
        CPUEffClock,
        CPULoad,
        CPUPower,
        CPUVoltage,
        CoresVoltage,
        CoresPower,
        CPUTemp,
        CPUMBTemp,
        CoresTemp,
        CPUFSB,
        SOCVoltage,
        CCD1Temp,
        CCD2Temp,
        CCDSTemp,
        CPUCoresVoltages,
        CPUCoresClocks,
        CPUCoresEffClocks,
        CPUCoresStretch,
        CPUCoresPower,
        CPUCoresTemps,
        CPUCoresC0,
        CPULogicalsLoad,
        CPUPPT,
        CPUTDC,
        CPUEDC,
        CCD1L3Temp,
        CCD2L3Temp,
    }
    public class DetailsGrid
    {
        public string Label { get; set; }
        public float? Val1 { get; set; }
        public float? Val2 { get; set; }
        public bool Bold { get; set; }
        public string Format { get; set; }
        public DetailsGrid(string _label, float? _val1, float? _val2, bool _bold, string _format)
        {
            Label = _label;
            Val1 = _val1;
            Val2 = _val2;
            Bold = _bold;
            Format = _format;
        }
    }

}
