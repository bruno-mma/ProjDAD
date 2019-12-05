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
    public class PartialView : MarshalByRefObject, IPartialView
    {
        private Client _client;
        private Dictionary<string, IPartialView> _view = new Dictionary<string, IPartialView>();

        public void Main(string[] args)
        {
            PartialView partialView = new PartialView();

            int port = 9090;
            bool portAvailable = false;

            while (!portAvailable)
            {
                try
                {
                    TcpChannel channel = new TcpChannel(port);
                    ChannelServices.RegisterChannel(channel, false);
                    RemotingServices.Marshal(partialView, "PartialView", typeof(IPartialView));
                    portAvailable = !portAvailable;
                }
                catch (System.Net.Sockets.SocketException)
                {
                    port++;
                }
            }
        }

        public PartialView()
        {
            string[] existingClients = File.ReadAllLines(Path.Combine(@"..\..\..\", "clientURLs.txt"));
            int numberOfClients = existingClients.Length;

            // Add peers to partial view
            if (numberOfClients <= 3)
            {
                foreach (string clientURL in existingClients)
                {
                    TcpChannel channel = new TcpChannel(9099);
                    ChannelServices.RegisterChannel(channel, false);
                    RemotingServices.Marshal(this, "PartialView", typeof(IPartialView));

                    IPartialView peer = (IPartialView) Activator.GetObject(typeof(IPartialView), clientURL);
                    _view.Add(clientURL, peer);
                }
            }

            else
            {
                Random random = new Random();

                for (int i = 0; i < 2; i++)
                {
                    TcpChannel channel = new TcpChannel(9099);
                    ChannelServices.RegisterChannel(channel, false);
                    RemotingServices.Marshal(this, "PartialView", typeof(IPartialView));

                    string choosenPeer = existingClients[random.Next(0, numberOfClients)];
                    IPartialView peer = (IPartialView) Activator.GetObject(typeof(IPartialView), choosenPeer);
                    _view.Add(choosenPeer, peer);
                }

            }
        }

        // check if exist new clients every X seconds
        private void UpdateView ()
        {

        }

        public void DisseminateMessage (string message)
        {
            foreach (IPartialView peer in _view.Values)
            {
                peer.SendMessage(message);
            }
        }

        public void SendMessage (string message)
        {
            DisseminateMessage(message);
        }

        public void ReceiveMessage ()
        {

        }
    }
}
