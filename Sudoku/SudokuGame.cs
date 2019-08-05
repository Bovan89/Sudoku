using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Sudoku
{
    public class SudokuGame
    {
        public Guid Id { get; }

        public string Name { get; set; }

        public SudokuGames Parent { get; }

        public SudokuGrid SudokuGrid { get; }

        public List<Player> Players { get; }

        private List<Action> Actions { get; }

        // Блокировка для обеспечения потокабезопасности
        private readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        private delegate void DelegateSendClients(string data);
        private DelegateSendClients SendClientsDelegate;

        //////////////////////////

        public SudokuGame(SudokuGrid sg, SudokuGames sgs)
        {
            SudokuGrid = sg;
            Parent = sgs;

            Id = Guid.NewGuid();            
            Players = new List<Player>();
            Actions = new List<Action>();
            SendClientsDelegate = SendClients;

            Parent.Add(this);
        }

        public bool AddPlayer(Player p)
        {
            if (Players.Contains(p))
            {
                return false;
            }

            Locker.EnterWriteLock();
            try
            {
                Players.Add(p);
            }
            finally
            {
                Locker.ExitWriteLock();
            }

            AddAction(p, String.Format("{0}#{1}", "new_player", p.Name));

            return true;
        }

        public void RemovePlayer(string name)
        {
            foreach (Player p in Players)
            {
                if (p.Name == name)
                {
                    Locker.EnterWriteLock();
                    try
                    {
                        Players.Remove(p);
                    }
                    finally
                    {
                        Locker.ExitWriteLock();
                    }

                    AddAction(p, String.Format("{0}#{1}", "exit_player", p.Name));

                    return;
                }
            }
        }

        public Player GetPlayer(string playerName)
        {
            foreach (Player p in Players)
            {
                if (p.Name == playerName)
                {
                    return p;
                }
            }

            return null;
        }

        public bool Move(Command move)
        {
            Player p = GetPlayer(move.Name);
            if (p == null)
            {
                return false;
            }

            int x = Convert.ToInt32(move.X);
            int y = Convert.ToInt32(move.Y);
            int value = Convert.ToInt32(move.Value);

            if (!SudokuGrid.FillValue(x, y, value))
            {
                return false;
            }

            AddAction(p, String.Format("move#{0}#{1}#{2}#{3}", p.Name, x, y, value));

            if (SudokuGrid.IsFinish)
            {
                Locker.EnterWriteLock();
                try
                {
                    Parent.Remove(this);
                }
                finally
                {
                    Locker.ExitWriteLock();
                }

                Command cmd = new Command(Parent, null, new string[] { "win", p.Name, this.Id.ToString() });
                cmd.Execute();

                AddAction(p, String.Format("win#{0}#{1}", p.Name, Id));
            }

            return true;
        }

        private void AddAction(Player player, string data)
        {
            Action a = new Action(player, data);

            //if (player == null || Players.Contains(player))
            //{
                Locker.EnterWriteLock();
                try
                {
                    Actions.Add(a);
                }
                finally
                {
                    Locker.ExitWriteLock();
                }

                //Асихронный вызов отправки игрокам
                SendClientsDelegate.BeginInvoke(a.Data, null, null);
            //}
        }

        void SendClients(string data)
        {
            //Передаём сообщение всем клиентам
            foreach (Player p in Players)
            {
                int tryCnt = 10;
                while (tryCnt-- > 0)
                {
                    try
                    {
                        p.Send(data);
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        RemovePlayer(p.Name);
                        break;
                    }
                    catch (InvalidOperationException)
                    {
                        Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(100);
                        string s = ex.Message;
                        s = ex.ToString();
                    }
                }                
            }
        }
        
        //////////////////////////

        private class Action
        {
            //public string Description { get; }

            public string Data;
            public Player Player { get; }
            public DateTime DateTime { get; set; }
            public Action(Player p, string data)
            {
                //Description = description;                
                Player = p;
                DateTime = DateTime.Now;
                Data = String.Format("{0}#{1}", data, DateTime);
            }
        }
    }

    public class SudokuGames : IEnumerable
    {
        private Dictionary<Guid, SudokuGame> Games;

        public SudokuGames()
        {
            Games = new Dictionary<Guid, SudokuGame>();
        }

        public void Add(SudokuGame game)
        {
            Games.Add(game.Id, game);
        }

        public void Remove(SudokuGame game)
        {
            Games.Remove(game.Id);
        }

        public SudokuGame Get(Guid id)
        {
            if (Games.ContainsKey(id))
            {
                return Games[id];
            }
            else
            {
                return null;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return Games.Values.GetEnumerator();
        }
    }
}