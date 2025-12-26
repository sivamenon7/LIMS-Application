using FluentValidation;
using MasterDataModule.DAL.Repositories;
using MasterDataModule.Models.Organism;

namespace MasterDataModule.Validators;

public sealed class CreateOrganismPayloadValidator : AbstractValidator<CreateOrganismPayload>
{
    private readonly IOrganismRepository _repository;

    public CreateOrganismPayloadValidator(IOrganismRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.TypeId)
            .NotNull()
            .WithMessage("Type is required");

        RuleFor(x => x.Genus)
            .NotEmpty()
            .WithMessage("Genus is required")
            .MaximumLength(255)
            .WithMessage("Genus must not exceed 255 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(255)
            .WithMessage("Description must not exceed 255 characters");

        RuleFor(x => x)
            .MustAsync(IsCombinationUnique)
            .WithMessage("An organism with the same Type, Genus and Characterization already exists.");
    }

    private async Task<bool> IsCombinationUnique(CreateOrganismPayload payload, CancellationToken cancellationToken)
    {
        if (!payload.TypeId.HasValue || string.IsNullOrEmpty(payload.Genus) || !payload.CharacterizationId.HasValue)
            return true; // Let other validators handle null checks

        return !await _repository.ExistsByCombinationAsync(
            payload.TypeId.Value,
            payload.Genus,
            payload.CharacterizationId.Value,
            cancellationToken);
    }
}

public sealed class UpdateOrganismPayloadValidator : AbstractValidator<UpdateOrganismPayload>
{
    private readonly IOrganismRepository _repository;

    public UpdateOrganismPayloadValidator(IOrganismRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");

        RuleFor(x => x.TypeId)
            .NotNull()
            .WithMessage("Type is required");

        RuleFor(x => x.Genus)
            .NotEmpty()
            .WithMessage("Genus is required")
            .MaximumLength(255)
            .WithMessage("Genus must not exceed 255 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(255)
            .WithMessage("Description must not exceed 255 characters");

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage("RowVersion is required for optimistic concurrency");
    }
}
