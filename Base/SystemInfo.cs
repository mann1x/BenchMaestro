using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Text.RegularExpressions;
using ZenStates.Core;

namespace BenchMaestro
{
    public class SystemInfo
    {
        public string AppVersion { get; set; }
        public bool STMT { get; set; }
        public string BoardManufacturer { get; set; }
        public string BoardModel { get; set; }
        public string BoardBIOS { get; set; }
        public string CPUBits { get; set; }
        public string CPUDescription { get; set; }
        public string CPUName { get; set; }
        public int CPUFamily { get; set; }
        public int CPUCores { get; set; }
        public int CPUEnabledCores { get; set; }
        public int CPUThreads { get; set; }
        public string CPUSocket { get; set; }
        public int CPULogicalProcessors { get; set; }
        public string CPULabel { get; set; }
        public string BoardLabel { get; set; }
        public string ProcessorsLabel { get; set; }
        public bool HyperThreading { get; set; }
        public int[,] CPPC { get; set; }
        public int[] CPPCActiveOrder { get; set; }
       
        public int[] CPPCOrder { get; set; }
        public int[] CPPCOrder1 { get; set; }
        public int[] CPPCCustomOrder { get; set; }
        public bool CPPCCustomEnabled { get; set; }
        public string CPPCLabel { get; set; }
        public string CPPCActiveLabel { get; set; }
        public string CPPCTagsLabel { get; set; }
        public string CPPCPerfLabel { get; set; }
        public Cpu Zen { get; set; }
        public bool ZenStates { get; set; }
        public string ZenSMUVer { get; set; }
        public int ZenBoost { get; set; }
        public int ZenScalar { get; set; }
        public int ZenPPT { get; set; }
        public int ZenTDC { get; set; }
        public int ZenEDC { get; set; }
        public int ZenTHM { get; set; }
        public int ZenFCLK { get; set; }
        public int ZenUCLK { get; set; }
        public int ZenMCLK { get; set; }
        public float ZenMemRatio { get; set; }
        public bool ZenCOb { get; set; }
        public int[] ZenCO { get; set; }
        public int[] ZenCoreMap { get; set; }
        public int[] CPPCTags { get; set; }
        public string ZenCOLabel { get; set; }

        private int EmptyTags()
        {
            int _remaining = CPUCores;
            for(int i = 0; i <= CPUCores-1; i++)
            {
                if (CPPCTags[i] > 0) _remaining--;
            }
            return _remaining;
        }
        private bool CPPCFoundAlready(int _core)
        {
            for (int i = 0; i < CPPC.GetLength(0); i++)
            {
                if (CPPC[i, 0] == _core) return true;
            }
            return false;
        }

        private int[] RemoveIndices(int[] IndicesArray, int RemoveAt)
        {
            int[] newIndicesArray = new int[IndicesArray.Length - 1];

            int i = 0;
            int j = 0;
            while (i < IndicesArray.Length)
            {
                if (i != RemoveAt)
                {
                    newIndicesArray[j] = IndicesArray[i];
                    j++;
                }

                i++;
            }

            return newIndicesArray;
        }

        public static IEnumerable<T[]> Filter<T>(T[,] source, Func<T[], bool> predicate)
        {
            for (int i = 0; i < source.GetLength(0); ++i)
            {
                T[] values = new T[source.GetLength(1)];
                for (int j = 0; j < values.Length; ++j)
                {
                    values[j] = source[i, j];
                }
                if (predicate(values))
                {
                    yield return values;
                }
            }
        }

