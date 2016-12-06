using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace PInvoke.Ntdll
{
	public class DirectoryObject : Object
	{
		public DirectoryObject(string name) : base(name, "Directory")
		{
			// todo throw if not directory object?
		}

		/// <summary>
		///     This only opens a directory object, returning a handle to it.
		/// </summary>
		public SafeFileHandle Open()
		{
			var objectAttributes = new OBJECT_ATTRIBUTES(Name, 0);
			var status = Ntdll.NtOpenDirectoryObject(out var handle, ACCESS_MASK.DIRECTORY_QUERY, ref objectAttributes);
			return status < 0 ? null : handle;
		}

		public IEnumerable<Object> QueryDirectoryObjects()
		{
			var directoryHandle = Open();
			// throw?
			if (directoryHandle == null) return Enumerable.Empty<Object>();

			using (directoryHandle)
			{
				return QueryDirectoryObjects(directoryHandle);
			}
		}

		private static IEnumerable<Object> QueryDirectoryObjects(SafeFileHandle directoryHandle)
		{
			var bufferSize = 1024;
			var buffer = Marshal.AllocHGlobal(bufferSize);
			uint context = 0;
			// todo use object model here too
			var objects = new List<Object>();
			for (;;)
			{
				var status = Ntdll.NtQueryDirectoryObject(directoryHandle, buffer, bufferSize,
					true, context == 0, ref context, out var lengthRead);
				if (status < 0) break;

				var objectDirectoryInformation = Marshal.PtrToStructure<OBJECT_DIRECTORY_INFORMATION>(buffer);
				objects.Add(objectDirectoryInformation.ToObject());
			}
			Marshal.FreeHGlobal(buffer);
			return objects;
		}
	}
}