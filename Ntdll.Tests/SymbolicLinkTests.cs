using PInvoke.Ntdll;
using Xunit;

namespace Ntdll.Tests
{
	public class SymbolicLinkObjectTests
	{
		[Fact]
		public void GetLinkTarget_IsASymLink_ReturnsTarget()
		{
			var target = new SymbolicLinkObject(@"\GLOBAL??\BitLocker").GetLinkTarget();

			Assert.True(target.Success);
			Assert.Equal(@"\Device\BitLocker", target.Value);
		}

		[Fact]
		public void GetLinkTarget_DosDevices_ShouldReturnTarget()
		{
			var target = new SymbolicLinkObject(@"\DosDevices").GetLinkTarget();

			Assert.Null(target.FailureReason);
			Assert.True(target.Success);
			Assert.Equal(@"\??", target.Value);
			// note the above symlink is relative to my dev env potentially
		}
	}
}