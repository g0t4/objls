using System.Linq;
using PInvoke.Ntdll;
using static System.Console;

internal class Program
{
	private static void Main(string[] args)
	{
		// default to root object namespace
		var objectName = @"\";
		if (args.Length > 0)
		{
			objectName = args[0];
		}

		var type = NtdllHelper.GetObjectType(objectName);
		WriteLine($"Object type {type}: {objectName}");

		switch (type)
		{
			case "Directory":
				ListDirectoryContents(objectName);
				break;
		}
	}

	private static void ListDirectoryContents(string objectName)
	{

		WriteLine();
		WriteLine($"Directory contents:");
		WriteLine();

		var objects = NtdllHelper.QueryDirectoryObjects(objectName);

		var typeColumnLength = objects.Max(o => o.TypeName.Length) + 1;

		var sortedObjects = objects
			.OrderByDescending(o => o.IsDirectory())
			.ThenBy(o => o.Name);

		foreach (var @object in sortedObjects)
		{
			WriteLine("{0,-" + typeColumnLength + "} {1}", @object.TypeName, @object.Name);
		}
	}
}