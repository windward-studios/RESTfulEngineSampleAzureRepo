using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

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

        //public static string BuildFilterString(FilterGroup filterGroup)
        //{
        //    if (filterGroup.Children.Count < 1)
        //    {
        //        return string.Empty;
        //    }

        //    string retFilter = BuildFilterString(filterGroup.Children[0]);
        //    if (filterGroup.Children.Count <= 1)
        //    {
        //        return retFilter;
        //    }

        //    string tableOperator = filterGroup.Operator switch
        //    {
        //        FilterGroupOperator.All => TableOperators.And,
        //        FilterGroupOperator.Any => TableOperators.Or,
        //        _ => throw new ArgumentOutOfRangeException("Unknown filter goup oporator.")
        //    };
        //    for (int i = 1; i < filterGroup.Children.Count; i++)
        //    {
        //        string childFilterStr = BuildFilterString(filterGroup.Children[i]);
        //        retFilter = TableQuery.CombineFilters(retFilter, tableOperator, childFilterStr);
        //    }
        //    return retFilter;
        //}

        //public static string BuildFilterString(FilterCondition filterCondition)
        //{
        //    string filterOperator = filterCondition.Operator switch
        //    {
        //        FilterConditionOperator.Equal => QueryComparisons.Equal,
        //        FilterConditionOperator.NotEqual => QueryComparisons.NotEqual,
        //        FilterConditionOperator.GreaterThan => QueryComparisons.GreaterThan,
        //        FilterConditionOperator.LessThan => QueryComparisons.LessThan,
        //        _ => throw new ArgumentOutOfRangeException()
        //    };

        //    object value = filterCondition.Value;
        //    return value switch
        //    {
        //        bool boolValue => TableQuery.GenerateFilterConditionForBool(filterCondition.ColumnName, filterOperator,
        //            boolValue),
        //        DateTime dateTimeVal => TableQuery.GenerateFilterConditionForDate(filterCondition.ColumnName,
        //            filterOperator, dateTimeVal),
        //        double doubleValue => TableQuery.GenerateFilterConditionForDouble(filterCondition.ColumnName,
        //            filterOperator, doubleValue),
        //        float floatValue => TableQuery.GenerateFilterConditionForDouble(filterCondition.ColumnName, filterOperator, floatValue),
        //        Guid guidValue => TableQuery.GenerateFilterConditionForGuid(filterCondition.ColumnName, filterOperator,
        //            guidValue),
        //        int intValue => TableQuery.GenerateFilterConditionForInt(filterCondition.ColumnName, filterOperator,
        //            intValue),
        //        _ => TableQuery.GenerateFilterCondition(filterCondition.ColumnName, filterOperator, value.ToString())
        //    };
        //}

        //public static FilterGroup AddOrgPartitionFilter(Guid orgId, FilterGroup filterGroup = null)
        //{
        //    var partitionKeyCondition = new FilterCondition
        //    {
        //        ColumnName = PARTITION_KEY,
        //        Operator = FilterConditionOperator.Equal,
        //        Value = orgId.ToString() // stored in db as a string not a guid, so we need to convert so we don't get a 400 error
        //    };
        //    FilterGroup returnGroup = filterGroup;
        //    if (returnGroup == null)
        //    {
        //        returnGroup = new FilterGroup
        //        {
        //            Children = new List<IFilterNode>()
        //            {
        //                partitionKeyCondition
        //            },
        //            Operator = FilterGroupOperator.All
        //        };
        //    }
        //    else if (returnGroup.Operator == FilterGroupOperator.All)
        //    {
        //        returnGroup.Children.Add(partitionKeyCondition);
        //    }
        //    else
        //    {
        //        returnGroup = new FilterGroup
        //        {
        //            Children = new List<IFilterNode>() { filterGroup, partitionKeyCondition },
        //            Operator = FilterGroupOperator.All
        //        };
        //    }

        //    return returnGroup;
        //}

        //public static string BuildFilterString(IFilterNode filterNode)
        //{
        //    return filterNode switch
        //    {
        //        FilterCondition filterCondition => BuildFilterString(filterCondition),
        //        FilterGroup filterGroup => BuildFilterString(filterGroup),
        //        _ => filterNode.ToString() // bug bug
        //    };
        //}
        #endregion
    }
}
