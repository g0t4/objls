using objls;
using System.Runtime.InteropServices;

/// <summary>
/// http://www.pinvoke.net/default.aspx/Structures/OBJECT_DIRECTORY_INFORMATION.html
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct OBJECT_DIRECTORY_INFORMATION
{
	public UNICODE_STRING Name;
	public UNICODE_STRING TypeName;
}
