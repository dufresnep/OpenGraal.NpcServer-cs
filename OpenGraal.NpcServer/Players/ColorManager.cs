using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.NpcServer;

namespace OpenGraal.NpcServer.Players
{
	public class ColorManager
	{
		/// <summary>
		/// Colors
		/// </summary>
		static string[] AllColors = new string[]
		{
			"white", "yellow", "orange", "pink", "red",
			"darkred","lightgreen", "green", "darkgreen", "lightblue",
			"blue", "darkblue", "brown", "cynober", "purple",
			"darkpurple", "lightgray", "gray", "black", "transparent"
		};

		/// <summary>
		/// Member Variables
		/// </summary>
		byte[] colors = new byte[5];
		internal dSendColors cFunction = null;
		internal delegate void dSendColors();

		/// <summary>
		/// Constructor
		/// </summary>
		internal ColorManager(dSendColors Delegate)
		{
			this.cFunction = Delegate;
		}

		/// <summary>
		/// Retrieve Color
		/// </summary>
		public byte this[int key]
		{
			get
			{
				if (key < colors.Length)
					return colors[key];
				return 0;
			}

			set
			{
				colors[key] = value;
				if (cFunction != null)
					cFunction();
			}
		}

		/*
		public string this[int key]
		{
			set
			{
				for (int i = 0; i < AllColors.Length; i++)
				{
					if (String.Compare(AllColors[i], value, true) == 0)
					{
						colors[key] = i;
						break;
					}
				}
				colors[key] = 0;
			}
		}
		*/

		/// <summary>
		/// String -> Colors
		/// </summary>
		public void FromString(String ColorStr)
		{
			string[] c = ColorStr.Split(',');
			for (int i = 0; i < Math.Min(colors.Length, c.Length); i++)
				colors[i] = Convert.ToByte(c[i]);

			if (cFunction != null)
				cFunction();
		}

		/// <summary>
		/// Colors -> String
		/// </summary>
		public override string ToString()
		{
			string retVal = String.Empty;
			for (int i = 0; i < colors.Length; i++)
				retVal += Convert.ToInt32(colors[i]) + (i == colors.Length - 1 ? "" : ",");
			return retVal;
		}

		/// <summary>
		/// Retrieve Packet for Colors
		/// </summary>
		internal CString GetPacket()
		{
			return new CString() + (byte)colors[0] + (byte)colors[1] + (byte)colors[2] + (byte)colors[3] + (byte)colors[4];
		}
	};
}
