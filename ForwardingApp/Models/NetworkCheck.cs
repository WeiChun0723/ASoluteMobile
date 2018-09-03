using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASolute_Mobile
{
	public class NetworkCheck
	{
		public static bool IsInternet()
		{
			if (CrossConnectivity.Current.IsConnected)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}