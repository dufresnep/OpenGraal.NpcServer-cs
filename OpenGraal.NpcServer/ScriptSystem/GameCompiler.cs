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

		public override ScriptObj InvokeConstruct(IRefObject Reference)
		{
			ScriptObj obj = null;

			if (Reference.Type == IRefObject.ScriptType.WEAPON)
				obj = (ScriptObj)(new ScriptWeapon(this.Server.GSConn, Reference));// (ScriptObj)Reference;//
			else if (Reference.Type == IRefObject.ScriptType.LEVELNPC)
				obj = (ScriptObj)(new ScriptLevelNpc(this.Server.GSConn, Reference));
			if (Reference.AttachToGlobalScriptInstance != null)
			{
				try
				{
					V8Instance.GetInstance().Evaluate(Reference.AttachToGlobalScriptInstance);
					V8Instance.forEachProp(V8Instance.GetInstance().Script, new Action<string>(propName =>
					{
						Console.WriteLine(propName);
					}));
					
					//if (V8Instance.hasMethod(V8Instance.GetInstance().Script, Reference.V8ScriptName, 1))
					{
						dynamic test = null;
						if (Reference.Type == IRefObject.ScriptType.WEAPON)
							test = V8Instance.InvokeFunction(Reference.V8ScriptName, new object[] {  });
						else if (Reference.Type == IRefObject.ScriptType.LEVELNPC)
							test = V8Instance.InvokeFunction(Reference.V8ScriptName, new object[] {  });
						test.onCreated();

					}

				}
				catch (Microsoft.ClearScript.ScriptEngineException e)
				{
					HandleErrors((Reference.Type == IRefObject.ScriptType.WEAPON ? "weapon" : "levelnpc_") + Reference.GetErrorText(), e.ErrorDetails.Replace('\n', ' '));
				}
			}
			return obj;
		}
		
	}
}
