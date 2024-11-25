using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapper.Test.Models
{
    public class UserInfoTarget
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? Age { get; set; }
        public string Address { get; set; }
        public string? Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool? Active { get; set; }
        public decimal Salary { get; set; }
        public string Department { get; set; }
        public string City { get; set; }
        public string? Country { get; set; }
        public JobTitleEnum JobTitle { get; set; }
        public DateTime? LastLogin { get; set; }
        public string Education { get; set; }
        public string? Notes { get; set; }
        public bool IsEmployed { get; set; }
        public long ExperienceInYears { get; set; }
        public string? Nationality { get; set; }
        public Uri? Web { get; set; }
    }
}
