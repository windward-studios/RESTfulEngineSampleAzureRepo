using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;
using Windward.Hub.StorageContract.Filtering;

namespace AzureStorage.Tables
{
    public class CloudTableUtils
    {
        #region Table Query Helper Methods
        public const string PARTITION_KEY = "PartitionKey";
        public const string ROW_KEY = "RowKey";

        public static string Equal(string a, string b)
        {
            return TableQuery.GenerateFilterCondition(a, QueryComparisons.Equal, b);
        }

        public static string EqualBool(string a, bool b)
        {
            return TableQuery.GenerateFilterConditionForBool(a, QueryComparisons.Equal, b);
        }

        public static string EqualGuid(string a, Guid b)
        {
            return TableQuery.GenerateFilterConditionForGuid(a, QueryComparisons.Equal, b);
        }

        public static string EqualInt(string a, int b)
        {
            return TableQuery.GenerateFilterConditionForInt(a, QueryComparisons.Equal, b);
        }

        public static string And(string a, string b)
        {
            return TableQuery.CombineFilters(a, TableOperators.And, b);
        }

        public static string Or(string a, string b)
        {
            return TableQuery.CombineFilters(a, TableOperators.Or, b);
        }

        public static string BuildFilterString(FilterGroup filterGroup)
        {
            if (filterGroup.Children.Count < 1)
            {
                return string.Empty;
            }

            string retFilter = BuildFilterString(filterGroup.Children[0]);
            if (filterGroup.Children.Count <= 1)
            {
                return retFilter;
            }

            string tableOperator = "";
            switch(filterGroup.Operator)
            {
                case FilterGroupOperator.All:
                    tableOperator = TableOperators.And;
                    break;
                case FilterGroupOperator.Any:
                    tableOperator = TableOperators.Or;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown filter group operator.");
            }

            for (int i = 1; i < filterGroup.Children.Count; i++)
            {
                string childFilterStr = BuildFilterString(filterGroup.Children[i]);
                retFilter = TableQuery.CombineFilters(retFilter, tableOperator, childFilterStr);
            }
            return retFilter;
        }

        public static string BuildFilterString(FilterCondition filterCondition)
        {
            string filterOperator = "";
            switch(filterCondition.Operator)
            {
                case FilterConditionOperator.Equal:
                    filterOperator = QueryComparisons.Equal;
                    break;
                case FilterConditionOperator.NotEqual:
                    filterOperator = QueryComparisons.NotEqual;
                    break;
                case FilterConditionOperator.GreaterThan:
                    filterOperator = QueryComparisons.GreaterThan;
                    break;
                case FilterConditionOperator.LessThan:
                    filterOperator = QueryComparisons.LessThan;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();

            }

            object value = filterCondition.Value;

            switch(value)
            {
                case bool boolValue:
                    return TableQuery.GenerateFilterConditionForBool(filterCondition.ColumnName, filterOperator,
                    boolValue);
                case DateTime dateTimeVal:
                    return TableQuery.GenerateFilterConditionForDate(filterCondition.ColumnName,
                    filterOperator, dateTimeVal);
                case double doubleValue:
                    return TableQuery.GenerateFilterConditionForDouble(filterCondition.ColumnName,
                    filterOperator, doubleValue);
                case float floatValue:
                    return TableQuery.GenerateFilterConditionForDouble(filterCondition.ColumnName, filterOperator, floatValue);
                case Guid guidValue:
                    return TableQuery.GenerateFilterConditionForGuid(filterCondition.ColumnName, filterOperator,
                    guidValue);
                case int intValue:
                    return TableQuery.GenerateFilterConditionForInt(filterCondition.ColumnName, filterOperator, intValue);
                default:
                    return TableQuery.GenerateFilterCondition(filterCondition.ColumnName, filterOperator, value.ToString());
            }
        }

        public static FilterGroup AddOrgPartitionFilter(Guid orgId, FilterGroup filterGroup = null)
        {
            var partitionKeyCondition = new FilterCondition
            {
                ColumnName = PARTITION_KEY,
                Operator = FilterConditionOperator.Equal,
                Value = orgId.ToString() // stored in db as a string not a guid, so we need to convert so we don't get a 400 error
            };
            FilterGroup returnGroup = filterGroup;
            if (returnGroup == null)
            {
                returnGroup = new FilterGroup
                {
                    Children = new List<IFilterNode>()
                    {
                        partitionKeyCondition
                    },
                    Operator = FilterGroupOperator.All
                };
            }
            else if (returnGroup.Operator == FilterGroupOperator.All)
            {
                returnGroup.Children.Add(partitionKeyCondition);
            }
            else
            {
                returnGroup = new FilterGroup
                {
                    Children = new List<IFilterNode>() { filterGroup, partitionKeyCondition },
                    Operator = FilterGroupOperator.All
                };
            }

            return returnGroup;
        }

        public static string BuildFilterString(IFilterNode filterNode)
        {
            switch(filterNode)
            {
                case FilterCondition filterCondition:
                    return BuildFilterString(filterCondition);
                case FilterGroup filterGroup:
                    return BuildFilterString(filterGroup);
                default:
                    return filterNode.ToString();
            }
        }
        #endregion
    }
}
