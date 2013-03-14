using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGraal.NpcServer.GraalLibrary
{
	public class GraalMap
	{
		/// <summary>
		/// Member Variables
		/// </summary>
		protected int Width = 0, Height = 0;
		protected List<String> MapList = null;

		/// <summary>
		/// Constructor
		/// </summary>
		internal GraalMap(int Width, int Height, string[] MapData)
		{
			this.ParseMapData(Width, Height, MapData);
		}

		/// <summary>
		/// Parse Map Data
		/// </summary>
		internal void ParseMapData(int Width, int Height, string[] MapData)
		{
		//	MapData.Split
		}
	}
}
