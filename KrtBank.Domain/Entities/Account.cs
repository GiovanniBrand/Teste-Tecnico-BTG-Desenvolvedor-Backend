using KrtBank.Domain.Common;
using KrtBank.Domain.Enums;

namespace KrtBank.Domain.Entities
{
    public class Account : BaseEntity
    {
        public string AccountHolderName { get; private set; }
        public string Cpf { get; private set; }
        public AccountStatus AccountStatus { get; private set; }

        // Construtor vazio para o Entity Framework
        protected Account() { }

        public Account(string accountHolderName, string cpf)
        {
            if (string.IsNullOrWhiteSpace(accountHolderName)) throw new ArgumentException("Nome do titular é obrigatório.");
            if (string.IsNullOrWhiteSpace(cpf)) throw new ArgumentException("CPF é obrigatório.");

            AccountHolderName = accountHolderName;
            Cpf = cpf;
            AccountStatus = AccountStatus.Active;
        }

        public void UpdateStatus(AccountStatus newStatus)
        {
            AccountStatus = newStatus;
        }
    }
}
