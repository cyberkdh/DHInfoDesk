//////////////////////////////////////////////////////////////////////////////////////////////////
//	Projects		: DHInfoDesk
//	Author			: CYBERKDH(cyberkdh@hotmail.com, cyberkdh@gmail.com), AI(Codex)
//	Module			: DHInfoDesk.module.win32
//	History			:
//	Copyrights		: Copyright ⓒCYBERKDH. All Rights Reserved.
//////////////////////////////////////////////////////////////////////////////////////////////////

using System.Runtime.InteropServices;

namespace DHInfoDesk.module.win32 {
	internal static class Kernel32 {
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public sealed class MEMORYSTATUSEX {
			public uint  dwLength;
			public uint  dwMemoryLoad;
			public ulong ullTotalPhys;
			public ulong ullAvailPhys;
			public ulong ullTotalPageFile;
			public ulong ullAvailPageFile;
			public ulong ullTotalVirtual;
			public ulong ullAvailVirtual;
			public ulong ullAvailExtendedVirtual;

			// Initializes the native structure size required by Windows.
			// Windows에서 요구하는 네이티브 구조체 크기를 초기화한다.
			public MEMORYSTATUSEX() {
				dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
			}
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

		[DllImport("kernel32.dll")]
		public static extern ulong GetTickCount64();

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetLogicalProcessorInformationEx(int relationship,
			System.IntPtr buffer, ref uint returnedLength);
	}
}
