//////////////////////////////////////////////////////////////////////////////////////////////////
//	Projects		: DHInfoDesk
//	Author			: CYBERKDH(cyberkdh@hotmail.com, cyberkdh@gmail.com), AI(Codex)
//	Module			: DHInfoDesk.module.sysinfo
//	History			:
//	Copyrights		: Copyright ⓒCYBERKDH. All Rights Reserved.
//////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace DHInfoDesk.module.sysinfo {
	internal sealed class DHSysInfoStorage {
		// Collects capacity and usage information for all logical drives.
		// 모든 논리 드라이브의 용량과 사용률 정보를 수집한다.
		public IList<DHSysInfoStorageData> Collect() {
			List<DHSysInfoStorageData> result = new List<DHSysInfoStorageData>();

			DriveInfo[] drives;
			try {
				drives = DriveInfo.GetDrives();
			}
			catch {
				return result;
			}

			int i;
			for (i = 0; i < drives.Length; i++) {
				DriveInfo drive = drives[i];
				DHSysInfoStorageData data = new DHSysInfoStorageData();
				data.Name = drive.Name;
				data.DriveType = drive.DriveType.ToString();

				try {
					if (drive.IsReady == true) {
						data.VolumeLabel = drive.VolumeLabel;
						data.FileSystem = drive.DriveFormat;
						data.TotalBytes = drive.TotalSize;
						data.AvailableBytes = drive.AvailableFreeSpace;
						data.UsedBytes = data.TotalBytes - data.AvailableBytes;
						if (data.TotalBytes > 0) {
							data.UsagePercent = (double)data.UsedBytes * 100.0 / data.TotalBytes;
						}
					}
				}
				catch {
				}

				result.Add(data);
			}

			return result;
		}
	}
}
