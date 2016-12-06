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
		var objects = NtdllHelper.QueryDirectoryObjects(objectName);

		WriteLine();
		WriteLine($"Directory contents ({objects.Count()} objects):");
		WriteLine();

		var typeColumnLength = objects.Max(o => o.TypeName.Length) + 1;

		var sortedObjects = objects
			.OrderByDescending(o => o.IsDirectory())
			.ThenBy(o => o.Name);

		foreach (var @object in sortedObjects)
		{
			if (@object.TypeName == "SymbolicLink")
			{
				var linkTarget = NtdllHelper.GetSymbolicLinkObjectTarget($"{objectName}\\{@object.Name}");
				WriteLine("{0,-" + typeColumnLength + "} {1} {2}", @object.TypeName, @object.Name, linkTarget);
			}
			else
			{
				WriteLine("{0,-" + typeColumnLength + "} {1}", @object.TypeName, @object.Name);
			}
		}
	}
}