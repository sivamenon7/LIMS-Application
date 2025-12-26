using LIMS.Application.Common.Models;
using LIMS.Core.Entities;
using LIMS.Core.Interfaces;
using MediatR;

namespace LIMS.Application.Samples.Commands;

public record CreateSampleCommand : IRequest<Result<Guid>>
{
    public string SampleNumber { get; init; } = string.Empty;
    public string BatchNumber { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public SampleType Type { get; init; }
    public DateTime ReceivedDate { get; init; }
    public DateTime? DueDate { get; init; }
    public Guid CustomerId { get; init; }
    public Guid? ProjectId { get; init; }
    public string? StorageLocation { get; init; }
    public decimal? Quantity { get; init; }
    public string? UnitOfMeasure { get; init; }
    public int Priority { get; init; }
    public string? Comments { get; init; }
}

public class CreateSampleCommandHandler : IRequestHandler<CreateSampleCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateSampleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateSampleCommand request, CancellationToken cancellationToken)
    {
        // Validate customer exists
        if (!await _unitOfWork.Customers.ExistsAsync(request.CustomerId, cancellationToken))
        {
            return Result<Guid>.Failure("Customer not found");
        }

        var sample = new Sample
        {
            SampleNumber = request.SampleNumber,
            BatchNumber = request.BatchNumber,
            Description = request.Description,
            Type = request.Type,
            Status = SampleStatus.Registered,
            ReceivedDate = request.ReceivedDate,
            DueDate = request.DueDate,
            CustomerId = request.CustomerId,
            ProjectId = request.ProjectId,
            StorageLocation = request.StorageLocation ?? string.Empty,
            Quantity = request.Quantity,
            UnitOfMeasure = request.UnitOfMeasure,
            Priority = request.Priority,
            Comments = request.Comments,
            CreatedBy = "SYSTEM" // TODO: Get from current user context
        };

        var created = await _unitOfWork.Samples.AddAsync(sample, cancellationToken);
        return Result<Guid>.Success(created.Id);
    }
}
