using System;
namespace OpenGraal.NpcServer
{
	interface IGServerConnection
	{
		void SendLogin(string Account, string Password, string Nickname);
	}
}
