using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapper.Test.Models
{
    public class Target
    {
        public string StringProp { get; set; }
        public int IntField;
        public int IntProp { get; set; }
        public int IntPropReadOnly { get; } = 117;
        public int? IntNullable { get; set; }
        public float FloatProp { get; set; }
        public double DoubleProp { get; set; }
        public long LongProp { get; set; }
        public DateTime DateTimeProp { get; set; }
        public DateTime? DateTimeNullableProp { get; set; }
        public Point[] Points { get; set; }
    }
}
