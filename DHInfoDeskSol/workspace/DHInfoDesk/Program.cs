//////////////////////////////////////////////////////////////////////////////////////////////////
//	Projects		: DHTool
//	Author			: CYBERKDH(cyberkdh@hotmail.com, cyberkdh@gmail.com), AI(Codex)
//	Module			: DHInfoDesk
//	History			: 
//	Copyrights		: Copyright ⓒCYBERKDH. All Rights Reserved.
//////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Windows.Forms;

namespace DHInfoDesk {
	internal static class Program {
		private const string DEF_MUTEX_NAME = @"Local\DHInfoDesk";

		[STAThread]
		// Starts the application as a single instance.
		// 애플리케이션을 단일 인스턴스로 시작한다.
		static void Main() {
			bool bcreated;
			using (Mutex mutex = new Mutex(true, DEF_MUTEX_NAME, out bcreated)) {
				if (bcreated == false) {
					return;
				}

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new DHInfoDeskAppContext());
			}
		}
	}
}
