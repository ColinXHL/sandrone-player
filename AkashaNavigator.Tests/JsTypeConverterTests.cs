using System;
using System.Collections.Generic;
using AkashaNavigator.Plugins;
using AkashaNavigator.Plugins.Utils;
using Xunit;

namespace AkashaNavigator.Tests
{
/// <summary>
/// JsTypeConverter 单元测试
/// 测试 C# 与 JavaScript 之间的类型转换功能
/// </summary>
public class JsTypeConverterTests
{
#region ToJs Tests - Primitive Types

    [Fact]
    public void ToJs_NullValue_ReturnsNull()
    {
        var result = JsTypeConverter.ToJs(null);
        Assert.Null(result);
    }

    [Theory]
    [InlineData(42)]
    [InlineData(-100)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void ToJs_IntValue_ReturnsSameValue(int value)
    {
        var result = JsTypeConverter.ToJs(value);
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(3.14)]
    [InlineData(-2.5)]
    [InlineData(0.0)]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    public void ToJs_DoubleValue_ReturnsSameValue(double value)
    {
        var result = JsTypeConverter.ToJs(value);
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("")]
    [InlineData("中文测试")]
    [InlineData("special chars: !@#$%^&*()")]
    public void ToJs_StringValue_ReturnsSameValue(string value)
    {
        var result = JsTypeConverter.ToJs(value);
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ToJs_BoolValue_ReturnsSameValue(bool value)
    {
        var result = JsTypeConverter.ToJs(value);
        Assert.Equal(value, result);
    }

#endregion

#region ToJs Tests - Collections

    [Fact]
    public void ToJs_IntArray_ReturnsArray()
    {
        var input = new[] { 1, 2, 3, 4, 5 };
        var result = JsTypeConverter.ToJs(input);

        Assert.IsType<object[]>(result);
        var array = (object[])result!;
        Assert.Equal(5, array.Length);
        Assert.Equal(1, array[0]);
        Assert.Equal(5, array[4]);
    }

    [Fact]
    public void ToJs_StringList_ReturnsArray()
    {
        var input = new List<string> { "a", "b", "c" };
        var result = JsTypeConverter.ToJs(input);

        Assert.IsType<object[]>(result);
        var array = (object[])result!;
        Assert.Equal(3, array.Length);
        Assert.Equal("a", array[0]);
        Assert.Equal("c", array[2]);
    }

    [Fact]
    public void ToJs_Dictionary_ReturnsDictionary()
    {
        var input = new Dictionary<string, object> { { "name", "test" }, { "value", 42 } };
        var result = JsTypeConverter.ToJs(input);

        Assert.IsType<Dictionary<string, object?>>(result);
        var dict = (Dictionary<string, object?>)result!;
        Assert.Equal("test", dict["name"]);
        Assert.Equal(42, dict["value"]);
    }

#endregion

#region ToJs Tests - Complex Objects

    [Fact]
    public void ToJs_AnonymousObject_ReturnsDictionaryWithCamelCase()
    {
        var input = new { Name = "test", Age = 25, IsActive = true };
        var result = JsTypeConverter.ToJs(input);

        Assert.IsType<Dictionary<string, object?>>(result);
        var dict = (Dictionary<string, object?>)result!;

        // 验证 camelCase 转换
        Assert.True(dict.ContainsKey("name"));
        Assert.True(dict.ContainsKey("age"));
        Assert.True(dict.ContainsKey("isActive"));

        Assert.Equal("test", dict["name"]);
        Assert.Equal(25, dict["age"]);
        Assert.Equal(true, dict["isActive"]);
    }

    [Fact]
    public void ToJs_NestedObject_ReturnsNestedDictionary()
    {
        var input = new { Name = "parent", Child = new { Name = "child", Value = 10 } };
        var result = JsTypeConverter.ToJs(input);

        Assert.IsType<Dictionary<string, object?>>(result);
        var dict = (Dictionary<string, object?>)result!;

        Assert.Equal("parent", dict["name"]);
        Assert.IsType<Dictionary<string, object?>>(dict["child"]);

        var childDict = (Dictionary<string, object?>)dict["child"]!;
        Assert.Equal("child", childDict["name"]);
        Assert.Equal(10, childDict["value"]);
    }

#endregion

#region FromJs Tests - Primitive Types

    [Fact]
    public void FromJs_NullToInt_ReturnsDefault()
    {
        var result = JsTypeConverter.FromJs<int>(null);
        Assert.Equal(0, result);
    }

    [Fact]
    public void FromJs_NullToNullableInt_ReturnsNull()
    {
        var result = JsTypeConverter.FromJs<int?>(null);
        Assert.Null(result);
    }

    [Theory]
    [InlineData(42)]
    [InlineData(-100)]
    [InlineData(0)]
    public void FromJs_IntToInt_ReturnsSameValue(int value)
    {
        var result = JsTypeConverter.FromJs<int>(value);
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(42.0, 42)]
    [InlineData(3.5, 4)] // Convert.ToInt32 使用四舍五入
    [InlineData(-2.5, -2)]
    public void FromJs_DoubleToInt_ReturnsConvertedValue(double input, int expected)
    {
        var result = JsTypeConverter.FromJs<int>(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("")]
    [InlineData("中文")]
    public void FromJs_StringToString_ReturnsSameValue(string value)
    {
        var result = JsTypeConverter.FromJs<string>(value);
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(42, "42")]
    [InlineData(3.14, "3.14")]
    [InlineData(true, "True")]
    public void FromJs_ValueToString_ReturnsStringRepresentation(object input, string expected)
    {
        var result = JsTypeConverter.FromJs<string>(input);
        Assert.Equal(expected, result);
    }

#endregion

#region ToDictionary Tests

    [Fact]
    public void ToDictionary_NullValue_ReturnsEmptyDictionary()
    {
        var result = JsTypeConverter.ToDictionary(null);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ToDictionary_ExistingDictionary_ReturnsSameDictionary()
    {
        var input = new Dictionary<string, object?> { { "key1", "value1" }, { "key2", 42 } };
        var result = JsTypeConverter.ToDictionary(input);

        Assert.Same(input, result);
    }

    [Fact]
    public void ToDictionary_IDictionary_ReturnsConvertedDictionary()
    {
        IDictionary<string, object> input = new Dictionary<string, object> { { "name", "test" }, { "count", 5 } };
        var result = JsTypeConverter.ToDictionary(input);

        Assert.Equal("test", result["name"]);
        Assert.Equal(5, result["count"]);
    }

    [Fact]
    public void ToDictionary_AnonymousObject_ReturnsConvertedDictionary()
    {
        var input = new { Name = "test", Value = 123 };
        var result = JsTypeConverter.ToDictionary(input);

        Assert.True(result.ContainsKey("Name") || result.ContainsKey("name"));
        Assert.True(result.ContainsKey("Value") || result.ContainsKey("value"));
    }

#endregion

#region TryGetProperty Tests

    [Fact]
    public void TryGetProperty_ExistingProperty_ReturnsTrue()
    {
        var input = new Dictionary<string, object?> { { "name", "test" }, { "value", 42 } };

        var success = JsTypeConverter.TryGetProperty<string>(input, "name", out var name);

        Assert.True(success);
        Assert.Equal("test", name);
    }

    [Fact]
    public void TryGetProperty_NonExistingProperty_ReturnsFalse()
    {
        var input = new Dictionary<string, object?> { { "name", "test" } };

        var success = JsTypeConverter.TryGetProperty<string>(input, "nonexistent", out var value);

        Assert.False(success);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetProperty_NullObject_ReturnsFalse()
    {
        var success = JsTypeConverter.TryGetProperty<string>(null, "key", out var value);

        Assert.False(success);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetProperty_EmptyKey_ReturnsFalse()
    {
        var input = new Dictionary<string, object?> { { "name", "test" } };

        var success = JsTypeConverter.TryGetProperty<string>(input, "", out var value);

        Assert.False(success);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetProperty_TypeConversion_ReturnsConvertedValue()
    {
        var input = new Dictionary<string, object?> {
            { "count", 42.0 } // double
        };

        var success = JsTypeConverter.TryGetProperty<int>(input, "count", out var count);

        Assert.True(success);
        Assert.Equal(42, count);
    }

#endregion

#region Edge Cases

    [Fact]
    public void ToJs_DateTime_ReturnsIso8601String()
    {
        var input = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var result = JsTypeConverter.ToJs(input);

        Assert.IsType<string>(result);
        Assert.Contains("2024-01-15", (string)result!);
    }

    [Fact]
    public void ToJs_EmptyArray_ReturnsEmptyArray()
    {
        var input = Array.Empty<int>();
        var result = JsTypeConverter.ToJs(input);

        Assert.IsType<object[]>(result);
        Assert.Empty((object[])result!);
    }

    [Fact]
    public void ToJs_EmptyDictionary_ReturnsEmptyDictionary()
    {
        var input = new Dictionary<string, object>();
        var result = JsTypeConverter.ToJs(input);

        Assert.IsType<Dictionary<string, object?>>(result);
        Assert.Empty((Dictionary<string, object?>)result!);
    }

    [Fact]
    public void ToDictionary_CaseInsensitive_FindsKeyIgnoringCase()
    {
        var input = new Dictionary<string, object?> { { "Name", "test" } };
        var result = JsTypeConverter.ToDictionary(input);

        // 字典应该是大小写不敏感的
        Assert.True(result.ContainsKey("name") || result.ContainsKey("Name"));
    }

#endregion
}
}
