using KrtBank.Application.Events;
using KrtBank.Application.Queries.Accounts;
using KrtBank.Domain.Entities;
using KrtBank.Domain.Exceptions;
using KrtBank.Domain.Interfaces;
using KrtBank.Domain.Repositories;
using MediatR;

namespace KrtBank.Application.Commands.Accounts
{
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountResponse>
    {
        private readonly IRepository<Account> _repository;
        private readonly IRedisCacheService _cache;
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;

        public CreateAccountCommandHandler(IRepository<Account> repository, IRedisCacheService cache, IMediator mediator, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _cache = cache;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
        }

        public async Task<AccountResponse> Handle(CreateAccountCommand request, CancellationToken ct)
        {
            await _unitOfWork.BeginTransactionAsync(ct);

            try
            {
                // Regra de Negócio: CPF Único
                var exists = await _repository.ExistsAsync(a => a.Cpf == request.Cpf, ct);
                if (exists) throw new BusinessException("Já existe uma conta com este CPF.");

                var account = new Account(request.Name, request.Cpf);

                await _repository.AddAsync(account, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                await _mediator.Publish(new AccountCreatedEvent(account), ct);

                await _unitOfWork.CommitAsync(ct);

                var response = new AccountResponse(
                    account.Id,
                    account.AccountHolderName,
                    account.Cpf,
                    account.AccountStatus.ToString()
                );

                await _cache.SetAsync($"account:{account.Cpf}", response, TimeSpan.FromHours(24));

                return response;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                throw;
            }
        }
    }
}
