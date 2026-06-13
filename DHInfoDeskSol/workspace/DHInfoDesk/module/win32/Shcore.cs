//////////////////////////////////////////////////////////////////////////////////////////////////
//	Projects		: DHInfoDesk
//	Author			: CYBERKDH(cyberkdh@hotmail.com, cyberkdh@gmail.com), AI(Codex)
//	Module			: DHInfoDesk.module.win32
//	History			:
//	Copyrights		: Copyright ⓒCYBERKDH. All Rights Reserved.
//////////////////////////////////////////////////////////////////////////////////////////////////

using System.Runtime.InteropServices;

namespace DHInfoDesk.module.win32 {
	internal static class Shcore {
		public enum MonitorDpiType {
			Effective = 0
		}

		[DllImport("shcore.dll")]
		public static extern int GetDpiForMonitor(System.IntPtr hmonitor, MonitorDpiType dpiType,
			out uint dpiX, out uint dpiY);
	}
}
