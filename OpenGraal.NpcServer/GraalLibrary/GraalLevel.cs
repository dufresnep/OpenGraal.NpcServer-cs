using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGraal.NpcServer
{
	public class GraalLevel
	{
		/// <summary>
		/// Member Variables
		/// </summary>
		protected Framework Server;
		internal Dictionary<int, GraalLevelNPC> NpcList = new Dictionary<int, GraalLevelNPC>();
		internal FlagManager FlagManager = null;
		internal List<GraalPlayer> Players = new List<GraalPlayer>();
		internal short[] Tiles = new short[4096];
		internal uint ModTime;
		internal string Name;

		/// <summary>
		/// Constructor
		/// </summary>
		internal GraalLevel(Framework Server, String LevelName)
		{
			this.Server = Server;
			this.Name = LevelName;
			this.FlagManager = new FlagManager(null);
		}

		/// <summary>
		/// Clear Level (test)
		/// </summary>
		internal void Clear()
		{
			// Reset Mod Time
			this.ModTime = 0;

			// Clear NPCS
			this.Players.Clear();
			lock (Server.TimerLock)
			{
				this.NpcList.Clear();
			}
		}

		/// <summary>
		/// Add Player to Level
		/// </summary>
		/// <param name="Player"></param>
		internal void AddPlayer(GraalPlayer Player)
		{
			if (!Players.Contains(Player))
			{
				Players.Add(Player);
				this.CallNPCs("onPlayerEnters", new object[] { Player });
			}
		}

		/// <summary>
		/// Delete Player from Level
		/// </summary>
		/// <param name="Player"></param>
		internal void DeletePlayer(GraalPlayer Player)
		{
			if (Players.Contains(Player))
			{
				Players.Remove(Player);
				this.CallNPCs("onPlayerLeaves", new object[] { Player });
			}
		}

		/// <summary>
		/// Call NPC Events
		/// </summary>
		internal void CallNPCs(String Event, object[] Args)
		{
			foreach (KeyValuePair<int, GraalLevelNPC> e in NpcList)
				e.Value.Call(Event, Args);
		}

		/// <summary>
		/// Delete NPC from List
		/// </summary>
		internal bool DeleteNPC(int Id)
		{
			lock (Server.TimerLock)
			{
				return NpcList.Remove(Id);
			}
		}

		/// <summary>
		/// Gets a npc from the level with the specified level id.
		/// </summary>
		internal GraalLevelNPC GetNPC(int Id)
		{
			GraalLevelNPC npc = null;
			if (!NpcList.TryGetValue(Id, out npc))
			{
				npc = new GraalLevelNPC(Server, this, Id);
				lock (Server.TimerLock)
				{
					NpcList[Id] = npc;
				}
			}
			return npc;
		}

		/// <summary>
		/// Set Mod Time
		/// </summary>
		internal void SetModTime(uint NewTime)
		{
			this.ModTime = NewTime;
		}

		/// <summary>
		/// Grab Local NPCS
		/// </summary>
		public List<dynamic> npcs
		{
			get
			{
				List<dynamic> obj = new List<dynamic>();
				foreach (KeyValuePair<int, GraalLevelNPC> l in NpcList)
				{
					if (l.Value.ScriptObject != null)
						obj.Add((dynamic)l.Value.ScriptObject);
				}
				return obj;
			}
		}

		/// <summary>
		/// Grab Local Players
		/// </summary>
		public List<GraalPlayer> players
		{
			get { return this.Players; }
		}

		/// <summary>
		/// Script Variable -> flags
		/// </summary>
		public FlagManager flags
		{
			get { return FlagManager; }
		}

		/// <summary>
		/// Script Variable -> name
		/// </summary>
		public string name
		{
			get { return this.Name; }
		}

		/// <summary>
		/// Player is on npc
		/// </summary>
		internal GraalLevelNPC isOnNPC(int x, int y)
		{
			foreach (KeyValuePair<int, GraalLevelNPC> v in NpcList)
			{
				GraalLevelNPC npc = v.Value;
				if (npc.Image != String.Empty)
				{
					if ((npc.VisFlags & 1) != 0) // && (npc.BlockFlags & 1) == 0
					{
						if (x >= npc.PixelX && x <= npc.PixelX + npc.Width && y >= npc.PixelY && y < npc.PixelY + npc.Height)
							return npc;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Check if X/Y is on a wall
		/// </summary>
		internal bool isOnWall(double x, double y)
		{
			if (x < 0 || x >= 64 || y < 0 || y >= 64)
				return true;
			return IsTileWall(Tiles[(int)x + ((int)y) * 64]);
		}

		/// <summary>
		/// Check if x/y is on water
		/// </summary>
		internal bool IsOnWater(double x, double y)
		{
			if (x < 0 || x >= 64 || y < 0 || y >= 64)
				return false;
			return IsTileWater(Tiles[(int)x + ((int)y) * 64]);
		}

		/// <summary>
		/// Check if a tile is blocking
		/// </summary>
		internal bool IsTileWall(int TileId)
		{
			int TileX = TileId % 16;
			int TileY = TileId / 16;
			return (TileId == 32) || // black tile
				((TileX >= 2 && TileY >= 26 && TileY < 28) || // lift objects
				(TileY >= 30 && TileY < 48) || // chest, movestone, jumpstone, throughthrough
				(TileY >= 84 && TileY < 96) || // lower 12 lines of foreground
				(TileY >= 116 && TileY < 128) || // lower 12 lines of foreground
				(TileY >= 128 && ((TileY / 16) & 1) != 0)); // lower half of normal tiles
		}

		/// <summary>
		/// Check if a tile is water
		/// </summary>
		internal bool IsTileWater(int TileId)
		{
			return (TileId >= 64 && TileId < 1152);
		}
	}
}
