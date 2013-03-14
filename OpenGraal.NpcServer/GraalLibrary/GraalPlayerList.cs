using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.NpcServer;

namespace OpenGraal.NpcServer
{
	public class GraalPlayerListOld : IEnumerable
	{
		/// <summary>
		/// Player-List
		/// </summary>
		internal Framework Server;
		protected Dictionary<Int16, GraalPlayer> PlayerList = new Dictionary<Int16, GraalPlayer>();
		protected Int32 Position = -1;

		/// <summary>
		/// Constructor
		/// </summary>
		public GraalPlayerList(Framework Server)
		{
			this.Server = Server;
		}

		/// <summary>
		/// Iterate Playerlist Manager
		/// </summary>
		public IEnumerator<GraalPlayer> GetEnumerator()
		{
			foreach (KeyValuePair<Int16, GraalPlayer> Player in PlayerList)
				yield return Player.Value;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Add Player to Playerlist (or return player)
		/// </summary>
		internal GraalPlayer AddPlayer(Int16 Id)
		{
			GraalPlayer pl = FindPlayer(Id);
			if (pl == null)
			{
				GraalPlayer Player = new GraalPlayer(this.Server, Id);
				PlayerList[Id] = Player;
				return Player;
			}

			return pl;
		}

		/// <summary>
		/// Delete Player from Playerlist
		/// </summary>
		internal bool DeletePlayer(Int16 pId)
		{
			return PlayerList.Remove(pId);
		}

		/// <summary>
		/// Find Player by Id
		/// </summary>
		public GraalPlayer FindPlayer(Int16 Id)
		{
			GraalPlayer player;
			if (PlayerList.TryGetValue(Id, out player))
				return player;
			return null;
		}

		/// <summary>
		/// Find Player by Account
		/// </summary>
		public GraalPlayer FindPlayer(String Account)
		{
			GraalPlayer rc = null;
			foreach (KeyValuePair<Int16, GraalPlayer> Player in PlayerList)
			{
				if (Player.Value.account == Account)
				{
					if (Player.Value.level != String.Empty)
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
		public GraalPlayer FindPlayer(String pAccount, Int16 pId)
		{
			foreach (KeyValuePair<Int16, GraalPlayer> Player in PlayerList)
			{
				if (Player.Value.account == pAccount && Player.Value.id == pId)
					return Player.Value;
			}

			return null;
		}

		public int count()
		{
			return PlayerList.Count;
		}
	}
}
