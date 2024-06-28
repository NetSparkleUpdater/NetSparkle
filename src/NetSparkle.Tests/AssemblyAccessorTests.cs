using NetSparkleUpdater;
using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.AssemblyAccessors;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NetSparkleUnitTests
{
    // https://stackoverflow.com/a/41985831/3938401 for fixture code
    public class AssemblyAccessorTestsFixture : IDisposable
    {
        private string _tmpDir;
        private string _dllPath;

        public const string AssemblyVersion = "2.0.1-beta-1";
        public const string FileVersion = "2.0.1";
        public const string Company = "Sparkle Company";
        public const string Description = "Sparkle Description";
        public const string Copyright = "Sparkle Copyright";
        public const string Title = "MyAwesomeLibTitle";
        public const string Product = "MyProduct";

        public bool DidSucceed
        {
            get => !string.IsNullOrWhiteSpace(_dllPath) && File.Exists(_dllPath);
        }

        public string DllPath
        {
            get => _dllPath;
        }

        public AssemblyAccessorTestsFixture()
        {
            BuildDll();
        }

        public void Dispose()
        {
            if (Directory.Exists(_tmpDir))
            {
                Directory.Delete(_tmpDir, true);
            }
        }

        // TODO: refactor some of these common test methods (between app cast and sparkle testing)
        // to a common testing DLL or something
        private string GetCleanTempDir()
        {
            var tempPath = Path.GetTempPath();
            var tempDir = Path.Combine(tempPath, "netsparkle-unit-tests-13927");
            // remove any files set up in previous tests
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }

        // https://stackoverflow.com/a/1344242/3938401
        private static string RandomString(int length)
        {
            Random random = new SecureRandom();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void BuildDll()
        {
            var envVersion = Environment.Version;
            var dotnetVersion = "net" + envVersion.Major + ".0";
            var csproj = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>" + dotnetVersion + @"</TargetFramework>
    <RootNamespace>assembly_accessor_testing</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <Version>" + AssemblyVersion + @"</Version>
    <AssemblyVersion>" + FileVersion + @"</AssemblyVersion>
    <AssemblyTitle>" + Title + @"</AssemblyTitle>
    <Description>" + Description + @"</Description>
    <Company>" + Company + @"</Company>
    <Product>" + Product + @"</Product>
    <Copyright>" + Copyright + @"</Copyright>
  </PropertyGroup>
</Project>".Trim();
            var program = @"Console.WriteLine(""Hello, World!"");";
            _tmpDir = GetCleanTempDir();
            var csprojPath = Path.Combine(_tmpDir, "proj.csproj");
            var programPath = Path.Combine(_tmpDir, "Program.cs");
            var innerFolder = RandomString(10);
            var buildPath = Directory.CreateDirectory(Path.Combine(_tmpDir, innerFolder)).FullName;
            try
            {
                File.WriteAllText(csprojPath, csproj);
                File.WriteAllText(programPath, program);
                // compile it
                var p = new Process()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = "dotnet",
                        WorkingDirectory = _tmpDir,
                        Arguments = $"build --framework " + dotnetVersion + " --output \"" + buildPath + "\""
                    }
                };
                p.OutputDataReceived += (o, e) => 
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        Console.WriteLine("[Unit test build output] " + e.Data);
                    }
                };
                p.ErrorDataReceived += (o, e) => 
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        Console.WriteLine("[Unit test build output] ERROR: " + e.Data);
                    }
                };
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();     
                p.WaitForExit();
                _dllPath = Path.Combine(buildPath, "proj.dll");
            }
            catch
            {
                Dispose();
            }
        }
    }


    [CollectionDefinition("Assembly collection")]
    public class AssemblyCollection : ICollectionFixture<AssemblyAccessorTestsFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    [Collection("Assembly collection")]
    public class AssemblyAccessorTests
    {
        AssemblyAccessorTestsFixture _fixture;

        public AssemblyAccessorTests(AssemblyAccessorTestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestAsmResolverAccessor()
        {
            if (!_fixture.DidSucceed)
            {
                Assert.True(false, "Failed to build DLL");
            }
            var accessor = new AsmResolverAccessor(_fixture.DllPath);
            Assert.Equal(AssemblyAccessorTestsFixture.Company, accessor.AssemblyCompany);
            Assert.Equal(AssemblyAccessorTestsFixture.Copyright, accessor.AssemblyCopyright);
            Assert.Equal(AssemblyAccessorTestsFixture.Description, accessor.AssemblyDescription);
            Assert.Equal(AssemblyAccessorTestsFixture.Title, accessor.AssemblyTitle);
            Assert.Equal(AssemblyAccessorTestsFixture.Product, accessor.AssemblyProduct);
            Assert.Equal(AssemblyAccessorTestsFixture.AssemblyVersion, accessor.AssemblyVersion);
            Assert.Equal(AssemblyAccessorTestsFixture.FileVersion, accessor.FileVersion);
        }

        [Fact]
        public void TestDiagnosticsAccessor()
        {
            if (!_fixture.DidSucceed)
            {
                Assert.True(false, "Failed to build DLL");
            }
            var accessor = new AssemblyDiagnosticsAccessor(_fixture.DllPath);
            Assert.Equal(AssemblyAccessorTestsFixture.Company, accessor.AssemblyCompany);
            Assert.Equal(AssemblyAccessorTestsFixture.Copyright, accessor.AssemblyCopyright);
            Assert.Equal(AssemblyAccessorTestsFixture.Description, accessor.AssemblyDescription);
            Assert.Equal(AssemblyAccessorTestsFixture.Product, accessor.AssemblyTitle);
            Assert.Equal(AssemblyAccessorTestsFixture.Product, accessor.AssemblyProduct);
            Assert.Equal(AssemblyAccessorTestsFixture.AssemblyVersion, accessor.AssemblyVersion);
            // Assert.Equal(AssemblyAccessorTestsFixture.FileVersion, accessor.FileVersion);
        }

        [Fact]
        public void TestReflectionAccessor()
        {
            if (!_fixture.DidSucceed)
            {
                Assert.True(false, "Failed to build DLL");
            }
            var accessor = new AssemblyReflectionAccessor(_fixture.DllPath);
            Assert.Equal(AssemblyAccessorTestsFixture.Company, accessor.AssemblyCompany);
            Assert.Equal(AssemblyAccessorTestsFixture.Copyright, accessor.AssemblyCopyright);
            Assert.Equal(AssemblyAccessorTestsFixture.Description, accessor.AssemblyDescription);
            Assert.Equal(AssemblyAccessorTestsFixture.Title, accessor.AssemblyTitle);
            Assert.Equal(AssemblyAccessorTestsFixture.Product, accessor.AssemblyProduct);
            Assert.Equal(AssemblyAccessorTestsFixture.AssemblyVersion, accessor.AssemblyVersion);
            // Assert.Equal(AssemblyAccessorTestsFixture.FileVersion, accessor.FileVersion);
        }
    }
}
