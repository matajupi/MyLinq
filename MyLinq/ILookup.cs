using System.Collections.Generic;
using System.Collections;

namespace MyLinq
{
    public interface ILookup<TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>
    {
        
    }
}