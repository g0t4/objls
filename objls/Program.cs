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
		var objects = new DirectoryObject(objectName).QueryDirectoryObjects();

		WriteLine();
		WriteLine($"Directory contents ({objects.Count()} objects):");
		WriteLine();

		var typeColumnLength = objects.Max(o => o.TypeName.Length) + 1;

		var sortedObjects = objects
			.OrderByDescending(o => o is DirectoryObject)
			.ThenBy(o => o.Name);

		foreach (var @object in sortedObjects)
		{
			if (@object.TypeName == "SymbolicLink")
			{
				var linkTarget = new SymbolicLinkObject($"{objectName}\\{@object.Name}").GetLinkTarget();
				var printLink = linkTarget.Success ? linkTarget.Value : linkTarget.FailureReason;
				WriteLine("{0,-" + typeColumnLength + "} {1} {2}", @object.TypeName, @object.Name, printLink);
			}
			else
			{
				WriteLine("{0,-" + typeColumnLength + "} {1}", @object.TypeName, @object.Name);
			}
		}
	}
}