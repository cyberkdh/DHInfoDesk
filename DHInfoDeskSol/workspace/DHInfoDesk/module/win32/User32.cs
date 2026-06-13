//////////////////////////////////////////////////////////////////////////////////////////////////
//	Projects		: DHInfoDesk
//	Author			: CYBERKDH(cyberkdh@hotmail.com, cyberkdh@gmail.com), AI(Codex)
//	Module			: DHInfoDesk.module.win32
//	History			:
//	Copyrights		: Copyright ⓒCYBERKDH. All Rights Reserved.
//////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace DHInfoDesk.module.win32 {
	internal static class User32 {
		public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

		public const int WH_MOUSE_LL      = 14;
		public const int GWL_EXSTYLE      = -20;
		public const int WS_EX_NOACTIVATE = 0x08000000;
		public const int WS_EX_TOOLWINDOW = 0x00000080;
		public const int WS_EX_TRANSPARENT = 0x00000020;
		public const int VK_LBUTTON       = 0x01;
		public const int VK_RBUTTON       = 0x02;
		public const int VK_MBUTTON       = 0x04;

		public const uint SWP_NOSIZE      = 0x0001;
		public const uint SWP_NOMOVE      = 0x0002;
		public const uint SWP_NOACTIVATE  = 0x0010;

		public const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT {
			public int X;
			public int Y;
		}

		public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
			int x, int y, int cx, int cy, uint uFlags);

		[DllImport("user32.dll")]
		public static extern short GetAsyncKeyState(int vKey);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn,
			IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll")]
		public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
			IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);
	}
}
