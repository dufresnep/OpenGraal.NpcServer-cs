using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.NpcServer;
using OpenGraal.NpcServer.Players;

namespace OpenGraal.NpcServer.Players
{
	public class PlayerList : IEnumerable
	{
		/// <summary>
		/// Player-List
		/// </summary>
		protected Dictionary<Int16, Player> _PlayerList = new Dictionary<Int16, Player>();
		protected Int32 Position = -1;
		protected Framework Server;
		protected GServerConnection GSConn;

		/// <summary>
		/// Constructor
		/// </summary>
		public PlayerList(Framework server, GServerConnection gsconn)
		{
			this.Server = server;
			this.GSConn = gsconn;
		}

		/// <summary>
		/// Iterate Playerlist Manager
		/// </summary>
		public IEnumerator<Player> GetEnumerator()
		{
			foreach (KeyValuePair<Int16, Player> Player in _PlayerList)
				yield return Player.Value;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Add Player to Playerlist (or return player)
		/// </summary>
		public virtual Player AddPlayer(Int16 Id)
		{
			Player pl = FindPlayer(Id);
			if (pl == null)
			{
				Player Player = new Player(this.Server,Id,this.GSConn);
				_PlayerList[Id] = Player;
				return Player;
			}

			return pl;
		}

		/// <summary>
		/// Add Player to Playerlist (or return player)
		/// </summary>
		public virtual Player AddPlayer(Int16 Id, GServerConnection socket)
		{
			this.GSConn = socket;
			Player pl = FindPlayer(Id);
			if (pl == null)
			{
				this.Server.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.PLGETWEPS + (short)Id);
				Player Player = new Player(this.Server,Id, socket);
				_PlayerList[Id] = Player;
				return Player;
			}

			return pl;
		}

		/// <summary>
		/// Delete Player from Playerlist
		/// </summary>
		public virtual bool DeletePlayer(Int16 pId)
		{
			return _PlayerList.Remove(pId);
		}

		/// <summary>
		/// Find Player by Id
		/// </summary>
		public virtual Player FindPlayer(Int16 Id)
		{
			Player player;
			if (_PlayerList.TryGetValue(Id, out player))
				return player;
			return null;
		}

		/// <summary>
		/// Find Player by Account
		/// </summary>
		public virtual Player FindPlayer(String Account)
		{
			Player rc = null;
			foreach (KeyValuePair<Int16, Player> Player in _PlayerList)
			{
				if (Player.Value.Account == Account)
				{
					if (Player.Value.Level != null)
						return Player.Value;
					else
						rc = Player.Value;
				}
			}

			return rc;
		}

		/// <summary>
		/// Find Player by Id/Account
		/// </summary>
		public virtual Player FindPlayer(String pAccount, Int16 pId)
		{
			foreach (KeyValuePair<Int16, Player> Player in _PlayerList)
			{
				if (Player.Value.Account == pAccount && Player.Value.Id == pId)
					return Player.Value;
			}

			return null;
		}

		public virtual int Count()
		{
			return _PlayerList.Count;
		}
	}
}
