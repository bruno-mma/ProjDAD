using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
	public static class URL
	{
		public static int GetPort(string URL)
		{
			// URL -> "tcp://localhost:3000/server1"
			int begin = URL.LastIndexOf(':') + 1;
			int end = URL.LastIndexOf('/') - 1;

			// 3000
			return Int32.Parse(URL.Substring(begin, end - begin + 1));
		}

		public static string GetIP(string URL)
		{
			// URL -> "tcp://localhost:3000/server1"
			int begin = URL.IndexOf(':') + 3;
			int end = URL.LastIndexOf(':') - 1;


			// "localhost"
			return URL.Substring(begin, end - begin + 1);
		}

		public static string GetURI(string URL)
		{
			// URL -> "tcp://localhost:3000/server1"
			int begin = URL.LastIndexOf('/') + 1;
			int end = URL.Length - 1;

			// "server1"
			return URL.Substring(begin, end - begin + 1);
		}
	}
}
