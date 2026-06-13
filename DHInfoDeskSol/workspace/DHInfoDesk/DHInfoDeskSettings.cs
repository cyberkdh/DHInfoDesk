//////////////////////////////////////////////////////////////////////////////////////////////////
//	Projects		: DHInfoDesk
//	Author			: CYBERKDH(cyberkdh@hotmail.com, cyberkdh@gmail.com), AI(Codex)
//	Module			: DHInfoDesk
//	History			:
//	Copyrights		: Copyright ⓒCYBERKDH. All Rights Reserved.
//////////////////////////////////////////////////////////////////////////////////////////////////

using Microsoft.Win32;
using System;

namespace DHInfoDesk {
	internal enum E_InfoDeskAlignment {
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight
	}

	internal enum E_InfoDeskMonitorMode {
		All,
		Primary,
		Selected
	}

	internal enum E_InfoDeskUiScale {
		Compact,
		Normal,
		Large
	}

	internal enum E_InfoDeskAccentColor {
		Blue,
		Green,
		Orange,
		Gray
	}

	internal enum E_InfoDeskTextColor {
		White,
		LightGray,
		Black,
		Accent
	}

	internal sealed class DHInfoDeskSettings {
		private const string DEF_REGISTRY_PATH = @"Software\DHTOOL\DHInfoDesk";

		public bool ShowSystem { get; set; }
		public bool ShowCpu { get; set; }
		public bool ShowMemory { get; set; }
		public bool ShowStorage { get; set; }
		public bool ShowNetwork { get; set; }
		public bool PrivacyMode { get; set; }
		public bool ShowComputerName { get; set; }
		public bool ShowUserName { get; set; }
		public bool ShowIpAddress { get; set; }
		public bool ShowMacAddress { get; set; }
		public int RefreshIntervalSeconds { get; set; }
		public E_InfoDeskUiScale UiScale { get; set; }
		public E_InfoDeskAccentColor AccentColor { get; set; }
		public E_InfoDeskTextColor TextColor { get; set; }
		public E_InfoDeskAlignment Alignment { get; set; }
		public E_InfoDeskMonitorMode MonitorMode { get; set; }
		public string SelectedMonitorDeviceName { get; set; }

		// Initializes the settings with default values.
		// 설정을 기본값으로 초기화한다.
		public DHInfoDeskSettings() {
			Reset();
		}

		// Resets all display settings to their default values.
		// 모든 표시 설정을 기본값으로 재설정한다.
		public void Reset() {
			ShowSystem = true;
			ShowCpu = true;
			ShowMemory = true;
			ShowStorage = true;
			ShowNetwork = true;
			PrivacyMode = true;
			ShowComputerName = false;
			ShowUserName = false;
			ShowIpAddress = false;
			ShowMacAddress = false;
			RefreshIntervalSeconds = 1;
			UiScale = E_InfoDeskUiScale.Normal;
			AccentColor = E_InfoDeskAccentColor.Blue;
			TextColor = E_InfoDeskTextColor.White;
			Alignment = E_InfoDeskAlignment.TopRight;
			MonitorMode = E_InfoDeskMonitorMode.All;
			SelectedMonitorDeviceName = "";
		}

		// Loads the saved settings from the current user registry.
		// 현재 사용자 레지스트리에서 저장된 설정을 불러온다.
		public static DHInfoDeskSettings Load() {
			DHInfoDeskSettings settings = new DHInfoDeskSettings();

			try {
				using (RegistryKey key = Registry.CurrentUser.OpenSubKey(DEF_REGISTRY_PATH, false)) {
					if (key == null) {
						return settings;
					}

					settings.ShowSystem = GetBool(key, "ShowSystem", settings.ShowSystem);
					settings.ShowCpu = GetBool(key, "ShowCpu", settings.ShowCpu);
					settings.ShowMemory = GetBool(key, "ShowMemory", settings.ShowMemory);
					settings.ShowStorage = GetBool(key, "ShowStorage", settings.ShowStorage);
					settings.ShowNetwork = GetBool(key, "ShowNetwork", settings.ShowNetwork);
					settings.PrivacyMode = GetBool(key, "PrivacyMode", settings.PrivacyMode);
					settings.ShowComputerName =
						GetBool(key, "ShowComputerName", settings.ShowComputerName);
					settings.ShowUserName = GetBool(key, "ShowUserName", settings.ShowUserName);
					settings.ShowIpAddress = GetBool(key, "ShowIpAddress", settings.ShowIpAddress);
					settings.ShowMacAddress =
						GetBool(key, "ShowMacAddress", settings.ShowMacAddress);
					settings.RefreshIntervalSeconds = GetRefreshInterval(key,
						"RefreshIntervalSeconds", settings.RefreshIntervalSeconds);

					E_InfoDeskUiScale uiScale;
					if (Enum.TryParse(Convert.ToString(key.GetValue("UiScale", "")),
						true, out uiScale) == true) {
						settings.UiScale = uiScale;
					}

					E_InfoDeskAccentColor accentColor;
					if (Enum.TryParse(Convert.ToString(key.GetValue("AccentColor", "")),
						true, out accentColor) == true) {
						settings.AccentColor = accentColor;
					}

					E_InfoDeskTextColor textColor;
					if (Enum.TryParse(Convert.ToString(key.GetValue("TextColor", "")),
						true, out textColor) == true) {
						settings.TextColor = textColor;
					}

					E_InfoDeskAlignment alignment;
					if (Enum.TryParse(Convert.ToString(key.GetValue("Alignment", "")),
						true, out alignment) == true) {
						settings.Alignment = alignment;
					}

					E_InfoDeskMonitorMode monitorMode;
					if (Enum.TryParse(Convert.ToString(key.GetValue("MonitorMode", "")),
						true, out monitorMode) == true) {
						settings.MonitorMode = monitorMode;
					}
					settings.SelectedMonitorDeviceName =
						Convert.ToString(key.GetValue("SelectedMonitorDeviceName", ""));
				}
			}
			catch {
			}

			return settings;
		}

