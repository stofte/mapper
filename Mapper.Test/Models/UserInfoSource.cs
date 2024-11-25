using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapper.Test.Models
{
    public class UserInfoSource
    {
        public Guid PK { get; set; }
        public string FullName { get; set; }
        public DateTime DateCreated { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
        public string? Email { get; set; }
        public string Phone { get; set; }
        public bool? IsActive { get; set; }
        public decimal Salary { get; set; }
        public string Department { get; set; }
        public string City { get; set; }
        public string? Country { get; set; }
        public int JobTitle { get; set; }
        public DateTime? LastLogin { get; set; }
        public string Education { get; set; }
        public string? Notes { get; set; }
        public bool IsEmployed { get; set; }
        public long ExperienceInYears { get; set; }
        public string? Nationality { get; set; }
        public string Uri { get; set; }
    }
}
