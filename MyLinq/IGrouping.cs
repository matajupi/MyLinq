using System.Collections.Generic;
using System.Collections;

namespace MyLinq
{
    public interface IGrouping<out TKey, out TElement> : IEnumerable<TElement>
    {
        
    }
}