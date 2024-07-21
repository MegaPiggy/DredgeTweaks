using System.ComponentModel;

namespace Tweaks;

public enum Spots
{
	[Description("vanilla")]
	Vanilla,
	[Description("never deplete")]
	NeverDeplete,
	[Description("never restock")]
	NeverRestock
}
