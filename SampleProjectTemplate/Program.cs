using System;
using System.Collections.Generic;
$if$ ($targetframeworkversion$ >= 3.5)using System.Linq;
$endif$using System.Text;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace $safeprojectname$
{
	public class TestClass
	{
		static void Main(string[] args)
		{
			AuthenticationResult authResult = GetToken.GetAuthContext();

			UseToken.MakeRequest(ConfigurationManager.AppSettings["ServiceUrl"],
				authResult.AccessToken);
		}
	}
}
