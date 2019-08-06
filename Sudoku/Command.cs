using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Web;

namespace Sudoku
{
    public class Command
    {
        private string[] Items;

        public SudokuGames Games { get; }
        public WebSocket WebSocket { get; }
        public string Action { get { if (Items.Length >= 1) return Items[0]; else return null; } }
        public string Name { get { if (Items.Length >= 2) return Items[1]; else return null; } }
        public string GameId { get { if (Items.Length >= 3) return Items[2]; else return null; } }
        public string X { get { if (Items.Length >= 4) return Items[3]; else return null; } }
        public string Y { get { if (Items.Length >= 5) return Items[4]; else return null; } }
        public string Value { get { if (Items.Length >= 6) return Items[5]; else return null; } }
        
        public Command(SudokuGames games, WebSocket socket, string[] items)
        {
            Games = games;
            WebSocket = socket;
            Items = items;
        }
        public string Execute()
        {
            switch (Action)
            {
                case "get_top":
                    return GetTop();
                case "win":
                    return Win();
                case "new":
                    return New();
                case "move":
                    return Move();
                case "join":
                    return Join();
                case "search_games":
                    return SearchGames();
                case "exit":
                    return Exit();
                default:
                    break;
            }

            return "";
        }

        private string Exit()
        {
            if (Games != null)
            {
                SudokuGame game = Games.Get(new Guid(GameId));
                if (game != null)
                {
                    game.RemovePlayer(Name);
                }
            }

            return "";
        }

        private string SearchGames()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append("search_games#");

            foreach (SudokuGame game in Games)
            {
                if (!game.SudokuGrid.IsFinish)
                {
                    ret.Append(game.Name);
                    ret.Append('#');
                    ret.Append(game.Id);
                    ret.Append('#');
                }
            }

            return ret.ToString();
        }

        private string Join()
        {
            if (Games != null)
            {
                SudokuGame game = Games.Get(new Guid(GameId));
                if (!game.AddPlayer(new Player(Name, WebSocket)))
                {
                    return "error#Такой игрок уже в игре";
                }

                return String.Format("{0}#{1}#{2}#{3}", "join", game.Id, game.SudokuGrid.ToString(), Name);
            }

            return "";
        }

        private string Move()
        {
            if (Games != null)
            {
                SudokuGame game = Games.Get(new Guid(GameId));
                if (!game.Move(this))
                {
                    return String.Format("error_move#{0}#{1}", X, Y);
                }
            }

            return "";
        }

        private string New()
        {
            if (Games != null)
            {
                int size = Convert.ToInt32(GameId);
                int difficult = Convert.ToInt32(X);

                SudokuGame game = new SudokuGame(new SudokuGrid(size, difficult), Games);
                game.Name = Y;                
                game.AddPlayer(new Player(Name, WebSocket));

                return String.Format("{0}#{1}#{2}#{3}", "new", game.Id, game.SudokuGrid.ToString(), Name);
            }

            return "";
        }

        private string GetTop()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append("get_top#");

            System.Web.Caching.Cache Cache = new System.Web.Caching.Cache();
            Dictionary<string, int> results = Cache["results"] as Dictionary<string, int>;
            
            if (results != null)
            {
                var myList = results.ToList();
                myList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

                foreach (var item in myList)
                {
                    ret.Append(item.Key);
                    ret.Append(" - ");
                    ret.Append(item.Value);
                    ret.Append("#");                    
                }
            }

            return ret.ToString();
        }

        private string Win()
        {
            //Сохранение в кэш
            System.Web.Caching.Cache Cache = new System.Web.Caching.Cache();
            Dictionary<string, int> results = Cache["results"] as Dictionary<string, int>;

            if (results != null)
            {
                if (results.ContainsKey(Name))
                {
                    results[Name]++;
                    Cache["results"] = results;
                }
                else
                {
                    results.Add(Name, 1);
                    Cache["results"] = results;
                }
            }
            else
            {
                results = new Dictionary<string, int>();
                results.Add(Name, 1);
                Cache["results"] = results;
            }

            return "";
        }
    }
}