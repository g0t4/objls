using PInvoke.Ntdll;
using Xunit;

namespace Ntdll.Tests
{
	public class ObjectTypeTests
	{


		[Fact]
		public void GetObjectType_IsDirectory()
		{
			// test global namespace root 
			Assert.Equal("Directory", NtdllHelper.GetObjectType(@"\"));

			// test ? on end of directory name
			Assert.Equal("Directory", NtdllHelper.GetObjectType(@"\GLOBAL??"));
		}

		[Fact]
		public void GetObjectType_IsNotDirectory()
		{
			Assert.Equal("Device", NtdllHelper.GetObjectType(@"\clfs"));
			// test C: symlink - tricky for parsing C: name
			Assert.Equal("SymbolicLink", NtdllHelper.GetObjectType(@"\GLOBAL??\C:"));
		}

	
	}
}