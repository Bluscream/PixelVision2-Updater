using System;
using System.Net;
using System.Windows.Forms;

namespace Unzipper
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
            // Fix for The request was aborted: Could not create SSL/TLS secure channel. at System.Net.HttpWebRequest.GetResponse()
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
	}
}
