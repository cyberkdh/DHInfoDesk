//////////////////////////////////////////////////////////////////////////////////////////////////
//	Projects		: DHInfoDesk
//	Author			: CYBERKDH(cyberkdh@hotmail.com, cyberkdh@gmail.com), AI(Codex)
//	Module			: DHInfoDesk.module.sysinfo
//	History			:
//	Copyrights		: Copyright ⓒCYBERKDH. All Rights Reserved.
//////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace DHInfoDesk.module.sysinfo {
	public sealed class DHSysInfoManager : IDisposable {
		private readonly DHSysInfoSystem m_system = new DHSysInfoSystem();
		private readonly DHSysInfoCpu m_cpu = new DHSysInfoCpu();
		private readonly DHSysInfoMemory m_memory = new DHSysInfoMemory();
		private readonly DHSysInfoStorage m_storage = new DHSysInfoStorage();
		private readonly DHSysInfoNetwork m_network = new DHSysInfoNetwork();
		private readonly DHSysInfoDisplay m_display = new DHSysInfoDisplay();

		private DHSysInfoSnapshot m_snapshot = new DHSysInfoSnapshot();
		private bool m_bDisposed = false;

		public DHSysInfoSnapshot Snapshot {
			get { return m_snapshot; }
		}

		public DHSysInfoSnapshot CollectAll() {
			CollectStatic();
			CollectDynamic();
			return m_snapshot;
		}

		public DHSysInfoSnapshot CollectStatic() {
			ThrowIfDisposed();

			DateTime collectedAt = DateTime.Now;

			try {
				DHSysInfoSystemData data = m_system.CollectStatic();
				m_snapshot.System = data;
				if (string.IsNullOrEmpty(data.OsName) == true) {
					m_snapshot.Status.System.SetPartial(collectedAt, "Operating system name is unavailable.");
				}
				else {
					m_snapshot.Status.System.SetSuccess(collectedAt);
				}
			}
			catch (Exception ex) {
				m_snapshot.Status.System.SetFailure(collectedAt, ex);
			}

			try {
				DHSysInfoCpuData data = m_cpu.CollectStatic();
				m_snapshot.Cpu = data;
				if (string.IsNullOrEmpty(data.Name) == true || data.PhysicalCoreCount <= 0 ||
					data.LogicalProcessorCount <= 0) {
					m_snapshot.Status.Cpu.SetPartial(collectedAt, "One or more CPU properties are unavailable.");
				}
				else {
					m_snapshot.Status.Cpu.SetSuccess(collectedAt);
				}
			}
			catch (Exception ex) {
				m_snapshot.Status.Cpu.SetFailure(collectedAt, ex);
			}

			try {
				m_snapshot.Storage = m_storage.Collect();
				if (m_snapshot.Storage.Count == 0) {
					m_snapshot.Status.Storage.SetPartial(collectedAt, "No storage devices were collected.");
				}
				else {
					m_snapshot.Status.Storage.SetSuccess(collectedAt);
				}
			}
			catch (Exception ex) {
				m_snapshot.Status.Storage.SetFailure(collectedAt, ex);
			}

			try {
				m_snapshot.Network = m_network.CollectStatic();
				if (m_snapshot.Network.Count == 0) {
					m_snapshot.Status.Network.SetPartial(collectedAt, "No network adapters were collected.");
				}
				else {
					m_snapshot.Status.Network.SetSuccess(collectedAt);
				}
			}
			catch (Exception ex) {
				m_snapshot.Status.Network.SetFailure(collectedAt, ex);
			}

			try {
				m_snapshot.Display = m_display.Collect();
				if (m_snapshot.Display.Count == 0) {
					m_snapshot.Status.Display.SetPartial(collectedAt, "No displays were collected.");
				}
				else {
					m_snapshot.Status.Display.SetSuccess(collectedAt);
				}
			}
			catch (Exception ex) {
				m_snapshot.Status.Display.SetFailure(collectedAt, ex);
			}

			m_snapshot.CollectedAt = collectedAt;

			return m_snapshot;
		}

		public DHSysInfoSnapshot CollectDynamic() {
			ThrowIfDisposed();

			DateTime collectedAt = DateTime.Now;

			try {
				m_system.UpdateDynamic(m_snapshot.System);
				if (string.IsNullOrEmpty(m_snapshot.System.OsName) == true) {
					m_snapshot.Status.System.SetPartial(collectedAt, "Operating system name is unavailable.");
				}
				else {
					m_snapshot.Status.System.SetSuccess(collectedAt);
				}
			}
			catch (Exception ex) {
				m_snapshot.Status.System.SetFailure(collectedAt, ex);
			}

			try {
				m_cpu.UpdateDynamic(m_snapshot.Cpu);
				if (string.IsNullOrEmpty(m_snapshot.Cpu.Name) == true ||
					m_snapshot.Cpu.PhysicalCoreCount <= 0 ||
					m_snapshot.Cpu.LogicalProcessorCount <= 0) {
					m_snapshot.Status.Cpu.SetPartial(collectedAt, "One or more CPU properties are unavailable.");
				}
				else {
					m_snapshot.Status.Cpu.SetSuccess(collectedAt);
				}
			}
			catch (Exception ex) {
				m_snapshot.Status.Cpu.SetFailure(collectedAt, ex);
			}

			try {
				DHSysInfoMemoryData data = m_memory.Collect();
				m_snapshot.Memory = data;
				if (data.TotalBytes == 0) {
					m_snapshot.Status.Memory.SetPartial(collectedAt, "Physical memory information is unavailable.");
				}
				else {
					m_snapshot.Status.Memory.SetSuccess(collectedAt);
				}
			}
			catch (Exception ex) {
				m_snapshot.Status.Memory.SetFailure(collectedAt, ex);
			}

			try {
				m_snapshot.Storage = m_storage.Collect();
				if (m_snapshot.Storage.Count == 0) {
					m_snapshot.Status.Storage.SetPartial(collectedAt, "No storage devices were collected.");
				}
				else {
					m_snapshot.Status.Storage.SetSuccess(collectedAt);
				}
			}
			catch (Exception ex) {
				m_snapshot.Status.Storage.SetFailure(collectedAt, ex);
			}

			try {
				m_snapshot.Network = m_network.CollectDynamic();
				if (m_snapshot.Network.Count == 0) {
					m_snapshot.Status.Network.SetPartial(collectedAt, "No network adapters were collected.");
				}
				else {
					m_snapshot.Status.Network.SetSuccess(collectedAt);
				}
			}
			catch (Exception ex) {
				m_snapshot.Status.Network.SetFailure(collectedAt, ex);
			}

			m_snapshot.CollectedAt = collectedAt;

			return m_snapshot;
		}

		private void ThrowIfDisposed() {
			if (m_bDisposed == true) {
				throw new ObjectDisposedException(GetType().FullName);
			}
		}

		public void Dispose() {
			if (m_bDisposed == true) {
				return;
			}

			m_cpu.Dispose();
			m_bDisposed = true;
		}
	}
}
