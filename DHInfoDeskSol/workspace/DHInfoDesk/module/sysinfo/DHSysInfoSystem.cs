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

namespace DHInfoDesk.module.sysinfo {
	internal sealed class DHSysInfoSystem {
		private const string DEF_REGISTRY_WINDOWS_PATH = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

		public DHSysInfoSystemData CollectStatic() {
			DHSysInfoSystemData data = new DHSysInfoSystemData();

			data.ComputerName = Environment.MachineName;
			data.UserName = Environment.UserDomainName + "\\" + Environment.UserName;
			data.Architecture = Environment.Is64BitOperatingSystem == true ? "x64" : "x86";
			data.TimeZone = TimeZoneInfo.Local.DisplayName;

			try {
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(DEF_REGISTRY_WINDOWS_PATH, false)) {
					if (key != null) {
						data.OsName = Convert.ToString(key.GetValue("ProductName", ""));
						data.OsVersion = Convert.ToString(key.GetValue("DisplayVersion", ""));
						if (string.IsNullOrEmpty(data.OsVersion) == true) {
							data.OsVersion = Convert.ToString(key.GetValue("ReleaseId", ""));
						}
						data.OsBuild = Convert.ToString(key.GetValue("CurrentBuildNumber", ""));
						string strubr = Convert.ToString(key.GetValue("UBR", ""));
						if (string.IsNullOrEmpty(strubr) == false) {
							data.OsBuild += "." + strubr;
						}
					}
				}
			}
			catch {
				data.OsName = Environment.OSVersion.VersionString;
			}

			UpdateDynamic(data);
			return data;
		}

		public void UpdateDynamic(DHSysInfoSystemData data) {
			if (data == null) {
				return;
			}

			try {
				data.Uptime = TimeSpan.FromMilliseconds(Kernel32.GetTickCount64());
				data.LastBootTime = DateTime.Now.Subtract(data.Uptime);
			}
			catch {
				data.Uptime = TimeSpan.Zero;
				data.LastBootTime = DateTime.MinValue;
			}
		}
	}
}
