using Mapper.Test.Models;
using System.Linq.Expressions;

namespace Mapper.Test
{
    public class MapperTests
    {
        static Circle[] Circles = new Circle[]
        {
            new Circle
            {
                Name = "c1",
                X = 1,
                Y = 2,
                Radius = 3
            },
            new Circle
            {
                Name = "c2",
                X = 20,
                Y = -1,
                Radius = 500
            }
        };

        static Source source = new Source
        {
            StringProp = "source",
            IntProp = 42,
            IntNullableProp = 44,
            FloatProp = 123.1f, // Should become 123.0999984741211 when cast to double
            DoubleProp = 21.620001, // Should become 21.62 when cast to float
            DateTimeProp = DateTime.Now,
            DateTimeOffsetNullableProp = DateTimeOffset.Now,
            Circles = new List<Circle>(Circles),
            Cat = new Cat
            {
                Name = "Cat",
                Color = "Pink",
                Height = 100
            },
            OtherCat = new Cat
            {
                Name = "Garfield",
                Color = "Yellow",
                Height = 101
            }
        };

        [Fact]
        public void Throws_InvalidOperationException_If_Assigning_To_ReadOnly_Member()
        {
            var target = new Target();
            var m = new Mapper<Source, Target>()
                .ForMember(t => t.IntPropReadOnly, s => s.IntProp);

            Assert.Throws<InvalidOperationException>(() => m.Build());
        }

        [Fact]
        public void Throws_InvalidOperationException_If_Mapping_Incompatible_Types()
        {
            var target = new Target();
            var m = new Mapper<Source, Target>()
                .ForMember(t => t.IntProp, s => s.FloatProp);

            Assert.Throws<InvalidOperationException>(() => m.Build());
        }

        [Fact]
        public void Allow_Mapping_Incompatible_Types_With_Cast()
        {
            var target = new Target();
            var changed = new Mapper<Source, Target>()
                .ForMember(t => t.IntProp, s => (int)s.FloatProp)
                .Build()
                .Map(source, target);

            Assert.True(changed);
            Assert.Equal((int)source.FloatProp, target.IntProp);
        }

        [Fact]
        public void Can_Map_Between_Different_Types_With_Implicit_Conversions()
        {
            var target = new Target();

            var changed = new Mapper<Source, Target>()
                .ForMember(t => t.DoubleProp, s => s.FloatProp)
                .ForMember(t => t.LongProp, s => s.IntProp)
                .Build()
                .Map(source, target);

            Assert.True(changed);
            Assert.Equal(source.FloatProp, target.DoubleProp);
            Assert.Equal(source.IntProp, target.LongProp);
        }

        [Fact]
        public void Can_Not_Map_Between_Different_Types_With_Explicit_Conversions()
        {
            var target = new Target();

            var m = new Mapper<Source, Target>()
                .ForMember(t => t.FloatProp, s => s.DoubleProp);

            Assert.Throws<InvalidOperationException>(() => m.Build());
        }

        [Fact]
        public void Can_Map_Int_Properties_To_Nullable_Int_Property()
        {
            var target = new Target();
            var changed = new Mapper<Source, Target>()
                .ForMember(t => t.IntNullable, s => s.IntProp)
                .Build()
                .Map(source, target);

            Assert.True(changed);
            Assert.Equal(source.IntProp, target.IntNullable);
        }

        [Fact]
        public void Can_Map_Int_Properties_To_Int_Property()
        {
            var target = new Target();
            var changed = new Mapper<Source, Target>()
                .ForMember(t => t.IntProp, s => s.IntProp)
                .Build()
                .Map(source, target);

            Assert.True(changed);
            Assert.Equal(source.IntProp, target.IntProp);
        }

        [Fact]
        public void Detect_If_No_Change_Was_Made()
        {
            var target = new Target { IntProp = source.IntProp, StringProp = source.StringProp };

            var m = new Mapper<Source, Target>()
                .ForMember(t => t.StringProp, s => s.StringProp)
                .ForMember(t => t.IntProp, s => s.IntProp)
                .Build();

            var changed = m.Map(source, target);

            Assert.False(changed);
            Assert.Equal(source.IntProp, target.IntProp);
            Assert.Equal(source.StringProp, target.StringProp);
        }

        [Fact]
        public void Detect_Change_Was_Made()
        {
            var target = new Target { IntProp = source.IntProp, StringProp = "target" };

            var m = new Mapper<Source, Target>()
                .ForMember(t => t.StringProp, s => s.StringProp)
                .ForMember(t => t.IntProp, s => s.IntProp)
                .Build();

            var changed = m.Map(source, target);

            Assert.True(changed);
            Assert.Equal(source.IntProp, target.IntProp);
            Assert.Equal(source.StringProp, target.StringProp);
        }

