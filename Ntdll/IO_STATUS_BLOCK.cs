using System;

namespace Ntdll
{
	/// <summary>
	/// https://msdn.microsoft.com/en-us/library/windows/hardware/ff550671(v=vs.85).aspx
	/// </summary>
	public struct IO_STATUS_BLOCK
	{
		public NtStatus Status;
		public IntPtr Information;
	}
}