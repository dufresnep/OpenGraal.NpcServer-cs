using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.NpcServer;

namespace OpenGraal.NpcServer
{
	public class GraalLevelNPC : IRefObject
	{
		/// <summary>
		/// NPC Properties Enum
		/// </summary>
		internal enum Properties
		{
			IMAGE = 0,
			SCRIPT = 1,
			NPCX = 2,
			NPCY = 3,
			NPCPOWER = 4,
			NPCRUPEES = 5,
			ARROWS = 6,
			BOMBS = 7,
			GLOVEPOWER = 8,
			BOMBPOWER = 9,
			SWORDIMG = 10,
			SHIELDIMG = 11,
			GANI = 12,
			VISFLAGS = 13,
			BLOCKFLAGS = 14,
			MESSAGE = 15,
			HURTDXDY = 16,
			NPCID = 17,
			SPRITE = 18,
			COLORS = 19,
			NICKNAME = 20,
			HORSEIMG = 21,
			HEADIMG = 22,
			SAVE0 = 23,
			SAVE1 = 24,
			SAVE2 = 25,
			SAVE3 = 26,
			SAVE4 = 27,
			SAVE5 = 28,
			SAVE6 = 29,
			SAVE7 = 30,
			SAVE8 = 31,
			SAVE9 = 32,
			ALIGNMENT = 33,
			IMAGEPART = 34,
			BODYIMG = 35,
			GATTRIB1 = 36,
			GATTRIB2 = 37,
			GATTRIB3 = 38,
			GATTRIB4 = 39,
			GATTRIB5 = 40,
			GMAPLVLX = 41,
			GMAPLVLY = 42,
			EMPTY43 = 43,
			GATTRIB6 = 44,
			GATTRIB7 = 45,
			GATTRIB8 = 46,
			GATTRIB9 = 47,
			PIXELX = 75,
			PIXELY = 76,
		};

		/// <summary>
		/// Member Variables
		/// </summary>
		private Framework Server;
		internal byte BlockFlags = 0, GMapX = 0, GMapY = 0, VisFlags = 0;
		internal double
			Hearts = 3.0;

		internal int
			Ap = 50,
			Arrows = 10,
			Bombs = 5,
			BombPower = 1,
			Dir = 2,
			FullHearts = 3,
			GlovePower = 1,
			Gralats = 0,
			Id = 0,
			PixelX = 480,
			PixelY = 488,
			ShieldPower = 1,
			SwordPower = 1,
			Width = 2,
			Height = 2;

		internal string
			Animation = "idle",
			BodyImage = "body.png",
			Chat = String.Empty,
			HeadImage = "head0.png",
			Image = String.Empty,
			Nickname = "unknown";

		internal CString ImagePart;
		internal GraalLevel Level = null;
		internal SaveIndex Save = null;

		/// <summary>
		/// Constructor
		/// </summary>
		internal GraalLevelNPC(Framework Server, GraalLevel Level, int Id)
			: base(ScriptType.LEVELNPC)
		{
			this.Server = Server;
			this.Level = Level;
			this.Id = Id;
			this.Save = new SaveIndex(this, 10);
		}

		/// <summary>
		/// Send Prop to GServer
		/// </summary>
		internal void SendProp(Properties PropId)
		{
			Server.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.NPCPROPSET + (int)this.Id + (byte)PropId + GetProp(PropId));
		}

