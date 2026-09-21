namespace zerocurve
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        [Owner("Bailin")]
        [Priority(0)]
        [Description("Tests that addition returns the expected sum.")]
        public void Addition_ReturnsExpectedSum()
        {
            Assert.AreEqual(4, 2 + 2);
        }

        [TestMethod]
        [Owner("Bailin")]
        [Priority(1)]
        [Description("Tests that string comparison returns equal.")]
        public void StringComparison_ReturnsEqual()
        {
            Assert.AreEqual("zerocurve", "zero" + "curve");
        }

        [TestMethod]
        [Owner("Bailin")]
        [Priority(2)]
        [Description("Tests an intentional failure scenario.")]
        public void IntentionalFailure_ReturnsFailure()
        {
            Assert.AreEqual(5, 2 + 2);
        }
    }
}
