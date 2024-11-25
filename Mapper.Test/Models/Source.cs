using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapper.Test.Models
{
    public class Source
    {
        public string StringProp { get; set; }
        public int IntProp { get; set; }
        public int? IntNullableProp { get; set; }
        public float FloatProp { get; set; }
        public double DoubleProp { get; set; }
        public long LongProp { get; set; }
        public DateTimeOffset? DateTimeOffsetNullableProp { get; set; }
        public DateTime DateTimeProp { get; set; }
        public DateTimeOffset DateTimeOffsetProp { get; set; }
        public IEnumerable<Circle> Circles { get; set; }
        public Cat Cat { get; set; }
        public Cat OtherCat { get; set; }
    }
}
