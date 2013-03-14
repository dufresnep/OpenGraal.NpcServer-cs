using System;
using System.IO;
using System.Linq;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.NpcServer;
using OpenGraal.NpcServer.Players;

namespace OpenGraal.NpcServer
{
	public class GraalMap
	{
		/// <summary>
		/// Member Variables
		/// </summary>
		public int MapWidth, MapHeight;
		public string MapName, MapImage, MiniMapImage;
		private string[] MapLevels = new string[] { };

		/// <summary>
		/// Constructor
		/// </summary>
		public GraalMap(String MapName)
		{
			this.MapName = MapName;
		}

		/// <summary>
		/// Constructor (load by file)
		/// </summary>
		public GraalMap(String MapName, String FilePath)
		{
			this.MapName = MapName;
			this.ParseFile(FilePath);
		}

		/// <summary>
		/// Get Level from Map X / Map Y
		/// </summary>
		public string GetLevelAt(int MapX, int MapY)
		{
			int pos = MapX + MapY * MapWidth;
			return (pos < MapLevels.Length ? MapLevels[pos] : String.Empty);
		}

		/// <summary>
		/// Level Exists
		/// </summary>
		public bool LevelExists(String LevelName)
		{
			return MapLevels.Contains(LevelName);
		}

		/// <summary>
		/// Parse GMap by File
		/// </summary>
		public void ParseFile(String File)
		{
			using (TextReader reader = new StreamReader(File))
			{
				this.Parse((StreamReader)reader);
			}
		}

		/// <summary>
		/// Parse GMAP by Stream
		/// </summary>
		public void Parse(StreamReader Stream)
		{
			String line = String.Empty;
			while ((line = Stream.ReadLine()) != null)
			{
				String[] tokens = line.Split(' ');
				if (tokens.Length < 1)
					continue;

				switch (tokens[0])
				{
					// Map Width
					case "WIDTH":
						if (tokens.Length > 1)
							this.MapWidth = Convert.ToInt32(tokens[1]);
						break;

					// Map Height
					case "HEIGHT":
						if (tokens.Length > 1)
							this.MapHeight = Convert.ToInt32(tokens[1]);
						break;

					// Map Image
					case "MAPIMG":
						if (tokens.Length > 1)
							this.MapImage = tokens[1];
						break;

					// Map Image
					case "MINIMAPIMG":
						if (tokens.Length > 1)
							this.MiniMapImage = tokens[1];
						break;

					// Option -> Load Full Map
					case "LOADFULLMAP":
						break;

					// Option -> Disable Automapping
					case "NOAUTOMAPPING":
						break;

					// GENERATED (unknown??)
					case "GENERATED":
						break;

					// Option -> Load Map at Start
					case "LOADATSTART":
						while ((line = Stream.ReadLine()) != null)
						{
							if (line == "LOADATSTARTEND")
								break;
						}
						break;

					// Map Levels
					case "LEVELNAMES":
					{
						// Create MapLevels Array
						this.MapLevels = new string[this.MapWidth * this.MapHeight];

						int gx = 0, gy = 0;
						while ((line = Stream.ReadLine()) != null)
						{
							if (line == "LEVELNAMESEND")
								break;

							if (line.Length > 0)
							{
								string[] levels = CString.untokenize(line).Replace("\r", "").Split('\n');
								foreach (string level in levels)
								{
									int pos = gx + gy * MapWidth;
									if (pos < MapLevels.Length)
										this.MapLevels[pos] = level;
									gx++;
								}
							}

							gx = 0;
							gy++;
						}

						break;
					}
				}
			}

			System.Console.WriteLine("File: " + MapName + " | Level Count: " + MapLevels.Length);
		}
	}
}
