using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Web;

namespace Sudoku
{
    public class Player : IEquatable<Player>, IEquatable<string>
    {
        public string Name { get; set; }

        public bool IsAvailable {
            get {
                try
                {
                    if (WebSocket == null || WebSocket.State != WebSocketState.Open)
                    {
                        return false;
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }                
            }
        }

        private WebSocket WebSocket { get; }

        public Player(string name, WebSocket webSocket)
        {
            Name = name;
            WebSocket = webSocket;
        }

        public void Send(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            ArraySegment<byte> buffer = new ArraySegment<byte>(bytes);
            
            if (WebSocket != null && WebSocket.State == WebSocketState.Open)
            {
                WebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public bool Equals(Player other)
        {
            if (other == null)
                return false;

            if (other.Name != this.Name)
                return false;

            return true;
        }

        public bool Equals(string other)
        {
            if (other == null || other == "")
                return false;

            if (other != this.Name)
                return false;

            return true;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}