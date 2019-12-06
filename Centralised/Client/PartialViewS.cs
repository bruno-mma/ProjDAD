using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Interfaces;
using System.IO;
using System.Timers;

namespace Client
{
	class PartialViewS : MarshalByRefObject, IPartialViewS
	{
		private Dictionary<string, IPartialViewC> _view = new Dictionary<string, IPartialViewC>();
		Timer timer = new Timer(5000);

		public void Main(string[] args)
		{
			int port = 9090;
			bool portAvailable = false;

			while (!portAvailable)
			{
				try
				{
					TcpChannel channel = new TcpChannel(port);
					ChannelServices.RegisterChannel(channel, false);
					RemotingConfiguration.RegisterWellKnownServiceType(typeof(IPartialViewS), "ParltialViewS", WellKnownObjectMode.Singleton);
					RemotingServices.Marshal(this, "PartialViewS", typeof(IPartialViewS));

					portAvailable = !portAvailable;
				}
				catch (System.Net.Sockets.SocketException)
				{
					port++;
				}
			}
		}

		public PartialViewS ()
		{
			PopulateView();

			timer.AutoReset = true;
			timer.Elapsed += new ElapsedEventHandler(UpdateView);
			timer.Start();
		}

		private void PopulateView ()
		{
			string[] existingClients = File.ReadAllLines(@"..\..\..\" + "clientURLs.txt");
			int numberOfClients = existingClients.Length;

			// Add peers to partial view
			if (numberOfClients <= 3)
			{
				foreach (string clientURL in existingClients)
				{
					IPartialViewC peer = (IPartialViewC)Activator.GetObject(typeof(IPartialViewC), clientURL);
					_view.Add(clientURL, peer);
				}
			}

			else
			{
				Random random = new Random();

				for (int i = 0; i < 2; i++)
				{
					string choosenPeer = existingClients[random.Next(0, numberOfClients)];
					IPartialViewC peer = (IPartialViewC)Activator.GetObject(typeof(IPartialViewC), choosenPeer);
					_view.Add(choosenPeer, peer);
				}
			}
		}

		private void UpdateView(object sender, ElapsedEventArgs e)
		{
			PopulateView();
		}

		public void DisseminateMeeting(MeetingData meeting)
		{
			foreach (IPartialViewC peer in _view.Values)
			{
				peer.SendMessage(meeting);
			}
		}
	}
}
