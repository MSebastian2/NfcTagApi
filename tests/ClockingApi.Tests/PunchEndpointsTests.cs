using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class PunchEndpointsTests : IClassFixture<TestingWebAppFactory>
{
    private readonly HttpClient _client;

    public PunchEndpointsTests(TestingWebAppFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private sealed record PunchResult(string status, double? minutes, DateTime? startedAtUtc, DateTime? endedAtUtc);

    [Fact]
    public async Task Punch_Open_Then_Close_Succeeds()
    {
        var body = new { TagUid = "04AABBCCDD22", ReaderCode = "LAB-001" };

        var r1 = await _client.PostAsJsonAsync("/punch", body);
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        var j1 = await r1.Content.ReadFromJsonAsync<PunchResult>();
        j1.Should().NotBeNull();
        j1!.status.Should().Be("opened");
        j1.startedAtUtc.Should().NotBeNull();

        var r2 = await _client.PostAsJsonAsync("/punch", body);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
        var j2 = await r2.Content.ReadFromJsonAsync<PunchResult>();
        j2.Should().NotBeNull();
        j2!.status.Should().Be("closed");
        j2.minutes.Should().NotBeNull();
        j2.minutes!.Value.Should().BeGreaterThanOrEqualTo(0);
        j2.endedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Punch_UnknownReader_Returns400()
    {
        var body = new { TagUid = "04AABBCCDD22", ReaderCode = "NOPE" };
        var r = await _client.PostAsJsonAsync("/punch", body);
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
