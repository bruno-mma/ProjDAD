﻿using System;
using System.Collections.Generic;
using System.IO;
using Interfaces;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

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
		public readonly int _port = 10001;

		//key is IP
		private Dictionary<string, IPCS> _PCSs = new Dictionary<string, IPCS>();

		//key is server id
		private Dictionary<string, IServer> _servers = new Dictionary<string, IServer>();

		//Room locations, key is location name
		private Dictionary<string, Location> _locations = new Dictionary<string, Location>();

		private readonly string clientURLsPath = @"..\..\..\" + "clientURLs.txt";
		private readonly string serverURLsPath = @"..\..\..\" + "serverURLs.txt";

		public PuppetMaster()
		{
			_PCSs.Add("localhost", new PCS.PCS());

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
			GetPCS(server_URL).StartServer(id, server_URL, max_faults, min_delay, max_delay);

			IServer server = (IServer)Activator.GetObject(typeof(IServer), server_URL);

			//weak check
			if (server == null)
			{
				return "PuppetMaster: Failed to connect to server at " + server_URL;
			}
			
			_servers[id] = server;
			server.SetRooms(_locations);

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

		private IPCS GetPCS(string pcs_URL)
		{
			string ip = URL.GetIP(pcs_URL);

			if (_PCSs.ContainsKey(ip))
			{
				return _PCSs[ip];
			}

			IPCS pcs = (IPCS)Activator.GetObject(typeof(IPCS), pcs_URL);

			//weak check
			if (pcs == null)
			{
				Console.WriteLine("Failed to connect to PCS at " + pcs_URL);
				return null;
			}

			Console.WriteLine("Connected to PCS at " + pcs_URL);

			_PCSs.Add(ip, pcs);

			return pcs;
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
