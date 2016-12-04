using System.Runtime.InteropServices;

namespace objls
{

	/// <summary>
	/// http://www.pinvoke.net/default.aspx/Enums/OBJECT_INFORMATION_CLASS.html
	/// </summary>
	public enum OBJECT_INFORMATION_CLASS : int
	{
		ObjectBasicInformation = 0,
		ObjectNameInformation = 1,
		ObjectTypeInformation = 2,
		ObjectAllTypesInformation = 3,
		ObjectHandleInformation = 4
	}

	// built manually from https://msdn.microsoft.com/en-us/library/bb432383(v=vs.85).aspx
	[StructLayout(LayoutKind.Sequential)]
	public struct PUBLIC_OBJECT_TYPE_INFORMATION
	{
		public UNICODE_STRING TypeName;
	}

	// built manually from https://msdn.microsoft.com/en-us/library/bb432383(v=vs.85).aspx
	[StructLayout(LayoutKind.Sequential)]
	public struct PUBLIC_OBJECT_BASIC_INFORMATION
	{
		public ulong Attributes;
		public ACCESS_MASK GrantedAccess;
		public ulong HandleCount;
		public ulong PointerCount;
	}
}
