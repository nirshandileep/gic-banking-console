using FluentValidation;
using GIC.BankingApplication.Application.Extensions;
using GIC.BankingApplication.Application.Services;
using GIC.BankingApplication.Infrastructure.Dtos;

namespace GIC.BankingApplication.ConsoleApp.Presentation;

public class MenuHandler(ITransactionService transactionService, IInterestRuleService interestRuleService,
    IStatementService statementService)
{
    private readonly ITransactionService _transactionService = transactionService;
    private readonly IInterestRuleService _interestRuleService = interestRuleService;
    private readonly IStatementService _statementService = statementService;

    public async Task ShowMainMenu()
    {
        bool exitRequested = false;

        while (!exitRequested)
        {
            PrintMainMenuPrompt();
            char key = Console.ReadKey(true).KeyChar;
            char input = char.ToUpperInvariant(key);

            try
            {
                switch (input)
                {
                    case 'T':
                        await HandleInputTransactions();
                        break;
                    case 'I':
                        await HandleDefineInterestRule();
                        break;
                    case 'P':
                        await HandlePrintStatement();
                        break;
                    case 'Q':
                        exitRequested = true;
                        HandleExit();
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }

    private void PrintMainMenuPrompt()
    {
        Console.WriteLine();
        Console.WriteLine("Welcome to AwesomeGIC Bank! What would you like to do?");
        Console.WriteLine("[T] Input transactions");
        Console.WriteLine("[I] Define interest rules");
        Console.WriteLine("[P] Print statement");
        Console.WriteLine("[Q] Quit");
        Console.Write("> ");
    }

    private async Task HandleInputTransactions()
    {
        Console.Clear();

        while (true)
        {
            Console.WriteLine("Please enter transaction details in <Date> <Account> <Type> <Amount> format");
            Console.WriteLine("(or enter blank to go back to main menu):");

            Console.Write("> ");
            var line = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4)
            {
                Console.WriteLine("Invalid input format. Try again or leave blank to go back.");
                Console.ReadKey();
                continue;
            }

            try
            {
                var dateInput = parts[0].ParseAsUtcDate();
                var accountInput = parts[1].ToUpper();
                var typeInput = parts[2].ToTransactionType();
                var amountInput = decimal.Parse(parts[3]);

                var createTransactionRequest = new CreateTransactionRequestDto
                {
                    Date = dateInput,
                    AccountNumber = accountInput,
                    Type = typeInput,
                    Amount = amountInput,
                };

                await _transactionService.InputTransaction(createTransactionRequest);
                var statement = await _statementService.GetAccountStatement(accountInput);
                PrintStatement(statement);
            }
            catch (ValidationException ex)
            {
                HandleValidationException(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing transaction: {ex.Message}");
            }
        }
    }

    private async Task HandleDefineInterestRule()
    {
        Console.Clear();

        while (true)
        {
            Console.WriteLine("Please enter interest rules details in <Date> <RuleId> <Rate in %> format");
            Console.WriteLine("(or enter blank to go back to main menu):");
            Console.Write("> ");
            var line = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                Console.WriteLine("Invalid input format. Try again or leave blank to go back.");
                continue;
            }

            var dateInput = parts[0].ParseAsUtcDate();
            var ruleId = parts[1].ToUpper();
            var rateInput = decimal.Parse(parts[2]);

            var interestRule = new CreateInterestRuleRequestDto
            {
                Date = dateInput,
                RuleId = ruleId,
                Rate = rateInput,
            };

            try
            {
                await _interestRuleService.DefineInterestRule(interestRule);

                var rules = await _interestRuleService.GetAllInterestRules();
                PrintInterestRules(rules);
            }
            catch (ValidationException ex)
            {
                HandleValidationException(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error defining interest rule: {ex.Message}");
            }
        }
    }

    private void HandleValidationException(ValidationException ex)
    {
        if (ex.Errors.Any())
        {
            foreach (var error in ex.Errors)
            {
                Console.WriteLine($"{error}");
            }
        }
        else
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async Task HandlePrintStatement()
    {
        Console.Clear();

        while (true)
        {
            Console.WriteLine("Please enter account and month to generate the statement <Account> <YYYYMM>");
            Console.WriteLine("(or enter blank to go back to main menu):");
            Console.Write("> ");
            var line = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                Console.WriteLine("Invalid input. Please try again or leave blank to go back.");
                continue;
            }

            var accountInput = parts[0].ToUpper();
            var yearMonthInput = parts[1];

            try
            {
                var statement = await _statementService.GetMonthlyStatement(accountInput, yearMonthInput);
                PrintMonthlyStatement(statement);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error printing statement: {ex.Message}");
            }
        }
    }

    private void HandleExit()
    {
        Console.Clear();
        Console.WriteLine("Thank you for banking with AwesomeGIC Bank.");
        Console.WriteLine("Have a nice day!");
        Console.ReadKey();
    }

    private void PrintStatement(StatementDto statement)
    {
        if (statement == null)
        {
            Console.WriteLine("No statement available.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Account: {statement.AccountNumber}");
        Console.WriteLine("| Date     | Txn Id       | Type | Amount  |");
        foreach (var txn in statement.Transactions)
        {
            Console.WriteLine($"| {txn.Date.ToYYYYMMddFormat()} | {txn.TxnId,-12} | {txn.Type.ToDisplayChar(),4} | {txn.Amount,7:F2} |");
        }
        Console.WriteLine();
    }

    private void PrintMonthlyStatement(StatementDto statement)
    {
        if (statement == null)
        {
            Console.WriteLine("No statement available.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Account: {statement.AccountNumber}");
        Console.WriteLine("| Date     | Txn Id       | Type | Amount  | Balance  |");
        foreach (var txn in statement.Transactions)
        {
            Console.WriteLine($"| {txn.Date.ToYYYYMMddFormat()} | {txn.TxnId,-12} | {txn.Type.ToDisplayChar(),4} | {txn.Amount,7:F2} | {txn.Balance,8:F2} |");
        }
        Console.WriteLine();
    }

    private void PrintInterestRules(IEnumerable<InterestRuleDto> rules)
    {
        Console.WriteLine("Interest rules:");
        Console.WriteLine("| Date     | RuleId | Rate (%) |");
        foreach (var rule in rules)
        {
            Console.WriteLine($"| {rule.Date.ToYYYYMMddFormat()} | {rule.RuleId,-6} | {rule.Rate,8:F2} |");
        }
        Console.WriteLine();
    }
}
