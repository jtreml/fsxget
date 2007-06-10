using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Fsxget
{
	class Md5Worker
	{
		static MD5 md5Hasher = MD5.Create();

		public static string getMd5Hash(string input)
		{
			byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

			StringBuilder sBuilder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}

			return sBuilder.ToString();
		}
	}
}
