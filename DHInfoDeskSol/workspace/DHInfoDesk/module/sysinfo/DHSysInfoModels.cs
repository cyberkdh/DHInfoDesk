//////////////////////////////////////////////////////////////////////////////////////////////////
//	Projects		: DHInfoDesk
//	Author			: CYBERKDH(cyberkdh@hotmail.com, cyberkdh@gmail.com), AI(Codex)
//	Module			: DHInfoDesk.module.sysinfo
//	History			:
//	Copyrights		: Copyright ⓒCYBERKDH. All Rights Reserved.
//////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace DHInfoDesk.module.sysinfo {
	public sealed class DHSysInfoSnapshot {
		public DateTime CollectedAt { get; internal set; }
		public DHSysInfoCollectionStatusSet Status { get; internal set; }
		public DHSysInfoSystemData System { get; internal set; }
		public DHSysInfoCpuData Cpu { get; internal set; }
		public DHSysInfoMemoryData Memory { get; internal set; }
		public IList<DHSysInfoStorageData> Storage { get; internal set; }
		public IList<DHSysInfoNetworkData> Network { get; internal set; }
		public IList<DHSysInfoDisplayData> Display { get; internal set; }

		// Initializes an empty system information snapshot.
		// 빈 시스템 정보 스냅샷을 초기화한다.
		public DHSysInfoSnapshot() {
			CollectedAt = DateTime.MinValue;
			Status = new DHSysInfoCollectionStatusSet();
			System = new DHSysInfoSystemData();
			Cpu = new DHSysInfoCpuData();
			Memory = new DHSysInfoMemoryData();
			Storage = new List<DHSysInfoStorageData>();
			Network = new List<DHSysInfoNetworkData>();
			Display = new List<DHSysInfoDisplayData>();
		}
	}

	public sealed class DHSysInfoCollectionStatusSet {
		public DHSysInfoCollectionStatus System { get; private set; }
		public DHSysInfoCollectionStatus Cpu { get; private set; }
		public DHSysInfoCollectionStatus Memory { get; private set; }
		public DHSysInfoCollectionStatus Storage { get; private set; }
		public DHSysInfoCollectionStatus Network { get; private set; }
		public DHSysInfoCollectionStatus Display { get; private set; }

		// Initializes collection status objects for every category.
		// 모든 카테고리의 수집 상태 객체를 초기화한다.
		public DHSysInfoCollectionStatusSet() {
			System = new DHSysInfoCollectionStatus();
			Cpu = new DHSysInfoCollectionStatus();
			Memory = new DHSysInfoCollectionStatus();
			Storage = new DHSysInfoCollectionStatus();
			Network = new DHSysInfoCollectionStatus();
			Display = new DHSysInfoCollectionStatus();
		}
	}

	public sealed class DHSysInfoCollectionStatus {
		public bool IsSuccess { get; internal set; }
		public bool IsPartial { get; internal set; }
		public string ErrorMessage { get; internal set; }
		public DateTime LastAttemptAt { get; internal set; }
		public DateTime LastSuccessAt { get; internal set; }

		// Initializes an empty collection status.
		// 빈 수집 상태를 초기화한다.
		public DHSysInfoCollectionStatus() {
			IsSuccess = false;
			IsPartial = false;
			ErrorMessage = "";
			LastAttemptAt = DateTime.MinValue;
			LastSuccessAt = DateTime.MinValue;
		}

		// Marks a collection attempt as successful.
		// 수집 시도를 성공 상태로 표시한다.
		internal void SetSuccess(DateTime collectedAt) {
			IsSuccess = true;
			IsPartial = false;
			ErrorMessage = "";
			LastAttemptAt = collectedAt;
			LastSuccessAt = collectedAt;
		}

		// Marks a collection attempt as failed.
		// 수집 시도를 실패 상태로 표시한다.
		internal void SetFailure(DateTime collectedAt, Exception ex) {
			IsSuccess = false;
			IsPartial = false;
			ErrorMessage = ex == null ? "Unknown collection error" : ex.Message;
			LastAttemptAt = collectedAt;
		}

		// Marks a collection attempt as partially successful.
		// 수집 시도를 부분 성공 상태로 표시한다.
		internal void SetPartial(DateTime collectedAt, string strmessage) {
			IsSuccess = false;
			IsPartial = true;
			ErrorMessage = strmessage == null ? "Partial collection result" : strmessage;
			LastAttemptAt = collectedAt;
		}
	}

	public sealed class DHSysInfoSystemData {
		public string ComputerName { get; internal set; }
		public string UserName { get; internal set; }
		public string OsName { get; internal set; }
		public string OsVersion { get; internal set; }
		public string OsBuild { get; internal set; }
		public string Architecture { get; internal set; }
		public string TimeZone { get; internal set; }
		public TimeSpan Uptime { get; internal set; }
		public DateTime LastBootTime { get; internal set; }

		// Initializes empty operating system information.
		// 빈 운영체제 정보를 초기화한다.
		public DHSysInfoSystemData() {
			ComputerName = "";
			UserName = "";
			OsName = "";
			OsVersion = "";
			OsBuild = "";
			Architecture = "";
			TimeZone = "";
			Uptime = TimeSpan.Zero;
			LastBootTime = DateTime.MinValue;
		}
	}

	public sealed class DHSysInfoCpuData {
		public string Name { get; internal set; }
		public string Manufacturer { get; internal set; }
		public int PhysicalCoreCount { get; internal set; }
		public int LogicalProcessorCount { get; internal set; }
		public uint MaxClockMHz { get; internal set; }
		public float UsagePercent { get; internal set; }

		// Initializes empty processor information.
		// 빈 프로세서 정보를 초기화한다.
		public DHSysInfoCpuData() {
			Name = "";
			Manufacturer = "";
		}
	}

	public sealed class DHSysInfoMemoryData {
		public ulong TotalBytes { get; internal set; }
		public ulong AvailableBytes { get; internal set; }
		public ulong UsedBytes { get; internal set; }
		public uint UsagePercent { get; internal set; }
	}

	public sealed class DHSysInfoStorageData {
		public string Name { get; internal set; }
		public string VolumeLabel { get; internal set; }
		public string DriveType { get; internal set; }
		public string FileSystem { get; internal set; }
		public long TotalBytes { get; internal set; }
		public long AvailableBytes { get; internal set; }
		public long UsedBytes { get; internal set; }
		public double UsagePercent { get; internal set; }

		// Initializes empty storage information.
		// 빈 저장 장치 정보를 초기화한다.
		public DHSysInfoStorageData() {
			Name = "";
			VolumeLabel = "";
			DriveType = "";
			FileSystem = "";
		}
	}

	public sealed class DHSysInfoNetworkData {
		public string Id { get; internal set; }
		public string Name { get; internal set; }
		public string Description { get; internal set; }
		public string Status { get; internal set; }
		public string MacAddress { get; internal set; }
		public IList<string> IpAddresses { get; internal set; }
		public long LinkSpeedBitsPerSecond { get; internal set; }
		public long BytesReceived { get; internal set; }
		public long BytesSent { get; internal set; }
		public long ReceiveBytesPerSecond { get; internal set; }
		public long SendBytesPerSecond { get; internal set; }

		// Initializes empty network adapter information.
		// 빈 네트워크 어댑터 정보를 초기화한다.
		public DHSysInfoNetworkData() {
			Id = "";
			Name = "";
			Description = "";
			Status = "";
			MacAddress = "";
			IpAddresses = new List<string>();
		}
	}

	public sealed class DHSysInfoDisplayData {
		public string DeviceName { get; internal set; }
		public int Left { get; internal set; }
		public int Top { get; internal set; }
		public int Width { get; internal set; }
		public int Height { get; internal set; }
		public int WorkingWidth { get; internal set; }
		public int WorkingHeight { get; internal set; }
		public bool IsPrimary { get; internal set; }
		public float DpiX { get; internal set; }
		public float DpiY { get; internal set; }

		// Initializes empty display information.
		// 빈 디스플레이 정보를 초기화한다.
		public DHSysInfoDisplayData() {
			DeviceName = "";
		}
	}
}
