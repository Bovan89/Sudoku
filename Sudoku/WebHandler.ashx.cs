using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;

namespace Sudoku
{
    /// <summary>
    /// Сводное описание для WebHandler
    /// </summary>
    public class WebHandler : IHttpHandler
    {
        //Список игр
        public static SudokuGames SGames = new SudokuGames();

        // Список всех клиентов
        private static readonly List<WebSocket> Clients = new List<WebSocket>();

        // Блокировка для обеспечения потокабезопасности
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        public void ProcessRequest(HttpContext context)
        {
            //Если запрос является запросом веб сокета
            if (context.IsWebSocketRequest)
                context.AcceptWebSocketRequest(WebSocketRequest);
        }

        private async Task WebSocketRequest(AspNetWebSocketContext context)
        {
            // Получаем сокет клиента из контекста запроса
            var socket = context.WebSocket;

            while (true)
            {
                // Ожидаем данные от него
                var buffer = new ArraySegment<byte>(new byte[1024]);
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                string[] cmdItems = GetItems(buffer);

                //Команда серверу от клиента
                Command cmd = new Command(SGames, socket, cmdItems);
                string response = cmd.Execute();

                //Ответ сервера клиенту
                if (response != "")
                {                    
                    byte[] bytes = Encoding.UTF8.GetBytes(response);
                    ArraySegment<byte> responseBuffer = new ArraySegment<byte>(bytes);
                    await socket.SendAsync(responseBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        private string[] GetItems(ArraySegment<byte> buffer)
        {
            byte[] msgContents = new byte[buffer.Count];
            Array.Copy(buffer.Array, buffer.Offset, msgContents, 0, msgContents.Length);
            string message = Encoding.UTF8.GetString(msgContents);
            message = message.Substring(0, message.IndexOf('\0'));
            string[] cmdItems = message.Split('#');
            return cmdItems;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}