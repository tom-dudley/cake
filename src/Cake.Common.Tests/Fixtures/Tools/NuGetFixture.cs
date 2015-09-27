﻿using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.IO.NuGet;
using Cake.Testing;
using NSubstitute;

namespace Cake.Common.Tests.Fixtures.Tools
{
    public abstract class NuGetFixture
    {
        public FakeFileSystem FileSystem { get; set; }
        public ICakeEnvironment Environment { get; set; }
        public IProcessRunner ProcessRunner { get; set; }
        public IProcess Process { get; set; }
        public ICakeLog Log { get; set; }
        public INuGetToolResolver NuGetToolResolver { get; set; }
        public IGlobber Globber { get; set; }

        protected NuGetFixture()
        {
            Environment = FakeEnvironment.CreateUnixEnvironment();

            Process = Substitute.For<IProcess>();
            Process.GetExitCode().Returns(0);
            ProcessRunner = Substitute.For<IProcessRunner>();
            ProcessRunner.Start(Arg.Any<FilePath>(), Arg.Any<ProcessSettings>()).Returns(Process);

            Globber = Substitute.For<IGlobber>();
            Globber.Match("./tools/**/nuget.exe").Returns(new[] { (FilePath)"/Working/tools/NuGet.exe" });
            Globber.Match("./tools/**/NuGet.exe").Returns(new[] { (FilePath)"/Working/tools/NuGet.exe" });

            NuGetToolResolver = Substitute.For<INuGetToolResolver>();

            Log = Substitute.For<ICakeLog>();
            FileSystem = new FakeFileSystem(Environment);

            // By default, there is a default tool.
            NuGetToolResolver.ResolvePath().Returns("/Working/tools/NuGet.exe");
            FileSystem.CreateFile("/Working/tools/NuGet.exe");

            // Set standard output.
            Process.GetStandardOutput().Returns(new string[0]);
        }

        public void GivenCustomToolPathExist(FilePath toolPath)
        {
            FileSystem.CreateFile(toolPath);
        }

        public void GivenDefaultToolDoNotExist()
        {
            var toolPath = new FilePath("/Working/tools/NuGet.exe");
            FileSystem.EnsureFileDoNotExist(toolPath);
            NuGetToolResolver.ResolvePath().Returns("/NonWorking/tools/NuGet.exe");
        }

        public void GivenProcessCannotStart()
        {
            ProcessRunner.Start(Arg.Any<FilePath>(), Arg.Any<ProcessSettings>()).Returns((IProcess)null);
        }

        public void GivenProcessReturnError()
        {
            Process.GetExitCode().Returns(1);
        }
    }
}