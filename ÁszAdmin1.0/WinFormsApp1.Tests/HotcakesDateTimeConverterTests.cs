using System.Globalization;
using System.Text.Json;

namespace WinFormsApp1.Tests;

[TestClass]
public sealed class HotcakesDateTimeConverterTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Converters = { new HotcakesDateTimeConverter() }
    };

    [TestMethod]
    public void Read_ParsesMicrosoftDateFormatAsUtc()
    {
        // Azt teszteli, hogy a Date() formatum helyesen UTC DateTime-ma alakul.
        const long unixMilliseconds = 1713916800000;
        DateTime expected = DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).UtcDateTime;

        DateTime parsed = JsonSerializer.Deserialize<DateTime>(
            $@"""/Date({unixMilliseconds}+0200)/""",
            SerializerOptions);

        Assert.AreEqual(expected, parsed);
        Assert.AreEqual(DateTimeKind.Utc, parsed.Kind);
    }

    [TestMethod]
    public void Read_ParsesUnixMillisecondsAsUtc()
    {
        // Azt teszteli, hogy a nyers unix millisecond ertek is helyesen parse-olodik.
        const long unixMilliseconds = 1713916800000;
        DateTime expected = DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).UtcDateTime;

        DateTime parsed = JsonSerializer.Deserialize<DateTime>(
            unixMilliseconds.ToString(CultureInfo.InvariantCulture),
            SerializerOptions);

        Assert.AreEqual(expected, parsed);
        Assert.AreEqual(DateTimeKind.Utc, parsed.Kind);
    }

    [TestMethod]
    public void Write_SerializesUsingRoundtripUtcFormat()
    {
        // Azt teszteli, hogy a szerializalas UTC roundtrip sztringet ir ki.
        DateTime value = new(2026, 4, 24, 10, 15, 0, DateTimeKind.Utc);

        string json = JsonSerializer.Serialize(value, SerializerOptions);

        Assert.AreEqual(@"""2026-04-24T10:15:00.0000000Z""", json);
    }
}
