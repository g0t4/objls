namespace PInvoke.Ntdll
{
	public static class ObjectNameExtensions
	{
		public static string ObjectNameJoin(this string root, string relative)
		{
			if (root.EndsWith(@"\"))
			{
				return root + relative;
			}
			return root + @"\" + relative;
		}
	}
}