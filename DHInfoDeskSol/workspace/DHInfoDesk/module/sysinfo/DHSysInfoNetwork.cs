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

		public IList<DHSysInfoNetworkData> CollectStatic() {
			return Collect(false);
		}

		public IList<DHSysInfoNetworkData> CollectDynamic() {
			return Collect(true);
		}

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
