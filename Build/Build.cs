// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// The re-motion Core Framework is free software; you can redistribute it
// and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 2.1 of the
// License, or (at your option) any later version.
//
// re-motion is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
//
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Customizations;
using JetBrains.Annotations;
using NuGet.Versioning;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Remotion.BuildScript;
using Remotion.BuildScript.Components;
using Remotion.BuildScript.GenerateSbom;
using Remotion.BuildScript.Test;
using Remotion.BuildScript.Test.Dimensions;
using Remotion.BuildScript.Test.Runtimes;
using static Customizations.Databases;
using static Remotion.BuildScript.Test.Dimensions.Configurations;
using static Remotion.BuildScript.Test.Dimensions.ExecutionRuntimes;
using static Remotion.BuildScript.Test.Dimensions.OperatingSystems;
using static Remotion.BuildScript.Test.Dimensions.Platforms;
using static Remotion.BuildScript.Test.Dimensions.TargetFrameworks;

// ReSharper disable RedundantTypeArgumentsOfMethod

class Build : RemotionBuild, IDependDB, ITest
{

  [Parameter(ValueProviderMember = nameof(SupportedTestSqlServers), Separator = "+")]
  public string[] TestSqlServers { get; set; } = [];

  [CanBeNull] private TestMatrix _databaseTestMatrix;
  [CanBeNull] private TestMatrix _normalTestMatrix;

  
  public static int Main () => Execute<Build>();

  [UsedImplicitly]
  public Target AddRemotionPackagingArtefacts => _ => _
      .TriggeredBy<IPack>()
      .Executes (() => { });

  public override ISbomGeneratorBuilder ConfigureSbomGenerationInfoBuilder (Solution solution)
  {
      var version = ((IBuildMetadata)this).GetBaseVersion();

      var semanticVersion = SemanticVersion.Parse(version);
      var shortenedVersion = $"{semanticVersion.Major}.{semanticVersion.Minor}.{semanticVersion.Patch}";

      var blacklistedProjects = new[]
                                {
                                    "Web.Dependencies.Javascript",
                                    "*.Analyzers*"
                                    // We disregard test projects when generating the sbom already, so these are not required here.
                                };

      var packageJsonPath = solution.Directory / "Remotion" / "Web" / "Dependencies.JavaScript" / "package.json";
      var packageJsonWithVersion = TemporaryDirectory / "sbom" / "package.json";

      AddVersionToPackageJson(packageJsonPath, packageJsonWithVersion, shortenedVersion);

      var outputFolderPath = ((IBaseBuild)this).OutputFolder / "SBOM";
      outputFolderPath.CreateDirectory();

      var outputSbomPath = outputFolderPath / "re-motion.sbom.json";

      // We do not require github auth because we do not do enough requests for licenses and package infos
      var builder = new SolutionSbomGeneratorBuilder(solution, semanticVersion.ToString(), TemporaryDirectory / "sbomGeneration", outputSbomPath, "", "")
              .WithProjectsBlackListed(blacklistedProjects)
              .WithPackageJsonFile(packageJsonWithVersion);

      return builder;
  }

  public override void ConfigureProjects (ProjectsBuilder projects)
  {
    [CanBeNull]
    static TestConfiguration CreateTestConfiguration (
        [CanBeNull] TestMatrix testMatrix,
        ITestExecutionRuntimeFactory testExecutionRuntimeFactory,
        ImmutableArray<ITestExecutionWrapper> testExecutionWrappers)
    {
      return testMatrix != null
          ? new TestConfiguration(testExecutionRuntimeFactory, testMatrix, testExecutionWrappers)
          : null;
    }

    var testExecutionRuntimeFactory = new DefaultTestExecutionRuntimeFactory(NullDockerRunSettingsCustomizer.Instance);

    // NOTE: Test matrices might be null if the CreateTestMatrix step was not called.
    // This is intended behavior as we want to support partial builds.
    // If there is no test matrix, the test configuration will be null as well.

    var normalTestConfiguration = CreateTestConfiguration(
        _normalTestMatrix,
        testExecutionRuntimeFactory,
        ImmutableArray<ITestExecutionWrapper>.Empty);

    projects.AddReleaseProject("Core");
    projects.AddReleaseProject("Development");
    projects.AddReleaseProject("Documentation");

    projects.AddUnitTestProject("Core.UnitTests", normalTestConfiguration);
    projects.AddUnitTestProject("IntegrationTests", normalTestConfiguration);
  }

