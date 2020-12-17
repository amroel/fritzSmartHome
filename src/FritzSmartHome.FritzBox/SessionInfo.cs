using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace FritzSmartHome.FritzBox
{
	public unsafe record SessionInfo
	{
		private const string PDKDF2_TOKEN = "$";
		private const string PDKDF2_START = "2" + PDKDF2_TOKEN;
		private static readonly uint[] LOOKUP_32_UNSAFE = CreateLookup32Unsafe();
		private static readonly uint* LOOKUP_32_UNSAFE_P = (uint*)GCHandle.Alloc(LOOKUP_32_UNSAFE, GCHandleType.Pinned).AddrOfPinnedObject();

		[XmlElement("SID")]
		public string SessionId { get; init; }
		[XmlElement("Challenge")]
		public string Challenge { get; init; }
		[XmlElement("BlockTime")]
		public int BlockTime { get; init; }

		public string CalculateChallengeResponse(string password)
		{
			if (IsPBKDF2())
				return CalculatePBKDF2Response(password);
			else
				return CalculateMD5Response(password);
		}

		private bool IsPBKDF2() => Challenge.StartsWith(PDKDF2_START);

		private string CalculatePBKDF2Response(string password)
		{
			var challengeParts = Challenge.Split(PDKDF2_TOKEN);
			// Extract all necessary values encoded into the challenge (ignoring challengeParts[0] = 2)
			var iter1 = int.Parse(challengeParts[1]);
			var salt1 = HexStringToBytes(challengeParts[2]);
			var iter2 = int.Parse(challengeParts[3]);
			var salt2 = HexStringToBytes(challengeParts[4]);

			var pbkdf2 = new Rfc2898DeriveBytes(password, salt1, iter1, HashAlgorithmName.SHA256);
			// Hash twice, once with static salt...
			var hash1 = pbkdf2.GetBytes(32);
			// and once with dynamic salt.
			pbkdf2 = new Rfc2898DeriveBytes(hash1, salt2, iter2, HashAlgorithmName.SHA256);
			var hash2 = ByteArrayToHex(pbkdf2.GetBytes(32));

			return $"{challengeParts[4]}{PDKDF2_TOKEN}{hash2}";
		}

		private string CalculateMD5Response(string password) => string.Empty;

		private static byte[] HexStringToBytes(string hexString)
		{
			var bytes = new byte[hexString.Length / 2];

			for (var i = 0; i < hexString.Length; i += 2)
				bytes[i/2] = Convert.ToByte(hexString.Substring(i, 2), 16);

			return bytes;
		}

		private static uint[] CreateLookup32Unsafe()
		{
			var result = new uint[256];
			for (var i = 0; i < 256; i++)
			{
				var s = i.ToString("X2");
				if (BitConverter.IsLittleEndian)
					result[i] = s[0] + ((uint)s[1] << 16);
				else
					result[i] = s[1] + ((uint)s[0] << 16);
			}
			return result;
		}

		public static string ByteArrayToHex(byte[] bytes)
		{
			var result = new char[bytes.Length * 2];
			fixed (byte* bytesP = bytes)
			fixed (char* resultP = result)
			{
				var resultP2 = (uint*)resultP;
				for (var i = 0; i < bytes.Length; i++)
				{
					resultP2[i] = LOOKUP_32_UNSAFE_P[bytesP[i]];
				}
			}
			return new string(result);
		}
	}
}
