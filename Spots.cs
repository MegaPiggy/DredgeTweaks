using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Tweaks;

[JsonConverter(typeof(StringEnumConverter))]
public enum Spots
{
	[EnumMember(Value = @"vanilla")] Vanilla,
	[EnumMember(Value = @"neverDeplete")] NeverDeplete,
	[EnumMember(Value = @"neverRestock")] NeverRestock
}