		/// <summary>
		/// Get Property
		/// </summary>
		internal CString GetProp(Properties PropId)
		{
			switch (PropId)
			{
				case Properties.IMAGE: // 0
					return new CString() + (byte)this.Image.Length + this.Image;

				case Properties.SCRIPT: // 1
					return new CString() + (long)this.Script.Length + this.Script;

				case Properties.VISFLAGS: // 13
					return new CString() + (byte)VisFlags;

				case Properties.BLOCKFLAGS: // 14
					return new CString() + (byte)BlockFlags;

				case Properties.MESSAGE: // 15
					return new CString() + (byte)Chat.Length + Chat;

				case Properties.SAVE0: // 23
					return new CString() + (byte)Save[0];

				case Properties.SAVE1: // 24
					return new CString() + (byte)Save[1];

				case Properties.SAVE2: // 25
					return new CString() + (byte)Save[2];

				case Properties.SAVE3: // 26
					return new CString() + (byte)Save[3];

				case Properties.SAVE4: // 27
					return new CString() + (byte)Save[4];

				case Properties.SAVE5: // 28
					return new CString() + (byte)Save[5];

				case Properties.SAVE6: // 29
					return new CString() + (byte)Save[6];

				case Properties.SAVE7: // 30
					return new CString() + (byte)Save[7];

				case Properties.SAVE8: // 31
					return new CString() + (byte)Save[8];

				case Properties.SAVE9: // 32
					return new CString() + (byte)Save[9];

				case Properties.IMAGEPART: // 34
					return ImagePart;

				case Properties.GMAPLVLX: // 41
					return new CString() + (byte)GMapX;

				case Properties.GMAPLVLY: // 42
					return new CString() + (byte)GMapY;

				case Properties.PIXELX: // 75
				{
					int res = (PixelX < 0 ? -PixelX : PixelX) << 1;
					if (PixelX < 0)
						res |= 0x0001;
					return new CString() + (short)res;
				}

				case Properties.PIXELY: // 76
				{
					int res = (PixelY < 0 ? -PixelY : PixelY) << 1;
					if (PixelY < 0)
						res |= 0x0001;
					return new CString() + (short)res;
				}

				default:
					return new CString();
			}
		}

