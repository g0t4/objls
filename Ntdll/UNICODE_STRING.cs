﻿using System;
using System.Runtime.InteropServices;

namespace PInvoke.Ntdll
{

	/// <summary>
	/// http://www.pinvoke.net/default.aspx/Structures/UNICODE_STRING.html
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct UNICODE_STRING : IDisposable
	{
		public ushort Length;
		public ushort MaximumLength;
		private IntPtr buffer;

		public UNICODE_STRING(string s)
		{
			Length = (ushort)(s.Length * 2);
			MaximumLength = (ushort)(Length + 2);
			buffer = Marshal.StringToHGlobalUni(s);
		}

		public void Dispose()
		{
			Marshal.FreeHGlobal(buffer);
			buffer = IntPtr.Zero;
		}

		public override string ToString()
		{
			return Marshal.PtrToStringUni(buffer);
		}
	}

}
