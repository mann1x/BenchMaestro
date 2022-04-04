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
using Octokit;
using System.Xml.Linq;
using System.ComponentModel;

namespace BenchMaestro
{
	public class SystemInfo : INotifyPropertyChanged
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
			try
			{
				WinMaxSize = 600;

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

				CPUSensorsSource = "LibreHardwareMonitor";
				HWMonitor.CPUSource = HWSensorSource.Libre;
				HWMonitor.NewSensors();

				try
				{
					if (CPUSocket == "AM4")
					{
						bool smucheck = false;
						try
						{
							Zen = new Cpu();
							smucheck = Zen.smu.Version != 0U;
							Trace.WriteLine($"Test SMU for Zen: {smucheck}");
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

							ZenCCDTotal = (int)ZenCCDS_Total;

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
									Trace.WriteLine($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] Zen1");

									int _maxcores = 8;

									CPUSensorsSource = "Zen PowerTable";
									HWMonitor.CPUSource = HWSensorSource.Zen;

									App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
									App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
									App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
									App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
									App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
									App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
									App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
									App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));
									App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD2L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

									ZenRefreshStatic(false);

									App.hwsensors.InitZen(HWSensorName.CPUPPT, 1);
									App.hwsensors.InitZen(HWSensorName.CPUTDC, 3);
									App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
									App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
									App.hwsensors.InitZen(HWSensorName.CPUPower, 22);
									int _vsoc = (int)Zen.powerTable.Table[45] == 0 ? 44 : 45;
									App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
									App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 158);
									App.hwsensors.InitZen(HWSensorName.CCD2L3Temp, 159);
									App.hwsensors.InitZen(HWSensorName.CPUFSB, 66);
									App.hwsensors.InitZen(HWSensorName.CPUVoltage, 40);
									App.hwsensors.InitZen(HWSensorName.CPUTemp, 5);
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
									App.hwsensors.SetValueOffset(HWSensorName.CPUTemp, -20);
									App.hwsensors.SetValueOffset(HWSensorName.CPUCoresTemps, -20);

								}
								else if (ZenPTVersion == 0x380804)
								{
									ZenPTKnown = true;
									Trace.WriteLine($"Configuring Zen Source for PT [0x{ZenPTVersion:X}]");

									int _maxcores = 16;
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
									App.hwsensors.InitZen(HWSensorName.CPUTemp, 5);

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

								}
								else if (ZenPTVersion == 0x380904)
								{
									ZenPTKnown = true;
									Trace.WriteLine($"Configuring Zen Source for PT [0x{ZenPTVersion:X}]");

									int _maxcores = 8;
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
									App.hwsensors.InitZen(HWSensorName.CPUTemp, 5);

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

								}
								else if (ZenPTVersion == 0x380805)
								{
									ZenPTKnown = true;
									Trace.WriteLine($"Configuring Zen Source for PT [0x{ZenPTVersion:X}]");

									int _maxcores = 16;
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
									App.hwsensors.InitZen(HWSensorName.CPUTemp, 5);

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

								}
								else if (ZenPTVersion == 0x380905)
								{
									ZenPTKnown = true;
									Trace.WriteLine($"Configuring Zen Source for PT [0x{ZenPTVersion:X}]");

									int _maxcores = 8;
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
									App.hwsensors.InitZen(HWSensorName.CPUTemp, 5);

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

								}
								else if (ZenPTVersion == 0x400005)
								{
									ZenPTKnown = true;
									Trace.WriteLine($"Configuring Zen Source for PT [0x{ZenPTVersion:X}]");

									int _maxcores = 8;
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
									App.hwsensors.InitZen(HWSensorName.CPUTemp, 17);

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

								}
								else if (ZenPTVersion == 0x240903)
								{
									ZenPTKnown = true;
									Trace.WriteLine($"Configuring Zen Source for PT [0x{ZenPTVersion:X}]");

									int _maxcores = 8;
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
									App.hwsensors.InitZen(HWSensorName.CPUTemp, 5);

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

								}
								else if (ZenPTVersion == 0x240803)
								{
									ZenPTKnown = true;
									Trace.WriteLine($"Configuring Zen Source for PT [0x{ZenPTVersion:X}]");

									int _maxcores = 16;
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
									App.hwsensors.InitZen(HWSensorName.CPUTemp, 5);

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
								}

								for (int it = 0; it < Zen.powerTable.Table.Length; ++it)
								{
									line = $"\t\t[{it*4:X3}][{it}] = [{Zen.powerTable.Table[it]}]";
									Trace.WriteLine(line);
									sb.AppendLine(line);
								}

								string path = @".\dumpzenpt.txt";
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
				CPPCOrder = new int[CPUCores];
				CPPCOrder1 = new int[CPUCores];

				Trace.WriteLine("WMI System Exception: " + e);
			}

			RefreshLabels();

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
				ZenPPT = (int)Zen.powerTable.Table[0];
				ZenTDC = (int)Zen.powerTable.Table[2];
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
					for (int r = 0; r < 80; ++r)
					{
						Thread.Sleep(25);
						status = Zen.RefreshPowerTable();
						if (status == SMU.Status.OK) r = 80;
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
			CPULabel = $"{CPUName} [Socket {CPUSocket}]\n{CPUDescription} x{CPUBits}";
			BoardLabel = $"{BoardModel} [BIOS Version {BoardBIOS}]\n{BoardManufacturer}";
			ProcessorsLabel = $"{CPUCores}";
			if (HyperThreading) ProcessorsLabel += $" [Threads: {CPULogicalProcessors}]";
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

				if (ZenCoreMapLabel.Length > 0) ProcessorsLabel += $"\nZen CoreMap: {ZenCoreMapLabel} ";

			}
			OnChange("CPULabel");
			OnChange("BoardLabel");
			OnChange("ProcessorsLabel");
			Trace.WriteLine($"RefreshLabels done");
		}
		protected void OnChange(string info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}
	}
}