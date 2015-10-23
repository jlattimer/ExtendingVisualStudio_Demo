using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace SampleProjectBase
{
	class Program
	{
		static void Main(string[] args)
		{
			AuthenticationResult authResult = GetToken.GetAuthContext();

			UseToken.MakeRequest(ConfigurationManager.AppSettings["ServiceUrl"],
				authResult.AccessToken);
		}
	}
}
