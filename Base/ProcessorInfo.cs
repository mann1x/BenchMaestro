using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BenchMaestro
{
	public static class ProcessorInfo
	{
		private static IHardwareCore[] cores;
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
		private static extern int GetCurrentProcessorNumber();

		#endregion

	}
}
