using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TimeZoneNames;

/// <summary>
/// Converts a json dictionary to a Dictionary&lt;<see cref="string"/>, <see paramref="TValue"/>&gt;
/// using <seealso ref="StringComparer.OrdinalIgnoreCase"/>
/// </summary>
/// <typeparam name="TValue">The value type of the dictionary</typeparam>
public sealed class CaseInsensitiveDictionaryConverter<TValue> : JsonConverter<Dictionary<string, TValue>>
{
    ///<inheritdoc />
    public override Dictionary<string, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dic = (IDictionary<string, TValue>)JsonSerializer.Deserialize(ref reader, typeToConvert, options)!;
        return new Dictionary<string, TValue>(dic, StringComparer.OrdinalIgnoreCase);
    }

    ///<inheritdoc />
    public override void Write(Utf8JsonWriter writer, Dictionary<string, TValue> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}