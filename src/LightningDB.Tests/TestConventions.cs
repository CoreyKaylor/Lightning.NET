using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Fixie;

namespace LightningDB.Tests;

class TestProject : ITestProject
{
    public void Configure(TestConfiguration configuration, TestEnvironment environment)
    {
        configuration.Conventions.Add<CustomDiscovery, ParameterizedExecution>();
    }
}
class ParameterizedExecution : IExecution
{
    public async Task Run(TestSuite testSuite)
    {
        foreach (var test in testSuite.Tests)
        {
            await test.Run();
        }
        TestBase.CleanupSession();
    }
}

class CustomDiscovery : IDiscovery
{
    public IEnumerable<Type> TestClasses(IEnumerable<Type> concreteClasses)
        => concreteClasses
            .Where(x => x.Name.EndsWith("Tests"));

    public IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
        => publicMethods
            .Where(x => !x.IsStatic)
            .Where(x => !x.Name.EndsWith("OnlyOn64BitPlatform") || RuntimeInformation.OSArchitecture != Architecture.X86);
}