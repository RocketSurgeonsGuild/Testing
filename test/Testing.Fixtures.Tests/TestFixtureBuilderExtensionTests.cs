using System.Collections.Generic;
using Xunit;

namespace Testing.Fixture.Tests
{
    public sealed class TestFixtureBuilderExtensionTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(100000)]
        public void Should_Return_Count(int count)
        {
            // Given, When
            TestFixture builder = new TestFixtureBuilder().WithCount(count);

            // Then
            Assert.Equal(count, builder.Count);
        }

        [Theory]
        [InlineData("Teresa")]
        [InlineData("Monica")]
        [InlineData("Sharron")]
        [InlineData("Nicki")]
        public void Should_Return_Name(string name)
        {
            // Given, When
            TestFixture builder = new TestFixtureBuilder().WithName(name);

            // Then
            Assert.Equal(name, builder.Name);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void Should_Add_Range(string test1, string test2, string test3)
        {
            // Given, When
            TestFixture builder = new TestFixtureBuilder().WithTests(new[] {test1, test2, test3});

            // Then
            Assert.Equal(new[] { test1, test2, test3 }, builder.Tests);
        }

        [Fact]
        public void Should_Add_To_List()
        {
            // Given, When
            TestFixture builder = new TestFixtureBuilder().WithTest("testing");

            // Then
            Assert.Equal(new[] {"testing"}, builder.Tests);
        }

        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { "testing", string.Empty, string.Empty },
                new object[] { "testing", "testing", string.Empty },
                new object[] { "testing", "testing", "one" },
                new object[] { "testing", "one", "two" }
            };
    }
}