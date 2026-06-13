//////////////////////////////////////////////////////////////////////////////////////////////////
//	Projects		: DHInfoDesk
//	Author			: CYBERKDH(cyberkdh@hotmail.com, cyberkdh@gmail.com), AI(Codex)
//	Module			: DHInfoDesk.module.sysinfo
//	History			:
//	Copyrights		: Copyright ⓒCYBERKDH. All Rights Reserved.
//////////////////////////////////////////////////////////////////////////////////////////////////

using DHInfoDesk.module.win32;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;

namespace DHInfoDesk.module.sysinfo {
	internal sealed class DHSysInfoCpu : IDisposable {
		private const string DEF_REGISTRY_CPU_PATH = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0";
		private const int DEF_RELATION_PROCESSOR_CORE = 0;
		private const int DEF_ERROR_INSUFFICIENT_BUFFER = 122;

		private PerformanceCounter m_counterUsage = null;

		public DHSysInfoCpu() {
			try {
				m_counterUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
				m_counterUsage.NextValue();
			}
			catch {
				m_counterUsage = null;
			}
		}

		public DHSysInfoCpuData CollectStatic() {
			DHSysInfoCpuData data = new DHSysInfoCpuData();
			data.LogicalProcessorCount = Environment.ProcessorCount;

			try {
				using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
					"SELECT Name, Manufacturer, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed FROM Win32_Processor")) {
					foreach (ManagementObject obj in searcher.Get()) {
						if (string.IsNullOrEmpty(data.Name) == true) {
							data.Name = Convert.ToString(obj["Name"]).Trim();
							data.Manufacturer = Convert.ToString(obj["Manufacturer"]).Trim();
							data.MaxClockMHz = ToUInt32(obj["MaxClockSpeed"]);
						}

						data.PhysicalCoreCount += ToInt32(obj["NumberOfCores"]);
						int nlogical = ToInt32(obj["NumberOfLogicalProcessors"]);
						if (nlogical > 0) {
							data.LogicalProcessorCount = nlogical;
						}
					}
				}
			}
			catch {
				data.PhysicalCoreCount = 0;
			}

			if (data.PhysicalCoreCount <= 0) {
				data.PhysicalCoreCount = GetPhysicalCoreCount();
			}

			if (string.IsNullOrEmpty(data.Name) == true) {
				CollectRegistryFallback(data);
			}

			UpdateDynamic(data);
			return data;
		}

		public void UpdateDynamic(DHSysInfoCpuData data) {
			if (data == null || m_counterUsage == null) {
				return;
			}

			try {
				data.UsagePercent = Math.Max(0.0f, Math.Min(100.0f, m_counterUsage.NextValue()));
			}
			catch {
				data.UsagePercent = 0.0f;
			}
		}

		private int GetPhysicalCoreCount() {
			uint nlength = 0;
			Kernel32.GetLogicalProcessorInformationEx(DEF_RELATION_PROCESSOR_CORE, IntPtr.Zero, ref nlength);
			if (nlength == 0 || Marshal.GetLastWin32Error() != DEF_ERROR_INSUFFICIENT_BUFFER) {
				return 0;
			}

			IntPtr ptrbuffer = Marshal.AllocHGlobal((int)nlength);
			try {
				if (Kernel32.GetLogicalProcessorInformationEx(DEF_RELATION_PROCESSOR_CORE,
					ptrbuffer, ref nlength) == false) {
					return 0;
				}

				int ncount = 0;
				long noffset = 0;
				while (noffset + 8 <= nlength) {
					IntPtr ptrrecord = new IntPtr(ptrbuffer.ToInt64() + noffset);
					int nrelationship = Marshal.ReadInt32(ptrrecord, 0);
					int nsize = Marshal.ReadInt32(ptrrecord, 4);
					if (nsize < 8 || noffset + nsize > nlength) {
						break;
					}

					if (nrelationship == DEF_RELATION_PROCESSOR_CORE) {
						ncount++;
					}
					noffset += nsize;
				}

				return ncount;
			}
			catch {
				return 0;
			}
			finally {
				Marshal.FreeHGlobal(ptrbuffer);
			}
		}

		private void CollectRegistryFallback(DHSysInfoCpuData data) {
			try {
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(DEF_REGISTRY_CPU_PATH, false)) {
					if (key == null) {
						return;
					}

					data.Name = Convert.ToString(key.GetValue("ProcessorNameString", "")).Trim();
					data.Manufacturer = Convert.ToString(key.GetValue("VendorIdentifier", "")).Trim();
					data.MaxClockMHz = ToUInt32(key.GetValue("~MHz", 0));
				}
			}
			catch {
			}
		}

		private int ToInt32(object value) {
			if (value == null) {
				return 0;
			}

			return Convert.ToInt32(value);
		}

		private uint ToUInt32(object value) {
			if (value == null) {
				return 0;
			}

			return Convert.ToUInt32(value);
		}

		public void Dispose() {
			if (m_counterUsage != null) {
				m_counterUsage.Dispose();
				m_counterUsage = null;
			}
		}
	}
}
