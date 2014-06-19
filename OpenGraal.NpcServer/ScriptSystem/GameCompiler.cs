using System;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CSharp;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.Common;
using OpenGraal.Common.Players;
using OpenGraal.Common.Levels;
using OpenGraal.Common.Scripting;

namespace OpenGraal.NpcServer
{
	public class GameCompiler : OpenGraal.Common.Scripting.GameCompiler
	{
		/// <summary>
		/// Member Variables
		/// </summary>
		protected Framework Server;
		
		/// <summary>
		/// Constructor -> Create Compiler, pass NPCServer reference
		/// </summary>
		public GameCompiler(Framework Server) : base() 
		{
			this.Server = Server;
		}

		public override void OutputError(string errorText)
		{
			this.Server.SendNCChat(errorText);
		}

		public override ServerClass FindClass(string Name)
		{
			return this.Server.FindClass(Name);
		}

		public override Dictionary<string,ServerClass> GetClasses()
		{
			return this.Server.ClassList;
		}

		public override ScriptObj InvokeConstruct(IRefObject Reference, ConstructorInfo constructor)
		{
			ScriptObj obj = (ScriptObj)constructor.Invoke(new object[] { this.Server.GSConn, Reference });
			return obj;
		}
	}
}
