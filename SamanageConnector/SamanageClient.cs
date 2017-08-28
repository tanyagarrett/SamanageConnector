using System;
using System.Collections.Generic;
using System.Linq;
using SamanageConnector.Entities;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Reflection;

namespace SamanageConnector
{
	public class SamanageClient : ISamanageClient
	{
		internal const string OAuthHeader = "OAuth oauth_token={0}";
		internal const string computerListUri = "https://api.samanage.com/hardwares.json";

		public List<Hardware> GetHardware(string accessToken)
		{
			var hardwareUri = string.Format(computerListUri);
			var hardwareResponses = CallSamanageApi<List<HardwareResponse>>(accessToken, computerListUri);
			var hardware = new List<Hardware>();

			foreach (var response in hardwareResponses)
			{
				hardware.Add(
					new Hardware(response));
			}

			return hardware.ToList();

		}

		public T CallSamanageApi<T>(string uri)
		{
			return CallSamanageApi<T>(string.Empty, uri);
		}


		public T CallSamanageApi<T>(string accessToken, string uri)
		{

			var request = WebRequest.Create(uri) as HttpWebRequest;
			request.Method = "GET";
			request.ContentType = "application/json";

			var responseObject = default(T);


			if (!string.IsNullOrEmpty(accessToken))
			{
				request.Headers.Add("Authorization", string.Format(OAuthHeader, accessToken));
			}

			using (var response = request.GetResponse() as HttpWebResponse)
			{

				if (response.StatusCode == HttpStatusCode.OK)
				{
					var jsonSerializer = new DataContractJsonSerializer(typeof(T));
					responseObject = (T)jsonSerializer.ReadObject(response.GetResponseStream());
				}
				else
				{
					//TODO: Handle HttStatusCode != OK
				}

			}

			return responseObject;

		}


		public U PostSamanageApi<T, U>(string accessToken, string uri, T data)
		{

			try
			{

				var request = WebRequest.Create(uri) as HttpWebRequest;
				request.Method = "POST";
				request.ContentType = "application/json";
				if (!string.IsNullOrEmpty(accessToken))
				{
					request.Headers.Add("Authorization", string.Format(OAuthHeader, accessToken));
				}

				var jsonSerializer = new DataContractJsonSerializer(typeof(T));
				jsonSerializer.WriteObject(request.GetRequestStream(), data);

				var auth = string.Format("Authorization: " + OAuthHeader, accessToken);

				using (var response = request.GetResponse() as HttpWebResponse)
				{

					if (response != null && response.StatusCode != HttpStatusCode.Created)
					{
						throw new WebException("Unable to create entity " + response.StatusDescription);
					}

					var jsonDeserializer = new DataContractJsonSerializer(typeof(U));
					var responseObject = (U)jsonDeserializer.ReadObject(response.GetResponseStream());

					return responseObject;
				}

			}
			catch (Exception ex)
			{
				throw new WebException("Unable to create entity", ex.InnerException);
			}

		}


		private PropertyInfo[] GetEntityFields<T>(T entity)
		{
			var type = typeof(T);
			var properties = type.GetProperties(BindingFlags.Instance |
				BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.GetProperty);

			return properties;

		}

	}
}

