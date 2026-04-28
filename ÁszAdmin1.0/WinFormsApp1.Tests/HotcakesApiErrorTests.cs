namespace WinFormsApp1.Tests;

[TestClass]
public sealed class HotcakesApiErrorTests
{
    [TestMethod]
    public void ToString_ReturnsCodeAndDescriptionWhenCodeIsFilled()
    {
        // Azt teszteli, hogy kitoltott hibakod eseten a kod es a leiras is megjelenik.
        HotcakesApiError error = new()
        {
            Code = "SKU_DUPLICATE",
            Description = "The SKU already exists."
        };

        string text = error.ToString();

        Assert.AreEqual("SKU_DUPLICATE: The SKU already exists.", text);
    }

    [TestMethod]
    public void ToString_ReturnsOnlyDescriptionWhenCodeIsBlank()
    {
        // Azt teszteli, hogy ures kod eseten csak a leiras marad a szovegben.
        HotcakesApiError error = new()
        {
            Code = " ",
            Description = "Only the description should remain."
        };

        string text = error.ToString();

        Assert.AreEqual("Only the description should remain.", text);
    }
}
