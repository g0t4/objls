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
		var objectName = @"\GLOBAL??\C:";
		var type = NtdllHelper.GetObjectType(objectName);

		WriteLine(objectName);
		WriteLine(type);
		return;

		var objects = NtdllHelper.QueryDirectoryObjects(objectName);
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

	

}
