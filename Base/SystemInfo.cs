using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using ZenStates.Core;
using Octokit;
using System.Xml.Linq;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Data;
using LibreHardwareMonitor.Hardware;
using System.Globalization;

namespace BenchMaestro
{
	public class SystemInfo : INotifyPropertyChanged
	{
		public Computer LibreComputer = new Computer();
		public string AppVersion { get; set; }
		public string LastVersionOnServer { get; set; }
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
		public int ZenPTVersion { get; set; }
		public int ZenBoost { get; set; }
		public int ZenScalar { get; set; }
		public int ZenPPT { get; set; }
		public int ZenTDC { get; set; }
		public int ZenEDC { get; set; }
		public int ZenTHM { get; set; }
		public int ZenFCLK { get; set; }
		public int ZenUCLK { get; set; }
		public int ZenMCLK { get; set; }
		public int ZenVDDP { get; set; }
		public int ZenVCCD { get; set; }
		public int ZenVIOD { get; set; }
		public int ZenVDDG { get; set; }
		public float ZenMemRatio { get; set; }
		public bool ZenCOb { get; set; }
		public bool ZenPerCCDTemp { get; set; }
		public int ZenCCDTotal { get; set; }
		public int[] ZenCO { get; set; }
		public int[] ZenCoreMap { get; set; }
		public string ZenCoreMapLabel { get; set; }
		public int[] CPPCTags { get; set; }
		public double WinMaxSize { get; set; }
		public string ZenCOLabel { get; set; }
		public string CPUSensorsSource { get; set; }
		public bool Zen1X { get; set; }
		public bool ZenPlus { get; set; }

		public bool IntelAVX512 { get; set; }
		public bool IntelHybrid { get; set; }
		public List<int> Pcores { get; set; }
		public List<int> Ecores { get; set; }
		public List<int> Plogicals { get; set; }
		public List<int> Elogicals { get; set; }

