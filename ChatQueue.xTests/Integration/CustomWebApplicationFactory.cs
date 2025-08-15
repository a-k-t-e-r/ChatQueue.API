using ChatQueue.Application.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace ChatQueue.xTests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public TestClock Clock { get; private set; } = new TestClock(DateTimeOffset.UtcNow);

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Use a fresh clock for each factory instance
        Clock = new TestClock(DateTimeOffset.UtcNow);

        builder.ConfigureServices(services =>
        {
            // Replace SystemClock with TestClock
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDateTimeProvider));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddSingleton<IDateTimeProvider>(Clock);

            // Use the InMemory repositories (already the default) but ensure singletons are preserved for integration.
            // No changes necessary if application already registers in-memory singletons.
        });

        return base.CreateHost(builder);
    }

    public HttpClient CreateClientWithNoAutoRedirect() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
}