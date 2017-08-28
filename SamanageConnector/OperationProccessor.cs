using System;
using System.Collections.Generic;
using System.Linq;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi;
using System.Reflection;

namespace SamanageConnector
{

	public class OperationProcessor
	{

		private readonly ISamanageClient _samanageClient;
		private readonly IDictionary<string, string> _connectionInfo;

		public OperationProcessor(ISamanageClient samanageClient, IDictionary<string, string> connectionInfo)
		{
			this._samanageClient = samanageClient;
			this._connectionInfo = connectionInfo;
		}


		public OperationResult ExecuteOperation(OperationInput operationInput)
		{

			var operationResult = new OperationResult();
			var operationSuccess = new List<bool>();
			var entitiesAffected = new List<int>();
			var errorList = new List<ErrorResult>();
			var entities = new List<DataEntity>();

			operationResult.Success = operationSuccess.ToArray();
			operationResult.ObjectsAffected = entitiesAffected.ToArray();
			operationResult.ErrorInfo = errorList.ToArray();
			operationResult.Output = entities.ToArray();

			return operationResult;

		}


		private T EntityToObject<T>(DataEntity scribeEntity) where T : new()
		{

			var restEntity = new T();

			var fieldInfo =
				typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public
				| BindingFlags.GetProperty).ToDictionary(key => key.Name, value => value);

			var matchingFieldValues = scribeEntity.Properties.Where(field => fieldInfo.ContainsKey(field.Key));

			foreach (var field in matchingFieldValues)
			{
				fieldInfo[field.Key].SetValue(restEntity, field.Value, null);
			}

			return restEntity;

		}

	}
}
