//////////////////////////////////////////////////////////////////////////////////////////////////
//	Projects		: DHInfoDesk
//	Author			: CYBERKDH(cyberkdh@hotmail.com, cyberkdh@gmail.com), AI(Codex)
//	Module			: DHInfoDesk.module.sysinfo
//	History			:
//	Copyrights		: Copyright ⓒCYBERKDH. All Rights Reserved.
//////////////////////////////////////////////////////////////////////////////////////////////////

using DHInfoDesk.module.win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DHInfoDesk.module.sysinfo {
	internal sealed class DHSysInfoDisplay {

		public IList<DHSysInfoDisplayData> Collect() {
			List<DHSysInfoDisplayData> result = new List<DHSysInfoDisplayData>();
			Screen[] screens = Screen.AllScreens;

			int i;
			for (i = 0; i < screens.Length; i++) {
				Screen screen = screens[i];
				DHSysInfoDisplayData data = new DHSysInfoDisplayData();

				data.DeviceName = screen.DeviceName;
				data.Left = screen.Bounds.Left;
				data.Top = screen.Bounds.Top;
				data.Width = screen.Bounds.Width;
				data.Height = screen.Bounds.Height;
				data.WorkingWidth = screen.WorkingArea.Width;
				data.WorkingHeight = screen.WorkingArea.Height;
				data.IsPrimary = screen.Primary;

				GetMonitorDpi(screen, out float dpiX, out float dpiY);
				data.DpiX = dpiX;
				data.DpiY = dpiY;

				result.Add(data);
			}

			return result;
		}

		private void GetMonitorDpi(Screen screen, out float dpiX, out float dpiY) {
			dpiX = 96.0f;
			dpiY = 96.0f;

			try {
				User32.POINT point = new User32.POINT {
					X = screen.Bounds.Left + screen.Bounds.Width / 2,
					Y = screen.Bounds.Top + screen.Bounds.Height / 2
				};
				System.IntPtr hmonitor = User32.MonitorFromPoint(point, User32.MONITOR_DEFAULTTONEAREST);
				uint x;
				uint y;
				if (hmonitor != System.IntPtr.Zero &&
					Shcore.GetDpiForMonitor(hmonitor, Shcore.MonitorDpiType.Effective, out x, out y) == 0) {
					dpiX = x;
					dpiY = y;
					return;
				}
			}
			catch {
			}

			try {
				using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero)) {
					dpiX = graphics.DpiX;
					dpiY = graphics.DpiY;
				}
			}
			catch {
				dpiX = 96.0f;
				dpiY = 96.0f;
			}
		}
	}
}
