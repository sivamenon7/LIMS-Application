using LIMS.EventBus.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LIMS.EventBus.Consumers;

public class SampleCreatedConsumer : IConsumer<SampleCreatedEvent>
{
    private readonly ILogger<SampleCreatedConsumer> _logger;

    public SampleCreatedConsumer(ILogger<SampleCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SampleCreatedEvent> context)
    {
        _logger.LogInformation(
            "Sample created event received: {SampleNumber} (ID: {SampleId})",
            context.Message.SampleNumber,
            context.Message.SampleId);

        // Process the event - send notifications, update audit logs, etc.
        await Task.CompletedTask;
    }
}

public class TestResultApprovedConsumer : IConsumer<TestResultApprovedEvent>
{
    private readonly ILogger<TestResultApprovedConsumer> _logger;

    public TestResultApprovedConsumer(ILogger<TestResultApprovedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TestResultApprovedEvent> context)
    {
        _logger.LogInformation(
            "Test result approved for sample: {SampleNumber} (ID: {SampleId})",
            context.Message.SampleNumber,
            context.Message.SampleId);

        // Process the event - generate certificates, send emails, etc.
        await Task.CompletedTask;
    }
}