		public string LiveCPUTemp { get; set; }
		public string LiveCPUClock { get; set; }
		public string LiveCPUPower { get; set; }
		public string LiveCPUAdditional { get; set; }
		public string LiveFinished { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		private int EmptyTags()
		{
			int _remaining = CPUCores;
			for (int i = 0; i <= CPUCores - 1; i++)
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
			CPUBits = "N/A";
			CPUDescription = "N/A";
			CPUName = "N/A";
			CPUCores = 1;
			CPUEnabledCores = 1;
			CPUThreads = 1;
			CPUSocket = "N/A";
			CPULogicalProcessors = 1;
			CPUFamily = 0;
			BoardBIOS = "N/A";
			BoardManufacturer = "N/A";
			BoardModel = "N/A";
			CPPCTagsLabel = "";
			HyperThreading = false;

			LastVersionOnServer = "N/A";

			CPPCOrder = new int[CPUCores];
			CPPCOrder1 = new int[CPUCores];
			CPPCTags = new int[CPUCores];

			IntelHybrid = false;
			Pcores = new();
			Ecores = new();
			Plogicals = new();
			Elogicals = new();

			try
			{
				CPPCTags = new int[CPUCores];
				CPPCOrder = new int[CPUCores];
				CPPCOrder1 = new int[CPUCores];

				ZenStates = false;
				ZenPerCCDTemp = false;
				ZenBoost = 0;
				ZenScalar = 0;
				ZenPPT = 0;
				ZenTDC = 0;
				ZenEDC = 0;
				ZenTHM = 0;
				ZenFCLK = 0;
				ZenUCLK = 0;
				ZenMCLK = 0;
				ZenVDDP = 0;
				ZenVDDG = 0;
				ZenVCCD = 0;
				ZenVIOD = 0;
				ZenCO = new int[CPUCores];
				ZenCoreMap = new int[CPUCores];
				ZenCOb = false;
				ZenCOLabel = "";
				ZenSMUVer = "N/A";
				ZenPTVersion = 0;
				ZenCoreMapLabel = "";
				Zen1X = false;
				ZenPlus = false;
				LiveCPUTemp = "N/A";
				LiveCPUClock = "N/A";
				LiveCPUPower = "N/A";
				LiveCPUAdditional = "N/A";

				WinMaxSize = 600;

				STMT = true;

				CPUSensorsSource = "LibreHardwareMonitor";
				HWMonitor.CPUSource = HWSensorSource.Libre;
				HWMonitor.NewSensors();

				WmiBasics();

				if (CPULogicalProcessors > CPUCores) HyperThreading = true;

				CPPCTagsInit();

				CpuIdInit();

				CpuSetInit();

				ZenStates = false;
				ZenPerCCDTemp = false;
				ZenBoost = 0;
				ZenScalar = 0;
				ZenPPT = 0;
				ZenTDC = 0;
				ZenEDC = 0;
				ZenTHM = 0;
				ZenFCLK = 0;
				ZenUCLK = 0;
				ZenMCLK = 0;
				ZenVDDP = 0;
				ZenVDDG = 0;
				ZenVCCD = 0;
				ZenVIOD = 0;
				ZenCO = new int[CPUCores];
				ZenCoreMap = new int[CPUCores];
				ZenCOb = false;
				ZenCOLabel = "";
				ZenSMUVer = "N/A";
				ZenPTVersion = 0;
				ZenCoreMapLabel = "";

				ZenMainInit();

			}
			catch (Exception ex)
			{
				Trace.WriteLine($"SystemInfo Exception: {ex}");
			}
			finally
			{
				RefreshLabels();
			}
		}
		static int CountSetBits(uint n)
		{
			uint count = 0;
			while (n > 0)
			{
				count += n & 1;
				n >>= 1;
			}
			return (int)count;
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
		public bool ZenRefreshStatic(bool refresh)
		{
			if (!ZenStates) return false;

			if (refresh)
			{
				bool _refreshpt = ZenRefreshPowerTable();

				if (!_refreshpt) return false;
			}

			if (ZenPTVersion == 0x380804)
			{
				ZenPPT = (int)Zen.powerTable.Table[0];
				ZenTDC = (int)Zen.powerTable.Table[2];
				ZenTHM = (int)Zen.powerTable.Table[4];
				ZenEDC = (int)Zen.powerTable.Table[8];
				ZenFCLK = (int)Zen.powerTable.Table[74];
				ZenUCLK = (int)Zen.powerTable.Table[78];
				ZenMCLK = (int)Zen.powerTable.Table[82];
				ZenScalar = (int)Zen.GetPBOScalar();
				ZenVDDP = (int)(Zen.powerTable.Table[137] * 1000);
				ZenVIOD = (int)(Zen.powerTable.Table[138] * 1000);
				ZenVCCD = (int)(Zen.powerTable.Table[139] * 1000);
			}
			else if (ZenPTVersion == 0x380904)
			{
				ZenPPT = (int)Zen.powerTable.Table[0];
				ZenTDC = (int)Zen.powerTable.Table[2];
				ZenTHM = (int)Zen.powerTable.Table[4];
				ZenEDC = (int)Zen.powerTable.Table[8];
				ZenFCLK = (int)Zen.powerTable.Table[74];
				ZenUCLK = (int)Zen.powerTable.Table[78];
				ZenMCLK = (int)Zen.powerTable.Table[82];
				ZenScalar = (int)Zen.GetPBOScalar();
				ZenVDDP = (int)(Zen.powerTable.Table[137] * 1000);
				ZenVIOD = (int)(Zen.powerTable.Table[138] * 1000);
				ZenVCCD = (int)(Zen.powerTable.Table[139] * 1000);
			}
			else if (ZenPTVersion == 0x380805)
			{
				ZenPPT = (int)Zen.powerTable.Table[0];
				ZenTDC = (int)Zen.powerTable.Table[2];
				ZenTHM = (int)Zen.powerTable.Table[4];
				ZenEDC = (int)Zen.powerTable.Table[8];
				ZenFCLK = (int)Zen.powerTable.Table[74];
				ZenUCLK = (int)Zen.powerTable.Table[78];
				ZenMCLK = (int)Zen.powerTable.Table[82];
				ZenScalar = (int)Zen.GetPBOScalar();
				ZenVDDP = (int)(Zen.powerTable.Table[137] * 1000);
				ZenVIOD = (int)(Zen.powerTable.Table[138] * 1000);
				ZenVCCD = (int)(Zen.powerTable.Table[139] * 1000);
			}
			else if (ZenPTVersion == 0x380905)
			{
				ZenPPT = (int)Zen.powerTable.Table[0];
				ZenTDC = (int)Zen.powerTable.Table[2];
				ZenTHM = (int)Zen.powerTable.Table[4];
				ZenEDC = (int)Zen.powerTable.Table[8];
				ZenFCLK = (int)Zen.powerTable.Table[74];
				ZenUCLK = (int)Zen.powerTable.Table[78];
				ZenMCLK = (int)Zen.powerTable.Table[82];
				ZenScalar = (int)Zen.GetPBOScalar();
				ZenVDDP = (int)(Zen.powerTable.Table[137] * 1000);
				ZenVIOD = (int)(Zen.powerTable.Table[138] * 1000);
				ZenVCCD = (int)(Zen.powerTable.Table[139] * 1000);
			}
			else if (ZenPTVersion == 0x400005)
			{
				ZenPPT = (int)Zen.powerTable.Table[4];
				ZenTDC = (int)Zen.powerTable.Table[8];
				ZenTHM = (int)Zen.powerTable.Table[16];
				ZenEDC = (int)Zen.powerTable.Table[12];
				ZenFCLK = (int)Zen.powerTable.Table[409];
				ZenUCLK = (int)Zen.powerTable.Table[410];
				ZenMCLK = (int)Zen.powerTable.Table[411];
				ZenScalar = (int)Zen.GetPBOScalar();
				ZenVDDP = (int)(Zen.powerTable.Table[565] * 1000);
			}
			else if (ZenPTVersion == 0x240903)
			{
				ZenPPT = (int)Zen.powerTable.Table[0];
				ZenTDC = (int)Zen.powerTable.Table[2];
				ZenTHM = (int)Zen.powerTable.Table[4];
				ZenEDC = (int)Zen.powerTable.Table[8];
				ZenFCLK = (int)Zen.powerTable.Table[70];
				ZenUCLK = (int)Zen.powerTable.Table[74];
				ZenMCLK = (int)Zen.powerTable.Table[78];
				ZenScalar = (int)Zen.GetPBOScalar();
				ZenVDDP = (int)(Zen.powerTable.Table[125] * 1000);
				ZenVDDG = (int)(Zen.powerTable.Table[126] * 1000);
			}
			else if (ZenPTVersion == 0x240803)
			{
				ZenPPT = (int)Zen.powerTable.Table[0];
				ZenTDC = (int)Zen.powerTable.Table[2];
				ZenTHM = (int)Zen.powerTable.Table[4];
				ZenEDC = (int)Zen.powerTable.Table[8];
				ZenFCLK = (int)Zen.powerTable.Table[70];
				ZenUCLK = (int)Zen.powerTable.Table[74];
				ZenMCLK = (int)Zen.powerTable.Table[78];
				ZenScalar = (int)Zen.GetPBOScalar();
				ZenVDDP = (int)(Zen.powerTable.Table[125] * 1000);
				ZenVDDG = (int)(Zen.powerTable.Table[126] * 1000);
			}
			else if (ZenSMUVer == "25.86.0")
			{
				if (Zen1X || ZenPlus)
				{
					ZenPPT = (int)Zen.powerTable.Table[0];
					ZenTDC = (int)Zen.powerTable.Table[2];
				}
				if (ZenPlus)
				{
					ZenEDC = (int)Zen.powerTable.Table[8];
				}
				ZenTHM = (int)Zen.powerTable.Table[4];
				ZenFCLK = (int)Zen.powerTable.Table[33];
				ZenUCLK = (int)Zen.powerTable.Table[33];
				ZenMCLK = (int)Zen.powerTable.Table[33];
			}
			Trace.WriteLine($"ZenRefreshStatic done");
			return true;
		}
		public bool ZenRefreshPowerTable()
		{
			try
			{
				SMU.Status status = Zen.RefreshPowerTable();

				if (status != SMU.Status.OK)
				{
					for (int r = 0; r < 10; ++r)
					{
						Thread.Sleep(25);
						status = ZenRefreshPowerTable2();
						if (status == SMU.Status.OK) r = 99;
					}
				}

				if (status == SMU.Status.OK) return true;
				return false;
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ZenRefreshPowerTable Exception: {ex}");
				return false;
			}

		}
		public SMU.Status ZenRefreshPowerTable2()
		{
			try
			{
				SMU.Status status = Zen.RefreshPowerTable();

				if (status != SMU.Status.OK)
				{
					for (int r = 0; r < 10; ++r)
					{
						status = Zen.RefreshPowerTable();
						Trace.WriteLine($"ZenRefreshPowerTable SMU Error: {status}");
						if (status == SMU.Status.OK) r = 99;
					}
				}

				return status;
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ZenRefreshPowerTable Exception: {ex}");
				return SMU.Status.FAILED;
			}

		}

		public void ZenRefreshCO()
		{
			try
			{
				if (Zen.smu.SMU_TYPE == SMU.SmuType.TYPE_CPU3 && CPUCores <= ZenCoreMap.Length)
				{
					ZenCOLabel = "";
					for (int ix = 0; ix < CPUCores; ix++)
					{
						int count = GetCount(ZenCoreMap[ix]);
						ZenCO[ix] = count;
					}
					for (int ic = 0; ic < CPUCores; ic++)
					{
						ZenCOLabel += String.Format("{0}#{1} ", ic, ZenCO[ic].ToString("+#;-#;0"));
						if (ic != CPUCores - 1) ZenCOLabel += ", ";
					}
					ZenCOb = true;
					Trace.WriteLine($"ZenRefreshCO: {string.Join(", ", ZenCO)}");
					OnChange("ZenCOLabel");
				}
			}
			catch (Exception ex)
			{
				ZenCOb = false;
				ZenCOLabel = "";
				Trace.WriteLine($"ZenRefreshCO Exception: {ex}");
			}
		}

		public void RefreshLabels()
		{
			try
			{
				CPULabel = $"{CPUName} [Socket {CPUSocket}]\n{CPUDescription} x{CPUBits}";
				BoardLabel = $"{BoardModel} [BIOS Version {BoardBIOS}]\n{BoardManufacturer}";
				ProcessorsLabel = $"{CPUCores}";
				if (HyperThreading) ProcessorsLabel += $" [Threads: {CPULogicalProcessors}]";
				if (IntelHybrid)
				{
					ProcessorsLabel += $" P-Cores: {Pcores.Count}";
					if (Plogicals.Count > Pcores.Count) ProcessorsLabel += $" [{Plogicals.Count}T]";
					ProcessorsLabel += $" E-Cores: {Ecores.Count}";
					if (Elogicals.Count > Ecores.Count) ProcessorsLabel += $" [{Elogicals.Count}T]";
				}
				if (ZenStates)
				{
					string _CPULabel = "";
					if (ZenPPT > 0) _CPULabel += $"PPT: {string.Format("{0:N0}W", ZenPPT)} ";
					if (ZenTDC > 0) _CPULabel += $"TDC: {string.Format("{0:N0}A", ZenTDC)} ";
					if (ZenEDC > 0) _CPULabel += $"EDC: {string.Format("{0:N0}A", ZenEDC)} ";
					if (ZenScalar > 0) _CPULabel += $"Scalar: {ZenScalar}x ";
					if (ZenTHM > 0) _CPULabel += $"THM: {string.Format("{0:N0}°C", ZenTHM)} ";

					if (_CPULabel.Length > 0) CPULabel += $"\n{_CPULabel}";

					_CPULabel = "";

					if (ZenMCLK > 0 || ZenFCLK > 0 || ZenUCLK > 0) _CPULabel += $"MCLK/FCLK/UCLK: {ZenMCLK}/{ZenFCLK}/{ZenUCLK} ";
					if (ZenBoost > 0) _CPULabel += $"Boost Clock: {ZenBoost} MHz ";

					if (_CPULabel.Length > 0) CPULabel += $"\n{_CPULabel}";

					_CPULabel = "";

					if (ZenVDDP > 0) _CPULabel += $"VDDP: {ZenVDDP}mV ";
					if (ZenVDDG > 0) _CPULabel += $"VDDG: {ZenVDDG}mV ";
					if (ZenVCCD > 0) _CPULabel += $"VDDG CCD: {ZenVCCD}mV ";
					if (ZenVIOD > 0) _CPULabel += $"VDDG IOD: {ZenVIOD}mV ";

					if (_CPULabel.Length > 0) CPULabel += $"\n{_CPULabel}";

					_CPULabel = "";

					if (ZenSMUVer.Length > 0) _CPULabel += $"SMU Version: {ZenSMUVer} ";
					if (ZenPTVersion > 0) _CPULabel += $"Power Table: 0x{ZenPTVersion:X} ";

					if (_CPULabel.Length > 0) CPULabel += $"\n{_CPULabel}";

					if (ZenCoreMapLabel.Length > 0) ProcessorsLabel += $"\nCoreMap: {ZenCoreMapLabel} ";

				}
				OnChange("CPULabel");
				OnChange("BoardLabel");
				OnChange("ProcessorsLabel");
				Trace.WriteLine($"RefreshLabels done");
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"RefreshLabels Exception: {ex}");
			}

		}

		public void WmiBasics()
		{
			try
			{
				string ClassName = "Win32_BIOS";

				ManagementClass SIManagementClass = new ManagementClass(ClassName);
				//Create a ManagementObjectCollection to loop through
				ManagementObjectCollection SIManagemenobjCol = SIManagementClass.GetInstances();
				//Get the properties in the class
				PropertyDataCollection SIproperties = SIManagementClass.Properties;

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
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"WmiBasics Exception: {ex}");
			}
		}
		public void CPPCTagsInit()
		{
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

							XDocument doc = XDocument.Parse(ev.ToXml());
							Dictionary<string, string> dataDictionary = new Dictionary<string, string>();

							foreach (XElement element in doc.Descendants().Where(p => p.HasElements == false))
							{
								int keyInt = 0;
								string keyName = element.Name.LocalName;
								while (dataDictionary.ContainsKey(keyName))
								{
									keyName = element.Name.LocalName + "_" + keyInt++;
								}
								dataDictionary.Add(keyName, element.Value);

								if (element.HasAttributes)
								{
									var lmsAttribute = element.FirstAttribute;
									if (lmsAttribute != null)
									{
										dataDictionary.Add($"{keyName}_{lmsAttribute.Name.LocalName}", lmsAttribute.Value);
									}
								}
							}

							foreach (KeyValuePair<string, string> kvp in dataDictionary)
							{
								sb.AppendLine($"Key = {kvp.Key}, Value = {kvp.Value}");
							}

							_procid = Convert.ToInt32(dataDictionary["Data_0"]);
							_procgroup = Convert.ToInt32(dataDictionary["Data"]);
							_procperf = Convert.ToInt32(dataDictionary["Data_4"]);

							sb.AppendLine();
							sb.Append($"Group: {_procgroup} Processor: {_procid} Processor: {_procperf}");
							sb.AppendLine();
							sb.AppendLine();
							sb.AppendLine();
							sb.AppendLine();

							if (_procid == 0 || !HyperThreading || (HyperThreading && (_procid % 2 == 0)))
							{
								int __procid = _procid == 0 ? _procid : HyperThreading ? _procid / 2 : _procid;
								//Trace.WriteLine($"Add Tag={__procid} {_procperf}");
								CPPCTags[__procid] = _procperf;
							}
							//Trace.WriteLine($"EmptyTags={EmptyTags()}");

							if (EmptyTags() <= 0) break;
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
						for (int ix = CPPC.GetLength(0) - 1; ix > -1; ix--)
						{
							//Trace.WriteLine($"CPPC Testing ix: {ix} perf: {CPPCTags[ix]} IsIn: {CPPCFoundAlready(ix)}");
							if (_highestperf <= CPPCTags[ix] && !CPPCFoundAlready(ix))
							{
								//Trace.WriteLine($"CPPC Highest ix: {ix} perf: {CPPCTags[ix]}");
								_highestperf = CPPCTags[ix];
								_highestcpu = ix;
							}
						}
						CPPCOrder[i] = _highestcpu;
						CPPCOrder1[i] = _highestcpu + 1;
						CPPC[i, 0] = _highestcpu;
						CPPC[i, 1] = _highestperf;

						string _cppctrace = "";
						for (int iz = 0; iz < CPPC.GetLength(0); iz++)
						{
							_cppctrace += String.Format("{0}#{1} ", CPPC[iz, 0], CPPC[iz, 1]);
						}
						//Trace.WriteLine($"CPPC: {_cppctrace}");

						//Trace.WriteLine($"CPPC i: {i} Core: {CPPC[i, 0]} Perf: {CPPC[i, 1]}");
					}

