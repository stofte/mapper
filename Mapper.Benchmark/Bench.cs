using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Bogus;
using Mapper.Test.Models;

namespace Mapper.Benchmark
{
    public class Bench
    {
        UserInfoSource source;
        UserInfoTarget targetMapper = new UserInfoTarget();
        UserInfoTarget targetManual = new UserInfoTarget();
        Mapper<UserInfoSource, UserInfoTarget> mapper;

        [GlobalSetup]
        public void SetupData()
        {
            source = new Faker<UserInfoSource>().StrictMode(true)
                .RuleFor(x => x.Address, f => f.Address.FullAddress())
                .RuleFor(x => x.Age, f => f.Random.Int(18, 100))
                .RuleFor(x => x.City, f => f.Address.City())
                .RuleFor(x => x.Country, f => f.Address.Country())
                .RuleFor(x => x.DateCreated, f => f.Date.Past())
                .RuleFor(x => x.Department, f => f.Random.AlphaNumeric(20))
                .RuleFor(x => x.Education, f => f.Random.AlphaNumeric(20))
                .RuleFor(x => x.Email, f => f.Internet.Email())
                .RuleFor(x => x.ExperienceInYears, f => f.Random.Long(min: 0, max: 80))
                .RuleFor(x => x.PK, f => f.Random.Guid())
                .RuleFor(x => x.IsActive, f => f.Random.Bool())
                .RuleFor(x => x.IsEmployed, f => f.Random.Bool())
                .RuleFor(x => x.JobTitle, f => f.Random.Int(min: 0, max: 3))
                .RuleFor(x => x.LastLogin, f => f.Date.Past())
                .RuleFor(x => x.FullName, f => f.Name.FullName())
                .RuleFor(x => x.Nationality, f => f.Address.Country())
                .RuleFor(x => x.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(x => x.Salary, f => f.Random.Decimal(min: 0, max: 10000000))
                .RuleFor(x => x.Notes, f => f.Lorem.Lines(f.Random.Int(10, 10000)))
                .RuleFor(x => x.Uri, f => f.Internet.Url())
                .Generate();
            
            Func<string, Uri?> uriFunc = (string s) =>
            {
                if (Uri.TryCreate(s, new UriCreationOptions(), out Uri? uriResult))
                {
                    return uriResult;
                }
                return null;
            };

            mapper = new Mapper<UserInfoSource, UserInfoTarget>()
                .ForMember(x => x.Address, x => x.Address)
                .ForMember(x => x.Age, x => x.Age)
                .ForMember(x => x.City, x => x.City)
                .ForMember(x => x.CreatedAt, x => x.DateCreated)
                .ForMember(x => x.Department, x => x.Department)
                .ForMember(x => x.Education, x => x.Education)
                .ForMember(x => x.Email, x => x.Email)
                .ForMember(x => x.ExperienceInYears, x => x.ExperienceInYears)
                .ForMember(x => x.Id, x => x.PK)
                .ForMember(x => x.Active, x => x.IsActive)
                .ForMember(x => x.IsEmployed, x => x.IsEmployed)
                .ForMember(x => x.JobTitle, x => SomeHelper.MapToJobTitle(x.JobTitle))
                .ForMember(x => x.LastLogin, x => x.LastLogin)
                .ForMember(x => x.Name, x => x.FullName)
                .ForMember(x => x.Nationality, x => x.Nationality)
                .ForMember(x => x.PhoneNumber, x => x.Phone)
                .ForMember(x => x.Salary, x => x.Salary)
                .ForMember(x => x.Notes, x => x.Notes)
                .ForMember(x => x.Web, x => uriFunc(x.Uri))
                .Build();
        }

        [Benchmark]
        public void Expression_Based_Mapping_Code()
        {
            var changed = mapper.Map(source, targetMapper);
        }

        [Benchmark]
        public void Manually_Written_Mapping_Code()
        {
            var changed = ManualMapper(source, targetManual);
        }

        private bool ManualMapper(UserInfoSource s, UserInfoTarget t)
        {
            var changed = false;
            if (t.Address != s.Address)
            {
                changed = true;
                t.Address = s.Address;
            }
            if (t.Age != s.Age)
            {
                changed = true;
                t.Age = s.Age;
            }
            if (t.City != s.City)
            {
                changed = true;
                t.City = s.City;
            }
            if (t.CreatedAt != s.DateCreated)
            {
                changed = true;
                t.CreatedAt = s.DateCreated;
            }
            if (t.Department != s.Department)
            {
                changed = true;
                t.Department = s.Department;
            }
            if (t.Education != s.Education)
            {
                changed = true;
                t.Education = s.Education;
            }
            if (t.Email != s.Email)
            {
                changed = true;
                t.Email = s.Email;
            }
            if (t.ExperienceInYears != s.ExperienceInYears)
            {
                changed = true;
                t.ExperienceInYears = s.ExperienceInYears;
            }
            if (t.Id != s.PK)
            {
                changed = true;
                t.Id = s.PK;
            }
            if (t.Active != s.IsActive)
            {
                changed = true;
                t.Active = s.IsActive;
            }
            if (t.IsEmployed != s.IsEmployed)
            {
                changed = true;
                t.IsEmployed = s.IsEmployed;
            }
            if (t.JobTitle != SomeHelper.MapToJobTitle(s.JobTitle))
            {
                changed = true;
                t.JobTitle = SomeHelper.MapToJobTitle(s.JobTitle);
            }
            if (t.LastLogin != s.LastLogin)
            {
                changed = true;
                t.LastLogin = s.LastLogin;
            }
            if (t.Name != s.FullName)
            {
                changed = true;
                t.Name = s.FullName;
            }
            if (t.Nationality != s.Nationality)
            {
                changed = true;
                t.Nationality = s.Nationality;
            }
            if (t.PhoneNumber != s.Phone)
            {
                changed = true;
                t.PhoneNumber = s.Phone;
            }
            if (t.Salary != s.Salary)
            {
                changed = true;
                t.Salary = s.Salary;
            }
            if (t.Notes != s.Notes)
            {
                changed = true;
                t.Notes = s.Notes;
            }
            if (t.Web != (Uri.TryCreate(s.Uri, new UriCreationOptions(), out Uri? uriResX) ? uriResX : null))
            {
                changed = true;
                t.Web = (Uri.TryCreate(s.Uri, new UriCreationOptions(), out Uri? uriRes) ? uriRes : null);
            }

            return changed;
        }

        public static void Main()
        {
            var summary = BenchmarkRunner.Run<Bench>();
        }
    }
}