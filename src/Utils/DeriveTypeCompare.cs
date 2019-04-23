using System;
using System.Collections.Generic;

namespace ESerializer.Utils
{
    public class DeriveTypeCompare : IComparer<Type>
    {
        public int Compare(Type x, Type y)
        {
            if (x == y)
            {
                return 0;
            }
            else if (x.IsSubclassOf(y))
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }
}
