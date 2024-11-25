using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapper.Test.Models
{
    public class Animal
    {
        public int Height { get; set; }
    }

    public class AnimalComparer : IEqualityComparer<Animal>
    {
        public bool Equals(Animal? x, Animal? y)
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
                return x.Height == y.Height;
            }
        }

        public int GetHashCode([DisallowNull] Animal obj)
        {
            return obj.Height.GetHashCode();
        }
    }
}
