
using Rocket.Surgery.Extensions.Testing;
using Xunit;

class A
{
    public A()
    {

        Xunit.Abstractions.ITestOutputHelper a = null;
        a.GetTest();
    }
}
