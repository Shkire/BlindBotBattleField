using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GameManagerActor.Sockets
{
    public class SocketClient
    {
        // The port number for the remote device.
        private const int lobbyPort = 11000;
        private const int gameSessionPort = 12000;

        public void StartLobbyClient(byte[] i_address, string i_message)
        {
            StartClient(i_address, i_message, lobbyPort);
        }

        public void StartGameSessionClient(byte[] i_address, string i_message)
        {
            StartClient(i_address, i_message, gameSessionPort);
        }

        public void StartClient(byte[] i_address, string i_message, int port)
        {
            // Connect to a remote device.
            try
            {
                ActorEventSource.Current.Message("Creating socket");
                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
                IPEndPoint remoteEP = new IPEndPoint(new IPAddress(i_address), port);
                ActorEventSource.Current.Message("IPAddress created");
                // Create a TCP/IP socket.
                Socket client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                ActorEventSource.Current.Message("Client created");
                client.Connect(remoteEP);
                ActorEventSource.Current.Message("Client connected");
                byte[] byteData = Encoding.ASCII.GetBytes(i_message);

                ActorEventSource.Current.Message("Sending: " + i_message);

                client.Send(byteData);

                ActorEventSource.Current.Message("SENT");

                // Release the socket.
                client.Shutdown(SocketShutdown.Both);
                client.Close();

            }
            catch (Exception e)
            {
                ActorEventSource.Current.Message(e.ToString());
            }
        }

    }
}
