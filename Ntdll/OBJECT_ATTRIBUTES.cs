﻿using System;
using System.Runtime.InteropServices;

namespace PInvoke.Ntdll
{
	/// <summary>
	///     http://www.pinvoke.net/default.aspx/Structures/OBJECT_ATTRIBUTES.html
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct OBJECT_ATTRIBUTES : IDisposable
	{
		public int Length;
		public IntPtr RootDirectory;
		private IntPtr objectName;
		public uint Attributes;
		public IntPtr SecurityDescriptor;
		public IntPtr SecurityQualityOfService;

		public OBJECT_ATTRIBUTES(string name, uint attrs)
		{
			Length = 0;
			RootDirectory = IntPtr.Zero;
			objectName = IntPtr.Zero;
			Attributes = attrs;
			SecurityDescriptor = IntPtr.Zero;
			SecurityQualityOfService = IntPtr.Zero;

			Length = Marshal.SizeOf(this);
			ObjectName = new UNICODE_STRING(name);
		}

		public UNICODE_STRING ObjectName
		{
			get { return Marshal.PtrToStructure<UNICODE_STRING>(objectName); }

			set
			{
				var fDeleteOld = objectName != IntPtr.Zero;
				if (!fDeleteOld)
					objectName = Marshal.AllocHGlobal(Marshal.SizeOf(value));
				Marshal.StructureToPtr(value, objectName, fDeleteOld);
			}
		}

		public void Dispose()
		{
			if (objectName != IntPtr.Zero)
			{
				Marshal.DestroyStructure<UNICODE_STRING>(objectName);
				Marshal.FreeHGlobal(objectName);
				objectName = IntPtr.Zero;
			}
		}
	}
}