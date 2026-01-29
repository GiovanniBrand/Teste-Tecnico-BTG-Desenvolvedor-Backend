using KrtBank.Domain.Entities;
using KrtBank.Domain.Exceptions;
using KrtBank.Domain.Interfaces;
using KrtBank.Domain.Repositories;
using MediatR;

namespace KrtBank.Application.Commands.Accounts
{
    public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, bool>
    {
        private readonly IRepository<Account> _repository;
        private readonly IRedisCacheService _cache;

        public DeleteAccountCommandHandler(IRepository<Account> repository, IRedisCacheService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken ct)
        {
            // Não usei cache aqui por segurança
            var account = await _repository.GetFirstAsync(a => a.Cpf == request.Cpf, ct);
            if (account == null)
                throw new NotFoundException("Conta não encontrada.");

            _repository.Delete(account);
            var rowsAffected = await _repository.SaveChangesAsync(ct);

            if (rowsAffected > 0)
            {
                await _cache.RemoveAsync($"account:{request.Cpf}");
            }

            return rowsAffected > 0;
        }
    }
}
