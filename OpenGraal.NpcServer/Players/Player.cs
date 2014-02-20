using System;
using System.Collections.Generic;
using System.Linq;
using OpenGraal.Core;
using OpenGraal.Common;
using OpenGraal.NpcServer;

namespace OpenGraal.NpcServer.Players
{
	public partial class Player : OpenGraal.Common.Players.GraalPlayer
	{
		protected Framework Server = null;
		public Player(Framework Server, short Id)
			: base(Id)
		{
			this.Server = Server;
		}
		public Player(Framework Server, short Id, GServerConnection socket)
			: base(Id, socket)
		{
			this.Server = Server;
		}

		public override void CurLevel(string name)
		{
			Console.WriteLine("PLAYER: "+this.Account+" LEVEL: " + name);
			// Remove from old level
			if (this.Level != null)
				this.Level.DeletePlayer(this);
			Console.Write("Does level \"" + name + "\" exist... ");
			this.Level = Server.FindLevel(name);
			if (this.Level != null)
				Console.WriteLine("Yes!");
			else
				Console.WriteLine("No... :'(");
			// Grab Map & Level Object
			//this.PlayerMap = (this.Level.EndsWith(".gmap") ? Server.FindMap(this.Level) : null);
			this.AdjustLevels();

			// Add Player
			if (this.Level != null)
			{
				Console.Write("Adding player \""+this.Account+"\" to level... ");
				this.Level.AddPlayer(this);
			}
		}

		public override void AdjustLevels()
		{
			//this.Level = Server.FindLevel((this.PlayerMap == null ? this.Level : this.PlayerMap.GetLevelAt(GmapX, GmapY)));
		}

		/// <summary>
		/// Function -> Send say2 Sign
		/// </summary>
		/// <param name="Message"></param>
		public override void Say2(String Message)
		{
			this.Server.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.PLMSGSIGN + (short)this.Id + Message);
		}

		/// <summary>
		/// Function -> Send rpgmessage to player
		/// </summary>
		/// <param name="Message"></param>
		public override void SendRpgMessage(String Message)
		{
			this.Server.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.PLRPGMSG + (short)this.Id + CString.tokenize(Message));
		}

		/// <summary>
		/// Send PM from NPC-Server
		/// </summary>
		public override void SendPM(String Message)
		{
			this.Server.SendPM(this.Id, Message, false);
		}
	}
}