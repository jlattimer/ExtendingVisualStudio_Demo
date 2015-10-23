using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace $safeprojectname$
{
	class GetToken
	{
		public static AuthenticationResult GetAuthContext()
		{
			AuthenticationContext authContext =
				new AuthenticationContext("https://login.windows.net/common", false);

			AuthenticationResult authResult = authContext.AcquireToken(
			   ConfigurationManager.AppSettings["ServiceUrl"],
			   ConfigurationManager.AppSettings["ClientId"],
			   new Uri(ConfigurationManager.AppSettings["RedirectUri"]));
			
			return authResult;
		}
	}
}
