using System;
using System.Collections.Generic;
using System.Text;
using System.Management;

namespace HardwareInfo
{
	public class HardwareInfo
	{
		public class Hdd
		{
			public struct Drive
			{
				public String Serial;
			}

			public Hdd(String Model)
			{
				this.Model = Model;
				Drives = new List<Drive>(2);
			}

			public String Model;
			public List<Drive> Drives;
		}

		public static List<String> CpuIds
		{
			get
			{
				List<String> listCpuIds = new List<String>(2);

				ManagementClass mcThis = new ManagementClass("Win32_Processor");
				ManagementObjectCollection mocThis = mcThis.GetInstances();

				foreach (ManagementObject moLoop in mocThis)
				{
					listCpuIds.Add(moLoop.Properties["ProcessorId"].Value.ToString());
				}

				return listCpuIds;
			}
		}

		public static List<Hdd> HddList
		{
			get
			{
				List<Hdd> listHdds = new List<Hdd>(2);

				foreach (ManagementObject drive in new ManagementObjectSearcher("select * from Win32_DiskDrive where InterfaceType='IDE'").Get())
				{
					Hdd hddCurrent;
					try
					{
						hddCurrent = new Hdd(drive["Model"].ToString());
					}
					catch
					{
						continue;
					}

					foreach (ManagementObject partition in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + drive["DeviceID"] + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition").Get())
					{
						foreach (ManagementObject disk in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE AssocClass = Win32_LogicalDiskToPartition").Get())
						{
							Hdd.Drive drvCurrent;
							try
							{
								drvCurrent.Serial = disk["VolumeSerialNumber"].ToString();
							}
							catch
							{
								continue;
							}

							hddCurrent.Drives.Add(drvCurrent);
						}
					}

					listHdds.Add(hddCurrent);
				}

				return listHdds;
			}
		}
	}
}
