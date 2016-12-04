using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using PInvoke.Ntdll;
using static System.Console;

class Program
{
	static void Main(string[] args)
	{
		var objectName = @"\clfs";
		var type = NtdllHelper.GetObjectType(objectName);

		WriteLine(objectName);
		WriteLine(type);
		return;

		var objects = QueryDirectoryObjects(objectName);
		WriteLine($"Object Directory of {objectName}");
		WriteLine();
		var maxTypeNameLength = objects.Max(o => o.TypeName.Length);
		foreach (var @object in objects
					.OrderByDescending(o => o.IsDirectory())
					.ThenBy(o => o.Name))
		{
			WriteLine("{0,-" + maxTypeNameLength + "} {1}", @object.TypeName, @object.Name);
		}
	}

	

	static IEnumerable<ObjectDirectoryInformation> QueryDirectoryObjects(string objectName)
	{
		var directoryHandle = NtdllHelper.OpenDirectoryObject(objectName);
		// throw?
		if (directoryHandle == null) return Enumerable.Empty<ObjectDirectoryInformation>();

		using (directoryHandle)
		{
			return QueryDirectoryObjects(directoryHandle);
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

}
