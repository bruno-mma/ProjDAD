using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public class Program
	{
		static void Main(string[] args)
		{
			Server server = new Server();
		}
	}


	public class Server
	{
		//key is meeting topic
		private Dictionary<string, Meeting> _meetings = new Dictionary<string, Meeting>();
	}
}
