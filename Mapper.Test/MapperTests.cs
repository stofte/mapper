using Mapper.Test.Models;
using System.Linq.Expressions;

namespace Mapper.Test
{
    public class MapperTests
    {
        static Point[] Points = new Point[]
        {
            new Point
            {
                Name = "p1",
                X = 1,
                Y = 2,
                Radius = 3
            },
            new Point
            {
                Name = "p2",
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
            FloatProp = 43.1f,
            DateTimeProp = DateTime.Now,
            DateTimeOffsetNullableProp = DateTimeOffset.Now,
            Points = new List<Point>(Points)
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
            Assert.Equal(target.IntProp, (int)source.FloatProp);
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
            Assert.Equal(target.IntNullable, source.IntProp);
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
            Assert.Equal(target.IntProp, source.IntProp);
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
            Assert.Equal(target.IntProp, source.IntProp);
            Assert.Equal(target.StringProp, source.StringProp);
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
            Assert.Equal(target.IntProp, source.IntProp);
            Assert.Equal(target.StringProp, source.StringProp);
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
            Assert.Equal(target.IntField, source.IntProp);
            Assert.Equal(target.StringProp, source.StringProp);
            Assert.Equal(target.DateTimeProp, source.DateTimeProp);
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
            Assert.Equal(target.DateTimeNullableProp, source.DateTimeOffsetNullableProp?.DateTime);
        }

        [Fact]
        public void Can_Map_IEnumerable_Point_Structs_To_Array_Of_Point_Structs()
        {
            var target = new Target();

            var changed = new Mapper<Source, Target>()
                .ForMember(t => t.Points, s => s.Points.ToArray())
                .Build()
                .Map(source, target);

            Assert.True(changed);
            Assert.Equal(source.Points.Count(), target.Points.Length);
            var idx = 0;
            Assert.All(target.Points, (p, i) =>
            {
                Assert.Equal(Points[i], p);
                Assert.Equal(idx++, i);
            });
        }

        [Fact]
        public void Can_Not_Detect_That_Lists_Of_Structs_Are_The_Same()
        {
            var target = new Target
            {
                Points = new List<Point>(Points).ToArray()
            };

            var changed = new Mapper<Source, Target>()
                .ForMember(t => t.Points, s => s.Points.ToArray())
                .Build()
                .Map(source, target);

            // Ideally we would like to not think the lists are different.
            // https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.sequenceequal
            Assert.True(changed);
        }
    }
}