		// Saves the current settings to the current user registry.
		// 현재 설정을 현재 사용자 레지스트리에 저장한다.
		public void Save() {
			try {
				using (RegistryKey key = Registry.CurrentUser.CreateSubKey(DEF_REGISTRY_PATH)) {
					key.SetValue("ShowSystem", ShowSystem == true ? 1 : 0, RegistryValueKind.DWord);
					key.SetValue("ShowCpu", ShowCpu == true ? 1 : 0, RegistryValueKind.DWord);
					key.SetValue("ShowMemory", ShowMemory == true ? 1 : 0, RegistryValueKind.DWord);
					key.SetValue("ShowStorage", ShowStorage == true ? 1 : 0, RegistryValueKind.DWord);
					key.SetValue("ShowNetwork", ShowNetwork == true ? 1 : 0, RegistryValueKind.DWord);
					key.SetValue("PrivacyMode", PrivacyMode == true ? 1 : 0,
						RegistryValueKind.DWord);
					key.SetValue("ShowComputerName", ShowComputerName == true ? 1 : 0,
						RegistryValueKind.DWord);
					key.SetValue("ShowUserName", ShowUserName == true ? 1 : 0,
						RegistryValueKind.DWord);
					key.SetValue("ShowIpAddress", ShowIpAddress == true ? 1 : 0,
						RegistryValueKind.DWord);
					key.SetValue("ShowMacAddress", ShowMacAddress == true ? 1 : 0,
						RegistryValueKind.DWord);
					key.SetValue("RefreshIntervalSeconds",
						NormalizeRefreshInterval(RefreshIntervalSeconds),
						RegistryValueKind.DWord);
					key.SetValue("UiScale", UiScale.ToString(), RegistryValueKind.String);
					key.SetValue("AccentColor", AccentColor.ToString(), RegistryValueKind.String);
					key.SetValue("TextColor", TextColor.ToString(), RegistryValueKind.String);
					key.SetValue("Alignment", Alignment.ToString(), RegistryValueKind.String);
					key.SetValue("MonitorMode", MonitorMode.ToString(), RegistryValueKind.String);
					key.SetValue("SelectedMonitorDeviceName",
						SelectedMonitorDeviceName == null ? "" : SelectedMonitorDeviceName,
						RegistryValueKind.String);
				}
			}
			catch {
			}
		}

		// Reads a Boolean setting from the registry.
		// 레지스트리에서 논리형 설정을 읽는다.
		private static bool GetBool(RegistryKey key, string strname, bool bdefault) {
			object value = key.GetValue(strname, bdefault == true ? 1 : 0);
			try {
				return Convert.ToInt32(value) != 0;
			}
			catch {
				return bdefault;
			}
		}

		// Reads and validates the refresh interval from the registry.
		// 레지스트리에서 새로 고침 간격을 읽고 검증한다.
		private static int GetRefreshInterval(RegistryKey key, string strname, int ndefault) {
			try {
				return NormalizeRefreshInterval(Convert.ToInt32(key.GetValue(strname, ndefault)));
			}
			catch {
				return NormalizeRefreshInterval(ndefault);
			}
		}

		// Normalizes the refresh interval to a supported value.
		// 새로 고침 간격을 지원되는 값으로 정규화한다.
		private static int NormalizeRefreshInterval(int nseconds) {
			if (nseconds == 1 || nseconds == 3 || nseconds == 5 || nseconds == 10) {
				return nseconds;
			}

			return 1;
		}
	}
}
