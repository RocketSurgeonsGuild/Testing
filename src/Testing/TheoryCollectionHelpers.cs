using System.Reflection;
using JetBrains.Annotations;

namespace Rocket.Surgery.Extensions.Testing;

internal static class TheoryCollectionHelpers
{
    private static MethodInfo GetTupleMethodInfo(Type type)
    {
        var methodInfo = typeof(TheoryCollectionHelpers)
                        .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                        .Where(x => x.GetParameters().FirstOrDefault()?.ParameterType.IsGenericType == true)
                        .Single(
                             x => x.GetParameters().FirstOrDefault()?.ParameterType.GetGenericTypeDefinition().GetGenericArguments().Length
                               == type.GetGenericArguments().Length
                         );
        return methodInfo.MakeGenericMethod(type.GetGenericArguments());
    }

    internal static Func<object, object?[]> GetConverterMethod(Type type)
    {
        if (type.IsArray)
        {
            return x => ( x as IEnumerable<object> ?? throw new InvalidOperationException() ).ToArray();
        }

        if (typeof(IEnumerable<object>).IsAssignableFrom(type) && type != typeof(string))
        {
            return x => ( x as IEnumerable<object> ?? throw new InvalidOperationException() ).ToArray();
        }

        if (type.IsGenericType)
        {
            if (
                type.GetGenericTypeDefinition() == typeof(ValueTuple<>) ||
                type.GetGenericTypeDefinition() == typeof(ValueTuple<,>) ||
                type.GetGenericTypeDefinition() == typeof(ValueTuple<,,>) ||
                type.GetGenericTypeDefinition() == typeof(ValueTuple<,,,>) ||
                type.GetGenericTypeDefinition() == typeof(ValueTuple<,,,,>)
            )
            {
                var methodInfo = GetTupleMethodInfo(type);
                return x => methodInfo.Invoke(null, new[] { x }) as object[] ?? throw new InvalidOperationException();
            }
        }

        return x => new[] { x };
    }

    [UsedImplicitly]
    private static object?[] FromValueTuple<T1>(ValueTuple<T1> value)
    {
        return new object?[] { value.Item1 };
    }

    [UsedImplicitly]
    private static object?[] FromValueTuple<T1, T2>(ValueTuple<T1, T2> value)
    {
        return new object?[] { value.Item1, value.Item2 };
    }

    [UsedImplicitly]
    private static object?[] FromValueTuple<T1, T2, T3>(ValueTuple<T1, T2, T3> value)
    {
        return new object?[] { value.Item1, value.Item2, value.Item3 };
    }

    [UsedImplicitly]
    private static object?[] FromValueTuple<T1, T2, T3, T4>(ValueTuple<T1, T2, T3, T4> value)
    {
        return new object?[] { value.Item1, value.Item2, value.Item3, value.Item4 };
    }

    [UsedImplicitly]
    private static object?[] FromValueTuple<T1, T2, T3, T4, T5>(ValueTuple<T1, T2, T3, T4, T5> value)
    {
        return new object?[] { value.Item1, value.Item2, value.Item3, value.Item4, value.Item5 };
    }
}
