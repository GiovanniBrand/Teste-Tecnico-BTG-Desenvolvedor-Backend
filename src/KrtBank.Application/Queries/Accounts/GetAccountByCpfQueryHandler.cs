using KrtBank.Domain.Entities;
using KrtBank.Domain.Exceptions;
using KrtBank.Domain.Interfaces;
using KrtBank.Domain.Repositories;
using MediatR;


namespace KrtBank.Application.Queries.Accounts
{
    public class GetAccountByCpfQueryHandler : IRequestHandler<GetAccountByCpfQuery, AccountResponse>
    {
        private readonly IRepository<Account> _repository;
        private readonly IRedisCacheService _cache;

        public GetAccountByCpfQueryHandler(IRepository<Account> repository, IRedisCacheService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<AccountResponse> Handle(GetAccountByCpfQuery request, CancellationToken ct)
        {
            var cacheKey = $"account:{request.Cpf}";
            var cachedResponse = await _cache.GetAsync<AccountResponse>(cacheKey);

            if (cachedResponse != null)
                return cachedResponse;

            var account = await _repository.GetFirstAsync(a => a.Cpf == request.Cpf, ct);

            if (account == null)
                throw new NotFoundException("Conta não encontrada.");

            var response = new AccountResponse(
                account.Id,
                account.AccountHolderName,
                account.Cpf,
                account.AccountStatus.ToString()
            );

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromHours(24));
            return response;
        }
    }
}