  public override void ConfigureSupportedTestDimensions (SupportedTestDimensionsBuilder supportedTestDimensions)
  {
    supportedTestDimensions.AddOperatingSystemsDimension();
    supportedTestDimensions.AddSupportedDimension<ExecutionRuntimes>(LocalMachine, EnforcedLocalMachine, Docker_Win_NET48, Docker_Win_NET472, Docker_Win_NET462);
    supportedTestDimensions.AddSupportedDimension<TargetFrameworks>(NET48, NET472, NET462);
    supportedTestDimensions.AddSupportedDimension<Configurations>(Debug, Release);
    supportedTestDimensions.AddSupportedDimension<Platforms>(x64, x86);
    supportedTestDimensions.AddSupportedDimension<Databases>(NoDB);
  }

  public override void ConfigureEnabledTestDimensions (EnabledTestDimensionsBuilder enabledTestDimensions)
  {
    base.ConfigureEnabledTestDimensions(enabledTestDimensions);

    enabledTestDimensions.AddEnabledOperatingSystems();

    if (SupportedTestDimensions.IsSupported<Databases>())
    {
      var testSqlServers = SupportedTestDimensions.ParseTestDimensionValuesOrDefault<Databases>(TestSqlServers)
                           ?? throw CreateConfigurationException<Databases>();

      enabledTestDimensions.AddEnabledDimension(testSqlServers);
    }

    return;

    static InvalidOperationException CreateConfigurationException<T> ()
        where T : TestDimension
    {
      return new InvalidOperationException($"The configuration for test dimension '{typeof(T).Name}' cannot be empty.");
    }
  }

  public override void ConfigureTestMatrix (TestMatricesBuilder builder)
  {

    _normalTestMatrix = builder.AddTestMatrix (
        "NormalTestMatrix",
        new TestDimension[,]
        {
            { Windows, Docker_Win_NET48, NET48, NoDB, Debug, x86 },
            { Windows, Docker_Win_NET48, NET48, NoDB, Release, x86 },
            { Windows, Docker_Win_NET48, NET48, NoDB, Debug, x64 },
            { Windows, Docker_Win_NET48, NET48, NoDB, Release, x64 },

            //  Local-->
            { Windows, LocalMachine, NET48, NoDB, Debug, x86 },
            { Windows, LocalMachine, NET48, NoDB, Release, x86 },
            { Windows, LocalMachine, NET48, NoDB, Debug, x64 },
            { Windows, LocalMachine, NET48, NoDB, Release, x64 },
            
            // Exercise compatibility between installed .NET version, target framework and SQL Server
            { Windows, Docker_Win_NET48, NET472, NoDB, Release, x64 },
            { Windows, Docker_Win_NET48, NET462, NoDB, Release, x64 },
            { Windows, Docker_Win_NET472, NET472, NoDB, Release, x64 },
            { Windows, Docker_Win_NET472, NET462, NoDB, Release, x64 },
            { Windows, Docker_Win_NET462, NET462, NoDB, Release, x64 },
        },
        allowEmpty: true);
  }


  protected IEnumerable<string> SupportedTestSqlServers => GetTestDimensionValueList<Databases>();

  private void AddVersionToPackageJson (AbsolutePath packageJsonPath, AbsolutePath duplicatedPackageJsonPath, string version)
  {
      var packageJsonContent = packageJsonPath.ReadAllText().Replace("$version$", version);

      duplicatedPackageJsonPath.WriteAllText(packageJsonContent);
  }
}
