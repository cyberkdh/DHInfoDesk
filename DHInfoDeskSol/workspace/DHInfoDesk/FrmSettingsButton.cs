//////////////////////////////////////////////////////////////////////////////////////////////////
//	Projects		: DHInfoDesk
//	Author			: CYBERKDH(cyberkdh@hotmail.com, cyberkdh@gmail.com), AI(Codex)
//	Module			: DHInfoDesk
//	History			:
//	Copyrights		: Copyright ⓒCYBERKDH. All Rights Reserved.
//////////////////////////////////////////////////////////////////////////////////////////////////

using DHInfoDesk.module.win32;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DHInfoDesk {
	internal sealed class FrmSettingsButton : Form, IMessageFilter {
		private const int WM_LBUTTONDOWN = 0x0201;
		private const int WM_RBUTTONDOWN = 0x0204;
		private const int WM_MBUTTONDOWN = 0x0207;
		private const int WM_NCLBUTTONDOWN = 0x00A1;
		private const int WM_NCRBUTTONDOWN = 0x00A4;
		private const int WM_NCMBUTTONDOWN = 0x00A7;

		public event EventHandler<bool> OnAutoRunChanged;
		public event EventHandler<DHInfoDeskSettings> OnSettingsChanged;
		public event EventHandler OnExit;

		private readonly Button m_btnSettings = new Button();
		private readonly ContextMenuStrip m_menuSettings = new ContextMenuStrip();
		private readonly Timer m_tmPopupClose = new Timer();
		private readonly ToolStripMenuItem m_menuDisplayItems = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuShowSystem = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuShowCpu = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuShowMemory = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuShowStorage = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuShowNetwork = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuPosition = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuTopLeft = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuTopRight = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuBottomLeft = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuBottomRight = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuMonitor = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuMonitorName = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuAllMonitors = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuPrimaryMonitor = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuThisMonitor = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuPrivacy = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuPrivacyMode = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuComputerName = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuUserName = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuIpAddress = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuMacAddress = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuAppearance = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuUiSize = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuSizeCompact = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuSizeNormal = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuSizeLarge = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuAccentColor = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuAccentBlue = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuAccentGreen = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuAccentOrange = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuAccentGray = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuTextColor = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuTextWhite = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuTextLightGray = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuTextBlack = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuTextAccent = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuRefreshInterval = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuRefresh1Second = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuRefresh3Seconds = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuRefresh5Seconds = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuRefresh10Seconds = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuResetSettings = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuAutoRun = new ToolStripMenuItem();
		private readonly ToolStripMenuItem m_menuExit = new ToolStripMenuItem();
		private readonly Screen m_screen;

		private DHInfoDeskSettings m_settings = new DHInfoDeskSettings();
		private bool m_bUpdatingMenu = false;
		private bool m_bMessageFilterRegistered = false;
		private bool m_bMouseButtonDown = false;
		private bool m_bPopupClosePending = false;
		private IntPtr m_hMouseHook = IntPtr.Zero;
		private User32.LowLevelMouseProc m_mouseHookProc = null;

		// Initializes the monitor-specific settings button and popup menu.
		// 모니터별 설정 버튼과 팝업 메뉴를 초기화한다.
		public FrmSettingsButton(Screen screen) {
			m_screen = screen;
			AutoScaleMode = AutoScaleMode.Dpi;
			BackColor = Color.FromArgb(28, 31, 38);
			ClientSize = new Size(36, 36);
			FormBorderStyle = FormBorderStyle.None;
			Location = new Point(screen.WorkingArea.Right - Width - 54, screen.WorkingArea.Top + 184);
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.Manual;
			TopMost = false;

			m_btnSettings.BackColor = Color.FromArgb(48, 52, 62);
			m_btnSettings.Dock = DockStyle.Fill;
			m_btnSettings.FlatAppearance.BorderColor = Color.FromArgb(90, 255, 255, 255);
			m_btnSettings.FlatStyle = FlatStyle.Flat;
			m_btnSettings.Font = new Font("Segoe UI Symbol", 12.0f, FontStyle.Regular);
			m_btnSettings.ForeColor = Color.White;
			m_btnSettings.Text = "⚙";
			m_btnSettings.UseVisualStyleBackColor = false;
			m_btnSettings.Click += m_btnSettings_Click;
			Controls.Add(m_btnSettings);

			m_tmPopupClose.Interval = 50;
			m_tmPopupClose.Tick += m_tmPopupClose_Tick;

			m_menuDisplayItems.Text = "Display Items";
			InitCheckMenu(m_menuShowSystem, "System", m_menuShowItem_CheckedChanged);
			InitCheckMenu(m_menuShowCpu, "CPU", m_menuShowItem_CheckedChanged);
			InitCheckMenu(m_menuShowMemory, "Memory", m_menuShowItem_CheckedChanged);
			InitCheckMenu(m_menuShowStorage, "Storage", m_menuShowItem_CheckedChanged);
			InitCheckMenu(m_menuShowNetwork, "Network", m_menuShowItem_CheckedChanged);
			m_menuDisplayItems.DropDownItems.Add(m_menuShowSystem);
			m_menuDisplayItems.DropDownItems.Add(m_menuShowCpu);
			m_menuDisplayItems.DropDownItems.Add(m_menuShowMemory);
			m_menuDisplayItems.DropDownItems.Add(m_menuShowStorage);
			m_menuDisplayItems.DropDownItems.Add(m_menuShowNetwork);

			m_menuPosition.Text = "Position";
			InitCheckMenu(m_menuTopLeft, "Top Left", m_menuPosition_Click);
			InitCheckMenu(m_menuTopRight, "Top Right", m_menuPosition_Click);
			InitCheckMenu(m_menuBottomLeft, "Bottom Left", m_menuPosition_Click);
			InitCheckMenu(m_menuBottomRight, "Bottom Right", m_menuPosition_Click);
			m_menuPosition.DropDownItems.Add(m_menuTopLeft);
			m_menuPosition.DropDownItems.Add(m_menuTopRight);
			m_menuPosition.DropDownItems.Add(m_menuBottomLeft);
			m_menuPosition.DropDownItems.Add(m_menuBottomRight);

			m_menuMonitor.Text = "Monitor";
			m_menuMonitorName.Text = m_screen.DeviceName;
			m_menuMonitorName.Enabled = false;
			InitCheckMenu(m_menuAllMonitors, "All Monitors", m_menuMonitorMode_Click);
			InitCheckMenu(m_menuPrimaryMonitor, "Primary Monitor Only", m_menuMonitorMode_Click);
			InitCheckMenu(m_menuThisMonitor, "This Monitor Only", m_menuMonitorMode_Click);
			m_menuMonitor.DropDownItems.Add(m_menuMonitorName);
			m_menuMonitor.DropDownItems.Add(new ToolStripSeparator());
			m_menuMonitor.DropDownItems.Add(m_menuAllMonitors);
			m_menuMonitor.DropDownItems.Add(m_menuPrimaryMonitor);
			m_menuMonitor.DropDownItems.Add(m_menuThisMonitor);

			m_menuPrivacy.Text = "Privacy";
			InitCheckMenu(m_menuPrivacyMode, "Privacy Mode", m_menuPrivacyMode_CheckedChanged);
			InitCheckMenu(m_menuComputerName, "Computer Name", m_menuPrivacyItem_CheckedChanged);
			InitCheckMenu(m_menuUserName, "User Name", m_menuPrivacyItem_CheckedChanged);
			InitCheckMenu(m_menuIpAddress, "IP Address", m_menuPrivacyItem_CheckedChanged);
			InitCheckMenu(m_menuMacAddress, "MAC Address", m_menuPrivacyItem_CheckedChanged);
			m_menuPrivacy.DropDownItems.Add(m_menuPrivacyMode);
			m_menuPrivacy.DropDownItems.Add(new ToolStripSeparator());
			m_menuPrivacy.DropDownItems.Add(m_menuComputerName);
			m_menuPrivacy.DropDownItems.Add(m_menuUserName);
			m_menuPrivacy.DropDownItems.Add(m_menuIpAddress);
			m_menuPrivacy.DropDownItems.Add(m_menuMacAddress);

			m_menuAppearance.Text = "Appearance";

			m_menuUiSize.Text = "UI Size";
			InitCheckMenu(m_menuSizeCompact, "Compact", m_menuUiSize_Click);
			InitCheckMenu(m_menuSizeNormal, "Normal", m_menuUiSize_Click);
			InitCheckMenu(m_menuSizeLarge, "Large", m_menuUiSize_Click);
			m_menuUiSize.DropDownItems.Add(m_menuSizeCompact);
			m_menuUiSize.DropDownItems.Add(m_menuSizeNormal);
			m_menuUiSize.DropDownItems.Add(m_menuSizeLarge);
			m_menuAppearance.DropDownItems.Add(m_menuUiSize);

			m_menuAccentColor.Text = "Accent Color";
			InitCheckMenu(m_menuAccentBlue, "Blue", m_menuAccent_Click);
			InitCheckMenu(m_menuAccentGreen, "Green", m_menuAccent_Click);
			InitCheckMenu(m_menuAccentOrange, "Orange", m_menuAccent_Click);
			InitCheckMenu(m_menuAccentGray, "Gray", m_menuAccent_Click);
			m_menuAccentColor.DropDownItems.Add(m_menuAccentBlue);
			m_menuAccentColor.DropDownItems.Add(m_menuAccentGreen);
			m_menuAccentColor.DropDownItems.Add(m_menuAccentOrange);
			m_menuAccentColor.DropDownItems.Add(m_menuAccentGray);
			m_menuAppearance.DropDownItems.Add(m_menuAccentColor);

			m_menuTextColor.Text = "Text Color";
			InitCheckMenu(m_menuTextWhite, "White", m_menuTextColor_Click);
			InitCheckMenu(m_menuTextLightGray, "Light Gray", m_menuTextColor_Click);
			InitCheckMenu(m_menuTextBlack, "Black", m_menuTextColor_Click);
			InitCheckMenu(m_menuTextAccent, "Accent", m_menuTextColor_Click);
			m_menuTextColor.DropDownItems.Add(m_menuTextWhite);
			m_menuTextColor.DropDownItems.Add(m_menuTextLightGray);
			m_menuTextColor.DropDownItems.Add(m_menuTextBlack);
			m_menuTextColor.DropDownItems.Add(m_menuTextAccent);
			m_menuAppearance.DropDownItems.Add(m_menuTextColor);

			m_menuRefreshInterval.Text = "Refresh Interval";
			InitCheckMenu(m_menuRefresh1Second, "1 second", m_menuRefreshInterval_Click);
			InitCheckMenu(m_menuRefresh3Seconds, "3 seconds", m_menuRefreshInterval_Click);
			InitCheckMenu(m_menuRefresh5Seconds, "5 seconds", m_menuRefreshInterval_Click);
			InitCheckMenu(m_menuRefresh10Seconds, "10 seconds", m_menuRefreshInterval_Click);
			m_menuRefreshInterval.DropDownItems.Add(m_menuRefresh1Second);
			m_menuRefreshInterval.DropDownItems.Add(m_menuRefresh3Seconds);
			m_menuRefreshInterval.DropDownItems.Add(m_menuRefresh5Seconds);
			m_menuRefreshInterval.DropDownItems.Add(m_menuRefresh10Seconds);

			m_menuResetSettings.Text = "Reset Settings";
			m_menuResetSettings.Click += m_menuResetSettings_Click;

			m_menuAutoRun.Text = "Run at logon";
			m_menuAutoRun.CheckOnClick = true;
			m_menuAutoRun.CheckedChanged += m_menuAutoRun_CheckedChanged;

			m_menuExit.Text = "Exit";
			m_menuExit.Click += m_menuExit_Click;

			m_menuSettings.Opened += m_menuSettings_Opened;
			m_menuSettings.Closed += m_menuSettings_Closed;
			m_menuSettings.Items.Add(m_menuDisplayItems);
			m_menuSettings.Items.Add(m_menuPosition);
			m_menuSettings.Items.Add(m_menuMonitor);
			m_menuSettings.Items.Add(m_menuPrivacy);
			m_menuSettings.Items.Add(m_menuAppearance);
			m_menuSettings.Items.Add(m_menuRefreshInterval);
			m_menuSettings.Items.Add(new ToolStripSeparator());
			m_menuSettings.Items.Add(m_menuResetSettings);
			m_menuSettings.Items.Add(m_menuAutoRun);
			m_menuSettings.Items.Add(new ToolStripSeparator());
			m_menuSettings.Items.Add(m_menuExit);
		}

		protected override bool ShowWithoutActivation {
			get { return true; }
		}

		protected override CreateParams CreateParams {
			get {
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= User32.WS_EX_NOACTIVATE;
				cp.ExStyle |= User32.WS_EX_TOOLWINDOW;
				return cp;
			}
		}

		// Opens or closes the settings popup menu.
		// 설정 팝업 메뉴를 열거나 닫는다.
		private void m_btnSettings_Click(object sender, EventArgs e) {
			if (m_menuSettings.Visible == true) {
				m_menuSettings.Close(ToolStripDropDownCloseReason.CloseCalled);
				return;
			}

			m_menuSettings.Show(this, new Point(Width, 0));
		}

		// Starts outside-click detection when the settings popup opens.
		// 설정 팝업이 열리면 외부 클릭 감지를 시작한다.
		private void m_menuSettings_Opened(object sender, EventArgs e) {
			if (m_bMessageFilterRegistered == false) {
				Application.AddMessageFilter(this);
				m_bMessageFilterRegistered = true;
			}

			m_bMouseButtonDown = IsMouseButtonDown();
			m_tmPopupClose.Start();
			DoInstallMouseHook();
		}

		// Stops outside-click detection when the settings popup closes.
		// 설정 팝업이 닫히면 외부 클릭 감지를 중지한다.
		private void m_menuSettings_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
			m_tmPopupClose.Stop();
			m_bMouseButtonDown = false;
			m_bPopupClosePending = false;
			DoRemoveMouseHook();
			DoRemoveMessageFilter();
		}

		// Installs the low-level mouse hook used for outside-click detection.
		// 외부 클릭 감지에 사용하는 저수준 마우스 훅을 설치한다.
		private void DoInstallMouseHook() {
			if (m_hMouseHook != IntPtr.Zero) {
				return;
			}

			m_mouseHookProc = MouseHookCallback;
			m_hMouseHook = User32.SetWindowsHookEx(User32.WH_MOUSE_LL,
				m_mouseHookProc, IntPtr.Zero, 0);
		}

		// Removes the low-level mouse hook.
		// 저수준 마우스 훅을 제거한다.
		private void DoRemoveMouseHook() {
			if (m_hMouseHook != IntPtr.Zero) {
				User32.UnhookWindowsHookEx(m_hMouseHook);
				m_hMouseHook = IntPtr.Zero;
			}

			m_mouseHookProc = null;
		}

		// Handles global mouse events and schedules popup closing.
		// 전역 마우스 이벤트를 처리하고 팝업 닫기를 예약한다.
		private IntPtr MouseHookCallback(int ncode, IntPtr wparam, IntPtr lparam) {
			if (ncode >= 0 && m_menuSettings.Visible == true &&
				IsMouseDownMessage(wparam.ToInt32()) == true) {
				Point point = Control.MousePosition;
				if (RectangleToScreen(ClientRectangle).Contains(point) == false &&
					IsPointInDropDown(m_menuSettings, point) == false &&
					m_bPopupClosePending == false) {
					m_bPopupClosePending = true;
					BeginInvoke(new MethodInvoker(DoClosePopup));
				}
			}

			return User32.CallNextHookEx(m_hMouseHook, ncode, wparam, lparam);
		}

		// Closes the settings popup after an outside click.
		// 외부 클릭 후 설정 팝업을 닫는다.
		private void DoClosePopup() {
			if (m_menuSettings.Visible == true) {
				m_menuSettings.Close(ToolStripDropDownCloseReason.AppClicked);
			}

			m_bPopupClosePending = false;
		}

		// Polls mouse button transitions as a popup-closing fallback.
		// 팝업 닫기 보조 수단으로 마우스 버튼 전환을 확인한다.
		private void m_tmPopupClose_Tick(object sender, EventArgs e) {
			bool bdown = IsMouseButtonDown();
			if (bdown == false) {
				m_bMouseButtonDown = false;
				return;
			}
			if (m_bMouseButtonDown == true) {
				return;
			}

			m_bMouseButtonDown = true;
			Point point = Control.MousePosition;
			if (RectangleToScreen(ClientRectangle).Contains(point) == false &&
				IsPointInDropDown(m_menuSettings, point) == false) {
				m_menuSettings.Close(ToolStripDropDownCloseReason.AppClicked);
			}
		}

		// Checks whether any primary mouse button is currently pressed.
		// 주요 마우스 버튼 중 하나가 현재 눌렸는지 확인한다.
		private bool IsMouseButtonDown() {
			return (User32.GetAsyncKeyState(User32.VK_LBUTTON) & 0x8000) != 0 ||
				(User32.GetAsyncKeyState(User32.VK_RBUTTON) & 0x8000) != 0 ||
				(User32.GetAsyncKeyState(User32.VK_MBUTTON) & 0x8000) != 0;
		}

		// Filters application messages to close the popup on outside clicks.
		// 외부 클릭 시 팝업을 닫도록 애플리케이션 메시지를 필터링한다.
		public bool PreFilterMessage(ref Message m) {
			if (m_menuSettings.Visible == false || IsMouseDownMessage(m.Msg) == false) {
				return false;
			}

			Point point = Control.MousePosition;
			if (RectangleToScreen(ClientRectangle).Contains(point) == true ||
				IsPointInDropDown(m_menuSettings, point) == true) {
				return false;
			}

			m_menuSettings.Close(ToolStripDropDownCloseReason.AppClicked);
			return false;
		}

		// Determines whether a window message represents a mouse button press.
		// 윈도우 메시지가 마우스 버튼 누름인지 확인한다.
		private bool IsMouseDownMessage(int nmessage) {
			return nmessage == WM_LBUTTONDOWN || nmessage == WM_RBUTTONDOWN ||
				nmessage == WM_MBUTTONDOWN || nmessage == WM_NCLBUTTONDOWN ||
				nmessage == WM_NCRBUTTONDOWN || nmessage == WM_NCMBUTTONDOWN;
		}

		// Checks whether a screen point is inside a popup or nested submenu.
		// 화면 좌표가 팝업 또는 하위 메뉴 내부인지 확인한다.
		private bool IsPointInDropDown(ToolStripDropDown dropdown, Point point) {
			if (dropdown.Visible == true && dropdown.Bounds.Contains(point) == true) {
				return true;
			}

			int i;
			for (i = 0; i < dropdown.Items.Count; i++) {
				ToolStripDropDownItem item = dropdown.Items[i] as ToolStripDropDownItem;
				if (item != null && item.HasDropDownItems == true &&
					IsPointInDropDown(item.DropDown, point) == true) {
					return true;
				}
			}

			return false;
		}

		// Removes the application message filter when registered.
		// 등록된 애플리케이션 메시지 필터를 제거한다.
		private void DoRemoveMessageFilter() {
			if (m_bMessageFilterRegistered == true) {
				Application.RemoveMessageFilter(this);
				m_bMessageFilterRegistered = false;
			}
		}

		// Applies changes to the enabled information sections.
		// 활성화된 정보 항목 변경을 적용한다.
		private void m_menuShowItem_CheckedChanged(object sender, EventArgs e) {
			if (m_bUpdatingMenu == true) {
				return;
			}

			m_settings.ShowSystem = m_menuShowSystem.Checked;
			m_settings.ShowCpu = m_menuShowCpu.Checked;
			m_settings.ShowMemory = m_menuShowMemory.Checked;
			m_settings.ShowStorage = m_menuShowStorage.Checked;
			m_settings.ShowNetwork = m_menuShowNetwork.Checked;
			DoRaiseSettingsChanged();
		}

		// Applies the selected panel alignment.
		// 선택한 패널 정렬 방식을 적용한다.
		private void m_menuPosition_Click(object sender, EventArgs e) {
			if (sender == m_menuTopLeft) {
				m_settings.Alignment = E_InfoDeskAlignment.TopLeft;
			}
			else if (sender == m_menuTopRight) {
				m_settings.Alignment = E_InfoDeskAlignment.TopRight;
			}
			else if (sender == m_menuBottomLeft) {
				m_settings.Alignment = E_InfoDeskAlignment.BottomLeft;
			}
			else if (sender == m_menuBottomRight) {
				m_settings.Alignment = E_InfoDeskAlignment.BottomRight;
			}

			DoUpdateSettingsMenu();
			DoRaiseSettingsChanged();
		}

		// Applies the selected monitor display mode.
		// 선택한 모니터 표시 모드를 적용한다.
		private void m_menuMonitorMode_Click(object sender, EventArgs e) {
			if (sender == m_menuAllMonitors) {
				m_settings.MonitorMode = E_InfoDeskMonitorMode.All;
				m_settings.SelectedMonitorDeviceName = "";
			}
			else if (sender == m_menuPrimaryMonitor) {
				m_settings.MonitorMode = E_InfoDeskMonitorMode.Primary;
				m_settings.SelectedMonitorDeviceName = "";
			}
			else if (sender == m_menuThisMonitor) {
				m_settings.MonitorMode = E_InfoDeskMonitorMode.Selected;
				m_settings.SelectedMonitorDeviceName = m_screen.DeviceName;
			}

			DoUpdateSettingsMenu();
			DoRaiseSettingsChanged();
		}

		// Enables or disables privacy mode.
		// 개인정보 보호 모드를 활성화하거나 비활성화한다.
		private void m_menuPrivacyMode_CheckedChanged(object sender, EventArgs e) {
			if (m_bUpdatingMenu == true) {
				return;
			}

			m_settings.PrivacyMode = m_menuPrivacyMode.Checked;
			DoUpdateSettingsMenu();
			DoRaiseSettingsChanged();
		}

		// Applies changes to visible privacy-sensitive items.
		// 표시할 개인정보 항목 변경을 적용한다.
		private void m_menuPrivacyItem_CheckedChanged(object sender, EventArgs e) {
			if (m_bUpdatingMenu == true) {
				return;
			}

			m_settings.ShowComputerName = m_menuComputerName.Checked;
			m_settings.ShowUserName = m_menuUserName.Checked;
			m_settings.ShowIpAddress = m_menuIpAddress.Checked;
			m_settings.ShowMacAddress = m_menuMacAddress.Checked;
			DoRaiseSettingsChanged();
		}

		// Applies the selected UI size.
		// 선택한 UI 크기를 적용한다.
		private void m_menuUiSize_Click(object sender, EventArgs e) {
			if (sender == m_menuSizeCompact) {
				m_settings.UiScale = E_InfoDeskUiScale.Compact;
			}
			else if (sender == m_menuSizeNormal) {
				m_settings.UiScale = E_InfoDeskUiScale.Normal;
			}
			else if (sender == m_menuSizeLarge) {
				m_settings.UiScale = E_InfoDeskUiScale.Large;
			}

			DoUpdateSettingsMenu();
			DoRaiseSettingsChanged();
		}

		// Applies the selected accent color.
		// 선택한 강조 색상을 적용한다.
		private void m_menuAccent_Click(object sender, EventArgs e) {
			if (sender == m_menuAccentBlue) {
				m_settings.AccentColor = E_InfoDeskAccentColor.Blue;
			}
			else if (sender == m_menuAccentGreen) {
				m_settings.AccentColor = E_InfoDeskAccentColor.Green;
			}
			else if (sender == m_menuAccentOrange) {
				m_settings.AccentColor = E_InfoDeskAccentColor.Orange;
			}
			else if (sender == m_menuAccentGray) {
				m_settings.AccentColor = E_InfoDeskAccentColor.Gray;
			}

			DoUpdateSettingsMenu();
			DoRaiseSettingsChanged();
		}

		// Applies the selected text color.
		// 선택한 글자 색상을 적용한다.
		private void m_menuTextColor_Click(object sender, EventArgs e) {
			if (sender == m_menuTextWhite) {
				m_settings.TextColor = E_InfoDeskTextColor.White;
			}
			else if (sender == m_menuTextLightGray) {
				m_settings.TextColor = E_InfoDeskTextColor.LightGray;
			}
			else if (sender == m_menuTextBlack) {
				m_settings.TextColor = E_InfoDeskTextColor.Black;
			}
			else if (sender == m_menuTextAccent) {
				m_settings.TextColor = E_InfoDeskTextColor.Accent;
			}

			DoUpdateSettingsMenu();
			DoRaiseSettingsChanged();
		}

		// Applies the selected system information refresh interval.
		// 선택한 시스템 정보 새로 고침 간격을 적용한다.
		private void m_menuRefreshInterval_Click(object sender, EventArgs e) {
			if (sender == m_menuRefresh1Second) {
				m_settings.RefreshIntervalSeconds = 1;
			}
			else if (sender == m_menuRefresh3Seconds) {
				m_settings.RefreshIntervalSeconds = 3;
			}
			else if (sender == m_menuRefresh5Seconds) {
				m_settings.RefreshIntervalSeconds = 5;
			}
			else if (sender == m_menuRefresh10Seconds) {
				m_settings.RefreshIntervalSeconds = 10;
			}

			DoUpdateSettingsMenu();
			DoRaiseSettingsChanged();
		}

		// Confirms and resets display settings to their defaults.
		// 확인 후 표시 설정을 기본값으로 재설정한다.
		private void m_menuResetSettings_Click(object sender, EventArgs e) {
			DialogResult result = MessageBox.Show(this,
				"Reset all display settings to their default values?\r\n\r\n" +
				"Run at logon will not be changed.",
				"DHInfoDesk - Reset Settings",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2);
			if (result != DialogResult.Yes) {
				return;
			}

			m_settings.Reset();
			DoUpdateSettingsMenu();
			DoRaiseSettingsChanged();
		}

		// Raises the auto-run setting change event.
		// 자동 실행 설정 변경 이벤트를 발생시킨다.
		private void m_menuAutoRun_CheckedChanged(object sender, EventArgs e) {
			if (OnAutoRunChanged != null) {
				OnAutoRunChanged(this, m_menuAutoRun.Checked);
			}
		}

		// Raises the application exit event.
		// 애플리케이션 종료 이벤트를 발생시킨다.
		private void m_menuExit_Click(object sender, EventArgs e) {
			if (OnExit != null) {
				OnExit(this, EventArgs.Empty);
			}
		}

		// Updates the auto-run menu state without raising a change event.
		// 변경 이벤트 없이 자동 실행 메뉴 상태를 갱신한다.
		public void SetAutoRun(bool bautorun) {
			m_menuAutoRun.CheckedChanged -= m_menuAutoRun_CheckedChanged;
			m_menuAutoRun.Checked = bautorun;
			m_menuAutoRun.CheckedChanged += m_menuAutoRun_CheckedChanged;
		}

		// Applies settings to the menu, button size, and position.
		// 설정을 메뉴, 버튼 크기, 위치에 적용한다.
		public void SetSettings(DHInfoDeskSettings settings) {
			if (settings == null) {
				return;
			}

			m_settings = settings;
			DoUpdateSettingsMenu();
			DoUpdateButtonScale();
			DoUpdateLocation();
		}

		// Keeps the settings button at the bottom while the popup is closed.
		// 팝업이 닫힌 동안 설정 버튼을 최하단에 유지한다.
		public void DoKeepBottomMost() {
			if (IsHandleCreated == true && m_menuSettings.Visible == false) {
				User32.SetWindowPos(Handle, User32.HWND_BOTTOM, 0, 0, 0, 0,
					User32.SWP_NOMOVE | User32.SWP_NOSIZE | User32.SWP_NOACTIVATE);
			}
		}

		// Initializes a checkable menu item and its click handler.
		// 선택 가능한 메뉴 항목과 클릭 처리기를 초기화한다.
		private void InitCheckMenu(ToolStripMenuItem menu, string strtext, EventHandler handler) {
			menu.Text = strtext;
			menu.CheckOnClick = true;
			menu.Click += handler;
		}

		// Synchronizes all settings menu states with the current settings.
		// 모든 설정 메뉴 상태를 현재 설정과 동기화한다.
		private void DoUpdateSettingsMenu() {
			m_bUpdatingMenu = true;
			try {
				m_menuShowSystem.Checked = m_settings.ShowSystem;
				m_menuShowCpu.Checked = m_settings.ShowCpu;
				m_menuShowMemory.Checked = m_settings.ShowMemory;
				m_menuShowStorage.Checked = m_settings.ShowStorage;
				m_menuShowNetwork.Checked = m_settings.ShowNetwork;

				m_menuTopLeft.Checked = m_settings.Alignment == E_InfoDeskAlignment.TopLeft;
				m_menuTopRight.Checked = m_settings.Alignment == E_InfoDeskAlignment.TopRight;
				m_menuBottomLeft.Checked = m_settings.Alignment == E_InfoDeskAlignment.BottomLeft;
				m_menuBottomRight.Checked = m_settings.Alignment == E_InfoDeskAlignment.BottomRight;

				m_menuAllMonitors.Checked =
					m_settings.MonitorMode == E_InfoDeskMonitorMode.All;
				m_menuPrimaryMonitor.Checked =
					m_settings.MonitorMode == E_InfoDeskMonitorMode.Primary;
				m_menuThisMonitor.Checked =
					m_settings.MonitorMode == E_InfoDeskMonitorMode.Selected &&
					string.Equals(m_settings.SelectedMonitorDeviceName, m_screen.DeviceName,
						StringComparison.OrdinalIgnoreCase) == true;

				m_menuPrivacyMode.Checked = m_settings.PrivacyMode;
				m_menuComputerName.Checked = m_settings.ShowComputerName;
				m_menuUserName.Checked = m_settings.ShowUserName;
				m_menuIpAddress.Checked = m_settings.ShowIpAddress;
				m_menuMacAddress.Checked = m_settings.ShowMacAddress;
				m_menuComputerName.Enabled = m_settings.PrivacyMode == false;
				m_menuUserName.Enabled = m_settings.PrivacyMode == false;
				m_menuIpAddress.Enabled = m_settings.PrivacyMode == false;
				m_menuMacAddress.Enabled = m_settings.PrivacyMode == false;

				m_menuSizeCompact.Checked = m_settings.UiScale == E_InfoDeskUiScale.Compact;
				m_menuSizeNormal.Checked = m_settings.UiScale == E_InfoDeskUiScale.Normal;
				m_menuSizeLarge.Checked = m_settings.UiScale == E_InfoDeskUiScale.Large;

				m_menuAccentBlue.Checked =
					m_settings.AccentColor == E_InfoDeskAccentColor.Blue;
				m_menuAccentGreen.Checked =
					m_settings.AccentColor == E_InfoDeskAccentColor.Green;
				m_menuAccentOrange.Checked =
					m_settings.AccentColor == E_InfoDeskAccentColor.Orange;
				m_menuAccentGray.Checked =
					m_settings.AccentColor == E_InfoDeskAccentColor.Gray;

				m_menuTextWhite.Checked = m_settings.TextColor == E_InfoDeskTextColor.White;
				m_menuTextLightGray.Checked =
					m_settings.TextColor == E_InfoDeskTextColor.LightGray;
				m_menuTextBlack.Checked = m_settings.TextColor == E_InfoDeskTextColor.Black;
				m_menuTextAccent.Checked = m_settings.TextColor == E_InfoDeskTextColor.Accent;

				m_menuRefresh1Second.Checked = m_settings.RefreshIntervalSeconds == 1;
				m_menuRefresh3Seconds.Checked = m_settings.RefreshIntervalSeconds == 3;
				m_menuRefresh5Seconds.Checked = m_settings.RefreshIntervalSeconds == 5;
				m_menuRefresh10Seconds.Checked = m_settings.RefreshIntervalSeconds == 10;
			}
			finally {
				m_bUpdatingMenu = false;
			}
		}

		// Raises the display settings change event.
		// 표시 설정 변경 이벤트를 발생시킨다.
		private void DoRaiseSettingsChanged() {
			if (OnSettingsChanged != null) {
				OnSettingsChanged(this, m_settings);
			}
		}

		// Positions the settings button inside the information panel.
		// 설정 버튼을 정보 패널 내부에 배치한다.
		private void DoUpdateLocation() {
			int panelmargin = Scale(48);
			int npanelwidth = Math.Min(Scale(382),
				Math.Max(Scale(300), m_screen.Bounds.Width - Scale(96)));
			int npanelheight = GetPanelHeight();
			int nx;
			int ny;

			if (m_settings.Alignment == E_InfoDeskAlignment.TopLeft ||
				m_settings.Alignment == E_InfoDeskAlignment.BottomLeft) {
				nx = m_screen.Bounds.Left + panelmargin + npanelwidth - Width - Scale(12);
			}
			else {
				nx = m_screen.Bounds.Right - panelmargin - Width - Scale(12);
			}

			if (m_settings.Alignment == E_InfoDeskAlignment.TopLeft ||
				m_settings.Alignment == E_InfoDeskAlignment.TopRight) {
				ny = m_screen.Bounds.Top + panelmargin + Scale(12);
			}
			else {
				ny = m_screen.Bounds.Bottom - panelmargin - npanelheight + Scale(12);
			}

			Location = new Point(nx, ny);
		}

		// Calculates the information panel height from enabled sections.
		// 활성화된 정보 항목을 기준으로 정보 패널 높이를 계산한다.
		private int GetPanelHeight() {
			int nheight = 104;

			if (m_settings.ShowSystem == true) {
				nheight += 56;
			}
			if (m_settings.ShowCpu == true) {
				nheight += 56;
			}
			if (m_settings.ShowMemory == true) {
				nheight += 56;
			}
			if (m_settings.ShowStorage == true) {
				nheight += 56;
			}
			if (m_settings.ShowNetwork == true) {
				nheight += 29;
			}
			if (m_settings.PrivacyMode == false) {
				if (m_settings.ShowComputerName == true) {
					nheight += 29;
				}
				if (m_settings.ShowUserName == true) {
					nheight += 29;
				}
				if (m_settings.ShowIpAddress == true) {
					nheight += 29;
				}
				if (m_settings.ShowMacAddress == true) {
					nheight += 29;
				}
			}

			return Scale(nheight);
		}

		// Resizes the settings button for the selected UI scale.
		// 선택한 UI 배율에 맞게 설정 버튼 크기를 조정한다.
		private void DoUpdateButtonScale() {
			ClientSize = new Size(Scale(24), Scale(24));
			Font oldfont = m_btnSettings.Font;
			m_btnSettings.Font = new Font("Segoe UI Symbol", 8.5f * GetUiScale(),
				FontStyle.Regular);
			if (oldfont != null) {
				oldfont.Dispose();
			}
		}

		// Returns the scale factor for the selected UI size.
		// 선택한 UI 크기의 배율을 반환한다.
		private float GetUiScale() {
			if (m_settings.UiScale == E_InfoDeskUiScale.Compact) {
				return 0.86f;
			}
			if (m_settings.UiScale == E_InfoDeskUiScale.Large) {
				return 1.18f;
			}

			return 1.0f;
		}

		// Scales a logical pixel value for the selected UI size.
		// 논리 픽셀 값을 선택한 UI 크기에 맞게 조정한다.
		private int Scale(int nvalue) {
			return Math.Max(1, (int)Math.Round(nvalue * GetUiScale()));
		}

		// Releases timers, hooks, and message filters.
		// 타이머, 훅, 메시지 필터를 해제한다.
		protected override void Dispose(bool disposing) {
			if (disposing == true) {
				m_tmPopupClose.Stop();
				m_tmPopupClose.Dispose();
				DoRemoveMouseHook();
				DoRemoveMessageFilter();
			}

			base.Dispose(disposing);
		}
	}
}
