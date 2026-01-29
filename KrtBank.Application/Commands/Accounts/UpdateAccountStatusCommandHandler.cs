using KrtBank.Domain.Entities;
using KrtBank.Domain.Exceptions;
using KrtBank.Domain.Interfaces;
using KrtBank.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrtBank.Application.Commands.Accounts
{
    public class UpdateAccountStatusCommandHandler : IRequestHandler<UpdateAccountStatusCommand, bool>
    {
        private readonly IRepository<Account> _repository;
        private readonly IRedisCacheService _cache;

        public UpdateAccountStatusCommandHandler(IRepository<Account> repository, IRedisCacheService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<bool> Handle(UpdateAccountStatusCommand request, CancellationToken ct)
        {
            // não usamos o cache aqui para garantir o dado mais atual
            var account = await _repository.GetFirstAsync(a => a.Cpf == request.Cpf, ct);

            if (account == null)  
                throw new NotFoundException("Conta não encontrada.");

            account.UpdateStatus(request.Status);

            _repository.Update(account);

            var rowsAffected = await _repository.SaveChangesAsync(ct);

            if (rowsAffected > 0)
            {
                await _cache.RemoveAsync($"account:{request.Cpf}");
            }

            return rowsAffected > 0;
        }
    }
}
