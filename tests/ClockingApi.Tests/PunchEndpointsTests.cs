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

    [Fact]
    public async Task Punch_Open_Then_Close_Succeeds()
    {
        var request = new { TagUid = "04AABBCCDD22", ReaderCode = "LAB-001" };

        // first punch = open session -> 201 Created
        var r1 = await _client.PostAsJsonAsync("/punch", request);
        r1.StatusCode.Should().Be(HttpStatusCode.Created);

        var open = await r1.Content.ReadFromJsonAsync<OpenResponse>();
        open.Should().NotBeNull();
        open!.id.Should().BeGreaterOrEqualTo(1);
        open.at.Should().BeOnOrAfter(DateTime.UtcNow.AddMinutes(-1));

        // second punch = close session -> 200 OK
        await Task.Delay(1000);
        var r2 = await _client.PostAsJsonAsync("/punch", request);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);

        var close = await r2.Content.ReadFromJsonAsync<CloseResponse>();
        close.Should().NotBeNull();
        close!.id.Should().Be(open.id);
        close.ended.Should().BeOnOrAfter(close.started);
    }

    [Fact]
    public async Task Punch_UnknownReader_Returns404()
    {
        var request = new { TagUid = "04AABBCCDD22", ReaderCode = "NOPE-999" };
        var r = await _client.PostAsJsonAsync("/punch", request);
        r.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private record OpenResponse(string message, int id, string worker, string reader, DateTime at);
    private record CloseResponse(string message, int id, string worker, string reader, DateTime started, DateTime ended);
}
