using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace objls
{
	class Ntdll
	{
		//http://www.pinvoke.net/default.aspx/ntdll.ntopendirectoryobject
		[DllImport("ntdll.dll")]
		public static extern int NtOpenDirectoryObject(out SafeFileHandle directoryHandle, ACCESS_MASK accessMask , ref OBJECT_ATTRIBUTES objectAttributes);

		// http://www.pinvoke.net/default.aspx/ntdll.ntquerydirectoryobject
		[DllImport("ntdll.dll")]
		public static extern int NtQueryDirectoryObject(SafeFileHandle objectHandle, IntPtr buffer, int length, bool returnSingleEntry, bool restartScan, ref uint context, out uint returnLength);

		/// <summary>
		/// http://www.pinvoke.net/default.aspx/ntdll.ntqueryobject
		/// </summary>
		[DllImport("ntdll.dll")]
		public static extern NtStatus NtQueryObject(SafeFileHandle objectHandle, OBJECT_INFORMATION_CLASS informationClass, IntPtr informationPtr, uint informationLength, ref uint returnLength);

		[DllImport("ntdll.dll")]
		public static extern NtStatus NtOpenFile(SafeFileHandle objectHandle, ACCESS_MASK desiredAccess,
			OBJECT_ATTRIBUTES objectAttributes, IO_STATUS_BLOCK ioStatusBlock, ulong shareAccess, ulong openOptions);



	}

	class NtdllHelper
	{
		/// <summary>
		/// This only opens a Directory type object, returning a handle to it.
		/// </summary>
		public static SafeFileHandle OpenDirectoryObject(string objectName)
		{
			var objectAttributes = new OBJECT_ATTRIBUTES(objectName, 0);
			var status = Ntdll.NtOpenDirectoryObject(out var handle, ACCESS_MASK.DIRECTORY_QUERY, ref objectAttributes);
			return status < 0 ? null : handle;
		}

		/// <summary>
		/// This opens any object using OpenFile, returning a handle to it.
		/// </summary>
		public static SafeFileHandle OpenAnyObject(string objectName)
		{
			// there isn't a generic version of NtOpenDirectoryObject that works on any object, to get a handle, but we do have NtOpenFile

			var objectAttributes = new OBJECT_ATTRIBUTES(objectName, 0);
			var ioStatusBlock = new IO_STATUS_BLOCK()
			var status = Ntdll.NtOpenFile(out var handle, ACCESS_MASK.FILE_READ_ATTRIBUTES, objectAttributes, ioStatusBlock, 0, 0);
			return status < 0 ? null : handle;
		}

		public static string QueryObjectType(string objectName)
		{
			var handle = OpenDirectoryObject(objectName);
			if(handle == null)
			{
				return null;
			}

			var informationPointer = NtQueryObject(handle, OBJECT_INFORMATION_CLASS.ObjectTypeInformation);
			var typeInfo = Marshal.PtrToStructure<PUBLIC_OBJECT_TYPE_INFORMATION>(informationPointer);
			return typeInfo.TypeName.ToString();
		}

		public static IntPtr NtQueryObject(SafeFileHandle objectHandle, OBJECT_INFORMATION_CLASS informationClass, uint informationLength = 0)
		{
			// http://www.pinvoke.net/default.aspx/ntdll.ntqueryobject

			if (informationLength == 0)
				informationLength = (uint)Marshal.SizeOf<uint>();

			var informationPointer = Marshal.AllocHGlobal((int)informationLength);
			int tries = 0;
			NtStatus result;

			while (true)
			{
				result = Ntdll.NtQueryObject(objectHandle, informationClass, informationPointer, informationLength, ref informationLength);

				if (result == NtStatus.InfoLengthMismatch || result == NtStatus.BufferOverflow || result == NtStatus.BufferTooSmall)
				{
					Marshal.FreeHGlobal(informationPointer);
					// todo why not just allocate this to begin with?
					informationPointer = Marshal.AllocHGlobal((int)informationLength);
					tries++;
					continue;
				}
				else if (result == NtStatus.Success || tries > 5)
					break;
				else
				{
					//throw new Exception("Unhandled NtStatus " + result);
					break;
				}
			}

			if (result == NtStatus.Success)
			{
				return informationPointer;//don't forget to free the pointer with Marshal.FreeHGlobal after you're done with it
			}
			else
				Marshal.FreeHGlobal(informationPointer);//free pointer when not Successful

			return IntPtr.Zero;
		}
	}
}
