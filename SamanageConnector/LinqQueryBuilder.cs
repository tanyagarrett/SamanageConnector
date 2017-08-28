using System;
using System.Text;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Query;
using System.Globalization;

namespace SamanageConnector
{

	public static class LinqQueryBuilder
	{

		public static string ToLinqExpression(this Query query)
		{

			var whereClause = new StringBuilder();
			if (query.Constraints != null)
			{
				ParseWhereClause(whereClause, query.Constraints);
			}

			return whereClause.ToString();

		}


		public static string ToOrderByLinqExpression(this Query query)
		{
			var orderByStatement = string.Empty;
			var queryEntity = query.RootEntity;
			var orderByBuilder = new StringBuilder();

			if (queryEntity != null && queryEntity.SequenceList.Count > 0)
			{

				foreach (var sequence in queryEntity.SequenceList)
				{

					orderByBuilder.Append(string.Format("{0} {1}, ",
						sequence.PropertyName, sequence.Direction.ToString()));

				}

				orderByStatement = orderByBuilder.ToString().Substring(
					0, orderByBuilder.ToString().Length - 2);

			}

			return orderByStatement;

		}


		private static void ParseWhereClause(StringBuilder whereClause, Expression lookupCondition)
		{

			if (lookupCondition == null)
			{
				return;
			}


			switch (lookupCondition.ExpressionType)
			{

				case ExpressionType.Comparison:
					var comparisonExpression = lookupCondition as ComparisonExpression;
					var comparisonBuilder = new StringBuilder();
					if (comparisonExpression == null)
					{
						throw new InvalidOperationException("This isn't a valid operation.");
					}

					switch (comparisonExpression.Operator)
					{

						case ComparisonOperator.Equal:
						case ComparisonOperator.Greater:
						case ComparisonOperator.GreaterOrEqual:
						case ComparisonOperator.IsNotNull:
						case ComparisonOperator.IsNull:
						case ComparisonOperator.Less:
						case ComparisonOperator.LessOrEqual:
						case ComparisonOperator.NotEqual:

							comparisonBuilder.Append(GetLeftFormattedComparisonValue(comparisonExpression.LeftValue));

							ParseNullOperators(comparisonExpression);

							comparisonBuilder.AppendFormat(" {0} ", ParseOperator(comparisonExpression.Operator));

							comparisonBuilder.Append(
								OperatorHasRightValue(comparisonExpression.Operator)
								? GetRightFormattedComparisonValue(comparisonExpression.RightValue)
								: "null");
							break;

						case ComparisonOperator.Like:
							comparisonBuilder.Append(BuildLike(comparisonExpression));
							break;

						case ComparisonOperator.NotLike:
							comparisonBuilder.Append(string.Format("!{0}", BuildLike(comparisonExpression)));
							break;

						default:
							throw new NotSupportedException("Operation not supported");
					}

					whereClause.Append(comparisonBuilder.ToString());
					break;

				case ExpressionType.Logical:
					var logicalExpression = lookupCondition as LogicalExpression;

					if (logicalExpression == null)
					{
						throw new InvalidOperationException("This isn't a valid operation");
					}

					ParseWhereClause(whereClause, logicalExpression.LeftExpression);

					switch (logicalExpression.Operator)
					{

						case LogicalOperator.And:
							whereClause.Append(" && ");
							break;

						case LogicalOperator.Or:
							whereClause.Append(" || ");
							break;

						default:
							throw new NotSupportedException(string.Format("Logical operator {0} not supported", logicalExpression.Operator.ToString()));

					}

					ParseWhereClause(whereClause, logicalExpression.RightExpression);

					break;

				default:
					break;
			}
		}


		private static string BuildLike(ComparisonExpression comparisonExpression)
		{
			const string format = "Regex.IsMatch({0} != null ? {0} :\"\", {1}, RegexOptions.IgnoreCase)";

			string returnString = string.Format(format, comparisonExpression.LeftValue.Value.ToString().Split('.')[1],
				Quote(string.Format("^{0}$",
				comparisonExpression.RightValue.Value.ToString().Replace("%", ".*"))));

			return returnString;

		}


		private static void ParseNullOperators(ComparisonExpression comparisonExpression)
		{
			if (comparisonExpression.RightValue == null ||
					comparisonExpression.RightValue.Value == null)
			{
				switch (comparisonExpression.Operator)
				{
					case ComparisonOperator.Equal:
						comparisonExpression.Operator = ComparisonOperator.IsNull;
						break;

					case ComparisonOperator.NotEqual:
						comparisonExpression.Operator = ComparisonOperator.IsNotNull;
						break;

					case ComparisonOperator.IsNotNull:
					case ComparisonOperator.IsNull:
						break;

					default:
						throw new NotSupportedException("This operation is not supported");
				}
			}
		}


		private static string ParseOperator(ComparisonOperator @comparisonOperator)
		{
			string operation;

			switch (@comparisonOperator)
			{

				case ComparisonOperator.Greater:
					operation = ">";
					break;

				case ComparisonOperator.GreaterOrEqual:
					operation = ">=";
					break;

				case ComparisonOperator.NotEqual:
				case ComparisonOperator.IsNotNull:
					operation = "!=";
					break;

				case ComparisonOperator.Equal:
				case ComparisonOperator.IsNull:
					operation = "==";
					break;

				case ComparisonOperator.Less:
					operation = "<";
					break;

				case ComparisonOperator.LessOrEqual:
					operation = "<=";
					break;

				default:
					throw new NotSupportedException("Operation is not supported");
			}

			return operation;

		}


		private static bool OperatorHasRightValue(ComparisonOperator @comparisonOperator)
		{
			var isLeft = @comparisonOperator == ComparisonOperator.IsNull || @comparisonOperator == ComparisonOperator.IsNotNull;

			return !isLeft;

		}


		private static string GetRightFormattedComparisonValue(ComparisonValue comparisonValue)
		{

			var isValueDate = (comparisonValue.Value is DateTime);
			var value = Convert.ToString(comparisonValue.Value, CultureInfo.InvariantCulture);
			string result;

			if (isValueDate)
			{

				var dateTimeValue = ((DateTime)(comparisonValue.Value));
				value = dateTimeValue.ToString("o");

				result = string.Format(
					"DateTime.Parse({0}, null, DateTimeStyles.RoundtripKind)",
					Quote(string.Format("{0}", value)));

			}
			else if (comparisonValue.ValueType == ComparisonValueType.Constant)
			{

				result = comparisonValue.Value is string
					? string.Format("{0}", Quote(value))
					: value;
			}
			else
			{

				result = value;
			}

			return result;

		}


		private static string GetLeftFormattedComparisonValue(ComparisonValue comparisonValue)
		{
			var formattedValue = new StringBuilder();

			if (comparisonValue.ValueType == ComparisonValueType.Property)
			{
				var propertyParts = comparisonValue.Value.ToString().Split('.');
				var propertyName = propertyParts[propertyParts.Length - 1];

				formattedValue.AppendFormat("{0}", propertyName);
			}
			else
			{

				formattedValue.Append(
					string.Format(comparisonValue.ValueType == ComparisonValueType.Constant ? Quote("{0}") : "{0}",
					comparisonValue.Value));
			}

			return formattedValue.ToString();

		}


		private static string Quote(string value)
		{
			return string.Format("\"{0}\"", value);
		}

	}
}
