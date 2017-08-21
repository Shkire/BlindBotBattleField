using BasicClasses.Common;
using BasicClasses.GameManager;
using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientBasicClasses.Sockets
{

    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();

        public string methodMessage;

        public List<string> parameters;
    }

    public static class LobbyListener
    {
        public static int port = 11000;

        public static ClientGameManager p_client;

        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        /*
        public AsynchronousSocketListener()
        {
        }
        */

        public static void StartListening(ClientGameManager i_client)
        {
            p_client = i_client;
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            IPHostEntry ipHostInfo = //Dns.GetHostEntry(Dns.GetHostName());
                Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.

            Socket listener = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
            
            // Bind the socket to the local endpoint and listen for incoming connections.

            try
            {

                listener.Bind(localEndPoint);
                listener.Listen(100);
                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    listener.BeginAccept(
                        new AsyncCallback(AcceptData),
                        listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                listener.Close();
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void AcceptData(IAsyncResult ar)
        {
            try
            {
                // Signal the main thread to continue.
                allDone.Set();

                // Get the socket that handles the client request.
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadData), state);
            }
            catch (Exception e)
            { }
        }

        //READ DATA
        public static void ReadData(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                string readString = Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead);
                

                // There  might be more data, so store the data received so far.
                state.sb.Append(readString);

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    content = content.Replace("<EOF>", "");

                    List<string> deserialized = content.DeserializeObject<List<string>>();

                    state.methodMessage = deserialized[0];

                    deserialized.RemoveAt(0);

                    state.parameters = deserialized;


                    if (p_client.state.Equals(ClientState.Lobby))
                    {
                        if (state.methodMessage.Equals("GameLobbyInfoUpdate"))
                        {
                            p_client.GameLobbyInfoUpdate(state.parameters[0].DeserializeObject<List<string>>());
                        }
                        else if (state.methodMessage.Equals("GameStart"))
                        {
                            p_client.StartGame(state.parameters[0].DeserializeObject<List<DictionaryEntry<string, int[]>>>().ToDictionary());
                        }
                    }
                    /*
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();

                    StartListening(p_client);
                    */

                    //SAVE LEFT STRING??
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadData), state);
                }
            }
        }


        /*
        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }
        */


        /*
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        */
    }

    public static class GameSessionListener
    {
        public static int port = 12000;

        public static ClientGameManager p_client;

        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        /*
        public AsynchronousSocketListener()
        {
        }
        */

        public static void StartListening(ClientGameManager i_client)
        {
            p_client = i_client;
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            IPHostEntry ipHostInfo = //Dns.GetHostEntry(Dns.GetHostName());
                Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.

            Socket listener = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {

                listener.Bind(localEndPoint);
                listener.Listen(100);
                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    
                    listener.BeginAccept(
                        new AsyncCallback(AcceptData),
                        listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                listener.Close();
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void AcceptData(IAsyncResult ar)
        {
            try
            {
                // Signal the main thread to continue.
                allDone.Set();

                // Get the socket that handles the client request.
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadData), state);
            }
            catch (Exception e)
            { }
        }

        //READ DATA
        public static void ReadData(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                string readString = Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead);
                

                // There  might be more data, so store the data received so far.
                state.sb.Append(readString);

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    content = content.Replace("<EOF>", "");

                    List<string> deserialized = content.DeserializeObject<List<string>>();

                    state.methodMessage = deserialized[0];

                    deserialized.RemoveAt(0);

                    state.parameters = deserialized;
                    if (p_client.state.Equals(ClientState.Game))
                    {
                        if (state.methodMessage.Equals("PlayerKilled"))
                        {
                            p_client.PlayerKilled(state.parameters[0].DeserializeObject<string>(), state.parameters[1].DeserializeObject<string>(), state.parameters[2].DeserializeObject<int[]>(), state.parameters[3].DeserializeObject<DeathReason>());
                        }
                        else if (state.methodMessage.Equals("PlayerDead"))
                        {
                            p_client.PlayerDead(state.parameters[0].DeserializeObject<string>(), state.parameters[1].DeserializeObject<int[]>(), state.parameters[2].DeserializeObject<DeathReason>());
                        }
                        else if (state.methodMessage.Equals("BombHits"))
                        {
                            p_client.BombHits(state.parameters[0].DeserializeObject<List<int[]>>());
                        }
                        else if (state.methodMessage.Equals("RadarUsed"))
                        {
                            p_client.RadarUsed(state.parameters[0].DeserializeObject<int[]>());
                        }
                        else if (state.methodMessage.Equals("GameFinished"))
                        {
                            p_client.GameFinished(state.parameters[0].DeserializeObject<string>());
                        }
                        else if (state.methodMessage.Equals("TurretAiming"))
                        {
                            p_client.TurretAiming(state.parameters[0].DeserializeObject<int[]>());
                        }
                    }

                    /*
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();

                    StartListening(p_client);
                    */

                    //SAVE LEFT STRING??
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadData), state);
                }
            }
        }


        /*
        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }
        */


        /*
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        */
    }

}
