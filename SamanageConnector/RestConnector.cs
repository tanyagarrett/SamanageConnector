using System;
using System.Collections.Generic;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.Query;
using Scribe.Core.ConnectorApi.ConnectionUI;
using System.Collections.ObjectModel;
using Scribe.Core.ConnectorApi.Common;
using Scribe.Core.ConnectorApi.Logger;
using Scribe.Core.ConnectorApi.Exceptions;

namespace SamanageConnector
{
	[ScribeConnector(
        ConnectorSettings.ConnectorTypeId,
        ConnectorSettings.Name,
        ConnectorSettings.Description,
		typeof(Connector),
        ConnectorSettings.SettingsUITypeName,
        ConnectorSettings.SettingsUIVersion,
        ConnectorSettings.ConnectionUITypeName,
        ConnectorSettings.ConnectionUIVersion,
        ConnectorSettings.XapFileName,
		new[] { "Scribe.IS.Target", "Scribe.IS.Source" },
        ConnectorSettings.SupportsCloud, 
		ConnectorSettings.ConnectorVersion
    )]

	public class Connector : IConnector
	{
		public bool IsConnected { get; private set; }
        //public readonly SamanageClient info = new SamanageClient(); //let's store in its own class
        public readonly ConnectionInfo info = new ConnectionInfo();
        private readonly Guid connectorTypeId = new Guid(ConnectorSettings.ConnectorTypeId);

        protected QueryProcessor QueryProcessor { get; set; }
		protected OperationProcessor OperationProcessor { get; set; }

        public string PreConnect(IDictionary<string, string> properties)
        {
            var form = new FormDefinition
            {
                CompanyName = "Samanage Software",
                CryptoKey = ConnectorSettings.cryptoKey,
                HelpUri = new Uri("http://www.samanage.com/api"),
                Entries =
                    new Collection<EntryDefinition>
                            {
                                new EntryDefinition
                                    {
                                        InputType = InputType.Text,
                                        IsRequired = true,
                                        Label = "API Token",
                                        PropertyName = "accessToken"
                                    },
                            }
            };
            return form.Serialize();
        }

        public void Connect(IDictionary<string, string> properties)
		{

			info.accessToken = properties["AccessToken"];
			this.IsConnected = true;
		}

        public Guid ConnectorTypeId
        {
            get
            {
                return connectorTypeId;
            }
        }

        public void Disconnect()
		{
			this.IsConnected = false;
		}

		public MethodResult ExecuteMethod(MethodInput input)
		{
			throw new NotImplementedException();
		}

		public OperationResult ExecuteOperation(OperationInput input)
		{
			OperationResult operationResult;

			using (new LogMethodExecution("Rest CDK", "Rest.ExecuteOperation()"))
			{

				try
				{
					operationResult = this.OperationProcessor.ExecuteOperation(input);
					LogOperationResults(operationResult);
				}
				catch (Exception ex)
				{
					var message = string.Format("{0} {1}", "Error!", ex.Message);
					Logger.Write(Logger.Severity.Error, "Rest CDK", message);
					throw new InvalidExecuteOperationException(message);
				}

			}

			return operationResult;

		}

		public IEnumerable<DataEntity> ExecuteQuery(Query query)
		{
			using (new LogMethodExecution("Rest CDK", "RestConnector.Connect()"))
			{

				IEnumerable<DataEntity> entities = null;

				try
				{
                    //pass in the info.accessToken here?
					entities = QueryProcessor.ExecuteQuery(query);
				}
				catch (Exception exception)
				{

					//Write the exception to the log: 
					var message = string.Format("{0} {1}", "Adapter Error", exception.Message);
					Logger.Write(Logger.Severity.Error, "Rest CDK", message);
					throw new InvalidExecuteQueryException(message);

				}

				return entities;
			}
		}

		private MetadataProvider metadataProvider = new MetadataProvider();
		public IMetadataProvider GetMetadataProvider()
		{
			return this.metadataProvider;
		}

		private void LogOperationResults(OperationResult operationResult)
		{
			string message;
			Logger.Severity severity;

			if (operationResult.Success[0])
			{
				message = "Successful operation";
				severity = Logger.Severity.Debug;
			}
			else
			{

				ErrorResult errorInfo = operationResult.ErrorInfo[0];
				if (errorInfo.Number == ErrorNumber.DuplicateUniqueKey)
				{

					message = string.Format("{0}{2}Number:{1}{2}Description:{3}{2}",
																						"Warning!", errorInfo.Number,
																						Environment.NewLine,
																						errorInfo.Description);
					severity = Logger.Severity.Warning;
				}
				else
				{

					message = string.Format("{0}{2}Error Number:{1}{2}Error Description:{3}{2}Error Detail:{4}",
																						"Error!", errorInfo.Number,
																						Environment.NewLine,
																						errorInfo.Description, errorInfo.Detail);
					severity = Logger.Severity.Error;
				}
			}
			Logger.Write(severity, "Rest CDK", message);
		}
	}
}
