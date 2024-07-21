using HarmonyLib;
using System.Reflection;
using Winch.Core;

namespace Tweaks
{
	public static class Main
	{
		public static void Initialize()
		{
			new Harmony("megapiggy.tweaks").PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}