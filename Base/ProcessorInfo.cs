using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BenchMaestro
{
	public static class ProcessorInfo
	{
		private static IHardwareCore[] cores;
		private static IHardwareCpuSet[] cpuset;
		private static int[] logicalCores;

		/// <summary>
		/// Hardware core
		/// </summary>
		public interface IHardwareCore
		{
			/// <summary>
			/// Logical core IDs
			/// </summary>
			int[] LogicalCores { get; }
		}

		/// <summary>
		/// Hardware CpuSets
		/// </summary>
		public interface IHardwareCpuSet
		{
			/// <summary>
			/// CpuSets ID
			/// </summary>
			int Id { get; }
			/// <summary>
			/// CpuSets EfficiencyClass
			/// </summary>
			int EfficiencyClass { get; }
			/// <summary>
			/// CpuSets LogicalProcessorIndex
			/// </summary>
			int LogicalProcessorIndex { get; }
			/// <summary>
			/// CpuSets CoreIndex
			/// </summary>
			int CoreIndex { get; }
			/// <summary>
			/// CpuSets NumaNodeIndex
			/// </summary>
			int NumaNodeIndex { get; }
			/// <summary>
			/// CpuSets LastLevelCacheIndex
			/// </summary>
			int LastLevelCacheIndex { get; }
			/// <summary>
			/// CpuSets Group
			/// </summary>
			int Group { get; }
			/// <summary>
			/// CpuSets SchedulingClass
			/// </summary>
			int SchedulingClass { get; }
			/// <summary>
			/// CpuSets AllocationTag
			/// </summary>
			long AllocationTag { get; }
			/// <summary>
			/// CpuSets AllFlagsStruct
			/// </summary>
			byte AllFlagsStruct { get; }
			/// <summary>
			/// CpuSets Parked
			/// </summary>
			int Parked
			{
				get
				{
					return (int)(AllFlagsStruct & 1u);
				}
			}
			/// <summary>
			/// CpuSets Allocated
			/// </summary>
			int Allocated
			{
				get
				{
					return (int)((AllFlagsStruct & 2u) / 2D);
				}
			}
			/// <summary>
			/// CpuSets AllocatedToTargetProcess
			/// </summary>
			int AllocatedToTargetProcess
			{
				get
				{
					return (int)((AllFlagsStruct & 4u) / 4D);
				}
			}
			/// <summary>
			/// CpuSets RealTime
			/// </summary>
			int RealTime
			{
				get
				{
					return (int)((AllFlagsStruct & 8u) / 8D);
				}
			}
		}

		/// <summary>
		/// Hardware cores
		/// </summary>
		public static IHardwareCore[] HardwareCores
		{
			get
			{
				return cores ?? (cores = GetLogicalProcessorInformation()
					.Where(x => x.Relationship == LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorCore)
					.Select(x => new HardwareCore((UInt64)x.ProcessorMask))
					.ToArray<IHardwareCore>());
			}
		}

		/// <summary>
		/// Hardware CpuSets
		/// </summary>
		public static IHardwareCpuSet[] HardwareCpuSets
		{
			get
			{
				return cpuset ?? (cpuset = GetSystemCpuSetInformation()
					.Where(x => x.Type == CPUSET_TYPE.CpuSetInformation && x.CpuSetUnion.CpuSet.Id != 0)
					.Select(x => new HardwareCpuSet(x))
					.OrderBy(x => x.LogicalProcessorIndex)
					.ToArray<IHardwareCpuSet>());
			}
		}

		/// <summary>
		/// All logical core IDs
		/// </summary>
		public static int[] LogicalCores
		{
			get
			{
				return logicalCores ?? (logicalCores = HardwareCores
					.SelectMany(x => x.LogicalCores)
					.ToArray());
			}
		}

		/// <summary>
		/// Return CPUSets Id
		/// </summary>
		public static int Id
		{
			get
			{
				return HardwareCpuSets
					.Select(x => x.Id).First()
					;
			}
		}

		/// <summary>
		/// Get Core ID from Logical
		/// </summary>
		public static int PhysicalCore(int logicalCore)
		{
			for (var i = 0; i < HardwareCores.Length; ++i)
			{
				if (HardwareCores[i].LogicalCores.Contains(logicalCore)) return i;
			}
			return 0;
		}
		/// <summary>
		/// Get Thread ID from Logical
		/// </summary>
		public static int ThreadID(int logicalCore)
		{
			for (var i = 0; i < HardwareCores.Length; ++i)
			{
				if (HardwareCores[i].LogicalCores.Contains(logicalCore))
				{
					for (int t = 0; t < HardwareCores[i].LogicalCores.Length; ++t)
					{
						if (HardwareCores[i].LogicalCores[t] == logicalCore) return t;

					}
				}
			}
			return 0;
		}

		/// <summary>
		/// Get Cores Sorted by SchedulingClass
		/// </summary>
		public static int[][] CoresByScheduling()
		{
			var coresbysched = new int[HardwareCores.Length][];
			int count = 0;
			var cpusetsbysched = HardwareCpuSets.OrderByDescending(x => x.SchedulingClass);
			int _prev = -1;
			foreach (HardwareCpuSet cpuset in cpusetsbysched)
            {
				int _pcore = PhysicalCore(cpuset.LogicalProcessorIndex);
				//System.Diagnostics.Trace.WriteLine($"Logical={cpuset.LogicalProcessorIndex} Physical={_pcore} Prev={_prev} ");
				if (_pcore != _prev)
				{
					//System.Diagnostics.Trace.WriteLine($"Add Physical={_pcore} Count={count} ");
					coresbysched[count] = new int[2];
					coresbysched[count][0] = _pcore;
					coresbysched[count][1] = cpuset.SchedulingClass;
					count++;
				}
				_prev = _pcore;
			}
			return coresbysched;
		}
		/// <summary>
		/// Get Cores Sorted by EfficiencyClass
		/// </summary>
		public static int[][] CoresByEfficiency()
		{
			var coresbyeff = new int[HardwareCores.Length][];
			int count = 0;
			var cpusetsbyeff = HardwareCpuSets.OrderByDescending(x => x.EfficiencyClass);
			int _prev = -1;
			foreach (HardwareCpuSet cpuset in cpusetsbyeff)
			{
				int _pcore = PhysicalCore(cpuset.LogicalProcessorIndex);
				//System.Diagnostics.Trace.WriteLine($"Logical={cpuset.LogicalProcessorIndex} Physical={_pcore} Prev={_prev} ");
				if (_pcore != _prev)
				{
					//System.Diagnostics.Trace.WriteLine($"Add Physical={_pcore} Count={count} ");
					coresbyeff[count] = new int[2];
					coresbyeff[count][0] = _pcore;
					coresbyeff[count][1] = cpuset.EfficiencyClass;
					count++;
				}
				_prev = _pcore;
			}
			return coresbyeff;
		}
		public static bool IsCoresByEfficiencyAllZeros()
		{
			int zeros = HardwareCpuSets.Where(x => x.EfficiencyClass == 0).Count();
			if (zeros == HardwareCpuSets.Length) return true;
			return false;
		}
		public static bool IsCoresBySchedulingAllZeros()
		{
			int zeros = HardwareCpuSets.Where(x => x.SchedulingClass == 0).Count();
			if (zeros == HardwareCpuSets.Length) return true;
			return false;
		}
		/// <summary>
		/// Get number of Logical processors
		/// </summary>
		public static int TotalLogicalProcessors()
		{
			int count = 0;
			for (var i = 0; i < HardwareCores.Length; ++i)
			{
				count += HardwareCores[i].LogicalCores.Count();
			}
			return count;
		}

		/// <summary>
		/// Get Last Thread ID for Processor
		/// </summary>
		public static int LastThreadID()
		{
			return HardwareCores[HardwareCores.Length - 1].LogicalCores.Last();
		}

		/// <summary>
		/// Current logical core ID
		/// </summary>
		public static int CurrentLogicalCore
		{
			get { return GetCurrentProcessorNumber(); }
		}

		private class HardwareCore : IHardwareCore
		{
			public HardwareCore(UInt64 logicalCoresMask)
			{
				var logicalCores = new List<int>();

				for (var i = 0; i < 64; ++i)
				{
					if (((logicalCoresMask >> i) & 0x1) == 0) continue;
					logicalCores.Add(i);
				}

				LogicalCores = logicalCores.ToArray();
			}

			public int[] LogicalCores { get; private set; }
		}

		/// <summary>
		/// CpuSet LogicalProcessorIndex for Logical
		/// </summary>
		public static int? CpuSetLogicalProcessorIndex(int logical)
		{
			if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
			return HardwareCpuSets[logical].LogicalProcessorIndex;
		}
		
		/// <summary>
		/// CpuSet ID for Logical
		/// </summary>
		public static int? CpuSetID(int logical)
		{
			if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
			return HardwareCpuSets[logical].Id;
		}
		/// <summary>
		/// CpuSet EfficiencyClass for Logical
		/// </summary>
		public static int? CpuSetEfficiencyClass(int logical)
		{
			if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
			return HardwareCpuSets[logical].EfficiencyClass;
		}
		/// <summary>
		/// CpuSet CoreIndex for Logical
		/// </summary>
		public static int? CpuSetCoreIndex(int logical)
		{
			if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
			return HardwareCpuSets[logical].CoreIndex;
		}
		/// <summary>
		/// CpuSet NumaNodeIndex for Logical
		/// </summary>
		public static int? CpuSetNumaNodeIndex(int logical)
		{
			if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
			return HardwareCpuSets[logical].NumaNodeIndex;
		}
		/// <summary>
		/// CpuSet LastLevelCacheIndex for Logical
		/// </summary>
		public static int? CpuSetLastLevelCacheIndex(int logical)
		{
			if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
			return HardwareCpuSets[logical].LastLevelCacheIndex;
		}
		/// <summary>
		/// CpuSet Group for Logical
		/// </summary>
		public static int? CpuSetGroup(int logical)
		{
			if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
			return HardwareCpuSets[logical].Group;
		}
		/// <summary>
		/// CpuSet SchedulingClass for Logical
		/// </summary>
		public static int? CpuSetSchedulingClass(int logical)
		{
			if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
			return HardwareCpuSets[logical].SchedulingClass;
		}
		/// <summary>
		/// CpuSet AllocationTag for Logical
		/// </summary>
		public static long? CpuSetAllocationTag(int logical)
		{
			if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
			return HardwareCpuSets[logical].AllocationTag;
		}
		/// <summary>
		/// CpuSet Parked for Logical
		/// </summary>
		public static int? CpuSetParked(int logical)
		{
			if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
			return HardwareCpuSets[logical].Parked;
		}
		/// <summary>
		/// CpuSet Allocated for Logical
		/// </summary>
		public static int? CpuSetAllocated(int logical)
		{
			if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
			return HardwareCpuSets[logical].Allocated;
		}
		/// <summary>
		/// CpuSet RealTime for Logical
		/// </summary>
		public static int? CpuSetRealtime(int logical)
		{
			if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
			return HardwareCpuSets[logical].RealTime;
		}
		/// <summary>
		/// CpuSet AllocatedToTargetProcess for Logical
		/// </summary>
		public static int? CpuSetAllocatedToTargetProcess(int logical)
		{
			if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
			return HardwareCpuSets[logical].AllocatedToTargetProcess;
		}
		private class HardwareCpuSet : IHardwareCpuSet
		{
			public HardwareCpuSet(SYSTEM_CPU_SET_INFORMATION x)
			{
				CpuSet = x;
				Id = (int)x.CpuSetUnion.CpuSet.Id;
				EfficiencyClass = (int)x.CpuSetUnion.CpuSet.EfficiencyClass;
				LogicalProcessorIndex = (int)x.CpuSetUnion.CpuSet.LogicalProcessorIndex;
				CoreIndex = (int)x.CpuSetUnion.CpuSet.CoreIndex;
				NumaNodeIndex = (int)x.CpuSetUnion.CpuSet.NumaNodeIndex;
				LastLevelCacheIndex = (int)x.CpuSetUnion.CpuSet.LastLevelCacheIndex;
				Group = (int)x.CpuSetUnion.CpuSet.Group;
				SchedulingClass = (int)x.CpuSetUnion.CpuSet.CpuSetSchedulingClass.SchedulingClass;
				AllocationTag = (int)x.CpuSetUnion.CpuSet.AllocationTag;
				AllFlagsStruct = x.CpuSetUnion.CpuSet.AllFlagsStruct.AllFlagsStruct;
			}
			public SYSTEM_CPU_SET_INFORMATION CpuSet { get; private set; }
			public int Id { get; private set; }
			public int EfficiencyClass { get; private set; }
			public int LogicalProcessorIndex { get; private set; }
			public int CoreIndex { get; private set; }
			public int NumaNodeIndex { get; private set; }
			public int LastLevelCacheIndex { get; private set; }
			public int Group { get; private set; }
			public int SchedulingClass { get; private set; }
			public long AllocationTag { get; private set; }
			public byte AllFlagsStruct { get; private set; }
		}

		#region Exports

		[StructLayout(LayoutKind.Sequential)]
		private struct PROCESSORCORE
		{
			public byte Flags;
		};

		[StructLayout(LayoutKind.Sequential)]
		private struct NUMANODE
		{
			public uint NodeNumber;
		}
		private enum PROCESSOR_CACHE_TYPE
		{
			CacheUnified,
			CacheInstruction,
			CacheData,
			CacheTrace
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct CACHE_DESCRIPTOR
		{
			public byte Level;
			public byte Associativity;
			public ushort LineSize;
			public uint Size;
			public PROCESSOR_CACHE_TYPE Type;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION_UNION
		{
			[FieldOffset(0)]
			public PROCESSORCORE ProcessorCore;
			[FieldOffset(0)]
			public NUMANODE NumaNode;
			[FieldOffset(0)]
			public CACHE_DESCRIPTOR Cache;
			[FieldOffset(0)]
			private UInt64 Reserved1;
			[FieldOffset(8)]
			private UInt64 Reserved2;
		}
		private enum LOGICAL_PROCESSOR_RELATIONSHIP
		{
			RelationProcessorCore,
			RelationNumaNode,
			RelationCache,
			RelationProcessorPackage,
			RelationGroup,
			RelationAll = 0xffff
		}
		private struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION
		{
			public UIntPtr ProcessorMask;
			public LOGICAL_PROCESSOR_RELATIONSHIP Relationship;
			public SYSTEM_LOGICAL_PROCESSOR_INFORMATION_UNION ProcessorInformation;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct SYSTEM_CPU_SET_INFORMATION_CPUSET
		{
			public int Id;
			public short Group;
			public byte LogicalProcessorIndex;
			public byte CoreIndex;
			public byte LastLevelCacheIndex;
			public byte NumaNodeIndex;
			public byte EfficiencyClass;
			public CPUSET_ALLFLAGS AllFlagsStruct;
			public CPUSET_SCHEDULINGCLASS CpuSetSchedulingClass;
			public long AllocationTag;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct CPUSET_SCHEDULINGCLASS
		{
			[FieldOffset(0)]
			public int Reserved;
			[FieldOffset(0)]
			public byte SchedulingClass;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct CPUSET_ALLFLAGS
		{
			[FieldOffset(0)]
			public byte AllFlags;
			[FieldOffset(0)]
			public byte AllFlagsStruct;
		}
		private enum CPUSET_TYPE
		{
			CpuSetInformation
		}
		[StructLayout(LayoutKind.Sequential)]
		private struct SYSTEM_CPU_SET_INFORMATION
		{
			public int Size;
			public CPUSET_TYPE Type;
			public SYSTEM_CPU_SET_INFORMATION_CPUSET_UNION CpuSetUnion;
		}
		[StructLayout(LayoutKind.Explicit)]
		private struct SYSTEM_CPU_SET_INFORMATION_CPUSET_UNION
		{
			[FieldOffsetAttribute(0)]
			public SYSTEM_CPU_SET_INFORMATION_CPUSET CpuSet;
		}

		[DllImport(@"kernel32.dll", SetLastError = true)]
		private static extern bool GetLogicalProcessorInformation(
			IntPtr Buffer,
			ref uint ReturnLength
		);

		private const int ERROR_INSUFFICIENT_BUFFER = 122;

		private static SYSTEM_LOGICAL_PROCESSOR_INFORMATION[] GetLogicalProcessorInformation()
		{
			uint ReturnLength = 0;
			GetLogicalProcessorInformation(IntPtr.Zero, ref ReturnLength);
			if (Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
			{
				IntPtr Ptr = Marshal.AllocHGlobal((int)ReturnLength);
				try
				{
					if (GetLogicalProcessorInformation(Ptr, ref ReturnLength))
					{
						int size = Marshal.SizeOf(typeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION));
						int len = (int)ReturnLength / size;
						SYSTEM_LOGICAL_PROCESSOR_INFORMATION[] Buffer = new SYSTEM_LOGICAL_PROCESSOR_INFORMATION[len];
						IntPtr Item = Ptr;
						for (int i = 0; i < len; i++)
						{
							Buffer[i] = (SYSTEM_LOGICAL_PROCESSOR_INFORMATION)Marshal.PtrToStructure(Item, typeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION));
							Item += size;
						}
						return Buffer;
					}
				}
				finally
				{
					Marshal.FreeHGlobal(Ptr);
				}
			}
			return null;
		}

		[DllImport(@"kernel32.dll", SetLastError = true)]
		private static extern bool GetSystemCpuSetInformation(
			IntPtr Buffer,
			uint BufferLength,
			ref uint ReturnLength,
			IntPtr handle,
			uint Flags
		);

		private static SYSTEM_CPU_SET_INFORMATION[] GetSystemCpuSetInformation()
		{
			uint ReturnLength = 0;
			IntPtr CurProc = Process.GetCurrentProcess().Handle;
			GetSystemCpuSetInformation(IntPtr.Zero, 0, ref ReturnLength, CurProc, 0);
			if (Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
			{
				IntPtr Ptr = Marshal.AllocHGlobal((int)ReturnLength);
				try
				{
					if (GetSystemCpuSetInformation(Ptr, ReturnLength, ref ReturnLength, CurProc, 0))
					{
						int size = Marshal.SizeOf(typeof(SYSTEM_CPU_SET_INFORMATION));
						int len = (int)ReturnLength / size;
						//System.Diagnostics.Trace.WriteLine($"CPUSET SIZE={size} len={len} ReturnLength={ReturnLength}");
						SYSTEM_CPU_SET_INFORMATION[] Buffer = new SYSTEM_CPU_SET_INFORMATION[len];
						IntPtr Item = Ptr;
						for (int i = 0; i < len; i++)
						{
							Buffer[i] = (SYSTEM_CPU_SET_INFORMATION)Marshal.PtrToStructure(Item, typeof(SYSTEM_CPU_SET_INFORMATION));
							System.Diagnostics.Trace.WriteLine($"PI CpuSet Logical={Buffer[i].CpuSetUnion.CpuSet.LogicalProcessorIndex} Cache={Buffer[i].CpuSetUnion.CpuSet.LastLevelCacheIndex} Sch={Buffer[i].CpuSetUnion.CpuSet.CpuSetSchedulingClass.SchedulingClass} Eff={Buffer[i].CpuSetUnion.CpuSet.EfficiencyClass} Id={Buffer[i].CpuSetUnion.CpuSet.Id}");
							Item += size;
						}
						return Buffer;
					}
				}
				finally
				{
					Marshal.FreeHGlobal(Ptr);
				}
			}
			return null;
		}

		[DllImport(@"kernel32.dll")]
		private static extern int GetCurrentProcessorNumber();

		#endregion

	}
}
