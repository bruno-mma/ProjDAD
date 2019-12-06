using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
	// DONT TOUCH THIS CODE !!!!!!!!!!!!!!!
	public class DistributedServerLock
	{
		//server where this instance of the lock is
		private IServer _server;
		public Dictionary<string, IServer> _servers;
		public HashSet<string> _offlineServers;

		private readonly Random _rdn = new Random();

		//if this server has the lock
		public bool _locked = false;


		public DistributedServerLock(IServer server, Dictionary<string, IServer> servers, HashSet<string> offlineServers)
		{
			_server = server;
			_servers = servers;
			_offlineServers = offlineServers;
		}


		public bool TryAcquireLock()
		{
			//adquire the lock locally
			lock (this)
			{
				if (_locked)
				{
					return false;
				}

				List<IServer> servers = new List<IServer>(_servers.Values);
				List<string> URLs = new List<string>(_servers.Keys);

				//try to lock other servers
				for (int i = 0; i < _servers.Count; i++)
				{
					IServer server = servers[i];

					try
					{
						//if we could not adquire the lock
						if (!server.AcquireRemoteLock())
						{
							return false;
						}
					}

					//if server is down
					catch (System.Net.Sockets.SocketException)
					{
						string URL = URLs[i];

						//remove it
						_servers.Remove(URL);
						_offlineServers.Add(URL);

						Console.WriteLine("Server at " + URL + " is down");
					}
				}

				//got the lock
				_locked = true;

				return true;
			}
		}

		public void AcquireLock()
		{
			while (!TryAcquireLock())
			{
				//Console.WriteLine("waiting for the lock");
				Thread.Sleep(_rdn.Next(0, 2000));
			}

			Console.WriteLine("ACQUIRED THE LOCK");
		}

		public void ReleaseLock()
		{
			lock (this)
			{
				List<IServer> servers = new List<IServer>(_servers.Values);
				List<string> URLs = new List<string>(_servers.Keys);

				for (int i = 0; i < _servers.Count; i++)
				{
					IServer server = servers[i];
					try
					{
						server.ReleaseRemoteLock();
					}

					//if server is down
					catch (System.Net.Sockets.SocketException)
					{
						string URL = URLs[i];

						//remove it, and continue
						_servers.Remove(URL);
						_offlineServers.Add(URL);

						Console.WriteLine("Server at " + URL + " is down");
					}
				}

				_locked = false;

				Console.WriteLine("RELEASED THE LOCK");

				Thread.Sleep(2000);
			}
		}
	}
}
