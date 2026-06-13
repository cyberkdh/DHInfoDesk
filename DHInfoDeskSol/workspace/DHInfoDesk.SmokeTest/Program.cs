//////////////////////////////////////////////////////////////////////////////////////////////////
//	Projects		: DHInfoDesk
//	Author			: CYBERKDH(cyberkdh@hotmail.com, cyberkdh@gmail.com), AI(Codex)
//	Module			: DHInfoDesk.SmokeTest
//	History			:
//	Copyrights		: Copyright ⓒCYBERKDH. All Rights Reserved.
//////////////////////////////////////////////////////////////////////////////////////////////////

using DHInfoDesk.module.sysinfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace DHInfoDesk.SmokeTest {
	internal static class Program {
		private const int DEF_DYNAMIC_SAMPLE_DELAY_MS = 1200;

		[STAThread]
		private static int Main(string[] args) {
			StringBuilder result = new StringBuilder();
			int nfailed = 0;
			int nwarnings = 0;

			WriteLine(result, "DHInfoDesk System Information Smoke Test");
			WriteLine(result, "Started: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			WriteLine(result, "");

			try {
				using (DHSysInfoManager manager = new DHSysInfoManager()) {
					DHSysInfoSnapshot snapshot = manager.CollectStatic();
					Thread.Sleep(DEF_DYNAMIC_SAMPLE_DELAY_MS);
					snapshot = manager.CollectDynamic();

					WriteStatus(result, "SYSTEM", snapshot.Status.System, ref nfailed, ref nwarnings);
					WriteStatus(result, "CPU", snapshot.Status.Cpu, ref nfailed, ref nwarnings);
					WriteStatus(result, "MEMORY", snapshot.Status.Memory, ref nfailed, ref nwarnings);
					WriteStatus(result, "STORAGE", snapshot.Status.Storage, ref nfailed, ref nwarnings);
					WriteStatus(result, "NETWORK", snapshot.Status.Network, ref nfailed, ref nwarnings);
					WriteStatus(result, "DISPLAY", snapshot.Status.Display, ref nfailed, ref nwarnings);

					WriteLine(result, "");
					WriteSummary(result, snapshot);
				}
			}
			catch (Exception ex) {
				nfailed++;
				WriteLine(result, "FATAL: " + ex.GetType().Name + ": " + ex.Message);
			}

			WriteLine(result, "");
			string strresult = nfailed > 0 ? "FAIL" : (nwarnings > 0 ? "PASS_WITH_WARNINGS" : "PASS");
			WriteLine(result, "Result: " + strresult);
			WriteLine(result, "Completed: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

			string strlogpath = GetLogPath(args);
			try {
				Directory.CreateDirectory(Path.GetDirectoryName(strlogpath));
				File.WriteAllText(strlogpath, result.ToString(), new UTF8Encoding(false));
				Console.WriteLine();
				Console.WriteLine("Log: " + strlogpath);
			}
			catch (Exception ex) {
				Console.Error.WriteLine("Log write failed: " + ex.Message);
				nfailed++;
			}

			return nfailed == 0 ? 0 : 1;
		}

		private static void WriteStatus(StringBuilder result, string strname,
			DHSysInfoCollectionStatus status, ref int nfailed, ref int nwarnings) {
			string strstate;
			if (status.IsSuccess == true) {
				strstate = "SUCCESS";
			}
			else if (status.IsPartial == true) {
				strstate = "PARTIAL";
			}
			else {
				strstate = "FAILURE";
			}

			string strline = string.Format("{0,-8}: {1}", strname, strstate);
			if (string.IsNullOrEmpty(status.ErrorMessage) == false) {
				strline += " - " + status.ErrorMessage;
			}
			WriteLine(result, strline);

			if (status.IsPartial == true) {
				nwarnings++;
			}
			else if (status.IsSuccess == false) {
				nfailed++;
			}
		}

		private static void WriteSummary(StringBuilder result, DHSysInfoSnapshot snapshot) {
			WriteLine(result, "OS       : " + snapshot.System.OsName + " " +
				snapshot.System.OsVersion + " (Build " + snapshot.System.OsBuild + ")");
			WriteLine(result, "CPU      : " + snapshot.Cpu.Name);
			WriteLine(result, "CPU Core : " + snapshot.Cpu.PhysicalCoreCount + " physical / " +
				snapshot.Cpu.LogicalProcessorCount + " logical");
			WriteLine(result, "CPU Usage: " + snapshot.Cpu.UsagePercent.ToString("F1") + "%");
			WriteLine(result, "Memory   : " + FormatBytes(snapshot.Memory.UsedBytes) + " / " +
				FormatBytes(snapshot.Memory.TotalBytes) + " (" + snapshot.Memory.UsagePercent + "%)");
			WriteLine(result, "Storage  : " + snapshot.Storage.Count + " drive(s)");
			WriteLine(result, "Network  : " + snapshot.Network.Count + " adapter(s)");
			WriteLine(result, "Display  : " + snapshot.Display.Count + " monitor(s)");

			long nreceive = 0;
			long nsend = 0;
			int i;
			for (i = 0; i < snapshot.Network.Count; i++) {
				nreceive += snapshot.Network[i].ReceiveBytesPerSecond;
				nsend += snapshot.Network[i].SendBytesPerSecond;
			}
			WriteLine(result, "Net Rate : RX " + FormatBytes((ulong)Math.Max(0, nreceive)) +
				"/s, TX " + FormatBytes((ulong)Math.Max(0, nsend)) + "/s");
		}

		private static string FormatBytes(ulong nbytes) {
			string[] units = new string[] { "B", "KB", "MB", "GB", "TB" };
			double value = nbytes;
			int nunit = 0;

			while (value >= 1024.0 && nunit < units.Length - 1) {
				value /= 1024.0;
				nunit++;
			}

			return value.ToString("F2") + " " + units[nunit];
		}

		private static string GetLogPath(string[] args) {
			if (args != null && args.Length > 0 && string.IsNullOrWhiteSpace(args[0]) == false) {
				return Path.GetFullPath(args[0]);
			}

			string strbase = AppDomain.CurrentDomain.BaseDirectory;
			return Path.GetFullPath(Path.Combine(strbase, @"..\..\..\test-results\sysinfo-smoke-test.txt"));
		}

		private static void WriteLine(StringBuilder result, string strline) {
			Console.WriteLine(strline);
			result.AppendLine(strline);
		}
	}
}
