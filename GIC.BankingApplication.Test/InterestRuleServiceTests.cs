using FluentAssertions;
using FluentValidation;
using GIC.BankingApplication.Application.Services;
using GIC.BankingApplication.Infrastructure.Dtos;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace GIC.BankingApplication.Test;

public class InterestRuleServiceTests : IClassFixture<TestFixture>
{
    private readonly ServiceProvider _serviceProvider;

    public InterestRuleServiceTests(TestFixture fixture)
    {
        _serviceProvider = fixture.ServiceProvider;
    }

    [Fact]
    public async Task AddAndRetrieveInterestRule_ShouldSucceed()
    {
        // Arrange
        var interestRuleService = _serviceProvider.GetRequiredService<IInterestRuleService>();

        var ruleDateUtc = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        var newRuleRequest = new CreateInterestRuleRequestDto
        {
            Date = ruleDateUtc,
            RuleId = "RULE01",
            Rate = 2.5m
        };

        // Act
        await interestRuleService.DefineInterestRule(newRuleRequest);

        var allRules = await interestRuleService.GetAllInterestRules();

        // Assert
        allRules.Should().NotBeNull("the service should return a list of rules");
        allRules.Should().HaveCount(1, "we added one rule in this test");

        var rule = allRules.First();
        rule.Date.Should().Be(ruleDateUtc, "the effective date should match the one we added");
        rule.RuleId.Should().Be("RULE01", "the rule ID should match the one we added");
        rule.Rate.Should().Be(2.5m, "the interest rate should match the one we added");
    }

    [Theory]
    [InlineData(0.01, true)]     // Minimum valid rate
    [InlineData(99.99, true)]    // Maximum valid rate
    [InlineData(0, false)]       // Zero rate
    [InlineData(-1, false)]      // Negative rate
    [InlineData(150, false)]     // Exceeding maximum rate
    public async Task AddInterestRule_WithBoundaryRates_ShouldValidateCorrectly(decimal rate, bool isValid)
    {
        // Arrange
        var interestRuleService = _serviceProvider.GetRequiredService<IInterestRuleService>();

        var ruleDateUtc = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        var newRuleRequest = new CreateInterestRuleRequestDto
        {
            Date = ruleDateUtc,
            RuleId = "RULE_BOUNDARY",
            Rate = rate
        };

        // Act
        Func<Task> act = async () =>
            interestRuleService.DefineInterestRule(newRuleRequest);

        // Assert
        if (isValid)
        {
            await act.Should().NotThrowAsync();

            var allRules = await interestRuleService.GetAllInterestRules();
            var rule = allRules.FirstOrDefault(r => r.Date == ruleDateUtc);
            rule.Should().NotBeNull("a valid rate should allow the rule to be added");
            rule!.Rate.Should().Be(rate, "the stored rate should match the input rate");
        }
        else
        {
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Rate is invalid*"); 

            var allRules = await interestRuleService.GetAllInterestRules();
            allRules.Should().NotContain(r => r.Rate == rate,
                "an invalid rate should not allow the rule to be added");
        }
    }
}
