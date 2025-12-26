using LIMS.Application.Common.Models;
using LIMS.Core.Entities;
using LIMS.Core.Interfaces;
using MediatR;

namespace LIMS.Application.Samples.Queries;

public record GetSampleByIdQuery(Guid Id) : IRequest<Result<Sample>>;

public class GetSampleByIdQueryHandler : IRequestHandler<GetSampleByIdQuery, Result<Sample>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSampleByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Sample>> Handle(GetSampleByIdQuery request, CancellationToken cancellationToken)
    {
        var sample = await _unitOfWork.Samples.GetByIdAsync(request.Id, cancellationToken);

        return sample != null
            ? Result<Sample>.Success(sample)
            : Result<Sample>.Failure("Sample not found");
    }
}

public record GetAllSamplesQuery : IRequest<Result<IEnumerable<Sample>>>;

public class GetAllSamplesQueryHandler : IRequestHandler<GetAllSamplesQuery, Result<IEnumerable<Sample>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllSamplesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<Sample>>> Handle(GetAllSamplesQuery request, CancellationToken cancellationToken)
    {
        var samples = await _unitOfWork.Samples.GetAllAsync(cancellationToken);
        return Result<IEnumerable<Sample>>.Success(samples);
    }
}
