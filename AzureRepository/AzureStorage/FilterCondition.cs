using System;
using System.Collections.Generic;
using System.Text;

namespace Windward.Hub.StorageContract.Filtering
{
    public class FilterCondition : IFilterNode
    {
        public string ColumnName { get; set; }

        public object Value { get; set; }

        public FilterConditionOperator Operator { get; set; } // eg/neq/gt/lt
    }
}
