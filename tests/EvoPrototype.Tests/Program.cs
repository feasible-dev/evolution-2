var harness = new TestHarness();

PhysicsTests.Register(harness);
EvolutionTests.Register(harness);
AnalyticsTests.Register(harness);

return harness.Finish();
