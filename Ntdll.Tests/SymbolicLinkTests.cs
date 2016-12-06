using PInvoke.Ntdll;
using Xunit;

namespace Ntdll.Tests
{
	public class SymbolicLinkObjectTests
	{
		[Fact]
		public void GetSymbolicLinkObjectTarget()
		{
			var target = new SymbolicLinkObject(@"\GLOBAL??\BitLocker").GetLinkTarget();

			Assert.True(target.Success);
			Assert.Equal(@"\Device\BitLocker", target.Value);
		}
	}
}