﻿using GIC.BankingApplication.Domain.Enums;

namespace GIC.BankingApplication.Application.Validators;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Request.AccountId)
            .NotEmpty()
            .WithMessage("Account is required");

        RuleFor(x => x.Request.Date)
            .NotEmpty()
            .WithMessage("Date is required.");

        RuleFor(x => x.Request.TransactionId)
            .NotEmpty()
            .WithMessage("TransactionId is required.");

        RuleFor(x => x.Request.Type)
            .IsInEnum()
            .WithMessage("Type is invalid.");

        RuleFor(x => x.Request.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");
    }
}
