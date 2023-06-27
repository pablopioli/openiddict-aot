using OpenIddict.Abstractions;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(OpenIddictConfiguration))]
[JsonSerializable(typeof(OpenIddictResponse))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
