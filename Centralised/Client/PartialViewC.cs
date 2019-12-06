using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace Client
{
    public class PartialViewC : MarshalByRefObject, IPartialViewC
    {
		private readonly PartialViewS _partialViewS = new PartialViewS();
		private Client _client;
		private string _clientURL;
		
		public void Main (string[] args)
		{
			int port = 9099;
			bool portAvailable = false;

			while (!portAvailable)
			{
				try
				{
					TcpChannel channel = new TcpChannel(port);
					ChannelServices.RegisterChannel(channel, false);
					RemotingServices.Marshal(this, "PartialViewC", typeof(IPartialViewC));
				}
				catch (System.Net.Sockets.SocketException)
				{
					port--;
				}
			}
		}

        public PartialViewC(Client client, string clientURL)
        {
			_client = client;
			_clientURL = clientURL;
        }

        public void SendMessage (MeetingData meeting)
        {
			_client.UpdateMeeting(meeting._meetingTopic, meeting);
			_partialViewS.DisseminateMeeting(meeting);
		}

        public void ReceiveMessage ()
        {

        }
    }
}
