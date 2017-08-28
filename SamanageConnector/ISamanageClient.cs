using SamanageConnector.Entities;
using System;
using System.Collections.Generic;

namespace SamanageConnector
{
	public interface ISamanageClient
	{

		List<Hardware> GetHardware(string accessToken);

		T CallSamanageApi<T>(string uri);

		T CallSamanageApi<T>(string accessToken, string uri);

		U PostSamanageApi<T, U>(string accessToken, string uri, T data);

	}
}
