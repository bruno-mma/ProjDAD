using Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace PCS
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("PCS started");
			//System.Threading.Thread.Sleep(15000);
			PCS pcs = new PCS();

			TcpChannel channel = new TcpChannel(PCS._port);
			ChannelServices.RegisterChannel(channel, false);

			RemotingServices.Marshal(pcs, "PCS", typeof(IPCS));
			Console.WriteLine("PCS finished starting process");

			Console.ReadLine();
		}
	}

	public class PCS : MarshalByRefObject, IPCS
	{
		public static readonly int _port = 10000;

		public bool LifeCheck()
		{
			return true;
		}

		public void StartClient(string name, string user_URL, string server_URL, string script_file)
		{
			string path = @"..\..\..\" + @"\Client\bin\Debug\Client.exe";
			Console.WriteLine("PCS: Starting client at " + user_URL + ", will connect with server at: " + server_URL);
			Process.Start(path, name + " " + user_URL + " " + server_URL + " " + script_file);
		}

		public void StartServer(string server_id, string server_URL, int max_faults, int min_delay, int max_delay, string allServersURL)
		{
			string path = @"..\..\..\" + @"\Server\bin\Debug\Server.exe";
			Console.WriteLine("PCS: Starting server at " + server_URL);
			Process.Start(path, server_id + " " + server_URL + " " + max_faults + " " + min_delay + " " + max_delay + " " + allServersURL);
		}
		public void StartServer(string server_id, string server_URL, int max_faults, int min_delay, int max_delay)
		{
			string path = @"..\..\..\" + @"\Server\bin\Debug\Server.exe";
			Console.WriteLine("PCS: Starting server at " + server_URL);
			Process.Start(path, server_id + " " + server_URL + " " + max_faults + " " + min_delay + " " + max_delay);
		}
	}
}
