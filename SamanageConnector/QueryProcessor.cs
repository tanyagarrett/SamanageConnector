using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Query;
using System.Reflection;
using SamanageConnector.Entities;

namespace SamanageConnector
{

	public class QueryProcessor
	{

		private readonly IDictionary<string, string> connectionInfo;
		private readonly ISamanageClient samanageClient = new SamanageClient();
		private const string AccessToken = "Bearer " + info.accessToken;

		public QueryProcessor(IDictionary<string, string> connectionInfo, ISamanageClient SamanageClient)
		{

			this.connectionInfo = connectionInfo;
			this.samanageClient = SamanageClient;

		}


		public IEnumerable<DataEntity> ExecuteQuery(Query query)
		{
			IEnumerable<DataEntity> results = new List<DataEntity>();
			string entityName = query.RootEntity.ObjectDefinitionFullName;

			switch (entityName)
			{

				case "Hardware":
					var Hardware = this.GetHardware();
					IEnumerable<Hardware> filteredHardware = ApplyFilters(Hardware, query);
					results = this.ToDataEntity<Hardware>(filteredHardware);
					break;

				default:
					break;

			}

			return results;

		}

		private IQueryable<Hardware> GetHardware()
		{

			List<Hardware> Hardware = new List<Hardware>();

			var hardware = samanageClient.GetHardware(connectionInfo[AccessToken]);
			return hardware.AsQueryable();

		}

		private IEnumerable<T> ApplyFilters<T>(IQueryable<T> entities, Query query)
		{

			var results = (query.Constraints != null) ? entities.Where(query.ToLinqExpression()) : entities;

			return (query.RootEntity.SequenceList.Count > 0) ? results.OrderBy(query.ToOrderByLinqExpression()) : results;

		}

		private IEnumerable<DataEntity> ToDataEntity<T>(IEnumerable<T> entities)
		{

			var type = typeof(T);
			var fields = type.GetProperties(BindingFlags.Instance |
				BindingFlags.FlattenHierarchy |
				BindingFlags.Public |
				BindingFlags.GetProperty);

			foreach (var entity in entities)
			{

				var dataEntity = new QueryDataEntity
				{
					ObjectDefinitionFullName = type.Name,
					Name = type.Name
				};


				foreach (var field in fields)
				{
					dataEntity.Properties.Add(
						field.Name,
						field.GetValue(entity, null));
				}

				yield return dataEntity.ToDataEntity();

			}

		}

	}
}
