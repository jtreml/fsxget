using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HardwareInfo;

namespace Fsxget
{
	public static class UniqueIDs
	{
		public static String Machine
		{
			get
			{
				return getMachineId();
			}
		}

		public static String User
		{
			get
			{
				return getUserId();
			}
		}

		private static String getMachineId()
		{
			List<String> listCpuIDs = HardwareInfo.HardwareInfo.CpuIds;
			List<HardwareInfo.HardwareInfo.Hdd> listHDDs = HardwareInfo.HardwareInfo.HddList;

			String szId = Md5Worker.getMd5Hash("");

			foreach (String szTemp in listCpuIDs)
				szId = Xor(szId, Md5Worker.getMd5Hash(szTemp));

			foreach (HardwareInfo.HardwareInfo.Hdd hddTemp in listHDDs)
				foreach (HardwareInfo.HardwareInfo.Hdd.Drive drvTemp in hddTemp.Drives)
					szId = Xor(szId, Md5Worker.getMd5Hash(drvTemp.Serial));

			return Md5Worker.getMd5Hash(szId);
		}

		private static String getUserId()
		{
			String szId = Md5Worker.getMd5Hash("");
			
			szId = Xor(szId, Md5Worker.getMd5Hash(System.Environment.MachineName));
			szId = Xor(szId, Md5Worker.getMd5Hash(System.Environment.UserName));

			return Md5Worker.getMd5Hash(szId);
		}

		private static String Xor(String string1, String string2)
		{
			int len = Math.Max(string1.Length, string2.Length);

			string1 = string1.PadLeft(len);
			string2 = string2.PadLeft(len);

			char[] char1 = string1.ToCharArray();
			char[] char2 = string2.ToCharArray();

			if(char1.Length != char2.Length || char1.Length != len)
				throw new Exception("Method misconception exception!");

			char[] charNew = new char[len];

			for (int i = 0; i < len; i++)
				charNew[i] = (char)((int)char1[i] ^ (int)char2[i]);

			return new String(charNew);
		}
	}
}
