using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapper.Test.Models
{
    public class Cat : Animal
    {
        public string Name { get; set; }
        public string Color { get; set; }
    }

    public class CatComparer : IEqualityComparer<Cat>
    {
        public bool Equals(Cat? x, Cat? y)
        {
            if (x == null)
            {
                return y == null;
            }
            else if (y == null)

            {
                return x == null;
            }
            else
            {
                return x.Name == y.Name;
            }
        }

        public int GetHashCode([DisallowNull] Cat obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