        public SystemInfo()
        {
            try
            {

                STMT = true;

                string ClassName = "Win32_BIOS";

                ManagementClass SIManagementClass = new ManagementClass(ClassName);
                //Create a ManagementObjectCollection to loop through
                ManagementObjectCollection SIManagemenobjCol = SIManagementClass.GetInstances();
                //Get the properties in the class
                PropertyDataCollection SIproperties = SIManagementClass.Properties;

                BoardBIOS = "N/A";

                foreach (ManagementObject obj in SIManagemenobjCol)
                {
                    foreach (PropertyData property in SIproperties)
                    {
                        try
                        {
                            if (property.Name == "Name" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    BoardBIOS = obj.Properties[property.Name].Value.ToString().Trim();
                            }
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine("WMI Win32_BIOS Exception: " + e);
                        }
                    }
                }


                ClassName = "Win32_BaseBoard";

                SIManagementClass = new ManagementClass(ClassName);
                SIManagemenobjCol = SIManagementClass.GetInstances();
                SIproperties = SIManagementClass.Properties;

                BoardManufacturer = "N/A";
                BoardModel = "N/A";

                foreach (ManagementObject obj in SIManagemenobjCol)
                {
                    foreach (PropertyData property in SIproperties)
                    {
                        try
                        {
                            if (property.Name == "Manufacturer" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    BoardManufacturer = obj.Properties[property.Name].Value.ToString().Trim();
                            }
                            if (property.Name == "Product" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    BoardModel = obj.Properties[property.Name].Value.ToString().Trim();
                            }
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine("WMI Win32_BaseBoard Exception: " + e);
                        }
                    }
                }

                ClassName = "Win32_Processor";

                SIManagementClass = new ManagementClass(ClassName);
                SIManagemenobjCol = SIManagementClass.GetInstances();
                SIproperties = SIManagementClass.Properties;

                CPUBits = "N/A";
                CPUDescription = "N/A";
                CPUName = "N/A";
                CPUCores = 1;
                CPUEnabledCores = 1;
                CPUThreads = 1;
                CPUSocket = "N/A";
                CPULogicalProcessors = 1;
                CPUFamily = 0;

                foreach (ManagementObject obj in SIManagemenobjCol)
                {
                    foreach (PropertyData property in SIproperties)
                    {
                        try
                        {
                            if (property.Name == "AddressWidth" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPUBits = obj.Properties[property.Name].Value.ToString().Trim();
                            }
                            if (property.Name == "Description" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPUDescription = obj.Properties[property.Name].Value.ToString().Trim();
                            }
                            if (property.Name == "Name" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPUName = obj.Properties[property.Name].Value.ToString().Trim();
                            }
                            if (property.Name == "NumberOfCores" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPUCores = Int32.Parse(obj.Properties[property.Name].Value.ToString().Trim());
                            }
                            if (property.Name == "NumberOfEnabledCore" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPUEnabledCores = Int32.Parse(obj.Properties[property.Name].Value.ToString().Trim());
                            }
                            if (property.Name == "NumberOfLogicalProcessors" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPUThreads = Int32.Parse(obj.Properties[property.Name].Value.ToString().Trim());
                            }
                            if (property.Name == "SocketDesignation" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPUSocket = obj.Properties[property.Name].Value.ToString().Trim();
                            }
                            if (property.Name == "NumberOfLogicalProcessors" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPULogicalProcessors = Int32.Parse(obj.Properties[property.Name].Value.ToString().Trim());
                            }

                            if (property.Name == "Family" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPUFamily = Int32.Parse(obj.Properties[property.Name].Value.ToString().Trim());
                            }

                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine("WMI Win32_Processor Exception: " + e);
                        }
                    }
                }

                if (CPULogicalProcessors > CPUCores) HyperThreading = true;
                
                CPPCTags = new int[CPUCores];

                string eventLogName = "System";

                string evtquery = "*[System/Provider/@Name=\"Microsoft-Windows-Kernel-Processor-Power\"]";

                try
                {
                    EventLogQuery elq = new EventLogQuery(eventLogName, PathType.LogName, evtquery);
                    elq.Session = new EventLogSession();

                    elq.ReverseDirection = true;

                    using (EventLogReader elr = new EventLogReader(elq))
                    {
                        StringBuilder sb = new StringBuilder();

                        EventRecord ev;
                        while ((ev = elr.ReadEvent()) != null)
                        {
                            if (ev.FormatDescription() != null && ev.Id == 55)
                            {
                                int _procid = 0;
                                int _procgroup = 0;
                                int _procperf = 0;

                                string _rawmessage = ev.FormatDescription();
                                sb.Append(_rawmessage);
                                sb.AppendLine();
                                sb.AppendLine();

                                using (var sreader = new StringReader(_rawmessage))
                                {
                                    string _message = sreader.ReadLine();
                                }
                                string pattern1 = @"Processor (?<procid>.*) in group (?<procgroup>.*) exposes the following power management capabilities:";
                                string pattern2 = @"Maximum performance percentage: (?<procperf>.*)";
                                Regex rgx1 = new Regex(pattern1, RegexOptions.Multiline);
                                Regex rgx2 = new Regex(pattern2, RegexOptions.Multiline);
                                Match m1 = rgx1.Match(_rawmessage);
                                Match m2 = rgx2.Match(_rawmessage);
                                if (m1.Success && m2.Success)
                                {
                                    string[] fields1 = rgx1.GetGroupNames();
                                    string[] fields2 = rgx2.GetGroupNames();
                                    foreach (var name in fields1)
                                    {
                                        Group grp = m1.Groups[name];
                                        if (name == "procid" && grp.Value.Length > 0) Trace.WriteLine($"{grp.Value.TrimEnd('\r', '\n')}");
                                        if (name == "procid" && grp.Value.Length > 0) _procid = Convert.ToInt32(grp.Value.TrimEnd('\r', '\n').Trim());
                                        if (name == "procgroup" && grp.Value.Length > 0) Trace.WriteLine($"{grp.Value.TrimEnd('\r', '\n')}");
                                        if (name == "procgroup" && grp.Value.Length > 0) _procgroup = Convert.ToInt32(grp.Value.TrimEnd('\r', '\n').Trim());
                                    }
                                    foreach (var name in fields2)
                                    {
                                        Group grp = m2.Groups[name];
                                        if (name == "procperf" && grp.Value.Length > 0) Trace.WriteLine($"{grp.Value.TrimEnd('\r', '\n')}");
                                        if (name == "procperf" && grp.Value.Length > 0) _procperf = Convert.ToInt32(grp.Value.TrimEnd('\r', '\n').Trim());
                                    }
                                    Trace.WriteLine($"{_procgroup} {_procid} {_procperf}");
                                    if (_procid == 0 || !HyperThreading || (HyperThreading && (_procid%2 == 0)))
                                    {
                                        int __procid = _procid == 0 ? _procid : HyperThreading ? _procid/2 : _procid;
                                        Trace.WriteLine($"Add Tag={__procid} {_procperf}");
                                        CPPCTags[__procid] = _procperf;
                                    }
                                    Trace.WriteLine($"EmptyTags={EmptyTags()}");
                                }
                                if (EmptyTags() <= 0) break;
                                //Trace.WriteLine(_rawmessage);
                            }

                        }

                        for (int i = 0; i < CPPCTags.Length; i++)
                        {
                            CPPCTagsLabel += String.Format("{0}#{1}", i, CPPCTags[i]);
                            if (i != CPPCTags.Length - 1) CPPCTagsLabel += ", ";
                        }
                        Trace.WriteLine($"CPPC: {CPPCTagsLabel}");

                        CPPC = new int[CPUCores, 2];
                        
                        for (int ii = 0; ii < CPPC.GetLength(0); ii++)
                        {
                            CPPC[ii, 0] = -100;
                            CPPC[ii, 1] = -100;
                        }

                        CPPCOrder = new int[CPUCores];
                        CPPCOrder1 = new int[CPUCores];

                        for (int i = 0; i < CPPC.GetLength(0); i++)
                        {
                            int _highestcpu = 0;
                            int _highestperf = 0;
                            for (int ix = CPPC.GetLength(0)-1; ix > -1; ix--)
                            {
                                Trace.WriteLine($"CPPC Testing ix: {ix} perf: {CPPCTags[ix]} IsIn: {CPPCFoundAlready(ix)}");
                                if (_highestperf <= CPPCTags[ix] && !CPPCFoundAlready(ix))
                                {
                                    Trace.WriteLine($"CPPC Highest ix: {ix} perf: {CPPCTags[ix]}");
                                    _highestperf = CPPCTags[ix];
                                    _highestcpu = ix;
                                }
                            }
                            CPPCOrder[i] = _highestcpu;
                            CPPCOrder1[i] = _highestcpu+1;
                            CPPC[i, 0] = _highestcpu;
                            CPPC[i, 1] = _highestperf;

                            string _cppctrace = "";
                            for (int iz = 0; iz < CPPC.GetLength(0); iz++)
                            {
                                _cppctrace += String.Format("{0}#{1} ", CPPC[iz, 0], CPPC[iz, 1]);
                            }
                            Trace.WriteLine($"CPPC: {_cppctrace}");

                            Trace.WriteLine($"CPPC i: {i} Core: {CPPC[i, 0]} Perf: {CPPC[i, 1]}");
                        }

                        for (int i = 0; i < CPPC.GetLength(0); i++)
                        {
                            CPPCPerfLabel += String.Format("{0}#{1}", CPPC[i, 0], CPPC[i, 1]);
                            if (i != CPPC.GetLength(0) - 1) CPPCPerfLabel += ", ";
                        }

                        for (int i = 0; i < CPPC.GetLength(0); i++)
                        {
                            CPPCLabel += String.Format("{0} ", CPPC[i, 0]);
                            if (i != CPPC.GetLength(0) - 1) CPPCLabel += ", ";

                        }

                        string path = @".\dumpcppc.txt";
                        if (!File.Exists(path)) File.Delete(path);

                        using (StreamWriter sw = File.CreateText(path))
                        {
                            sw.WriteLine(sb.ToString());
                        }


                    }
                }
                catch(Exception e)
                {
                    CPPCTagsLabel = "Failed parsing CPPC Tags from System Event Log";

                    CPPC = new int[CPUCores, 2];
                    for (int i = 0; i < CPPC.GetLength(0); i++)
                    {
                        CPPC[i, 0] = i;
                        CPPC[i, 1] = 100;
                    }
                    for (int i = 0; i < CPPC.GetLength(0); i++)
                        CPPCPerfLabel += String.Format("[{0} {1}] ", CPPC[i, 0], CPPC[i, 1]);

                    for (int i = 0; i < CPPC.GetLength(0); i++)
                    {
                        CPPCLabel += String.Format("{0} ", CPPC[i, 0]);
                        if (i != CPPC.GetLength(0) - 1) CPPCLabel += ", ";

                    }

                    Trace.WriteLine($"CPPC Tags EventReader Exception: {e}");
                }

                ZenStates = false;
                ZenBoost = 0;
                ZenScalar = 0;
                ZenPPT = 0;
                ZenTDC = 0;
                ZenEDC = 0;
                ZenTHM = 0;
                ZenFCLK = 0;
                ZenUCLK = 0;
                ZenMCLK = 0;
                ZenCO = new int[CPUCores];
                ZenCoreMap = new int[CPUCores];
                ZenCOb = false;
                ZenCOLabel = "";

                try
                {
                    if (CPUSocket == "AM4")
                    {
                        Zen = new Cpu();
                        bool smucheck = Zen.smu.Version == 0U;
                        Trace.WriteLine($"Test for Zen {smucheck}");
                        if (!smucheck)
                        {
                            ZenStates = true;
                            uint smu_ver = Zen.smu.Version;
                            uint ver_maj = smu_ver >> 16 & 255U;
                            uint ver_min = smu_ver >> 8 & 255U;
                            uint ver_rev = smu_ver & 255U;
                            ZenSMUVer = string.Format("{0}.{1}.{2}", ver_maj, ver_min, ver_rev);

                            uint[] args = new uint[6];
                            uint cmd = Zen.smu.Hsmp.ReadBoostLimit;
                            SMU.Status status = Zen.SendSmuCommand(Zen.smu.Hsmp, cmd, ref args);
                            if (status == SMU.Status.OK)
                            {
                                ZenBoost = (int)args[0];
                            }
                            status = Zen.RefreshPowerTable();
                            if (status == SMU.Status.OK)
                            {
                                ZenPPT = (int)Zen.powerTable.Table[0];
                                ZenTDC = (int)Zen.powerTable.Table[2];
                                ZenTHM = (int)Zen.powerTable.Table[4];
                                ZenEDC = (int)Zen.powerTable.Table[8];
                                ZenFCLK = (int)Zen.powerTable.FCLK;
                                ZenUCLK = (int)Zen.powerTable.UCLK;
                                ZenMCLK = (int)Zen.powerTable.MCLK;
                                ZenScalar = (int)Zen.GetPBOScalar();
                                ZenMemRatio = Zen.powerTable.MemRatio;
                                for (int it = 0; it < Zen.powerTable.Table.Length; ++it)
                                {
                                    Trace.WriteLine($"\t\t[{it}] = [{Zen.powerTable.Table[it]}]");
                                }
                            }
                            uint ZenCCD_Fuse = Zen.ReadDword(381464U);
                            uint ZenCore_Fuse1 = Zen.ReadDword(805838232U);
                            uint ZenCore_Fuse2 = Zen.ReadDword(839392664U);
                            uint ZenCCD_Total = BitSlice(ZenCCD_Fuse, 22, 23);
                            uint ZenCCD_Disabled = BitSlice(ZenCCD_Fuse, 30, 31);
                            uint ZenCCD1_Fuse = BitSlice(ZenCore_Fuse1, 0, 7);
                            uint ZenCCD2_Fuse = BitSlice(ZenCore_Fuse2, 0, 7);
                            uint ZenCore_Layout = ZenCCD1_Fuse | ZenCCD2_Fuse << 8 | 4294901760U;

                            uint cores_t = ZenCore_Layout;
                            
                            int i = 0;
                            int j = 0;
                            
                            while (i < ZenCCD_Total * 8)
                            {
                                bool flag = (cores_t & 1U) == 0U;
                                if (flag)
                                {
                                    ZenCoreMap[j++] = i;
                                }
                                i++;
                                cores_t >>= 1;
                            }
                            Trace.WriteLine($"ZenCoreMap: {string.Join(", ", ZenCoreMap)}");

                            if (CPUFamily >= 25)
                            {
                                for (int ix = 0; ix < CPUCores; ix++)
                                {
                                    int count = GetCount(ZenCoreMap[ix]);
                                    ZenCO[ix] = count;
                                }
                                for (int ic = 0; ic < CPUCores; ic++) 
                                { 
                                    ZenCOLabel += String.Format("{0}#{1} ", ic, ZenCO[ic]);
                                    if (ic != CPUCores - 1) ZenCOLabel += ", ";
                                }
                                ZenCOb = true;
                                Trace.WriteLine($"CO: {string.Join(", ", ZenCO)}");
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Error reading Zen SMU: {ex}");
                }

            }

            catch (Exception e)
            {
                BoardManufacturer = "N/A";
                BoardModel = "N/A";
                BoardBIOS = "N/A";
                CPUBits = "N/A";
                CPUDescription = "N/A";
                CPUName = "N/A";
                CPUCores = 1;
                CPUEnabledCores = 1;
                CPUThreads = 1;
                CPUSocket = "N/A";
                CPULogicalProcessors = 1;
                HyperThreading = false;

                Trace.WriteLine("WMI System Exception: " + e);
            }

            CPULabel = $"{CPUName} [Socket {CPUSocket}]\n{CPUDescription} x{CPUBits}";
            BoardLabel = $"{BoardModel} [BIOS Version {BoardBIOS}]\n{BoardManufacturer}";
            ProcessorsLabel = $"{CPUCores}";
            if (HyperThreading) ProcessorsLabel += $" [Threads: {CPULogicalProcessors}]";
            if (ZenStates) { 
                CPULabel += $"\nPPT: {string.Format("{0:N0}W", ZenPPT)} TDC: {string.Format("{0:N0}A", ZenTDC)} EDC: {string.Format("{0:N0}A", ZenEDC)} Scalar: {ZenScalar}x THM: {string.Format("{0:N0}°C", ZenTHM)}";
                CPULabel += $"\nMCLK/FCLK/UCLK: {ZenMCLK}/{ZenFCLK}/{ZenUCLK} MHz Boost Clock: {ZenBoost} MHz";
                CPULabel += $"\nSMU Version: {ZenSMUVer}";
            }
        }
        private uint BitSlice(uint arg, int start, int end)
        {
            uint mask = (2U << end - start) - 1U;
            return arg >> start & mask;
        }
        private int GetCount(int core_id)
        {
            uint[] args = new uint[6];
            args[0] = (uint)((uint)((core_id & 8) << 5 | (core_id & 7)) << 20);
            try
            {
                SMU.Status status = Zen.SendSmuCommand(Zen.smu.Mp1Smu, 72U, ref args);
                if (status != SMU.Status.OK) Trace.WriteLine($"Error Zen GetCount Core {core_id} SMU Cmd");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error Zen GetCount Core {core_id} Exception: {ex}");
            }
            return (int)args[0];
        }
        private bool SetCount(int core_id = 0, int count = 0)
        {
            uint[] args = new uint[6];
            args[0] = (uint)(((core_id & 8) << 5 | (core_id & 7)) << 20 | (count & 65535));
            try
            {
                SMU.Status status = Zen.SendSmuCommand(Zen.smu.Mp1Smu, 53U, ref args);
                if (status == SMU.Status.OK)
                {
                    return true;
                }
                else
                {
                    Trace.WriteLine($"Error Zen SetCount Core {core_id} SMU: {status}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error Zen SetCount Core {core_id} Exception: {ex}");
                return false;
            }
        }
        private bool ResetCounts()
        {
            try
            {
                uint[] args = new uint[6];
                args[0] = 0U;
                SMU.Status status = Zen.SendSmuCommand(Zen.smu.Mp1Smu, 54U, ref args);
                if (status == SMU.Status.OK)
                {
                    return true;
                }
                else
                {
                    Trace.WriteLine($"Error Zen ResetCounts SMU: {status}");
                    return false;
                }
            }
            catch (ApplicationException ex)
            {
                Trace.WriteLine($"Error Zen ResetCounts Core Exception: {ex}");
                return false;
            }
        }
    }

}
