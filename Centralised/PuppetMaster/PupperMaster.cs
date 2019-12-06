using System;
using System.Collections.Generic;
using System.IO;
using Interfaces;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.Sockets;

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

				Console.WriteLine("Finished config file execution");
			}

			catch (FileNotFoundException)
			{
				Console.WriteLine("No configuration file provided");
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
		private static Mutex mut = new Mutex();


		public readonly int _port = 10001;

		//key is IP
		private Dictionary<string, IPCS> _PCSs = new Dictionary<string, IPCS>();

		//key is server id
		private Dictionary<string, IServer> _servers = new Dictionary<string, IServer>();

		//used to pass existing servers, as argument, to new  servers
		private static string _allServersURL ="";

		//Room locations, key is location name
		private Dictionary<string, Location> _locations = new Dictionary<string, Location>();

		private readonly string clientURLsPath = @"..\..\..\" + "clientURLs.txt";
		private readonly string serverURLsPath = @"..\..\..\" + "serverURLs.txt";

		public PuppetMaster()
		{
			string path = @"..\..\..\" + @"\PCS\bin\Debug\Pcs.exe";
			Console.WriteLine("Starting Pcs at localhost");
			Process.Start(path);

			//check if server or clien URL files exist, if so delete them
			if (File.Exists(clientURLsPath))
			{
				File.Delete(clientURLsPath);
			}

			if (File.Exists(serverURLsPath))
			{
				File.Delete(serverURLsPath);
			}
		}

		// Delegates for async PuppetMaster operations
		public delegate string RemoteAsyncStartServerDelegate(string id, string server_URL, int max_faults, int min_delay, int max_delay);

		public delegate void RemoteAsyncStartClientDelegate(string name, string user_URL, string server_URL, string script_file);

		public delegate string RemoteAsyncFreezeDelegate(string server_id);

		public delegate string RemoteAsyncStatusDelegate();


		// Async
		public void StartClient(string name, string user_URL, string server_URL, string script_file)
		{
			GetPCS(server_URL).StartClient(name, user_URL, server_URL, script_file);
		}

		// Async
		public string StartServer(string id, string server_URL, int max_faults, int min_delay, int max_delay)
		{
			mut.WaitOne();
			Console.WriteLine("start mutex");
			Console.WriteLine("_allServersURL: "+_allServersURL);
			Console.WriteLine("Server ID: "+ id);

			if (_allServersURL.Length > 0)
			{
				GetPCS(server_URL).StartServer(id, server_URL, max_faults, min_delay, max_delay, _allServersURL);
			}
			else
			{
				GetPCS(server_URL).StartServer(id, server_URL, max_faults, min_delay, max_delay);
			}

			IServer server = (IServer)Activator.GetObject(typeof(IServer), server_URL);

			//weak check
			if (server == null)
			{
				return "PuppetMaster: Failed to connect to server at " + server_URL;
			}
			
			_servers[id] = server;
			server.SetRooms(_locations);

			_allServersURL += id;
			_allServersURL += ",";
			_allServersURL += server_URL;
			_allServersURL += ";";
			Console.WriteLine("end mutex");
			mut.ReleaseMutex();
			return "PuppetMaster: Connected to server at " + server_URL;
		}


		public void AddRoom(string location, string name, int capacity)
		{
			if (!_locations.ContainsKey(location))
			{
				_locations[location] = new Location(location);
			}

			_locations[location]._rooms.Add(name, new Room(location, name, capacity));
		}

		private IPCS GetPCS(string server_URL)
		{
			string pcs_ip = URL.GetIP(server_URL);

			IPCS pcs = (IPCS)Activator.GetObject(typeof(IPCS), "tcp://"+ pcs_ip + ":10000/PCS");

			try
			{
				pcs.LifeCheck();
			}
			catch (System.Net.Sockets.SocketException)
			{
				Console.WriteLine("MIssing PCS at IP: " + pcs_ip);
				return null;
			}
			
			try
			{
				_PCSs.Add(pcs_ip, pcs);
				Console.WriteLine("New PCS connection: " + "tcp://" + pcs_ip + ":10000/PCS");
				return pcs;
			}
			catch(System.ArgumentException)
			{
				return _PCSs[pcs_ip];
			}		
		}

		public string FreezeServer(string server_id)
		{
			_servers[server_id].Freeze();

			return "Freezing server " + server_id;
		}

		public string UnfreezeServer(string server_id)
		{
			_servers[server_id].Unfreeze();

			return "Unfreezing server " + server_id;
		}

		public string PrintStatus()
		{
			//TODO: servers could also print their own view of the system.

			string print = "Status:" + Environment.NewLine;

			foreach (KeyValuePair<string, IServer> kvp in _servers)
			{
				print += "Server: " + kvp.Key + " -> ";

				if (kvp.Value.IsFrozen())
				{
					print += "Frozen";
				}
				else
				{
					print += "Running";
				}

				print += Environment.NewLine;
			}

			return print;
		}

		public static void StartServerCallBack(IAsyncResult result)
		{
			//Use the callback to get the return value
			RemoteAsyncStartServerDelegate del = (RemoteAsyncStartServerDelegate)((AsyncResult)result).AsyncDelegate;

			Console.WriteLine(del.EndInvoke(result));
		}

		public static void FreezeCallBack(IAsyncResult result)
		{
			//Use the callback to get the return value
			RemoteAsyncFreezeDelegate del = (RemoteAsyncFreezeDelegate)((AsyncResult)result).AsyncDelegate;

			Console.WriteLine(del.EndInvoke(result));
		}

		public static void StatusCallBack(IAsyncResult result)
		{
			//Use the callback to get the return value
			RemoteAsyncStatusDelegate del = (RemoteAsyncStatusDelegate)((AsyncResult)result).AsyncDelegate;

			Console.WriteLine(del.EndInvoke(result));
		}

		public void CrashServer(string server_id)
		{
			Console.WriteLine("Crashing server " + server_id);

			try
			{
			  _servers[server_id].Crash();
			}
			//ignoring the exception raised from closing the connection abruptly
			catch (System.Net.Sockets.SocketException) { }
		}

	}
}
