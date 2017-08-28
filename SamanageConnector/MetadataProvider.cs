using System;
using System.Collections.Generic;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;
using Scribe.Core.ConnectorApi.Logger;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using SamanageConnector.Entities;

namespace SamanageConnector
{

	public class MetadataProvider : IMetadataProvider
	{
		private Dictionary<string, Type> EntityCollection = new Dictionary<string, Type>();

		public MetadataProvider()
		{

			EntityCollection = PopulateEntityCollection();

		}

		public void ResetMetadata()
		{
			return;
		}

		public IEnumerable<IActionDefinition> RetrieveActionDefinitions()
		{

			using (LogMethodExecution logger = new LogMethodExecution("Rest CDK", "RetrieveActionDefinitions"))
			{
				var actionDefinitions = new List<IActionDefinition>();
				var createDef = new ActionDefinition
				{
					SupportsInput = true,
					KnownActionType = KnownActions.Create,
					SupportsBulk = false,
					FullName = KnownActions.Create.ToString(),
					Name = KnownActions.Create.ToString(),
					Description = string.Empty
				};

				actionDefinitions.Add(createDef);

				var queryDef = new ActionDefinition
				{
					SupportsConstraints = true,
					SupportsRelations = false,
					SupportsLookupConditions = false,
					SupportsSequences = false,
					KnownActionType = KnownActions.Query,
					SupportsBulk = false,
					Name = KnownActions.Query.ToString(),
					FullName = KnownActions.Query.ToString(),
					Description = string.Empty
				};

				actionDefinitions.Add(queryDef);

				return actionDefinitions;
			}
		}

		public IMethodDefinition RetrieveMethodDefinition(string objectName, bool shouldGetParameters = false)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IMethodDefinition> RetrieveMethodDefinitions(bool shouldGetParameters = false)
		{
			throw new NotImplementedException();
		}


		public IEnumerable<IObjectDefinition> RetrieveObjectDefinitions(bool shouldGetProperties = false, bool shouldGetRelations = false)
		{

			using (LogMethodExecution logger = new LogMethodExecution("Rest CDK", "RetrieveObjectDefinitions"))
			{
				foreach (var entityType in EntityCollection)
				{
					yield return RetrieveObjectDefinition(entityType.Key, shouldGetProperties, shouldGetRelations);
				}
			}
		}

		public IObjectDefinition RetrieveObjectDefinition(string objectName, bool shouldGetProperties = false, bool shouldGetRelations = false)
		{

			IObjectDefinition objectDefinition = null;

			using (LogMethodExecution logger = new LogMethodExecution("Rest CDK", "RetrieveObjectDefinition"))
			{

				if (EntityCollection.Count > 0)
				{
					foreach (var keyValuePair in EntityCollection)
					{
						if (keyValuePair.Key == objectName)
						{
							Type entityType = keyValuePair.Value;
							if (entityType != null)
							{
								objectDefinition = GetObjectDefinition(entityType, shouldGetProperties);
							}
						}
					}
				}

			}

			return objectDefinition;

		}

		public void Dispose()
		{
			return;
		}


		private IObjectDefinition GetObjectDefinition(Type entityType, bool shouldGetFields)
		{

			IObjectDefinition objectDefinition = null;

			objectDefinition = new ObjectDefinition
			{
				Name = entityType.Name,
				FullName = entityType.Name,
				Description = string.Empty,
				Hidden = false,
				RelationshipDefinitions = new List<IRelationshipDefinition>(),
				PropertyDefinitions = new List<IPropertyDefinition>(),
				SupportedActionFullNames = new List<string>()
			};

			objectDefinition.SupportedActionFullNames.Add("Query");

			if (shouldGetFields)
			{
				objectDefinition.PropertyDefinitions = GetFieldDefinitions(entityType);
			}

			return objectDefinition;

		}


		private List<IPropertyDefinition> GetFieldDefinitions(Type entityType)
		{

			var fields = new List<IPropertyDefinition>();

			var fieldsFromType = entityType.GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy |
				BindingFlags.Public | BindingFlags.GetProperty);

			foreach (var field in fieldsFromType)
			{

				var propertyDefinition = new PropertyDefinition
				{
					Name = field.Name,
					FullName = field.Name,
					PropertyType = field.PropertyType.ToString(),
					PresentationType = field.PropertyType.ToString(),
					Nullable = false,
					IsPrimaryKey = false,
					UsedInQueryConstraint = true,
					UsedInQuerySelect = true,
					UsedInActionOutput = true,
					UsedInQuerySequence = true,
					Description = field.Name,
				};


				foreach (var attribute in field.GetCustomAttributes(false))
				{

					if (attribute is ReadOnlyAttribute)
					{
						var readOnly = (ReadOnlyAttribute)attribute;
						propertyDefinition.UsedInActionInput = readOnly == null || !readOnly.IsReadOnly;
					}

					if (attribute is RequiredAttribute)
					{
						propertyDefinition.RequiredInActionInput = true;
					}

					if (attribute is KeyAttribute)
					{
						propertyDefinition.UsedInLookupCondition = true;
					}

				}

				fields.Add(propertyDefinition);

			}

			return fields;

		}


		private Dictionary<string, Type> PopulateEntityCollection()
		{

			Dictionary<string, Type> entities = new Dictionary<string, Type>();

			entities.Add("Hardware", typeof(Hardware));

			return entities;

		}
	}
}

