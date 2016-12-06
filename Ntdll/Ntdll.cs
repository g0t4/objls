using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace PInvoke.Ntdll
{
	public class Ntdll
	{
		//http://www.pinvoke.net/default.aspx/ntdll.ntopendirectoryobject
		[DllImport("ntdll.dll")]
		public static extern int NtOpenDirectoryObject(out SafeFileHandle directoryHandle, ACCESS_MASK accessMask,
			ref OBJECT_ATTRIBUTES objectAttributes);

		// http://www.pinvoke.net/default.aspx/ntdll.ntquerydirectoryobject
		[DllImport("ntdll.dll")]
		public static extern int NtQueryDirectoryObject(SafeFileHandle objectHandle, IntPtr buffer, int length,
			bool returnSingleEntry, bool restartScan, ref uint context, out uint returnLength);

		/// <summary>
		///     http://www.pinvoke.net/default.aspx/ntdll.ntqueryobject
		/// </summary>
		[DllImport("ntdll.dll")]
		public static extern NtStatus NtQueryObject(SafeFileHandle objectHandle, OBJECT_INFORMATION_CLASS informationClass,
			IntPtr informationPtr, uint informationLength, ref uint returnLength);

		[DllImport("ntdll.dll")]
		public static extern NtStatus NtOpenFile(
			out SafeFileHandle objectHandle,
			ACCESS_MASK desiredAccess,
			ref OBJECT_ATTRIBUTES objectAttributes,
			out IO_STATUS_BLOCK ioStatusBlock,
			ulong shareAccess,
			ulong openOptions);

		[DllImport("ntdll.dll")]
		public static extern int NtOpenSymbolicLinkObject(
			out SafeFileHandle linkHandle,
			ACCESS_MASK desiredAccess,
			ref OBJECT_ATTRIBUTES objectAttributes);

		[DllImport("ntdll.dll")]
		public static extern int NtQuerySymbolicLinkObject(
			SafeFileHandle linkHandle,
			ref UNICODE_STRING linkTarget,
			out int returnedLength);
	}

	public class NtdllHelper
	{
		public static string GetObjectType(string objectName)
		{
			// todo remove - should work with else condition if directory too
			var handle = new DirectoryObject(objectName).Open();
			if (handle != null)
				return ObjectTypeFromHandle(handle);
			// Seems to be no way to get a handle for any file easily, NtOpenFile might work but seems overkill, so how about just
			// query the directory above if OpenDirectoryObject fails because teh objectName isn't a directory, then look at child items for 
			// the objectName's type
			// http://www.osronline.com/showThread.cfm?link=52516
			return TryGetTypeFromDirectoryEntires(objectName);
		}

		private static string TryGetTypeFromDirectoryEntires(string objectName)
		{
			// note: Path.GetFileName doesn't work with C: on end, other likely trouble so I'm not going to use it
			var split = objectName.LastIndexOf(@"\");
			// if last \ found at 0, then that's the root object namespace, make sure not to set parentDirectory to an empty string
			var parentDirectory = split == 0 ? @"\" : objectName.Substring(0, split);
			var objectFileName = objectName.Substring(split + 1);

			// todo what if parentDirectory is a SymbolicLink?
			var objects = new DirectoryObject(parentDirectory).QueryDirectoryObjects();
			var desiredObject = objects.FirstOrDefault(o => o.Name == objectFileName);
			return desiredObject?.TypeName;
		}

		public static string ObjectTypeFromHandle(SafeFileHandle handle)
		{
			var informationPointer = NtQueryObject(handle, OBJECT_INFORMATION_CLASS.ObjectTypeInformation);
			var typeInfo = Marshal.PtrToStructure<PUBLIC_OBJECT_TYPE_INFORMATION>(informationPointer);
			return typeInfo.TypeName.ToString();
		}

		public static IntPtr NtQueryObject(SafeFileHandle objectHandle, OBJECT_INFORMATION_CLASS informationClass,
			uint informationLength = 0)
		{
			// http://www.pinvoke.net/default.aspx/ntdll.ntqueryobject

			if (informationLength == 0)
				informationLength = (uint) Marshal.SizeOf<uint>();

			var informationPointer = Marshal.AllocHGlobal((int) informationLength);
			var tries = 0;
			NtStatus result;

			while (true)
			{
				result = Ntdll.NtQueryObject(objectHandle, informationClass, informationPointer, informationLength,
					ref informationLength);

				if (result == NtStatus.InfoLengthMismatch || result == NtStatus.BufferOverflow || result == NtStatus.BufferTooSmall)
				{
					Marshal.FreeHGlobal(informationPointer);
					// todo why not just allocate this to begin with?
					informationPointer = Marshal.AllocHGlobal((int) informationLength);
					tries++;
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
				return informationPointer; //don't forget to free the pointer with Marshal.FreeHGlobal after you're done with it
			Marshal.FreeHGlobal(informationPointer); //free pointer when not Successful

			return IntPtr.Zero;
		}


		// https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx
		private enum ShareAccess : ulong
		{
			FILE_SHARE_READ = 0x01
		}

		private enum CreateOptions : ulong
		{
			CREATE_ALWAYS = 2,
			CREATE_NEW = 1,
			OPEN_ALWAYS = 4,
			OPEN_EXISTING = 3,
			TRUNCATE_EXISTING = 5
		}
	}
}