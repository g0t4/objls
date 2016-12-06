using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace PInvoke.Ntdll
{
    public class DirectoryObject
    {
	    public string Name { get; }

	    public DirectoryObject(string name)
	    {
			// todo throw if not directory object?
			Name = name;
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

		public IEnumerable<ObjectDirectoryInformation> QueryDirectoryObjects()
		{
			var directoryHandle = Open();
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
			// todo use object model here too
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
