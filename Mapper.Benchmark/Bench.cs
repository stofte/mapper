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
            mapper.Map(source, targetMapper);
        }

        [Benchmark]
        public void Manually_Written_Mapping_Code()
        {
            ManualMapper(source, targetManual);
        }

        private void ManualMapper(UserInfoSource s, UserInfoTarget t)
        {
            var same = t.Address != s.Address &&
                t.Age != s.Age &&
                t.City != s.City &&
                t.CreatedAt != s.DateCreated &&
                t.Department != s.Department &&
                t.Education != s.Education &&
                t.Email != s.Email &&
                t.ExperienceInYears != s.ExperienceInYears &&
                t.Id != s.PK &&
                t.Active != s.IsActive &&
                t.IsEmployed != s.IsEmployed &&
                t.JobTitle != SomeHelper.MapToJobTitle(s.JobTitle) &&
                t.LastLogin != s.LastLogin &&
                t.Name != s.FullName &&
                t.Nationality != s.Nationality &&
                t.PhoneNumber != s.Phone &&
                t.Salary != s.Salary &&
                t.Notes != s.Notes &&
                t.Web != (Uri.TryCreate(s.Uri, new UriCreationOptions(), out Uri? uriRes) ? uriRes : null);

            if (!same)
            {
                t.Address = s.Address;
                t.Age = s.Age;
                t.City = s.City;
                t.CreatedAt = s.DateCreated;
                t.Department = s.Department;
                t.Education = s.Education;
                t.Email = s.Email;
                t.ExperienceInYears = s.ExperienceInYears;
                t.Id = s.PK;
                t.Active = s.IsActive;
                t.IsEmployed = s.IsEmployed;
                t.JobTitle = SomeHelper.MapToJobTitle(s.JobTitle);
                t.LastLogin = s.LastLogin;
                t.Name = s.FullName;
                t.Nationality = s.Nationality;
                t.PhoneNumber = s.Phone;
                t.Salary = s.Salary;
                t.Notes = s.Notes;
                t.Web = Uri.TryCreate(s.Uri, new UriCreationOptions(), out Uri? uriResult) ? uriResult : null;
            }
        }

        public static void Main()
        {
            var summary = BenchmarkRunner.Run<Bench>();
        }
    }
}