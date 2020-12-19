using System.Diagnostics.CodeAnalysis;

namespace FritzSmartHome.FritzBox
{
#nullable enable

	internal static class Utils
	{
		[return: NotNullIfNotNull("src")]
		public static byte[]? CloneByteArray(this byte[]? src)
		{
			if (src == null)
			{
				return null;
			}

			return (byte[])(src.Clone());
		}
	}
}