					CPPCLabels();

					string path = @".\Logs\dumpcppc.txt";
					if (!File.Exists(path)) File.Delete(path);

					using (StreamWriter sw = File.CreateText(path))
					{
						sw.WriteLine(sb.ToString());
					}


				}
			}
			catch (Exception e)
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
		}

		public void CPPCLabels()
		{
			try
			{
				CPPCLabel = "";
				CPPCPerfLabel = "";
					
				for (int i = 0; i<CPPC.GetLength(0); i++)
				{
					CPPCPerfLabel += String.Format("{0}#{1}", CPPC[i, 0], CPPC[i, 1]);
					if (i != CPPC.GetLength(0) - 1) CPPCPerfLabel += ", ";
				}

				for (int i = 0; i<CPPC.GetLength(0); i++)
				{
					CPPCLabel += String.Format("{0} ", CPPC[i, 0]);
					if (i != CPPC.GetLength(0) - 1) CPPCLabel += ", ";

				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"CPPCLabels Exception: {ex}");
			}
		}
		public void CpuIdInit()
		{
			try
			{
				HWMonitor.computer = new Computer
				{
					IsCpuEnabled = true,
					IsGpuEnabled = false,
					IsMemoryEnabled = false,
					IsMotherboardEnabled = false,
					IsControllerEnabled = false,
					IsNetworkEnabled = false,
					IsStorageEnabled = false
				};
				HWMonitor.computer.Open();
				HWMonitor.computer.Accept(new UpdateVisitor());

				uint hybridreg = 0x0;
				uint hybridflag = 0x0;
				uint coretypereg = 0x0;
				ulong coretype = 0x0;
				string hybridstr = "";
				string coretypestr = "";
				uint avx512 = 0x0;
				uint avx512reg = 0x0;
				string avx512str = "No";

				for (int j = 0; j < CPULogicalProcessors; j++)
				{
					Trace.WriteLine($" CPU Logical Processor: {j}");

					LibreHardwareMonitor.Hardware.CPU.CpuId _cpuid = LibreHardwareMonitor.Hardware.CPU.CpuId.Get(0, j);
					Trace.WriteLine($"CPUID_0 {_cpuid.Data[0, 0]:X}");
					Trace.WriteLine($"CPUID_EXT {_cpuid.ExtData[0, 0]:X}");

					uint offset = LibreHardwareMonitor.Hardware.CPU.CpuId.CPUID_0;
					uint offsetext = LibreHardwareMonitor.Hardware.CPU.CpuId.CPUID_EXT;

					try
					{
						if (_cpuid.Data.GetLength(0) >= 0x7)
						{
							hybridreg = _cpuid.Data[0x7, 3];
							hybridflag = BitSlice(hybridreg, 15, 15);
						}
						if (_cpuid.Data.GetLength(0) >= 0x1A)
                        {
							coretypereg = _cpuid.Data[0x1A, 0];
							coretype = BitSlice(coretypereg, 24, 31);
						}
						if (_cpuid.Data.GetLength(0) >= 0xD)
						{
							avx512reg = _cpuid.Data[0xD, 3];
							avx512 = BitSlice(hybridreg, 5, 5);
						}
					}
					catch (Exception ex)
                    {
						Trace.WriteLine($" Error Reading Hybrid/Coretype: {ex}");
						hybridreg = 3;
						coretypereg = 0;
					}
					switch (hybridflag)
					{
						case 0:
							hybridstr = "No";
							break;
						case 1:
							IntelHybrid = true;
							hybridstr = "Yes";
							break;
						default:
							hybridstr = "Unknown";
							break;
					}
					switch (coretype)
					{
						case 0x20:
							if (!Ecores.Contains(ProcessorInfo.PhysicalCore(j)))
								Ecores.Add(ProcessorInfo.PhysicalCore(j));
							Elogicals.Add(j + 1);
							coretypestr = "E-Core";
							break;
						case 0x40:
							if (!Pcores.Contains(ProcessorInfo.PhysicalCore(j)))
								Pcores.Add(ProcessorInfo.PhysicalCore(j));
							Plogicals.Add(j + 1);
							coretypestr = "P-Core";
							break;
						default:
							coretypestr = "Unknown";
							break;
					}
					switch (avx512)
					{
						case 0:
							avx512str = "No";
							break;
						case 1:
							IntelAVX512 = true;
							avx512str = "Yes";
							break;
						default:
							avx512str = "Unknown";
							break;
					}
					Trace.WriteLine($" Hybrid: [{hybridstr}] CoreType: [{coretypestr}] AVX-512: [{avx512str}]");

					Trace.WriteLine("");
					Trace.WriteLine(" Function  EAX       EBX       ECX       EDX");
					string _line = "";
					for (int i = 0; i < _cpuid.Data.GetLength(0); i++)
					{
						_line = " ";
						_line += (i + offset).ToString("X8", CultureInfo.InvariantCulture);
						for (int ij = 0; ij < 4; ij++)
						{
							_line += "  ";
							_line += _cpuid.Data[i, ij].ToString("X8", CultureInfo.InvariantCulture);
						}
						Trace.WriteLine(_line);


					}
					Trace.WriteLine(" Function  EAX       EBX       ECX       EDX");
					for (int i = 0; i < _cpuid.ExtData.GetLength(0); i++)
					{
						_line = " ";
						_line += (i + offsetext).ToString("X8", CultureInfo.InvariantCulture);
						for (int ij = 0; ij < 4; ij++)
						{
							_line += "  ";
							_line += _cpuid.ExtData[i, ij].ToString("X8", CultureInfo.InvariantCulture);
						}
						Trace.WriteLine(_line);

					}
					Trace.WriteLine("");

				}
				
				if (IntelHybrid)
                {
					string pcoresstr = String.Join(", ", Pcores.ToArray());
					string plogicalsstr = String.Join(", ", Plogicals.ToArray());
					string ecoresstr = String.Join(", ", Ecores.ToArray());
					string elogicalsstr = String.Join(", ", Elogicals.ToArray());
					Trace.WriteLine($"P-Cores: {pcoresstr}");
					Trace.WriteLine($"P-Logicals: {plogicalsstr}");
					Trace.WriteLine($"E-Cores: {ecoresstr}");
					Trace.WriteLine($"E-Logicals: {elogicalsstr}");
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"CpuIdInit Exception: {ex}");
			}
		}
		
		public void CpuSetInit()
		{
			try
			{
				Trace.WriteLine($"CpuSetInfo");
				Trace.WriteLine($"");

				Trace.WriteLine($"CoresByScheduling:");
				var coresbysched = ProcessorInfo.CoresByScheduling();

				if (ProcessorInfo.IsCoresBySchedulingAllZeros())
				{
					Trace.WriteLine($"Not available");
				}
				else
				{
					for (int ix = 0; ix < coresbysched.Count(); ++ix)
					{
						Trace.WriteLine($"#{coresbysched[ix][0]}:{coresbysched[ix][1]}");
					}

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
						int _highestindex = 0;
						int _tag = 0;
						for (int ix = CPPC.GetLength(0) - 1; ix > -1; ix--)
						{
							//Trace.WriteLine($"CPPC Scheduler Testing ix: {ix} core: {coresbysched[ix][0]} perf: {coresbysched[ix][1]} IsIn: {CPPCFoundAlready(ix)}");
							if (_highestperf <= coresbysched[ix][1] && !CPPCFoundAlready(coresbysched[ix][0]))
							{
								//Trace.WriteLine($"CPPC Highest ix: {ix} perf: {coresbysched[ix][1]}");
								_highestperf = coresbysched[ix][1];
								_tag = CPPCTags[coresbysched[ix][0]];
								_highestcpu = coresbysched[ix][0];
								_highestindex = ix;
							}
						}
						CPPCOrder[i] = _highestcpu;
						CPPCOrder1[i] = _highestcpu + 1;
						CPPC[i, 0] = _highestcpu;
						CPPC[i, 1] = _tag;

						string _cppctrace = "";
						for (int iz = 0; iz < CPPC.GetLength(0); iz++)
						{
							_cppctrace += String.Format("{0}#{1} ", CPPC[iz, 0], CPPC[iz, 1]);
						}
						Trace.WriteLine($"Scheduler CPPC: {_cppctrace}");
						//Trace.WriteLine($"Scheduler CPPC i: {i} Core: {CPPC[i, 0]} Tag: {CPPC[i, 1]}");
					}
				}
				Trace.WriteLine($"");

				Trace.WriteLine($"CoresByEfficiency:");
				var coresbyeff = ProcessorInfo.CoresByEfficiency();
				if (ProcessorInfo.IsCoresByEfficiencyAllZeros())
				{
					Trace.WriteLine($"Not available");
				}
				else
				{
					for (int ix = 0; ix < coresbyeff.Count(); ++ix)
					{
						Trace.WriteLine($"#{coresbyeff[ix][0]}:{coresbyeff[ix][1]}");
					}
				}
				Trace.WriteLine($"");
				for (int i = 0; i < CPULogicalProcessors; ++i)
				{
					Trace.WriteLine($"CPU Logical Processor: {i + 1}");
					Trace.WriteLine($" ProcessorInfo.LogicalProcessorIndex: {ProcessorInfo.CpuSetLogicalProcessorIndex(i) + 1}");
					Trace.WriteLine($" ProcessorInfo.EfficiencyClass: {ProcessorInfo.CpuSetEfficiencyClass(i)}");
					Trace.WriteLine($" ProcessorInfo.CoreIndex: {ProcessorInfo.CpuSetCoreIndex(i)}");
					Trace.WriteLine($" ProcessorInfo.NumaNodeIndex: {ProcessorInfo.CpuSetNumaNodeIndex(i)}");
					Trace.WriteLine($" ProcessorInfo.LastLevelCacheIndex: {ProcessorInfo.CpuSetLastLevelCacheIndex(i)}");
					Trace.WriteLine($" ProcessorInfo.Group: {ProcessorInfo.CpuSetGroup(i)}");
					Trace.WriteLine($" ProcessorInfo.SchedulingClass: {ProcessorInfo.CpuSetSchedulingClass(i)}");
					Trace.WriteLine($" ProcessorInfo.AllocationTag: {ProcessorInfo.CpuSetAllocationTag(i)}");
					Trace.WriteLine($" ProcessorInfo.Parked: {ProcessorInfo.CpuSetParked(i)}");
					Trace.WriteLine($" ProcessorInfo.Allocated: {ProcessorInfo.CpuSetAllocated(i)}");
				}

				CPPCLabels();
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"CpuSetsInit Exception: {ex}");
			}
		}
		public void ZenMainInit()
		{
			try
			{
				if (CPUSocket == "AM4")
				{
					bool smucheck = false;
					try
					{
						Zen = new Cpu();
						Trace.WriteLine($"Zen Name: {Zen.info.cpuName}");
						Trace.WriteLine($"Zen CodeName: {Zen.info.codeName}");
						Trace.WriteLine($"Zen Family: {Zen.info.family}");
						Trace.WriteLine($"Zen Model: {Zen.info.model}");
						Trace.WriteLine($"Zen BaseModel: {Zen.info.baseModel}");
						Trace.WriteLine($"Zen ExtModel: {Zen.info.extModel}");
						Trace.WriteLine($"Zen Socket: {Zen.info.packageType}");
						Trace.WriteLine($"Zen CpuID: {Zen.info.cpuid:X8}");
						Trace.WriteLine($"Zen SVI2: {Zen.info.svi2.coreAddress:X8}:{Zen.info.svi2.socAddress:X8}");
						smucheck = Zen.smu.Version != 0U;
						Trace.WriteLine($"Zen Test SMU: {smucheck}");
						Trace.WriteLine($"Zen SMU Type: {Zen.smu.SMU_TYPE}");
					}
					catch
					{
						Trace.WriteLine($"ZenCore DLL couldn't be loaded");
					}

					if (smucheck)
					{
						ZenStates = true;

						uint smu_ver = Zen.smu.Version;
						uint ver_maj = smu_ver >> 16 & 255U;
						uint ver_min = smu_ver >> 8 & 255U;
						uint ver_rev = smu_ver & 255U;
						ZenSMUVer = string.Format("{0}.{1}.{2}", ver_maj, ver_min, ver_rev);

						string line = $"SMU Ver [{ZenSMUVer}]";
						Trace.WriteLine(line);

						uint[] args = new uint[6];
						uint cmd = Zen.smu.Hsmp.ReadBoostLimit;

						SMU.Status status = Zen.SendSmuCommand(Zen.smu.Hsmp, cmd, ref args);
						if (status == SMU.Status.OK)
						{
							ZenBoost = (int)args[0];
						}
						else
						{
							Trace.WriteLine($"Failed SMU check for BoostClock: {status}");
						}

						uint ccd_fuse1a = 0x5D218;
						uint ccd_fuse2a = 0x5D21C;
						uint core_fuse1a = 0x30081800 + 0x238;
						uint core_fuse2a = 0x30081800 + 0x238 + 0x2000000;

						if (Zen.info.family == Cpu.Family.FAMILY_17H && Zen.info.model != 0x71)
						{
							ccd_fuse1a += 0x40;
							ccd_fuse2a += 0x40;
						}
						uint ZenCCD_Fuse1 = Zen.ReadDword(ccd_fuse1a);
						uint ZenCCD_Fuse2 = Zen.ReadDword(ccd_fuse2a);

						if (Zen.info.family == Cpu.Family.FAMILY_19H)
						{
							core_fuse1a = 0x30081800 + 0x598;
							core_fuse2a = 0x30081800 + 0x598 + 0x2000000;
						}
						uint ZenCore_Fuse1 = Zen.ReadDword(core_fuse1a);
						uint ZenCore_Fuse2 = Zen.ReadDword(core_fuse2a);
						uint ZenCCDS_Total = BitSlice(ZenCCD_Fuse1, 22, 23);
						uint ZenCCD_Disabled = BitSlice(ZenCCD_Fuse1, 30, 31);
						uint ZenCCDS_Total2 = BitSlice(ZenCCD_Fuse2, 22, 23);
						uint ZenCCD_Disabled2 = BitSlice(ZenCCD_Fuse2, 30, 31);
						uint ZenCCD1_Fuse = BitSlice(ZenCore_Fuse1, 0, 7);
						uint ZenCCD2_Fuse = BitSlice(ZenCore_Fuse2, 0, 7);

						uint ZenCore_Layout = ZenCCD1_Fuse | ZenCCD2_Fuse << 8 | 0xFFFF0000;
						int ZenCores_per_ccd = (ZenCCD1_Fuse == 0 || ZenCCD2_Fuse == 0) ? 8 : 6;
						int ZenCCD_Total = CountSetBits(ZenCCDS_Total);
						int ZenCCD_Total2 = CountSetBits(ZenCCDS_Total2);

						uint cores_t = ZenCore_Layout;

						ZenCCDTotal = (int)ZenCCDS_Total - (int)ZenCCD_Disabled;

						Trace.WriteLine($"ZenCCD_Total {ZenCCD_Total:X2}");
						Trace.WriteLine($"ZenCCD_Total2 {ZenCCD_Total2:X2}");
						Trace.WriteLine($"ZenCore_Fuse1 {ZenCore_Fuse1:X8}");
						Trace.WriteLine($"ZenCore_Fuse2 {ZenCore_Fuse2:X8}");
						Trace.WriteLine($"ZenCCD_Disabled {ZenCCD_Disabled:X2}");
						Trace.WriteLine($"ZenCCD_Disabled2 {ZenCCD_Disabled2:X2}");
						Trace.WriteLine($"ZenCCD_Fuse1 {ZenCCD_Fuse1:X8}");
						Trace.WriteLine($"ZenCCD_Fuse2{ZenCCD_Fuse2:X8}");
						Trace.WriteLine($"ZenCCD1_Fuse {ZenCCD1_Fuse:X8}");
						Trace.WriteLine($"ZenCCD2_Fuse {ZenCCD2_Fuse:X8}");
						Trace.WriteLine($"ZenCore_Layout {ZenCore_Layout:X8}");
						Trace.WriteLine($"ZenCores_per_ccd {ZenCores_per_ccd}");

						if (ZenCCD_Total > 0 && cores_t > 0)
						{
							ZenCoreMap = new int[CountSetBits(~ZenCore_Layout)];
							int last = ZenCCD_Total * 8;
							for (int i = 0, k = 0; i < ZenCCD_Total * 8; cores_t = cores_t >> 1)
							{
								ZenCoreMapLabel += (i == 0) ? "[" : (i % 8 != 0) ? "." : "";
								if ((cores_t & 1) == 0)
								{
									ZenCoreMap[k++] = i;
									ZenCoreMapLabel += $"{i}";
								}
								else
								{
									ZenCoreMapLabel += "x";
								}
								i++;
								ZenCoreMapLabel += (i % 8 == 0 && i != last) ? "][" : i == last ? "]" : "";
							}
							Trace.WriteLine($"ZenCoreMap: {string.Join(", ", ZenCoreMap)}");
							Trace.WriteLine($"ZenCoreMapLabel: {ZenCoreMapLabel}");
						}

						ZenRefreshCO();

						bool _refreshpt = ZenRefreshPowerTable();

						if (_refreshpt || ver_maj > 0)
						{
							bool ZenPTKnown = false;

							StringBuilder sb = new StringBuilder();

							ZenPTVersion = (int)Zen.GetTableVersion();

							line = $"SMU Ver [{ZenSMUVer}] PT Ver [0x{ZenPTVersion:X}]";
							Trace.WriteLine(line);

							sb.AppendLine(line);
							sb.AppendLine($"CPUName {CPUName}");
							sb.AppendLine($"CPUFamily {CPUFamily}");


							if (ZenSMUVer == "25.86.0")
							{
								ZenPTKnown = true;
								int _maxcores = 8;
								Trace.WriteLine($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen1");

								CPUSensorsSource = "Zen PowerTable";
								HWMonitor.CPUSource = HWSensorSource.Zen;

								string model_pattern = @"^AMD Ryzen (?<series>\d+) (?<generation>\d)(?<model>\d*)(?<ex>.?) .*";
								Regex model_rgx = new Regex(model_pattern, RegexOptions.Multiline);
								Match model_m = model_rgx.Match(Zen.info.cpuName);

								string generation = "";
								string ex = "";
								string model = "";
								string series = "";

								if (model_m.Success)
								{
									string[] results = model_rgx.GetGroupNames();

									foreach (var name in results)
									{
										Group grp = model_m.Groups[name];
										if (name == "ex" && grp.Value.Length > 0)
										{
											ex = grp.Value.TrimEnd('\r', '\n').Trim();
										}
										if (name == "generation" && grp.Value.Length > 0)
										{
											generation = grp.Value.TrimEnd('\r', '\n').Trim();
										}
										if (name == "model" && grp.Value.Length > 0)
										{
											model = grp.Value.TrimEnd('\r', '\n').Trim();
										}
										if (name == "series" && grp.Value.Length > 0)
										{
											series = grp.Value.TrimEnd('\r', '\n').Trim();
										}
									}
									if (ex == "X") Zen1X = true;
									if (ex == "2") ZenPlus = true;
								}

								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD2L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

								ZenRefreshStatic(false);

								if (ZenPlus)
								{
									App.hwsensors.InitZen(HWSensorName.CPUPPT, 1);
									App.hwsensors.InitZen(HWSensorName.CPUTDC, 3);
									App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
									App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
									App.hwsensors.InitZen(HWSensorName.CPUEDC, 9);
									App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
								}
								App.hwsensors.InitZen(HWSensorName.CPUPower, 22);
								App.hwsensors.InitZen(HWSensorName.SOCVoltage, 26);
								App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 127);
								App.hwsensors.InitZen(HWSensorName.CCD2L3Temp, 128);
								App.hwsensors.InitZen(HWSensorName.CPUFSB, 31);
								App.hwsensors.InitZen(HWSensorName.CPUVoltage, 40);
								App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);
								App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
								App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
								App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

								for (int _core = 1; _core <= CPUCores; ++_core)
								{
									int _coreoffset = _core - 1;
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 41 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 57 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 65 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 81 + _coreoffset, _core, 1000);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 89 + _coreoffset, _core, 1000);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 97 + _coreoffset, _core);
								}

								for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
								{
									App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
								}

								float tempoffset = 0;

								if (ex == "X" && model == "700" && generation == "2") tempoffset = -10.0f;
								if (ex == "X" && generation == "1") tempoffset = -20.0f;
								if (tempoffset != 0)
								{
									Trace.WriteLine($"Setting Zen Temp Offset={tempoffset}");
									App.hwsensors.SetValueOffset(HWSensorName.CCD1L3Temp, tempoffset);
									App.hwsensors.SetValueOffset(HWSensorName.CCD2L3Temp, tempoffset);
									App.hwsensors.SetValueOffset(HWSensorName.CPUCoresTemps, tempoffset);
								}

								Trace.WriteLine($"Zen Flags series={series} generation={generation} model={model} ex={ex} ZenPlus={ZenPlus} Zen1X={Zen1X}");

								Trace.WriteLine($"Configuring Zen Source done");
							}
							else if (ZenPTVersion == 0x380804)
							{
								ZenPTKnown = true;
								int _maxcores = 16;
								Trace.WriteLine($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen3");

								ZenPerCCDTemp = true;

								CPUSensorsSource = "Zen PowerTable";
								HWMonitor.CPUSource = HWSensorSource.Zen;

								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD2L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

								ZenRefreshStatic(false);

								App.hwsensors.InitZen(HWSensorName.CPUPPT, 1);
								App.hwsensors.InitZen(HWSensorName.CPUTDC, 3);
								App.hwsensors.InitZen(HWSensorName.CPUEDC, 9);
								App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUPower, 29);
								int _vsoc = (int)Zen.powerTable.Table[45] == 0 ? 44 : 45;
								App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
								App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 525);
								App.hwsensors.InitZen(HWSensorName.CCD2L3Temp, 526);
								App.hwsensors.InitZen(HWSensorName.CPUFSB, 70);
								App.hwsensors.InitZen(HWSensorName.CPUVoltage, 41);
								App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);

								App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
								App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
								App.hwsensors.InitZen(HWSensorName.CCD1Temp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CCD2Temp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CCDSTemp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

								for (int _core = 1; _core <= CPUCores; ++_core)
								{
									int _coreoffset = ZenCoreMap[_core - 1];
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 169 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 185 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 201 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 249 + _coreoffset, _core, 1000);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 265 + _coreoffset, _core, 1000);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 281 + _coreoffset, _core);
								}
								for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
								{
									App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
								}
								Trace.WriteLine($"Configuring Zen Source done");

							}
							else if (ZenPTVersion == 0x380904)
							{
								ZenPTKnown = true;
								int _maxcores = 8;
								Trace.WriteLine($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen3");

								ZenPerCCDTemp = true;

								CPUSensorsSource = "Zen PowerTable";
								HWMonitor.CPUSource = HWSensorSource.Zen;

								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

								ZenRefreshStatic(false);

								App.hwsensors.InitZen(HWSensorName.CPUPPT, 1);
								App.hwsensors.InitZen(HWSensorName.CPUTDC, 3);
								App.hwsensors.InitZen(HWSensorName.CPUEDC, 9);
								App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUPower, 29);
								int _vsoc = (int)Zen.powerTable.Table[45] == 0 ? 44 : 45;
								App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
								App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 347);
								App.hwsensors.InitZen(HWSensorName.CPUFSB, 70);
								App.hwsensors.InitZen(HWSensorName.CPUVoltage, 41);
								App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);

								App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
								App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
								App.hwsensors.InitZen(HWSensorName.CCD1Temp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CCD2Temp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CCDSTemp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

								for (int _core = 1; _core <= CPUCores; ++_core)
								{
									int _coreoffset = ZenCoreMap[_core - 1];
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 169 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 177 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 185 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 209 + _coreoffset, _core, 1000);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 217 + _coreoffset, _core, 1000);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 225 + _coreoffset, _core);
								}
								for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
								{
									App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
								}
								Trace.WriteLine($"Configuring Zen Source done");

							}
							else if (ZenPTVersion == 0x380805)
							{
								ZenPTKnown = true;
								int _maxcores = 16;
								Trace.WriteLine($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen3");

								ZenPerCCDTemp = true;

								CPUSensorsSource = "Zen PowerTable";
								HWMonitor.CPUSource = HWSensorSource.Zen;

								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD2L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

								ZenRefreshStatic(false);

								App.hwsensors.InitZen(HWSensorName.CPUPPT, 1);
								App.hwsensors.InitZen(HWSensorName.CPUTDC, 3);
								App.hwsensors.InitZen(HWSensorName.CPUEDC, 9);
								App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUPower, 29);
								int _vsoc = (int)Zen.powerTable.Table[45] == 0 ? 44 : 45;
								App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
								App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 544);
								App.hwsensors.InitZen(HWSensorName.CCD2L3Temp, 545);
								App.hwsensors.InitZen(HWSensorName.CPUFSB, 70);
								App.hwsensors.InitZen(HWSensorName.CPUVoltage, 41);
								App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);

								App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
								App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
								App.hwsensors.InitZen(HWSensorName.CCD1Temp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CCD2Temp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CCDSTemp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

								for (int _core = 1; _core <= CPUCores; ++_core)
								{
									int _coreoffset = ZenCoreMap[_core - 1];
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 172 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 188 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 204 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 252 + _coreoffset, _core, 1000);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 268 + _coreoffset, _core, 1000);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 284 + _coreoffset, _core);
								}
								for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
								{
									App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
								}
								Trace.WriteLine($"Configuring Zen Source done");

							}
							else if (ZenPTVersion == 0x380905)
							{
								ZenPTKnown = true;
								int _maxcores = 8;
								Trace.WriteLine($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen3");

								ZenPerCCDTemp = true;

								CPUSensorsSource = "Zen PowerTable";
								HWMonitor.CPUSource = HWSensorSource.Zen;

								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

								ZenRefreshStatic(false);

								App.hwsensors.InitZen(HWSensorName.CPUPPT, 1);
								App.hwsensors.InitZen(HWSensorName.CPUTDC, 3);
								App.hwsensors.InitZen(HWSensorName.CPUEDC, 9);
								App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUPower, 29);
								int _vsoc = (int)Zen.powerTable.Table[45] == 0 ? 44 : 45;
								App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
								App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 358);
								App.hwsensors.InitZen(HWSensorName.CPUFSB, 70);
								App.hwsensors.InitZen(HWSensorName.CPUVoltage, 41);
								App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);

								App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
								App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
								App.hwsensors.InitZen(HWSensorName.CCD1Temp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CCD2Temp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CCDSTemp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

								for (int _core = 1; _core <= CPUCores; ++_core)
								{
									int _coreoffset = ZenCoreMap[_core - 1];
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 172 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 180 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 188 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 212 + _coreoffset, _core, 1000);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 220 + _coreoffset, _core, 1000);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 228 + _coreoffset, _core);
								}
								for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
								{
									App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
								}
								Trace.WriteLine($"Configuring Zen Source done");

							}
							else if (ZenPTVersion == 0x400005)
							{
								ZenPTKnown = true;
								int _maxcores = 8;
								Trace.WriteLine($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen3 APU");

								ZenPerCCDTemp = true;

								CPUSensorsSource = "Zen PowerTable";
								HWMonitor.CPUSource = HWSensorSource.Zen;

								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

								ZenRefreshStatic(false);

								App.hwsensors.InitZen(HWSensorName.CPUPPT, 5);
								App.hwsensors.InitZen(HWSensorName.CPUTDC, 9);
								App.hwsensors.InitZen(HWSensorName.CPUEDC, 12);
								App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUPower, 38);
								int _vsoc = (int)Zen.powerTable.Table[103] == 0 ? 102 : 103;
								App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
								App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 386);
								App.hwsensors.InitZen(HWSensorName.CPUFSB, 78);
								App.hwsensors.InitZen(HWSensorName.CPUVoltage, 99);
								//App.hwsensors.InitZen(HWSensorName.CPUTemp, 17);
								App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);
								App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
								App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
								App.hwsensors.InitZen(HWSensorName.CCD1Temp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CCD2Temp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CCDSTemp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

								for (int _core = 1; _core <= CPUCores; ++_core)
								{
									int _coreoffset = ZenCoreMap[_core - 1];
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 200 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 208 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 216 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 240 + _coreoffset, _core, 1000);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 248 + _coreoffset, _core, 1000);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 256 + _coreoffset, _core);
								}
								for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
								{
									App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
								}
								Trace.WriteLine($"Configuring Zen Source done");

							}
							else if (ZenPTVersion == 0x240903)
							{
								ZenPTKnown = true;
								int _maxcores = 8;
								Trace.WriteLine($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen2");

								ZenPerCCDTemp = true;

								CPUSensorsSource = "Zen PowerTable";
								HWMonitor.CPUSource = HWSensorSource.Zen;

								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

								ZenRefreshStatic(false);

								App.hwsensors.InitZen(HWSensorName.CPUPPT, 1);
								App.hwsensors.InitZen(HWSensorName.CPUTDC, 3);
								App.hwsensors.InitZen(HWSensorName.CPUEDC, 9);
								App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUPower, 29);
								int _vsoc = (int)Zen.powerTable.Table[45] == 0 ? 44 : 45;
								App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
								App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 303);
								App.hwsensors.InitZen(HWSensorName.CPUFSB, 66);
								App.hwsensors.InitZen(HWSensorName.CPUVoltage, 40);
								//App.hwsensors.InitZen(HWSensorName.CPUTemp, 5);
								App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);

								App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
								App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
								App.hwsensors.InitZen(HWSensorName.CCD1Temp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CCD2Temp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CCDSTemp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

								for (int _core = 1; _core <= CPUCores; ++_core)
								{
									int _coreoffset = ZenCoreMap[_core - 1];
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 147 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 155 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 163 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 187 + _coreoffset, _core, 1000);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 195 + _coreoffset, _core, 1000);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 203 + _coreoffset, _core);
								}
								for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
								{
									App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
								}
								Trace.WriteLine($"Configuring Zen Source done");

							}
							else if (ZenPTVersion == 0x240803)
							{
								ZenPTKnown = true;
								int _maxcores = 16;
								Trace.WriteLine($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen2");

								ZenPerCCDTemp = true;

								CPUSensorsSource = "Zen PowerTable";
								HWMonitor.CPUSource = HWSensorSource.Zen;

								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
								App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

								ZenRefreshStatic(false);

								App.hwsensors.InitZen(HWSensorName.CPUPPT, 1);
								App.hwsensors.InitZen(HWSensorName.CPUTDC, 3);
								App.hwsensors.InitZen(HWSensorName.CPUEDC, 9);
								App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
								App.hwsensors.InitZen(HWSensorName.CPUPower, 29);
								int _vsoc = (int)Zen.powerTable.Table[45] == 0 ? 44 : 45;
								App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
								App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 459);
								App.hwsensors.InitZen(HWSensorName.CPUFSB, 66);
								App.hwsensors.InitZen(HWSensorName.CPUVoltage, 40);
								App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);

								App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
								App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
								App.hwsensors.InitZen(HWSensorName.CCD1Temp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CCD2Temp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CCDSTemp, -1, 1);
								App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

								for (int _core = 1; _core <= CPUCores; ++_core)
								{
									int _coreoffset = ZenCoreMap[_core - 1];
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 147 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 163 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 179 + _coreoffset, _core);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 227 + _coreoffset, _core, 1000);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 243 + _coreoffset, _core, 1000);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
									App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 259 + _coreoffset, _core);
								}
								for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
								{
									App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
								}
								Trace.WriteLine($"Configuring Zen Source done");
							}

							for (int it = 0; it < Zen.powerTable.Table.Length; ++it)
							{
								line = $"\t\t[{it * 4:X3}][{it}] = [{Zen.powerTable.Table[it]}]";
								Trace.WriteLine(line);
								sb.AppendLine(line);
							}

							string path = @".\Logs\dumpzenpt.txt";
							if (!File.Exists(path)) File.Delete(path);

							using (StreamWriter sw = File.CreateText(path))
							{
								sw.WriteLine(sb.ToString());
							}

							if (!ZenPTKnown)
							{
								try
								{
									string ZenPTSubject = $"Support for Zen PowerTable [0x{ZenPTVersion:X}]";
									string ZenPTBody = sb.ToString();

									var client = new GitHubClient(new ProductHeaderValue("BenchMaestro"));
									var issues = client.Issue.GetAllForRepository("mann1x", "BenchMaestro");
									issues.Wait();
									bool _newpt = true;
									if (issues.IsCompleted)
									{
										foreach (Issue issue in issues.Result)
										{
											if (issue.Title == ZenPTSubject) _newpt = false;
										}
									}
									if (_newpt && issues.IsCompletedSuccessfully)
									{
										App.ZenPTBody = ZenPTBody;
										App.ZenPTSubject = ZenPTSubject;
									}
								}
								catch (Exception ex)
								{
									Trace.WriteLine($"Failed to get GitHub Issues: {ex}");
								}
							}

							sb.Clear();
							sb = null;

						}
						else
						{
							Trace.WriteLine($"Failed SMU PowerTable refresh: {status}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"Exception ZenMainInit: {ex}");
			}
		}
		public void UpdateLiveCPUTemp(string _value)
        {
			LiveCPUTemp = _value.Length > 0 ? _value : "N/A";
			//Trace.WriteLine($"{_value}");
			OnChange("LiveCPUTemp");			
		}
		public void UpdateLiveCPUPower(string _value)
		{
			LiveCPUPower = _value.Length > 0 ? _value : "N/A";
			//Trace.WriteLine($"{_value}");
			OnChange("LiveCPUPower");
		}
		public void UpdateLiveCPUClock(string _value)
		{
			LiveCPUClock = _value.Length > 0 ? _value : "N/A";
			//Trace.WriteLine($"{_value}");
			OnChange("LiveCPUClock");
		}
		public void UpdateLiveCPUAdditional(string _value)
		{
			LiveCPUAdditional = _value.Length > 0 ? _value : "N/A";
			//Trace.WriteLine($"{_value}");
			OnChange("LiveCPUAdditional");
		}
		public void UpdateLiveFinished(string _value)
		{
			LiveFinished = _value.Length > 0 ? _value : "N/A";
			//Trace.WriteLine($"{_value}");
			OnChange("LiveFinished");
		}
		public void SetLastVersionOnServer(string _value)
		{
			LastVersionOnServer = _value.Length > 0 ? _value : "N/A";
			//Trace.WriteLine($"{_value}");
			OnChange("LastVersionOnServer");
		}
		protected void OnChange(string info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
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