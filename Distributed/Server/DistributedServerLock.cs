using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
	public class DistributedServerLock
	{
		//server where this instance of the lock is
		private IServer _server;
		public Dictionary<string, IServer> _servers;

		private readonly Random _rdn = new Random();

		//if this server has the lock
		public bool _locked = false;


		public DistributedServerLock(IServer server, Dictionary<string, IServer> servers)
		{
			_server = server;
			_servers = servers;
		}


		public bool TryAcquireLock(DateTime operation_start)
		{
			//adquire the lock locally
			lock (this)
			{
				if (_locked)
				{
					return false;
				}

				//try to lock other servers
				foreach (IServer server in _servers.Values)
				{
					//if we could not adquire the lock
					if (!server.AcquireRemoteLock(operation_start))
					{
						return false;
					}
				}

				_locked = true;

				//got the lock
				return true;
			}
		}

		public void AcquireLock()
		{
			DateTime operation_start = DateTime.Now;

			while (!TryAcquireLock(operation_start))
			{
				//Console.WriteLine("waiting for the lock");
				Thread.Sleep(_rdn.Next(10, 200));
			}

			Console.WriteLine("GOT THE LOCK");
		}

		public void ReleaseLock()
		{
			lock (this)
			{
				foreach (IServer server in _servers.Values)
				{
					server.ReleaseRemoteLock();
				}

				_locked = false;

				Console.WriteLine("RELEASED THE LOCK");

				Thread.Sleep(500);
			}
		}

		
	}
}
