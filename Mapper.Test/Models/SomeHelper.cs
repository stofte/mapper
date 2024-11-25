using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapper.Test.Models
{
    public class SomeHelper
    {
        public static JobTitleEnum MapToJobTitle(int i)
        {
            return (JobTitleEnum) i;
        }
    }
}