        [Fact]
        public void Can_Map_Fields_And_Properties()
        {
            var target = new Target();

            var changed = new Mapper<Source, Target>()
                .ForMember(t => t.IntField, s => s.IntProp)
                .ForMember(t => t.StringProp, s => s.StringProp)
                .ForMember(t => t.DateTimeProp, s => s.DateTimeProp)
                .Build()
                .Map(source, target);

            Assert.True(changed);
            Assert.Equal(source.IntProp, target.IntField);
            Assert.Equal(source.StringProp, target.StringProp);
            Assert.Equal(source.DateTimeProp, target.DateTimeProp);
        }

        [Fact]
        public void Can_Map_Nullable_DateTimeOffset_Property_To_Nullable_DateTime_Property()
        {
            var target = new Target();

            var changed = new Mapper<Source, Target>()
                .ForMember(t => t.DateTimeNullableProp, s => s.DateTimeOffsetNullableProp.HasValue ? (DateTime?)s.DateTimeOffsetNullableProp.Value.DateTime : null)
                .Build()
                .Map(source, target);

            Assert.True(changed);
            Assert.Equal(source.DateTimeOffsetNullableProp?.DateTime, target.DateTimeNullableProp);
        }

        [Fact]
        public void Can_Map_IEnumerable_Structs_To_Array_Of_Structs()
        {
            var target = new Target();

            var changed = new Mapper<Source, Target>()
                .ForMember(t => t.Circles, s => s.Circles.ToArray())
                .Build()
                .Map(source, target);

            Assert.True(changed);
            Assert.Equal(target.Circles.Length, source.Circles.Count());
            var idx = 0;
            Assert.All(target.Circles, (p, i) =>
            {
                Assert.Equal(Circles[i], p);
                Assert.Equal(idx++, i);
            });
        }

        [Fact]
        public void Can_Not_Detect_That_Lists_Of_Structs_Are_The_Same()
        {
            var target = new Target
            {
                Circles = new List<Circle>(Circles).ToArray()
            };

            var changed = new Mapper<Source, Target>()
                .ForMember(t => t.Circles, s => s.Circles.ToArray())
                .Build()
                .Map(source, target);

            // Ideally we would like to not think the lists are different.
            // https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.sequenceequal
            Assert.True(changed);
        }

        [Fact]
        public void Can_Map_Real_Classes()
        {
            var target = new Target();
            
            var changed = new Mapper<Source, Target>()
                .ForMember(t => t.Cat, s => s.Cat)
                .Build()
                .Map(source, target);

            Assert.True(changed);
            Assert.Equal(target.Cat, source.Cat);
        }

        [Fact]
        public void Does_Not_Map_Same_Class_Instance()
        {
            var target = new Target { Cat = source.Cat };

            var changed = new Mapper<Source, Target>()
                .ForMember(t => t.Cat, s => s.Cat)
                .Build()
                .Map(source, target);

            Assert.False(changed);
        }

        [Fact]
        public void Can_Map_Different_Class_Instance()
        {
            var target = new Target { Cat = new Cat { Name = "Peter", Color = "Yellow", Height = 1 } };

            var changed = new Mapper<Source, Target>()
                .ForMember(t => t.Cat, s => s.Cat)
                .Build()
                .Map(source, target);

            Assert.True(changed);
            Assert.Equal(target.Cat.Name, source.Cat.Name);
        }

        [Fact]
        public void Does_Map_Different_Class_Instance_With_EqualityComparer()
        {
            var target = new Target
            {
                // First source cat SHOULD NOT be mapped, as it has the same name as the target cat
                Cat = new Cat { Name = source.Cat.Name, Color = "Yellow", Height = 1 },
                // Second source cat SHOULD be mapped, as it has different name then second target cat
                SecondCat = new Cat { Name = "Foo" }
            };

            var targetFirstCat = target.Cat;
            var targetSecondCat = target.SecondCat;

            var cc = new CatComparer();
            var changed = new Mapper<Source, Target>()
                .ForMember(t => t.Cat, s => s.Cat, cc)
                .ForMember(t => t.SecondCat, s => s.OtherCat, cc)
                .Build()
                .Map(source, target);

            Assert.True(changed);
            Assert.NotEqual(source.Cat, target.Cat);
            Assert.Equal(targetFirstCat, target.Cat);
        }

        [Fact]
        public void Can_Perform_Diff_And_Return_Information_About_Difference()
        {
            var target = new Target();

            var diffList = new Mapper<Source, Target>()
                .ForMember(t => t.IntField, s => s.IntProp)
                .Build()
                .Diff(source, target);

            Assert.Single(diffList);
            var diff = diffList.Single();
            Assert.Equal("IntField", diff.Name);
            Assert.Equal(0, diff.Target);
            Assert.Equal(42, diff.Source);
        }
    }
}