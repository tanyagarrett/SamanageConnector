using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamanageConnector
{
	class ConnectorSettings
	{
		public const string SettingsUITypeName = "";
		public const string SettingsUIVersion = "1.0";
		public const string ConnectionUITypeName = "ScribeOnline.Views.OAuthConnectionUI";
		public const string ConnectionUIVersion = "1.0";
		public const string XapFileName = "ScribeOnline";
		public const string ConnectorTypeId = "{D83FE2AA-0687-40BA-BDA7-227A778A362F}";
		public const string ConnectorVersion = "1.0";
		public const string Name = "Samanage Connector";
		public const string Description = "Connector for Samanage API";
		public const string MetadataPrefix = "Samanage";
		public const bool SupportsCloud = true;

	}

	public class ConnectionInfo
	{
		public string baseURL = "https://api.samanage.com";
		public string accessToken { get; set; }
	}

}
