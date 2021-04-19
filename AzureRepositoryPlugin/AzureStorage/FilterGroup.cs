using System;
using System.Collections.Generic;
using System.Text;

namespace Windward.Hub.StorageContract.Filtering
{
    public class FilterGroup: IFilterNode
    {
        public FilterGroupOperator Operator { get; set; } // all/any

        public List<IFilterNode> Children { get; set; }
    }
}
