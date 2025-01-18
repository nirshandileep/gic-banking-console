namespace GIC.BankingApplication.Domain.Models;

public class Account
{
    public int Id { get; private set; }
    public string AccountNumber { get; private set; }
    public decimal Balance { get; private set; }
    public List<Transaction> Transactions { get; private set; }

    public static Account CreateNewAccount(string accountNumber, decimal balance)
    {
        return new Account
        {
            AccountNumber = accountNumber,
            Balance = balance
        };
    }

    public Account SetBalance(decimal balance) 
    {
        Balance = balance;
        return this;
    }
}
