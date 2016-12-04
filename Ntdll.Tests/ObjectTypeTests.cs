using PInvoke.Ntdll;
using Xunit;

namespace Ntdll.Tests
{
	public class ObjectTypeTests
	{
		[Fact]
		public void GetObjectType_IsDirectory_ReturnsDirectory()
		{
			var directory = @"\GLOBAL??";

			var type = NtdllHelper.GetObjectType(directory);

			Assert.Equal(type, "Directory");
		}

		[Fact]
		public void GetObjectType_IsNotADirectory_ReturnsOtherwise()
		{
			var directory = @"\GLOBAL??\C:";

			var type = NtdllHelper.GetObjectType(directory);

			Assert.Equal(type, "NOT");
		}
	}
}