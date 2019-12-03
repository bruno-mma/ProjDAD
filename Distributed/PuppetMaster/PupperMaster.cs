using System;
using System.Collections.Generic;
using System.IO;
using Interfaces;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace PuppetMaster
{

	class Program
	{
	
		static void Main()
		{

			PuppetMaster puppetMaster = new PuppetMaster();
			PuppetMasterParser puppetMasterParser = new PuppetMasterParser(puppetMaster);

			TcpChannel channel = new TcpChannel(puppetMaster._port);
			ChannelServices.RegisterChannel(channel, false);

			try
			{
				//file has to be inside solution folder
				string[] configFileContents = File.ReadAllLines(Path.Combine(@"..\..\..\", "configFile.txt"));

				Console.WriteLine("Config file found. Executing...");

				foreach (string line in configFileContents)
				{
					Console.WriteLine("Running Command: " + line);
					puppetMasterParser.ParseExecute(line);
					Console.WriteLine();
				}

				Console.Write("Finished config file execution. ");
			}

			catch (FileNotFoundException)
			{
				Console.Write("No configuration file provided. ");
			}

			Console.WriteLine("Reading commands from console");

			while (true)
			{
				puppetMasterParser.ParseExecute(Console.ReadLine());
				Console.WriteLine();
			}
		}
	}

	public class PuppetMaster : MarshalByRefObject
	{
		public readonly int _port = 10001;

		//key is IP
		private Dictionary<string, IPCS> _PCSs = new Dictionary<string, IPCS>();

		//key is server id
		private Dictionary<string, IServer> _servers = new Dictionary<string, IServer>();

		//Room locations, key is location name
		private Dictionary<string, Location> _locations = new Dictionary<string, Location>();

		public PuppetMaster()
		{
			_PCSs.Add("localhost", new PCS.PCS());
		}

		//TODO: All PuppetMaster commands should be executed asynchronously except for the Wait command.

		public void StartClient(string name, string user_URL, string server_URL, string script_file)
		{
			string ip = URL.GetIP(user_URL);

			_PCSs[ip].StartClient(name, user_URL, server_URL, script_file);
		}

		public void StartServer(string id, string server_URL, int max_faults, int min_delay, int max_delay)
		{
			string ip = URL.GetIP(server_URL);

			_PCSs[ip].StartServer(id, server_URL, max_faults, min_delay, max_delay);

			IServer server = (IServer)Activator.GetObject(typeof(IServer), server_URL);

			//weak check
			if (server == null)
			{
				Console.WriteLine("Failed to connect to server at " + server_URL);
			}
			else
			{
				Console.WriteLine("Connected to server at " + server_URL);
			}

			_servers[id] = server;
			server.SetRooms(_locations);
		}

		public void AddRoom(string location, string name, int capacity)
		{
			if (!_locations.ContainsKey(location))
			{
				_locations[location] = new Location(location);
			}

			_locations[location]._rooms.Add(name, new Room(location, name, capacity));
		}

		public void AddServer(string server_id, IServer server)
		{
			_servers.Add(server_id, server);

			server.SetRooms(_locations);
		}

		public void AddPCS(string ip)
		{
			string pcs_URL = "tcp://" + ip + ":10000/PCS";

			IPCS pcs = (IPCS)Activator.GetObject(typeof(IPCS), pcs_URL);

			//weak check
			if (pcs == null)
			{
				Console.WriteLine("Failed to connect to PCS at " + pcs_URL);
			}
			else
			{
				Console.WriteLine("Connected to PCS at " + pcs_URL);
			}

			_PCSs.Add(ip, pcs);
		}

		public void FreezeServer(string server_id)
		{
			Console.WriteLine("Freezing server " + server_id);

			_servers[server_id].Freeze();
		}

		public void UnfreezeServer(string server_id)
		{
			Console.WriteLine("Unfreezing server " + server_id);

			_servers[server_id].Unfreeze();
		}

		public void PrintStatus()
		{
			//TODO: servers could also print their own view of the system.

			Console.WriteLine("Status:");

			foreach (KeyValuePair<string, IServer> kvp in _servers)
			{
				Console.Write("Server: " + kvp.Key + " -> ");
				if (kvp.Value.IsFrozen())
				{
					Console.WriteLine("Frozen");
				}

				else
				{
					Console.WriteLine("Running");
				}
			}
		}
	}
}
