namespace PInvoke.Ntdll
{
	public class SymbolicLinkObject : Object
	{
		public SymbolicLinkObject(string name)
			: base(name, "SymbolicLink")
		{
		}

		public Result<string> GetLinkTarget()
		{
			var type = NtdllHelper.GetObjectType(Name);
			if (type != "SymbolicLink")
				return null;

			var objectAttributes = new OBJECT_ATTRIBUTES(Name, 0);
			var status = Ntdll.NtOpenSymbolicLinkObject(
				out var linkHandle,
				ACCESS_MASK.GENERIC_READ,
				ref objectAttributes);
			if (status < 0)
			{
				return Result<string>.Failed("Open file failed with status " + status);
			}

			using (linkHandle)
			{
				var targetBuffer = new UNICODE_STRING(new string(' ', 512));
				status = Ntdll.NtQuerySymbolicLinkObject(
					linkHandle,
					ref targetBuffer,
					out var len);
				return status < 0
					? Result<string>.Failed("Query link failed with status " + status)
					: Result<string>.Succeeded(targetBuffer.ToString());
			}
		}
	}
}