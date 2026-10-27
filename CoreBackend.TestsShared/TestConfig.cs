using NUnit.Framework;

[assembly: LevelOfParallelism(6)]
[assembly: FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[assembly: Parallelizable(ParallelScope.All)]
