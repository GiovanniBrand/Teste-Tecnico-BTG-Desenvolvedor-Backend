using KrtBank.Application.Events;
using KrtBank.Application.Interfaces;
using KrtBank.Domain.Entities;
using KrtBank.Domain.Exceptions;
using KrtBank.Domain.Interfaces;
using KrtBank.Domain.Repositories;
using MediatR;

namespace KrtBank.Application.Commands.Accounts;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, bool>
{
    private readonly IRepository<Account> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IRedisCacheService _cache;

    public DeleteAccountCommandHandler(IRepository<Account> repository, IUnitOfWork unitOfWork, IMediator mediator, IRedisCacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var account = await _repository.GetFirstAsync(a => a.Cpf == request.Cpf, ct);
            if (account == null)
                throw new NotFoundException("Conta não encontrada.");

            _repository.Delete(account);
            await _unitOfWork.SaveChangesAsync(ct);
            await _mediator.Publish(new AccountDeletedEvent(account), ct);

            await _unitOfWork.CommitAsync(ct);

            await _cache.RemoveAsync($"account:{request.Cpf}");

            return true;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }
}