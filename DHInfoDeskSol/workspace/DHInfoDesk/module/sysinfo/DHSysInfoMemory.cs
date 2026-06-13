//////////////////////////////////////////////////////////////////////////////////////////////////
//	Projects		: DHInfoDesk
//	Author			: CYBERKDH(cyberkdh@hotmail.com, cyberkdh@gmail.com), AI(Codex)
//	Module			: DHInfoDesk.module.sysinfo
//	History			:
//	Copyrights		: Copyright ⓒCYBERKDH. All Rights Reserved.
//////////////////////////////////////////////////////////////////////////////////////////////////

using DHInfoDesk.module.win32;

namespace DHInfoDesk.module.sysinfo {
	internal sealed class DHSysInfoMemory {

		// Collects current physical memory capacity and usage.
		// 현재 물리 메모리 용량과 사용률을 수집한다.
		public DHSysInfoMemoryData Collect() {
			DHSysInfoMemoryData data = new DHSysInfoMemoryData();

			try {
				Kernel32.MEMORYSTATUSEX status = new Kernel32.MEMORYSTATUSEX();
				if (Kernel32.GlobalMemoryStatusEx(status) == true) {
					data.TotalBytes = status.ullTotalPhys;
					data.AvailableBytes = status.ullAvailPhys;
					data.UsedBytes = data.TotalBytes - data.AvailableBytes;
					data.UsagePercent = status.dwMemoryLoad;
				}
			}
			catch {
				data = new DHSysInfoMemoryData();
			}

			return data;
		}
	}
}