		/// <summary>
		/// Set Properties
		/// </summary>
		/// <param name="Packet"></param>
		internal void SetProps(CString Packet)
		{
			bool compileScript = false;

			while (Packet.BytesLeft > 0)
			{
				Int32 PropId = Packet.ReadGUByte1();

				switch ((Properties)PropId)
				{
					case Properties.IMAGE: // 0
						this.Image = Packet.ReadChars(Packet.ReadGUByte1());
						break;

					case Properties.SCRIPT: // 1
						this.Script = Packet.ReadChars((int)Packet.ReadGUByte5()).Replace("\xa7", "\n");
						if (this.Script.IndexOf("void") > 0)
							compileScript = true;
						break;

					case Properties.NPCX: // 2 - obsolete
						Packet.ReadGByte1();
						break;

					case Properties.NPCY: // 3 - obsolete
						Packet.ReadGByte1();
						break;

					case Properties.NPCPOWER: // 4
						Packet.ReadGUByte1();
						break;

					case Properties.NPCRUPEES: // 5
						this.Gralats = (int)Packet.ReadGUByte3();
						break;

					case Properties.ARROWS: // 6
						this.Arrows = Packet.ReadGUByte1();
						break;

					case Properties.BOMBS: // 7
						this.Bombs = Packet.ReadGUByte1();
						break;

					case Properties.GLOVEPOWER: // 8
						Packet.ReadGUByte1();
						break;

					case Properties.BOMBPOWER: // 9
						Packet.ReadGUByte1();
						break;

					case Properties.GANI: // 12
						Packet.ReadChars(Packet.ReadGUByte1());
						break;

					case Properties.VISFLAGS: // 13
						this.VisFlags = Packet.ReadGUByte1();
						break;

					case Properties.BLOCKFLAGS: // 14
						this.BlockFlags = Packet.ReadGUByte1();
						break;

					case Properties.MESSAGE: // 15
						this.Chat = Packet.ReadChars(Packet.ReadGUByte1());
						break;

					case Properties.NPCID: // 17
						Packet.ReadGUByte3();
						break;

					case Properties.SPRITE: // 18
						Packet.ReadGUByte1();
						break;

					case Properties.COLORS: // 19
						Packet.ReadGUByte5();
						break;

					case Properties.NICKNAME: // 20
						this.Nickname = Packet.ReadChars(Packet.ReadGUByte1());
						break;

					case Properties.HORSEIMG: // 21
						Packet.ReadChars(Packet.ReadGUByte1());
						break;

					case Properties.HEADIMG: // 22
					{
						Int32 len = Packet.ReadGUByte1();
						this.HeadImage = (len < 100 ? "head" + len + ".png" : Packet.ReadChars(len - 100));
						break;
					}

					case Properties.SAVE0: // 23
						this.Save[0] = Packet.ReadGUByte1();
						break;

					case Properties.SAVE1: // 24
						this.Save[1] = Packet.ReadGUByte1();
						break;

					case Properties.SAVE2: // 25
						this.Save[2] = Packet.ReadGUByte1();
						break;

					case Properties.SAVE3: // 26
						this.Save[3] = Packet.ReadGUByte1();
						break;

					case Properties.SAVE4: // 27
						this.Save[4] = Packet.ReadGUByte1();
						break;

					case Properties.SAVE5: // 28
						this.Save[5] = Packet.ReadGUByte1();
						break;

					case Properties.SAVE6: // 29
						this.Save[6] = Packet.ReadGUByte1();
						break;

					case Properties.SAVE7: // 30
						this.Save[7] = Packet.ReadGUByte1();
						break;

					case Properties.SAVE8: // 31
						this.Save[8] = Packet.ReadGUByte1();
						break;

					case Properties.SAVE9: // 32
						this.Save[9] = Packet.ReadGUByte1();
						break;

					case Properties.ALIGNMENT: // 33
						Packet.ReadGUByte1();
						break;

					case Properties.IMAGEPART: // 34
						this.ImagePart = Packet.Read(6);
						break;

					case Properties.BODYIMG: // 35
						this.BodyImage = Packet.ReadChars(Packet.ReadGUByte1());
						break;

					case Properties.GMAPLVLX: // 41
						this.GMapX = Packet.ReadGUByte1();
						break;

					case Properties.GMAPLVLY: // 42
						this.GMapY = Packet.ReadGUByte1();
						break;

					case Properties.PIXELX: // 75
					{
						int tmp = this.PixelX = Packet.ReadGUByte2();
						
						// If the first bit is 1, our position is negative.
						this.PixelX >>= 1;
						if ((tmp & 0x0001) != 0)
							this.PixelX = -this.PixelX;
						break;
					}

					case Properties.PIXELY: // 76
					{
						int tmp = this.PixelY = Packet.ReadGUByte2();

						// If the first bit is 1, our position is negative.
						this.PixelY >>= 1;
						if ((tmp & 0x0001) != 0)
							this.PixelY = -this.PixelY;
						break;
					}

					default:
						System.Console.WriteLine("Unknown NPC Prop: " + PropId);
						return;
				}
			}

			// Compile script if script changed.
			if (compileScript)
				Server.Compiler.CompileAdd(this);
		}
	}

	public class SaveIndex
	{
		private byte[] SaveData;
		private GraalLevelNPC LevelNpc = null;

		public SaveIndex(GraalLevelNPC LevelNpc, int size)
		{
			this.LevelNpc = LevelNpc;

			SaveData = new byte[size];
			for (int i = 0; i < size; i++)
				SaveData[i] = 0;
		}

		public byte this[int pos]
		{
			get
			{
				return SaveData[pos];
			}
			set
			{
				SaveData[pos] = value;
				LevelNpc.SendProp(GraalLevelNPC.Properties.SAVE0 + pos);
			}
		}
	}

	/// <summary>
	/// Class: ScriptLevelNpc Object
	/// </summary>
	public class ScriptLevelNpc : ScriptObj
	{
		// -- Member Variables -- //
		internal readonly GraalLevelNPC Ref;
		public readonly bool isweapon = true;

		/// <summary>
		/// NPC -> Visible
		/// </summary>
		public bool visible
		{
			get { return ((Ref.VisFlags & 1) != 0); }
		}

		/// <summary>
		/// NPC -> X
		/// </summary>
		public double x
		{
			get { return Ref.PixelX / 16; }
			set { Ref.PixelX = Convert.ToInt32(value * 16.0); Ref.SendProp(GraalLevelNPC.Properties.PIXELX); }
		}

		/// <summary>
		/// NPC -> Y
		/// </summary>
		public double y
		{
			get { return Ref.PixelY / 16; }
			set { Ref.PixelY = Convert.ToInt32(value * 16.0); Ref.SendProp(GraalLevelNPC.Properties.PIXELY); }
		}

		/// <summary>
		/// Name -> Read Only
		/// </summary>
		public int id
		{
			get { return Ref.Id; }
		}

		/// <summary>
		/// Image -> Read Only
		/// </summary>
		public string image
		{
			get { return Ref.Image; }
		}

		/// <summary>
		/// Level -> Read Only
		/// </summary>
		public GraalLevel level
		{
			get { return Ref.Level; }
		}

		/// <summary>
		/// Save[0] - Save[9]
		/// </summary>
		public SaveIndex save
		{
			get { return Ref.Save;  }
		}

		/// <summary>
		/// Block Again
		/// </summary>
		public void blockagain()
		{
			Ref.BlockFlags = 0;
			Ref.SendProp(GraalLevelNPC.Properties.BLOCKFLAGS);
		}

		/// <summary>
		/// Dont Block
		/// </summary>
		public void dontblock()
		{
			Ref.BlockFlags = 1;
			Ref.SendProp(GraalLevelNPC.Properties.BLOCKFLAGS);
		}

		/// <summary>
		/// Draw Under Player
		/// </summary>
		public void drawoverplayer()
		{
			Ref.VisFlags = 3;
			Ref.SendProp(GraalLevelNPC.Properties.VISFLAGS);
		}

		/// <summary>
		/// Draw Under Player
		/// </summary>
		public void drawunderplayer()
		{
			Ref.VisFlags = 5;
			Ref.SendProp(GraalLevelNPC.Properties.VISFLAGS);
		}

		/// <summary>
		/// Message / NPC Chat
		/// </summary>
		public void message(string msg)
		{
			Ref.Chat = msg;
			Ref.SendProp(GraalLevelNPC.Properties.MESSAGE);
		}

		/// <summary>
		/// Attach Player to Object
		/// </summary>
		public void attachplayer(GraalPlayer player)
		{
			if (player != null)
				Server.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.PLSETPROPS + (short)player.id + (byte)GraalPlayer.Properties.PLATTACHNPC + (byte)0 + (int)this.id);
		}

		/// <summary>
		/// Attach Player to Object
		/// </summary>
		public void detachplayer(GraalPlayer player)
		{
			if (player != null)
				Server.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.PLSETPROPS + (short)player.id + (byte)GraalPlayer.Properties.PLATTACHNPC + (byte)0 + (int)0);
		}

		/// <summary>
		/// Move NPC
		/// </summary>
		public void move(double dx, double dy, double time, int options)
		{
			int start_x = (Math.Abs(Ref.PixelX) << 1) | (Ref.PixelX < 0 ? 0x0001 : 0x0000);
			int start_y = (Math.Abs(Ref.PixelY) << 1) | (Ref.PixelY < 0 ? 0x0001 : 0x0000);
			int pdx = (((short)Math.Abs(dx) * 16) << 1) | (dx < 0 ? 0x0001 : 0x0000);
			int pdy = (((short)Math.Abs(dy) * 16) << 1) | (dy < 0 ? 0x0001 : 0x0000);
			int itime = (short)(time / 0.05);
			Server.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.NPCMOVE + (int)this.id + (short)start_x + (short)start_y + (short)pdx + (short)pdy + (short)itime + (byte)options);

			Ref.PixelX = Ref.PixelX + Convert.ToInt32(dx * 16);
			Ref.PixelY = Ref.PixelY + Convert.ToInt32(dy * 16);
			if ((options & 8) != 0)
				this.ScheduleEvent(time, "onMovementFinished");
		}

		/// <summary>
		/// Setshape
		/// </summary>
		public void setshape(int type, int width, int height)
		{
			Ref.Width = width;
			Ref.Height = height;
		}

		/// <summary>
		/// Set Image
		/// </summary>
		public void setimg(string image)
		{
			Ref.Image = image;
			Ref.ImagePart = new CString() + (short)0 + (short)0 + (byte)0 + (byte)0;
			Ref.SendProp(GraalLevelNPC.Properties.IMAGE);
			Ref.SendProp(GraalLevelNPC.Properties.IMAGEPART);
		}

		/// <summary>
		/// Set Image Part (Image, X, Y, W, H)
		/// </summary>
		public void setimgpart(string image, int x, int y, int w, int h)
		{
			Ref.Image = image;
			Ref.ImagePart = new CString() + (short)x + (short)y + (byte)w + (byte)h;
			Ref.SendProp(GraalLevelNPC.Properties.IMAGE);
			Ref.SendProp(GraalLevelNPC.Properties.IMAGEPART);
		}

		/// <summary>
		/// NPC -> Hide
		/// </summary>
		public void hide()
		{
			Ref.VisFlags = 0;
			Ref.SendProp(GraalLevelNPC.Properties.VISFLAGS);
		}

		/// <summary>
		/// NPC -> Show
		/// </summary>
		public void show()
		{
			Ref.VisFlags = 5;
			Ref.SendProp(GraalLevelNPC.Properties.VISFLAGS);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public ScriptLevelNpc() { }
		public ScriptLevelNpc(Framework Server, IRefObject Ref)
			: base(Server)
		{
			this.Ref = (GraalLevelNPC)Ref;
		}
	};
}
