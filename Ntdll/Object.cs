namespace PInvoke.Ntdll
{
	public class Object
	{
		//public uint Context // todo is this useful to show?;

		public Object(string name, string typeName)
		{
			Name = name;
			TypeName = typeName;
		}

		public string Name { get; }
		public string TypeName { get; }
	}
}