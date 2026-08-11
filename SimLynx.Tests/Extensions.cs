using System;
using System.Collections.Generic;

namespace SimLynx.Tests;

public class Extensions
{
    public class AssemblyExtensions
    {
        private static readonly Type nonGenericType = typeof(TestNonGenericType);
        private static readonly Type genericDefType1 = typeof(TestGenericType1<>);
        private static readonly Type genericDefType2 = typeof(TestGenericType2<,>);
        private static readonly Type nestedType = typeof(TestNonGenericType.TestNestedType);
        private static readonly Type closedGenericType1 = typeof(TestGenericType1<TestNonGenericType>);
        private static readonly Type closedGenericType2 = typeof(TestGenericType2<
            TestNonGenericType,
            TestGenericType1<TestNonGenericType>
        >);

        private static readonly Dictionary<string, Type> typeTestMap = new()
        {
            // simple name
            { "TestNonGenericType", typeof(TestNonGenericType) },
            { "TestGenericType1`1", typeof(TestGenericType1<>) },
            { "TestGenericType2`2", typeof(TestGenericType2<,>) },
            { "TestNestedType", typeof(TestNonGenericType.TestNestedType) },
            // full name
            { nonGenericType.FullName!, nonGenericType },
            { nestedType.FullName!, nestedType },
            { genericDefType1.FullName!, genericDefType1 },
            { genericDefType2.FullName!, genericDefType2 },
            { closedGenericType1.FullName!, closedGenericType1 },
            { closedGenericType2.FullName!, closedGenericType2 },
            // assembly qualified name
            { nonGenericType.AssemblyQualifiedName!, nonGenericType },
            { nestedType.AssemblyQualifiedName!, nestedType },
            { genericDefType1.AssemblyQualifiedName!, genericDefType1 },
            { genericDefType2.AssemblyQualifiedName!, genericDefType2 },
            { closedGenericType1.AssemblyQualifiedName!, closedGenericType1 },
            { closedGenericType2.AssemblyQualifiedName!, closedGenericType2 },
            // ToString() representation
            // { nonGenericType.ToString(), nonGenericType },
            // { nestedType.ToString(), nestedType },
            { genericDefType1.ToString(), genericDefType1 },
            { genericDefType2.ToString(), genericDefType2 },
            { closedGenericType1.ToString(), closedGenericType1 },
            { closedGenericType2.ToString(), closedGenericType2 },
            // "user-friendly" generic
            { "TestGenericType1<>", genericDefType1 },
            { "TestGenericType2<,>", genericDefType2 },
            { "TestGenericType1<TestNonGenericType>", closedGenericType1 },
            { "TestGenericType2<TestNonGenericType,TestGenericType1<TestNonGenericType>>", closedGenericType2 },
        };

        private static readonly string[] typeTestBadNames =
        [
            // missing types
            "NonExistentType",
            "NonExistentType<>",
            "NonExistentType`1[System.String]",
            "TestGenericType1<NonExistentType>",
            "TestGenericType2<TestNonGenericType,NonExistentType>",
            // invalid types
            "TestGenericType2`2[System.String,]",
        ];

        [Fact]
        public void TryFindType_FindsTypes()
        {
            List<Exception> exceptions = [];

            foreach (var (name, type) in typeTestMap)
            {
                try
                {
                    Assert.True(
                        AppDomain.CurrentDomain.TryFindType(name, out var foundType),
                        $"Failed to find type for name '{name}'"
                    );
                    Assert.Equal(type, foundType);
                }
                catch (Xunit.Sdk.XunitException ex)
                {
                    exceptions.Add(ex);
                }
                catch (Exception ex)
                {
                    exceptions.Add(
                        new Exception($"Unexpected exception when expecting to find '{name}': {ex.Message}", ex)
                    );
                }
            }

            foreach (var name in typeTestBadNames)
            {
                try
                {
                    Assert.False(
                        AppDomain.CurrentDomain.TryFindType(name, out var foundType),
                        $"Unexpectedly found type for name '{name}': {foundType}"
                    );
                }
                catch (Xunit.Sdk.XunitException ex)
                {
                    exceptions.Add(ex);
                }
                catch (Exception ex)
                {
                    exceptions.Add(
                        new Exception($"Unexpected exception when expecting not to find '{name}': {ex.Message}", ex)
                    );
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}

class TestNonGenericType
{
    public class TestNestedType { }
}

class TestGenericType1<T> { }

class TestGenericType2<T1, T2> { }
