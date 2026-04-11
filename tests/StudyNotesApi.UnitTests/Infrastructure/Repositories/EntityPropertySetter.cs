using FluentAssertions;
using System.Reflection;

namespace StudyNotesApi.UnitTests.Infrastructure.Repositories;

internal static class EntityPropertySetter
{
    public static void SetProperty<T>(T instance, string propertyName, object? value)
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property.Should().NotBeNull($"property '{propertyName}' should exist on {typeof(T).Name}");
        property!.SetValue(instance, value);
    }
}
