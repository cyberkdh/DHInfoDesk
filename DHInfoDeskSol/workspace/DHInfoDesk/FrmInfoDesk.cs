//////////////////////////////////////////////////////////////////////////////////////////////////
//	Projects		: DHInfoDesk
//	Author			: CYBERKDH(cyberkdh@hotmail.com, cyberkdh@gmail.com), AI(Codex)
//	Module			: DHInfoDesk
//	History			:
//	Copyrights		: Copyright ⓒCYBERKDH. All Rights Reserved.
//////////////////////////////////////////////////////////////////////////////////////////////////

using DHInfoDesk.module.sysinfo;
using DHInfoDesk.module.win32;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace DHInfoDesk {
	internal sealed class FrmInfoDesk : Form {
		private static readonly Color DEF_TRANSPARENT_COLOR = Color.Fuchsia;

		private readonly Screen m_screen;
		private DHSysInfoSnapshot m_snapshot = null;
		private DHInfoDeskSettings m_settings = new DHInfoDeskSettings();

		public string ScreenDeviceName {
			get { return m_screen.DeviceName; }
		}

		// Initializes the transparent information window for a monitor.
		// 모니터용 투명 정보 창을 초기화한다.
		public FrmInfoDesk(Screen screen) {
			m_screen = screen;

			AutoScaleMode = AutoScaleMode.Dpi;
			BackColor = DEF_TRANSPARENT_COLOR;
			Bounds = m_screen.Bounds;
			FormBorderStyle = FormBorderStyle.None;
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.Manual;
			TopMost = false;
			TransparencyKey = DEF_TRANSPARENT_COLOR;

			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.UserPaint, true);
		}

		protected override bool ShowWithoutActivation {
			get { return true; }
		}

		protected override CreateParams CreateParams {
			get {
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= User32.WS_EX_NOACTIVATE;
				cp.ExStyle |= User32.WS_EX_TOOLWINDOW;
				cp.ExStyle |= User32.WS_EX_TRANSPARENT;
				return cp;
			}
		}

		// Places the window at the bottom after it is shown.
		// 창이 표시된 후 최하단에 배치한다.
		protected override void OnShown(EventArgs e) {
			base.OnShown(e);
			DoKeepBottomMost();
		}

		// Renders the configured system information on the transparent window.
		// 구성된 시스템 정보를 투명 창에 그린다.
		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);

			e.Graphics.TextRenderingHint =
				System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

			int npanelwidth = Math.Min(Scale(382),
				Math.Max(Scale(300), ClientSize.Width - Scale(96)));
			int npanelheight = GetPanelHeight();
			Rectangle rtpanel = GetPanelRectangle(npanelwidth, npanelheight);
			Color accent = GetAccentColor();
			Color textcolor = GetTextColor();
			Color shadowcolor = GetShadowColor(textcolor);
			using (Font fonttitle = new Font("Segoe UI", 16.0f * GetUiScale(), FontStyle.Bold))
			using (Font fontlabel = new Font("Segoe UI", 9.5f * GetUiScale(), FontStyle.Bold))
			using (Font fonttext = new Font("Segoe UI", 9.5f * GetUiScale(), FontStyle.Regular))
			using (Font fontsmall = new Font("Segoe UI", 8.5f * GetUiScale(), FontStyle.Regular))
			using (SolidBrush brtitle = new SolidBrush(textcolor))
			using (SolidBrush brlabel = new SolidBrush(accent))
			using (SolidBrush brtext = new SolidBrush(textcolor))
			using (SolidBrush brmuted = new SolidBrush(textcolor))
			using (SolidBrush brshadow = new SolidBrush(shadowcolor)) {
				DrawStringWithShadow(e.Graphics, "Information Desk", fonttitle, brtitle, brshadow,
					rtpanel.Left + Scale(20), rtpanel.Top + Scale(14));
				if (m_settings.PrivacyMode == true) {
					SizeF size = e.Graphics.MeasureString("PRIVACY", fontsmall);
					DrawStringWithShadow(e.Graphics, "PRIVACY", fontsmall, brlabel, brshadow,
						rtpanel.Right - size.Width - Scale(52), rtpanel.Top + Scale(25));
				}

				if (m_snapshot == null) {
					DrawStringWithShadow(e.Graphics, "Collecting system information...",
						fonttext, brtext, brshadow,
						rtpanel.Left + Scale(20), rtpanel.Top + Scale(64));
					return;
				}

				int ny = rtpanel.Top + Scale(64);
				if (m_settings.ShowSystem == true) {
					DrawSystem(e.Graphics, rtpanel, ref ny, fontlabel, fonttext, brlabel,
						brtext, brshadow);
				}
				if (m_settings.ShowCpu == true) {
					DrawCpu(e.Graphics, rtpanel, ref ny, fontlabel, fonttext, brlabel,
						brtext, brshadow);
				}
				if (m_settings.ShowMemory == true) {
					DrawMemory(e.Graphics, rtpanel, ref ny, fontlabel, fonttext, brlabel,
						brtext, brshadow);
				}
				if (m_settings.ShowStorage == true) {
					DrawStorage(e.Graphics, rtpanel, ref ny, fontlabel, fonttext, brlabel,
						brtext, brshadow);
				}
				if (m_settings.ShowNetwork == true) {
					DrawNetwork(e.Graphics, rtpanel, ref ny, fontlabel, fonttext, brlabel,
						brtext, brshadow);
				}
				if (m_settings.PrivacyMode == false) {
					DrawPrivacyItems(e.Graphics, rtpanel, ref ny, fontlabel, fonttext,
						brlabel, brtext, brshadow);
				}

				string strfooter = m_screen.DeviceName + "  |  Updated " +
					m_snapshot.CollectedAt.ToString("HH:mm:ss");
				DrawStringWithShadow(e.Graphics, strfooter, fontsmall, brmuted, brshadow,
					rtpanel.Left + Scale(20), rtpanel.Bottom - Scale(24));
			}
		}

		// Applies display settings and requests repainting.
		// 표시 설정을 적용하고 다시 그리기를 요청한다.
		public void SetSettings(DHInfoDeskSettings settings) {
			if (settings == null) {
				return;
			}

			m_settings = settings;
			Invalidate();
		}

		// Applies the latest system information snapshot.
		// 최신 시스템 정보 스냅샷을 적용한다.
		public void SetSysInfo(DHSysInfoSnapshot snapshot) {
			m_snapshot = snapshot;
			Invalidate();
		}

		// Keeps the information window at the bottom without activation.
		// 정보 창을 활성화하지 않고 최하단에 유지한다.
		public void DoKeepBottomMost() {
			if (IsHandleCreated == true) {
				User32.SetWindowPos(Handle, User32.HWND_BOTTOM, 0, 0, 0, 0,
					User32.SWP_NOMOVE | User32.SWP_NOSIZE | User32.SWP_NOACTIVATE);
			}
		}

		// Calculates the panel height from the enabled information sections.
		// 활성화된 정보 항목을 기준으로 패널 높이를 계산한다.
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

		// Calculates the panel rectangle from the selected alignment.
		// 선택한 정렬 방식으로 패널 영역을 계산한다.
		private Rectangle GetPanelRectangle(int nwidth, int nheight) {
			int margin = Scale(48);
			int nx;
			int ny;

			if (m_settings.Alignment == E_InfoDeskAlignment.TopLeft ||
				m_settings.Alignment == E_InfoDeskAlignment.BottomLeft) {
				nx = margin;
			}
			else {
				nx = ClientSize.Width - nwidth - margin;
			}

			if (m_settings.Alignment == E_InfoDeskAlignment.TopLeft ||
				m_settings.Alignment == E_InfoDeskAlignment.TopRight) {
				ny = margin;
			}
			else {
				ny = ClientSize.Height - nheight - margin;
			}

			return new Rectangle(nx, ny, nwidth, nheight);
		}

		// Draws operating system and uptime information.
		// 운영체제와 가동 시간 정보를 그린다.
		private void DrawSystem(Graphics graphics, Rectangle rtpanel, ref int ny, Font fontlabel,
			Font fonttext, Brush brlabel, Brush brtext, Brush brshadow) {
			string strvalue = GetStatusValue(m_snapshot.Status.System,
				m_snapshot.System.OsName + " " + m_snapshot.System.OsVersion);
			DrawRow(graphics, rtpanel, ref ny, "SYSTEM", strvalue, fontlabel, fonttext,
				brlabel, brtext, brshadow);

			if (m_snapshot.Status.System.IsSuccess == true) {
				string struptime = string.Format("Uptime {0}d {1:00}:{2:00}",
					(int)m_snapshot.System.Uptime.TotalDays, m_snapshot.System.Uptime.Hours,
					m_snapshot.System.Uptime.Minutes);
				DrawSubRow(graphics, rtpanel, ref ny, struptime, fonttext, brtext, brshadow);
			}
		}

		// Draws processor details and current usage.
		// 프로세서 세부 정보와 현재 사용률을 그린다.
		private void DrawCpu(Graphics graphics, Rectangle rtpanel, ref int ny, Font fontlabel,
			Font fonttext, Brush brlabel, SolidBrush brtext, SolidBrush brshadow) {
			string strvalue = GetStatusValue(m_snapshot.Status.Cpu, m_snapshot.Cpu.Name);
			DrawRow(graphics, rtpanel, ref ny, "CPU", strvalue, fontlabel, fonttext,
				brlabel, brtext, brshadow);

			if (m_snapshot.Status.Cpu.IsSuccess == true) {
				string strusage = string.Format("{0:F1}%  |  {1}C / {2}T",
					m_snapshot.Cpu.UsagePercent, m_snapshot.Cpu.PhysicalCoreCount,
					m_snapshot.Cpu.LogicalProcessorCount);
				DrawBarRow(graphics, rtpanel, ref ny, strusage, m_snapshot.Cpu.UsagePercent,
					fonttext, brtext, brshadow);
			}
		}

		// Draws physical memory capacity and usage.
		// 물리 메모리 용량과 사용률을 그린다.
		private void DrawMemory(Graphics graphics, Rectangle rtpanel, ref int ny, Font fontlabel,
			Font fonttext, Brush brlabel, SolidBrush brtext, SolidBrush brshadow) {
			string strvalue = m_snapshot.Status.Memory.IsSuccess == true ?
				FormatBytes(m_snapshot.Memory.UsedBytes) + " / " +
				FormatBytes(m_snapshot.Memory.TotalBytes) :
				GetStatusValue(m_snapshot.Status.Memory, "");
			DrawRow(graphics, rtpanel, ref ny, "MEMORY", strvalue, fontlabel, fonttext,
				brlabel, brtext, brshadow);

			if (m_snapshot.Status.Memory.IsSuccess == true) {
				DrawBarRow(graphics, rtpanel, ref ny, m_snapshot.Memory.UsagePercent + "%",
					m_snapshot.Memory.UsagePercent, fonttext, brtext, brshadow);
			}
		}

		// Draws primary storage capacity and usage.
		// 주 저장 장치 용량과 사용률을 그린다.
		private void DrawStorage(Graphics graphics, Rectangle rtpanel, ref int ny, Font fontlabel,
			Font fonttext, Brush brlabel, SolidBrush brtext, SolidBrush brshadow) {
			DHSysInfoStorageData storage = GetPrimaryStorage();
			string strvalue = storage == null ? GetStatusValue(m_snapshot.Status.Storage, "") :
				storage.Name + " " + FormatBytes((ulong)Math.Max(0, storage.AvailableBytes)) + " free";
			DrawRow(graphics, rtpanel, ref ny, "STORAGE", strvalue, fontlabel, fonttext,
				brlabel, brtext, brshadow);

			if (storage != null) {
				DrawBarRow(graphics, rtpanel, ref ny, storage.UsagePercent.ToString("F0") + "%",
					storage.UsagePercent, fonttext, brtext, brshadow);
			}
		}

		// Draws aggregate network receive and send rates.
		// 전체 네트워크 수신 및 송신 속도를 그린다.
		private void DrawNetwork(Graphics graphics, Rectangle rtpanel, ref int ny, Font fontlabel,
			Font fonttext, Brush brlabel, Brush brtext, Brush brshadow) {
			long nreceive = 0;
			long nsend = 0;
			int i;
			for (i = 0; i < m_snapshot.Network.Count; i++) {
				nreceive += m_snapshot.Network[i].ReceiveBytesPerSecond;
				nsend += m_snapshot.Network[i].SendBytesPerSecond;
			}

			string strvalue = m_snapshot.Status.Network.IsSuccess == true ?
				"RX " + FormatBytes((ulong)Math.Max(0, nreceive)) + "/s  TX " +
				FormatBytes((ulong)Math.Max(0, nsend)) + "/s" :
				GetStatusValue(m_snapshot.Status.Network, "");
			DrawRow(graphics, rtpanel, ref ny, "NETWORK", strvalue, fontlabel, fonttext,
				brlabel, brtext, brshadow);
		}

		// Draws privacy-sensitive values enabled by the user.
		// 사용자가 허용한 개인정보 항목을 그린다.
		private void DrawPrivacyItems(Graphics graphics, Rectangle rtpanel, ref int ny,
			Font fontlabel, Font fonttext, Brush brlabel, Brush brtext, Brush brshadow) {
			if (m_settings.ShowComputerName == true) {
				DrawRow(graphics, rtpanel, ref ny, "COMPUTER",
					GetPrivateValue(m_snapshot.System.ComputerName), fontlabel, fonttext,
					brlabel, brtext, brshadow);
			}
			if (m_settings.ShowUserName == true) {
				DrawRow(graphics, rtpanel, ref ny, "USER",
					GetPrivateValue(m_snapshot.System.UserName), fontlabel, fonttext,
					brlabel, brtext, brshadow);
			}

			DHSysInfoNetworkData adapter = GetActiveNetworkAdapter();
			if (m_settings.ShowIpAddress == true) {
				DrawRow(graphics, rtpanel, ref ny, "IP",
					GetPrivateValue(GetPreferredIpAddress(adapter)), fontlabel, fonttext,
					brlabel, brtext, brshadow);
			}
			if (m_settings.ShowMacAddress == true) {
				DrawRow(graphics, rtpanel, ref ny, "MAC",
					GetPrivateValue(adapter == null ? "" : adapter.MacAddress),
					fontlabel, fonttext, brlabel, brtext, brshadow);
			}
		}

		// Draws a labeled information row.
		// 레이블이 있는 정보 행을 그린다.
		private void DrawRow(Graphics graphics, Rectangle rtpanel, ref int ny, string strlabel,
			string strvalue, Font fontlabel, Font fonttext, Brush brlabel, Brush brtext,
			Brush brshadow) {
			DrawStringWithShadow(graphics, strlabel, fontlabel, brlabel, brshadow,
				rtpanel.Left + Scale(20), ny);
			RectangleF rtvalue = new RectangleF(rtpanel.Left + Scale(104), ny,
				rtpanel.Width - Scale(124), Scale(25));
			DrawStringWithShadow(graphics, strvalue, fonttext, brtext, brshadow, rtvalue);
			ny += Scale(29);
		}

		// Draws an indented secondary information row.
		// 들여쓰기된 보조 정보 행을 그린다.
		private void DrawSubRow(Graphics graphics, Rectangle rtpanel, ref int ny, string strvalue,
			Font fonttext, Brush brtext, Brush brshadow) {
			DrawStringWithShadow(graphics, strvalue, fonttext, brtext, brshadow,
				rtpanel.Left + Scale(104), ny);
			ny += Scale(27);
		}

		// Draws a usage bar and its percentage text.
		// 사용률 막대와 백분율 텍스트를 그린다.
		private void DrawBarRow(Graphics graphics, Rectangle rtpanel, ref int ny, string strvalue,
			double percent, Font fonttext, SolidBrush brtext, SolidBrush brshadow) {
			int nleft = rtpanel.Left + Scale(104);
			int nright = rtpanel.Right - Scale(20);
			int ngap = Scale(8);
			int nminimumbarwidth = Scale(64);
			SizeF textsize = graphics.MeasureString(strvalue, fonttext);
			int ntextwidth = Math.Min((int)Math.Ceiling(textsize.Width), Math.Max(Scale(56),
				nright - nleft - nminimumbarwidth - ngap));
			int nbarwidth = Math.Max(nminimumbarwidth, nright - nleft - ntextwidth - ngap);
			Rectangle rtbar = new Rectangle(nleft, ny + Scale(6), nbarwidth, Scale(9));
			RectangleF rttext = new RectangleF(rtbar.Right + ngap, ny - Scale(1),
				Math.Max(1, nright - rtbar.Right - ngap), Scale(23));
			double limited = Math.Max(0.0, Math.Min(100.0, percent));
			Rectangle rtfill = new Rectangle(rtbar.Left, rtbar.Top,
				(int)(rtbar.Width * limited / 100.0), rtbar.Height);

			using (SolidBrush brbar = new SolidBrush(Color.FromArgb(255, 65, 70, 82)))
			using (SolidBrush brfill = new SolidBrush(GetUsageColor(limited))) {
				graphics.FillRectangle(brbar, rtbar);
				graphics.FillRectangle(brfill, rtfill);
			}
			using (StringFormat format = new StringFormat()) {
				format.Alignment = StringAlignment.Far;
				format.LineAlignment = StringAlignment.Near;
				format.FormatFlags = StringFormatFlags.NoWrap;
				format.Trimming = StringTrimming.EllipsisCharacter;

				RectangleF rtshadow = rttext;
				rtshadow.Offset(Scale(1), Scale(1));
				graphics.DrawString(strvalue, fonttext, brshadow, rtshadow, format);
				graphics.DrawString(strvalue, fonttext, brtext, rttext, format);
			}
			ny += Scale(27);
		}

		// Returns the system drive or the first available storage item.
		// 시스템 드라이브 또는 첫 번째 사용 가능한 저장 장치를 반환한다.
		private DHSysInfoStorageData GetPrimaryStorage() {
			string strsystemroot = Path.GetPathRoot(Environment.SystemDirectory);
			int i;
			for (i = 0; i < m_snapshot.Storage.Count; i++) {
				if (string.Equals(m_snapshot.Storage[i].Name, strsystemroot,
					StringComparison.OrdinalIgnoreCase) == true) {
					return m_snapshot.Storage[i];
				}
			}

			return m_snapshot.Storage.Count > 0 ? m_snapshot.Storage[0] : null;
		}

		// Returns an active network adapter with a usable address.
		// 사용 가능한 주소를 가진 활성 네트워크 어댑터를 반환한다.
		private DHSysInfoNetworkData GetActiveNetworkAdapter() {
			DHSysInfoNetworkData fallback = null;
			int i;
			for (i = 0; i < m_snapshot.Network.Count; i++) {
				DHSysInfoNetworkData adapter = m_snapshot.Network[i];
				if (fallback == null && adapter.IpAddresses.Count > 0) {
					fallback = adapter;
				}
				if (string.Equals(adapter.Status, "Up", StringComparison.OrdinalIgnoreCase) == true &&
					adapter.IpAddresses.Count > 0) {
					return adapter;
				}
			}

			return fallback;
		}

		// Selects a preferred non-loopback IP address from an adapter.
		// 어댑터에서 루프백이 아닌 우선 IP 주소를 선택한다.
		private string GetPreferredIpAddress(DHSysInfoNetworkData adapter) {
			if (adapter == null) {
				return "";
			}

			string stripv6 = "";
			int i;
			for (i = 0; i < adapter.IpAddresses.Count; i++) {
				IPAddress address;
				if (IPAddress.TryParse(adapter.IpAddresses[i], out address) == false) {
					continue;
				}
				if (address.AddressFamily == AddressFamily.InterNetwork &&
					IPAddress.IsLoopback(address) == false) {
					return address.ToString();
				}
				if (address.AddressFamily == AddressFamily.InterNetworkV6 &&
					address.IsIPv6LinkLocal == false && string.IsNullOrEmpty(stripv6) == true) {
					stripv6 = address.ToString();
				}
			}

			return stripv6;
		}

		// Normalizes a privacy-sensitive value for display.
		// 개인정보 값을 화면 표시용으로 정규화한다.
		private string GetPrivateValue(string strvalue) {
			return string.IsNullOrWhiteSpace(strvalue) == true ? "N/A" : strvalue.Trim();
		}

		// Converts a collection status and value into display text.
		// 수집 상태와 값을 화면 표시 텍스트로 변환한다.
		private string GetStatusValue(DHSysInfoCollectionStatus status, string strvalue) {
			if (status.IsSuccess == true) {
				return string.IsNullOrEmpty(strvalue) == true ? "N/A" : strvalue.Trim();
			}
			if (status.IsPartial == true) {
				return "PARTIAL";
			}

			return "N/A";
		}

		// Formats a byte count using a readable binary unit.
		// 바이트 수를 읽기 쉬운 이진 단위로 변환한다.
		private string FormatBytes(ulong nbytes) {
			string[] units = new string[] { "B", "KB", "MB", "GB", "TB" };
			double value = nbytes;
			int nunit = 0;

			while (value >= 1024.0 && nunit < units.Length - 1) {
				value /= 1024.0;
				nunit++;
			}

			return value.ToString(nunit >= 3 ? "F1" : "F0") + " " + units[nunit];
		}

		// Selects a color based on the usage percentage.
		// 사용률에 따라 색상을 선택한다.
		private Color GetUsageColor(double percent) {
			if (percent >= 90.0) {
				return Color.FromArgb(255, 235, 87, 87);
			}
			if (percent >= 70.0) {
				return Color.FromArgb(255, 245, 181, 75);
			}

			return GetAccentColor();
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

		// Returns the configured accent color.
		// 구성된 강조 색상을 반환한다.
		private Color GetAccentColor() {
			if (m_settings.AccentColor == E_InfoDeskAccentColor.Green) {
				return Color.FromArgb(255, 63, 190, 140);
			}
			if (m_settings.AccentColor == E_InfoDeskAccentColor.Orange) {
				return Color.FromArgb(255, 245, 166, 35);
			}
			if (m_settings.AccentColor == E_InfoDeskAccentColor.Gray) {
				return Color.FromArgb(255, 165, 170, 180);
			}

			return Color.FromArgb(255, 112, 201, 255);
		}

		// Returns the configured text color.
		// 구성된 글자 색상을 반환한다.
		private Color GetTextColor() {
			if (m_settings.TextColor == E_InfoDeskTextColor.LightGray) {
				return Color.FromArgb(255, 220, 220, 220);
			}
			if (m_settings.TextColor == E_InfoDeskTextColor.Black) {
				return Color.Black;
			}
			if (m_settings.TextColor == E_InfoDeskTextColor.Accent) {
				return GetAccentColor();
			}

			return Color.White;
		}

		// Returns a contrasting shadow color for readable text.
		// 읽기 쉬운 텍스트를 위한 대비 그림자 색상을 반환한다.
		private Color GetShadowColor(Color color) {
			int nluminance = (color.R * 299 + color.G * 587 + color.B * 114) / 1000;
			return nluminance >= 145 ? Color.Black : Color.White;
		}

		// Draws text with a one-pixel shadow at a point.
		// 지정한 위치에 1픽셀 그림자가 있는 텍스트를 그린다.
		private void DrawStringWithShadow(Graphics graphics, string strtext, Font font,
			Brush brtext, Brush brshadow, float x, float y) {
			graphics.DrawString(strtext, font, brshadow, x + Scale(1), y + Scale(1));
			graphics.DrawString(strtext, font, brtext, x, y);
		}

		// Draws text with a one-pixel shadow inside a rectangle.
		// 사각 영역 안에 1픽셀 그림자가 있는 텍스트를 그린다.
		private void DrawStringWithShadow(Graphics graphics, string strtext, Font font,
			Brush brtext, Brush brshadow, RectangleF rt) {
			RectangleF rtshadow = rt;
			rtshadow.Offset(Scale(1), Scale(1));
			graphics.DrawString(strtext, font, brshadow, rtshadow);
			graphics.DrawString(strtext, font, brtext, rt);
		}
	}
}
