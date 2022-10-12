using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal sealed class SipHashBasedStringEqualityComparer : IEqualityComparer<string>
	{
		private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

		private readonly ulong _k0;

		private readonly ulong _k1;

		public SipHashBasedStringEqualityComparer()
			: this(GenerateRandomKeySegment(), GenerateRandomKeySegment())
		{
		}

		internal SipHashBasedStringEqualityComparer(ulong k0, ulong k1)
		{
			_k0 = k0;
			_k1 = k1;
		}

		public bool Equals(string x, string y)
		{
			return string.Equals(x, y);
		}

		private static ulong GenerateRandomKeySegment()
		{
			byte[] array = new byte[8];
			_rng.GetBytes(array);
			return (ulong)BitConverter.ToInt64(array, 0);
		}

		public unsafe int GetHashCode(string obj)
		{
			if (obj == null)
			{
				return 0;
			}
			fixed (char* bytes = obj)
			{
				return GetHashCode((byte*)bytes, checked((uint)obj.Length * 2u));
			}
		}

		internal unsafe int GetHashCode(byte* bytes, uint len)
		{
			return (int)SipHash_2_4_UlongCast_ForcedInline(bytes, len, _k0, _k1);
		}

		private unsafe static ulong SipHash_2_4_UlongCast_ForcedInline(byte* finb, uint inlen, ulong k0, ulong k1)
		{
			ulong num = 0x736F6D6570736575uL ^ k0;
			ulong num2 = 0x646F72616E646F6DuL ^ k1;
			ulong num3 = 0x6C7967656E657261uL ^ k0;
			ulong num4 = 0x7465646279746573uL ^ k1;
			ulong num5 = (ulong)inlen << 56;
			if (inlen != 0)
			{
				uint num6 = inlen & 7u;
				byte* ptr = finb + inlen - num6;
				ulong* ptr2 = (ulong*)finb;
				for (ulong* ptr3 = (ulong*)ptr; ptr2 < ptr3; ptr2++)
				{
					num4 ^= *ptr2;
					num += num2;
					num2 = (num2 << 13) | (num2 >> 51);
					num2 ^= num;
					num = (num << 32) | (num >> 32);
					num3 += num4;
					num4 = (num4 << 16) | (num4 >> 48);
					num4 ^= num3;
					num += num4;
					num4 = (num4 << 21) | (num4 >> 43);
					num4 ^= num;
					num3 += num2;
					num2 = (num2 << 17) | (num2 >> 47);
					num2 ^= num3;
					num3 = (num3 << 32) | (num3 >> 32);
					num += num2;
					num2 = (num2 << 13) | (num2 >> 51);
					num2 ^= num;
					num = (num << 32) | (num >> 32);
					num3 += num4;
					num4 = (num4 << 16) | (num4 >> 48);
					num4 ^= num3;
					num += num4;
					num4 = (num4 << 21) | (num4 >> 43);
					num4 ^= num;
					num3 += num2;
					num2 = (num2 << 17) | (num2 >> 47);
					num2 ^= num3;
					num3 = (num3 << 32) | (num3 >> 32);
					num ^= *ptr2;
				}
				for (int i = 0; i < num6; i++)
				{
					num5 |= (ulong)ptr[i] << 8 * i;
				}
			}
			num4 ^= num5;
			num += num2;
			num2 = (num2 << 13) | (num2 >> 51);
			num2 ^= num;
			num = (num << 32) | (num >> 32);
			num3 += num4;
			num4 = (num4 << 16) | (num4 >> 48);
			num4 ^= num3;
			num += num4;
			num4 = (num4 << 21) | (num4 >> 43);
			num4 ^= num;
			num3 += num2;
			num2 = (num2 << 17) | (num2 >> 47);
			num2 ^= num3;
			num3 = (num3 << 32) | (num3 >> 32);
			num += num2;
			num2 = (num2 << 13) | (num2 >> 51);
			num2 ^= num;
			num = (num << 32) | (num >> 32);
			num3 += num4;
			num4 = (num4 << 16) | (num4 >> 48);
			num4 ^= num3;
			num += num4;
			num4 = (num4 << 21) | (num4 >> 43);
			num4 ^= num;
			num3 += num2;
			num2 = (num2 << 17) | (num2 >> 47);
			num2 ^= num3;
			num3 = (num3 << 32) | (num3 >> 32);
			num ^= num5;
			num3 ^= 0xFF;
			num += num2;
			num2 = (num2 << 13) | (num2 >> 51);
			num2 ^= num;
			num = (num << 32) | (num >> 32);
			num3 += num4;
			num4 = (num4 << 16) | (num4 >> 48);
			num4 ^= num3;
			num += num4;
			num4 = (num4 << 21) | (num4 >> 43);
			num4 ^= num;
			num3 += num2;
			num2 = (num2 << 17) | (num2 >> 47);
			num2 ^= num3;
			num3 = (num3 << 32) | (num3 >> 32);
			num += num2;
			num2 = (num2 << 13) | (num2 >> 51);
			num2 ^= num;
			num = (num << 32) | (num >> 32);
			num3 += num4;
			num4 = (num4 << 16) | (num4 >> 48);
			num4 ^= num3;
			num += num4;
			num4 = (num4 << 21) | (num4 >> 43);
			num4 ^= num;
			num3 += num2;
			num2 = (num2 << 17) | (num2 >> 47);
			num2 ^= num3;
			num3 = (num3 << 32) | (num3 >> 32);
			num += num2;
			num2 = (num2 << 13) | (num2 >> 51);
			num2 ^= num;
			num = (num << 32) | (num >> 32);
			num3 += num4;
			num4 = (num4 << 16) | (num4 >> 48);
			num4 ^= num3;
			num += num4;
			num4 = (num4 << 21) | (num4 >> 43);
			num4 ^= num;
			num3 += num2;
			num2 = (num2 << 17) | (num2 >> 47);
			num2 ^= num3;
			num3 = (num3 << 32) | (num3 >> 32);
			num += num2;
			num2 = (num2 << 13) | (num2 >> 51);
			num2 ^= num;
			num = (num << 32) | (num >> 32);
			num3 += num4;
			num4 = (num4 << 16) | (num4 >> 48);
			num4 ^= num3;
			num += num4;
			num4 = (num4 << 21) | (num4 >> 43);
			num4 ^= num;
			num3 += num2;
			num2 = (num2 << 17) | (num2 >> 47);
			num2 ^= num3;
			num3 = (num3 << 32) | (num3 >> 32);
			return num ^ num2 ^ num3 ^ num4;
		}
	}
}
