//////////////////////////////////////////////////////////////////////////////////////////////////
//	Projects		: DHInfoDesk
//	Author			: CYBERKDH(cyberkdh@hotmail.com, cyberkdh@gmail.com), AI(Codex)
//	Module			: DHInfoDesk
//	History			:
//	Copyrights		: Copyright ⓒCYBERKDH. All Rights Reserved.
//////////////////////////////////////////////////////////////////////////////////////////////////

using DHInfoDesk.module.sysinfo;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace DHInfoDesk {
	internal sealed class DHInfoDeskAppContext : ApplicationContext {
		private const string DEF_REGISTRY_AUTORUN_PATH = @"Software\Microsoft\Windows\CurrentVersion\Run";
		private const string DEF_REGISTRY_AUTORUN_NAME = "DHInfoDesk";

		private readonly List<FrmInfoDesk> m_lstInfoDesk = new List<FrmInfoDesk>();
		private readonly List<FrmSettingsButton> m_lstSettingsButton = new List<FrmSettingsButton>();
		private readonly Timer m_tmBottomMost = new Timer();
		private readonly Timer m_tmSysInfo = new Timer();
		private readonly DHSysInfoManager m_sysInfoManager = new DHSysInfoManager();
		private readonly DHInfoDeskSettings m_settings;

		private bool m_bCollectingSysInfo = false;

		// Initializes the application context and starts periodic updates.
		// 애플리케이션 컨텍스트를 초기화하고 주기적 갱신을 시작한다.
		public DHInfoDeskAppContext() {
			m_settings = DHInfoDeskSettings.Load();
			SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

			m_sysInfoManager.CollectStatic();
			m_sysInfoManager.CollectDynamic();

			m_tmBottomMost.Interval = 1000;
			m_tmBottomMost.Tick += m_tmBottomMost_Tick;
			m_tmBottomMost.Start();

			DoCreateMonitorWindows();

			DoUpdateRefreshInterval();
			m_tmSysInfo.Tick += m_tmSysInfo_Tick;
			m_tmSysInfo.Start();
		}

		// Recreates monitor windows after the display configuration changes.
		// 디스플레이 구성이 변경되면 모니터 창을 다시 생성한다.
		private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e) {
			if (m_lstInfoDesk.Count > 0 && m_lstInfoDesk[0].InvokeRequired == true) {
				m_lstInfoDesk[0].BeginInvoke(new MethodInvoker(DoCreateMonitorWindows));
				return;
			}

			DoCreateMonitorWindows();
		}

		// Keeps all application windows at the bottom of the window order.
		// 모든 애플리케이션 창을 창 순서의 최하단에 유지한다.
		private void m_tmBottomMost_Tick(object sender, EventArgs e) {
			int i;

			for (i = 0; i < m_lstSettingsButton.Count; i++) {
				m_lstSettingsButton[i].DoKeepBottomMost();
			}

			for (i = 0; i < m_lstInfoDesk.Count; i++) {
				m_lstInfoDesk[i].DoKeepBottomMost();
			}
		}

		// Collects dynamic system information on each timer tick.
		// 타이머 주기마다 동적 시스템 정보를 수집한다.
		private void m_tmSysInfo_Tick(object sender, EventArgs e) {
			if (m_bCollectingSysInfo == true) {
				return;
			}

			m_bCollectingSysInfo = true;
			try {
				DHSysInfoSnapshot snapshot = m_sysInfoManager.CollectDynamic();
				DoUpdateSysInfo(snapshot);
			}
			finally {
				m_bCollectingSysInfo = false;
			}
		}

		// Creates information and settings windows for the target monitors.
		// 대상 모니터별 정보 창과 설정 창을 생성한다.
		private void DoCreateMonitorWindows() {
			DoCloseMonitorWindows();

			IList<Screen> screens = GetTargetScreens();

			int i;
			for (i = 0; i < screens.Count; i++) {
				FrmInfoDesk frmInfoDesk = new FrmInfoDesk(screens[i]);
				frmInfoDesk.SetSettings(m_settings);
				frmInfoDesk.SetSysInfo(m_sysInfoManager.Snapshot);
				FrmSettingsButton frmSettingsButton = new FrmSettingsButton(screens[i]);

				frmSettingsButton.OnAutoRunChanged += FrmSettingsButton_OnAutoRunChanged;
				frmSettingsButton.OnSettingsChanged += FrmSettingsButton_OnSettingsChanged;
				frmSettingsButton.OnExit += FrmSettingsButton_OnExit;
				frmSettingsButton.SetAutoRun(IsAutoRun());
				frmSettingsButton.SetSettings(m_settings);

				m_lstInfoDesk.Add(frmInfoDesk);
				m_lstSettingsButton.Add(frmSettingsButton);

				frmInfoDesk.Show();
				frmSettingsButton.Show();
			}
		}

		// Applies the latest system snapshot to all information windows.
		// 최신 시스템 스냅샷을 모든 정보 창에 적용한다.
		private void DoUpdateSysInfo(DHSysInfoSnapshot snapshot) {
			int i;
			for (i = 0; i < m_lstInfoDesk.Count; i++) {
				m_lstInfoDesk[i].SetSysInfo(snapshot);
			}
		}

		// Closes and disposes all monitor-specific windows.
		// 모든 모니터별 창을 닫고 해제한다.
		private void DoCloseMonitorWindows() {
			int i;

			for (i = 0; i < m_lstSettingsButton.Count; i++) {
				m_lstSettingsButton[i].Close();
				m_lstSettingsButton[i].Dispose();
			}
			m_lstSettingsButton.Clear();

			for (i = 0; i < m_lstInfoDesk.Count; i++) {
				m_lstInfoDesk[i].Close();
				m_lstInfoDesk[i].Dispose();
			}
			m_lstInfoDesk.Clear();
		}

		// Applies an auto-run setting change to the registry and all buttons.
		// 자동 실행 설정 변경을 레지스트리와 모든 버튼에 적용한다.
		private void FrmSettingsButton_OnAutoRunChanged(object sender, bool bautorun) {
			SetAutoRun(bautorun);

			int i;
			for (i = 0; i < m_lstSettingsButton.Count; i++) {
				m_lstSettingsButton[i].SetAutoRun(bautorun);
			}
		}

		// Saves and applies a display setting change.
		// 표시 설정 변경을 저장하고 적용한다.
		private void FrmSettingsButton_OnSettingsChanged(object sender, DHInfoDeskSettings settings) {
			bool brebuild = IsMonitorWindowRebuildRequired();
			m_settings.Save();
			DoUpdateRefreshInterval();

			if (brebuild == true) {
				Control control = sender as Control;
				if (control != null && control.IsHandleCreated == true) {
					control.BeginInvoke(new MethodInvoker(DoCreateMonitorWindows));
				}
				else {
					DoCreateMonitorWindows();
				}
				return;
			}

			int i;
			for (i = 0; i < m_lstInfoDesk.Count; i++) {
				m_lstInfoDesk[i].SetSettings(m_settings);
			}
			for (i = 0; i < m_lstSettingsButton.Count; i++) {
				m_lstSettingsButton[i].SetSettings(m_settings);
			}
		}

		// Applies the configured system information refresh interval.
		// 구성된 시스템 정보 새로 고침 간격을 적용한다.
		private void DoUpdateRefreshInterval() {
			m_tmSysInfo.Interval = m_settings.RefreshIntervalSeconds * 1000;
		}

		// Returns the monitors selected by the current monitor mode.
		// 현재 모니터 모드에서 선택된 모니터를 반환한다.
		private IList<Screen> GetTargetScreens() {
			List<Screen> result = new List<Screen>();
			Screen[] screens = Screen.AllScreens;

			if (screens == null || screens.Length == 0) {
				if (Screen.PrimaryScreen != null) {
					result.Add(Screen.PrimaryScreen);
				}
				return result;
			}

			if (m_settings.MonitorMode == E_InfoDeskMonitorMode.All) {
				result.AddRange(screens);
				return result;
			}

			if (m_settings.MonitorMode == E_InfoDeskMonitorMode.Selected) {
				int i;
				for (i = 0; i < screens.Length; i++) {
					if (string.Equals(screens[i].DeviceName, m_settings.SelectedMonitorDeviceName,
						StringComparison.OrdinalIgnoreCase) == true) {
						result.Add(screens[i]);
						return result;
					}
				}

				m_settings.MonitorMode = E_InfoDeskMonitorMode.Primary;
				m_settings.SelectedMonitorDeviceName = "";
				m_settings.Save();
			}

			Screen primary = Screen.PrimaryScreen;
			result.Add(primary == null ? screens[0] : primary);
			return result;
		}

		// Determines whether monitor-specific windows must be rebuilt.
		// 모니터별 창을 다시 생성해야 하는지 확인한다.
		private bool IsMonitorWindowRebuildRequired() {
			IList<Screen> target = GetTargetScreens();
			if (target.Count != m_lstInfoDesk.Count) {
				return true;
			}

			int i;
			for (i = 0; i < target.Count; i++) {
				bool bfound = false;
				int j;
				for (j = 0; j < m_lstInfoDesk.Count; j++) {
					if (string.Equals(target[i].DeviceName, m_lstInfoDesk[j].ScreenDeviceName,
						StringComparison.OrdinalIgnoreCase) == true) {
						bfound = true;
						break;
					}
				}

				if (bfound == false) {
					return true;
				}
			}

			return false;
		}

		// Exits the application when the settings menu requests it.
		// 설정 메뉴에서 종료를 요청하면 애플리케이션을 종료한다.
		private void FrmSettingsButton_OnExit(object sender, EventArgs e) {
			ExitThread();
		}

		// Checks whether the application is registered to run at logon.
		// 애플리케이션이 로그온 시 실행되도록 등록되었는지 확인한다.
		private bool IsAutoRun() {
			using (RegistryKey key = Registry.CurrentUser.OpenSubKey(DEF_REGISTRY_AUTORUN_PATH, false)) {
				string strvalue = key == null ? "" : key.GetValue(DEF_REGISTRY_AUTORUN_NAME, "") as string;
				return string.IsNullOrEmpty(strvalue) == false;
			}
		}

		// Adds or removes the current executable from the logon run registry.
		// 현재 실행 파일을 로그온 실행 레지스트리에 추가하거나 제거한다.
		private void SetAutoRun(bool bautorun) {
			using (RegistryKey key = Registry.CurrentUser.CreateSubKey(DEF_REGISTRY_AUTORUN_PATH)) {
				if (bautorun == true) {
					string strpath = Process.GetCurrentProcess().MainModule.FileName;
					key.SetValue(DEF_REGISTRY_AUTORUN_NAME, "\"" + Path.GetFullPath(strpath) + "\"");
				}
				else {
					key.DeleteValue(DEF_REGISTRY_AUTORUN_NAME, false);
				}
			}
		}

		// Releases application resources before the message loop exits.
		// 메시지 루프 종료 전에 애플리케이션 리소스를 해제한다.
		protected override void ExitThreadCore() {
			m_tmSysInfo.Stop();
			m_tmBottomMost.Stop();
			SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
			DoCloseMonitorWindows();
			m_sysInfoManager.Dispose();
			base.ExitThreadCore();
		}
	}
}
