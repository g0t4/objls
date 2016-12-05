using System;
using System.Collections.Generic;
using System.Text;
using PInvoke.Ntdll;
using Xunit;

namespace Ntdll.Tests
{
    public class SymbolicLinkObjectTests
    {
		[Fact]
		public void GetSymbolicLinkObjectTarget()
		{
			Assert.Equal(@"\Device\BitLocker", NtdllHelper.GetSymbolicLinkObjectTarget(@"\GLOBAL??\BitLocker"));
		}
	}
}
