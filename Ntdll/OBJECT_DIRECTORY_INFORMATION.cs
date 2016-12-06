using System.Runtime.InteropServices;

namespace PInvoke.Ntdll
{
	/// <summary>
	///     http://www.pinvoke.net/default.aspx/Structures/OBJECT_DIRECTORY_INFORMATION.html
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct OBJECT_DIRECTORY_INFORMATION
	{
		public UNICODE_STRING Name;
		public UNICODE_STRING TypeName;

		public Object ToObject()
		{
			switch (TypeName.ToString())
			{
				case "Directory":
					return new DirectoryObject(Name.ToString());
				case "SymbolicLink":
					return new SymbolicLinkObject(Name.ToString());
				default:
					return new Object(Name.ToString(), TypeName.ToString());
			}
		}
	}
}