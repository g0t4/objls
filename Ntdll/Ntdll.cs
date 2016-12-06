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
		public static Result<string> GetSymbolicLinkObjectTarget(string objectName)
		{
			var type = GetObjectType(objectName);
			if (type != "SymbolicLink")
				return null;

			var objectAttributes = new OBJECT_ATTRIBUTES(objectName, 0);
			var status = Ntdll.NtOpenSymbolicLinkObject(
				out var linkHandle,
				ACCESS_MASK.GENERIC_READ,
				ref objectAttributes);
			if (status < 0)
			{
				return Result<string>.Failed("Open file failed with status " + status);
			}

			using (linkHandle)
			{
				var targetBuffer = new UNICODE_STRING(new string(' ', 512));
				status = Ntdll.NtQuerySymbolicLinkObject(
					linkHandle,
					ref targetBuffer,
					out var len);
				return status < 0
					? Result<string>.Failed("Query link failed with status " + status)
					: Result<string>.Succeeded(targetBuffer.ToString());
			}
		}

		/// <summary>
		///     This only opens a Directory type object, returning a handle to it.
		/// </summary>
		public static SafeFileHandle OpenDirectoryObject(string objectName)
		{
			var objectAttributes = new OBJECT_ATTRIBUTES(objectName, 0);
			var status = Ntdll.NtOpenDirectoryObject(out var handle, ACCESS_MASK.DIRECTORY_QUERY, ref objectAttributes);
			return status < 0 ? null : handle;
		}

		public static string GetObjectType(string objectName)
		{
			var handle = OpenDirectoryObject(objectName);
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
			var objects = QueryDirectoryObjects(parentDirectory);
			var desiredObject = objects.FirstOrDefault(o => o.Name == objectFileName);
			return desiredObject.TypeName;
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

		public static IEnumerable<ObjectDirectoryInformation> QueryDirectoryObjects(string objectName)
		{
			var directoryHandle = OpenDirectoryObject(objectName);
			// throw?
			if (directoryHandle == null) return Enumerable.Empty<ObjectDirectoryInformation>();

			using (directoryHandle)
			{
				return QueryDirectoryObjects(directoryHandle);
			}
		}

		private static IEnumerable<ObjectDirectoryInformation> QueryDirectoryObjects(SafeFileHandle directoryHandle)
		{
			var bufferSize = 1024;
			var buffer = Marshal.AllocHGlobal(bufferSize);
			uint context = 0;
			var objects = new List<ObjectDirectoryInformation>();
			for (;;)
			{
				var status = Ntdll.NtQueryDirectoryObject(directoryHandle, buffer, bufferSize,
					true, context == 0, ref context, out var lengthRead);
				if (status < 0) break;

				var objectDirectoryInformation = Marshal.PtrToStructure<OBJECT_DIRECTORY_INFORMATION>(buffer);
				objects.Add(new ObjectDirectoryInformation(objectDirectoryInformation, context));
			}
			Marshal.FreeHGlobal(buffer);
			return objects;
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

		public struct ObjectDirectoryInformation
		{
			public string Name;
			public string TypeName;
			public uint Context;

			public ObjectDirectoryInformation(OBJECT_DIRECTORY_INFORMATION objectDirectoryInformation, uint context) : this()
			{
				Name = objectDirectoryInformation.Name.ToString();
				TypeName = objectDirectoryInformation.TypeName.ToString();
				Context = context;
			}

			public bool IsDirectory()
			{
				return TypeName == "Directory";
			}
		}
	}
}