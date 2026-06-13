//////////////////////////////////////////////////////////////////////////////////////////////////
//	Projects		: DHInfoDesk
//	Author			: CYBERKDH(cyberkdh@hotmail.com, cyberkdh@gmail.com), AI(Codex)
//	Module			: DHInfoDesk.module.sysinfo
//	History			:
//	Copyrights		: Copyright ⓒCYBERKDH. All Rights Reserved.
//////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DHInfoDesk.module.sysinfo {
	internal sealed class DHSysInfoNetwork {
		private sealed class NetworkSample {
			public long BytesReceived;
			public long BytesSent;
			public DateTime CollectedAt;
		}

		private readonly Dictionary<string, NetworkSample> m_mapSample =
			new Dictionary<string, NetworkSample>(StringComparer.OrdinalIgnoreCase);

		// Collects network adapter properties without transfer rates.
		// 전송 속도를 제외한 네트워크 어댑터 속성을 수집한다.
		public IList<DHSysInfoNetworkData> CollectStatic() {
			return Collect(false);
		}

		// Collects network adapter properties and transfer rates.
		// 네트워크 어댑터 속성과 전송 속도를 수집한다.
		public IList<DHSysInfoNetworkData> CollectDynamic() {
			return Collect(true);
		}

		// Collects network adapter data with optional rate calculation.
		// 선택적으로 속도를 계산하며 네트워크 어댑터 데이터를 수집한다.
		private IList<DHSysInfoNetworkData> Collect(bool bcomputerate) {
			List<DHSysInfoNetworkData> result = new List<DHSysInfoNetworkData>();
			NetworkInterface[] adapters;

			try {
				adapters = NetworkInterface.GetAllNetworkInterfaces();
			}
			catch {
				return result;
			}

			DateTime now = DateTime.UtcNow;
			int i;
			for (i = 0; i < adapters.Length; i++) {
				NetworkInterface adapter = adapters[i];
				DHSysInfoNetworkData data = new DHSysInfoNetworkData();

				data.Id = adapter.Id;
				data.Name = adapter.Name;
				data.Description = adapter.Description;
				data.Status = adapter.OperationalStatus.ToString();
				data.MacAddress = FormatMacAddress(adapter.GetPhysicalAddress());
				data.LinkSpeedBitsPerSecond = adapter.Speed;

				try {
					IPInterfaceProperties properties = adapter.GetIPProperties();
					foreach (UnicastIPAddressInformation address in properties.UnicastAddresses) {
						if (address.Address.AddressFamily == AddressFamily.InterNetwork ||
							address.Address.AddressFamily == AddressFamily.InterNetworkV6) {
							data.IpAddresses.Add(address.Address.ToString());
						}
					}
				}
				catch {
				}

				try {
					IPv4InterfaceStatistics statistics = adapter.GetIPv4Statistics();
					data.BytesReceived = statistics.BytesReceived;
					data.BytesSent = statistics.BytesSent;
					UpdateRate(data, now, bcomputerate);
				}
				catch {
				}

				result.Add(data);
			}

			RemoveMissingSamples(result);
			return result;
		}

		// Calculates transfer rates from the previous adapter sample.
		// 이전 어댑터 샘플을 기준으로 전송 속도를 계산한다.
		private void UpdateRate(DHSysInfoNetworkData data, DateTime now, bool bcomputerate) {
			NetworkSample previous;
			if (bcomputerate == true && m_mapSample.TryGetValue(data.Id, out previous) == true) {
				double seconds = (now - previous.CollectedAt).TotalSeconds;
				if (seconds > 0.0) {
					data.ReceiveBytesPerSecond = Math.Max(0,
						(long)((data.BytesReceived - previous.BytesReceived) / seconds));
					data.SendBytesPerSecond = Math.Max(0,
						(long)((data.BytesSent - previous.BytesSent) / seconds));
				}
			}

			m_mapSample[data.Id] = new NetworkSample {
				BytesReceived = data.BytesReceived,
				BytesSent = data.BytesSent,
				CollectedAt = now
			};
		}

		// Removes cached samples for adapters that no longer exist.
		// 더 이상 존재하지 않는 어댑터의 캐시 샘플을 제거한다.
		private void RemoveMissingSamples(IList<DHSysInfoNetworkData> result) {
			HashSet<string> active = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			int i;
			for (i = 0; i < result.Count; i++) {
				active.Add(result[i].Id);
			}

			List<string> remove = new List<string>();
			foreach (string id in m_mapSample.Keys) {
				if (active.Contains(id) == false) {
					remove.Add(id);
				}
			}

			for (i = 0; i < remove.Count; i++) {
				m_mapSample.Remove(remove[i]);
			}
		}

		// Formats a physical address using hexadecimal byte pairs.
		// 물리 주소를 16진수 바이트 쌍으로 변환한다.
		private string FormatMacAddress(PhysicalAddress address) {
			if (address == null) {
				return "";
			}

			byte[] bytes = address.GetAddressBytes();
			if (bytes.Length == 0) {
				return "";
			}

			return BitConverter.ToString(bytes);
		}
	}
}
