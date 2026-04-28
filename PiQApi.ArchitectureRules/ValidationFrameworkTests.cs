// PiQApi.ArchitectureRules/ValidationFrameworkTests.cs
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PiQApi.ArchitectureRules;

public class ValidationFrameworkTests : BaseArchitectureTest
{
    [Fact]
    public void ValidationContext_Should_Have_Required_Properties()
    {
        var rule = Classes()
            .That().HaveFullName("PiQApi.Abstractions.Validation.CertValidationContext")
            .Should().HavePropertyMemberWithName("ValidationModeType")
            .AndShould().HavePropertyMemberWithName("CorrelationId")
            .AndShould().HavePropertyMemberWithName("Properties")
            .Because("CertValidationContext must have all required properties");

        AssertArchRule(rule);
    }

    [Fact]
    public void ValidationRule_Interface_Should_Have_ValidateAsync_Method()
    {
        var rule = Interfaces()
            .That().HaveFullNameContaining("PiQApi.Abstractions.Validation.IValidationRule`1")
            .Should().HaveMethodMemberWithName("ValidateAsync")
            .Because("IValidationRule<T> must have a ValidateAsync method");

        AssertArchRule(rule);
    }

    [Fact]
    public void ValidateAsync_Should_Accept_CancellationToken()
    {
        // Custom rule to check if ValidateAsync method accepts CancellationToken
        var rule = MethodMembers()
            .That().AreDeclaredIn(Interfaces().That().HaveFullNameContaining("PiQApi.Abstractions.Validation.IValidationRule`1"))
            .And().HaveNameMatching("ValidateAsync")
            .Should().FollowCustomCondition(m => m.Parameters.Any(p => p.Type.Name.Contains("CancellationToken")),
                "accept CancellationToken parameter",
                "doesn't accept CancellationToken parameter")
            .Because("Async validation methods should support cancellation");

        AssertArchRule(rule);
    }

    [Fact]
    public void Validation_Types_Should_Not_Conflict_With_System_ComponentModel()
    {
        // Ensure our validation types don't collide with System.ComponentModel.DataAnnotations
        var rule = Types()
            .That().ResideInNamespace("PiQApi.Abstractions.Validation")
            .Should().NotHaveAnyNameOf("ValidationResult", "ValidationContext")
            .Because("Validation types should not conflict with System.ComponentModel.DataAnnotations types");

        AssertArchRule(rule);

        var rule2 = Types()
            .That().HaveAnyNameOf("CertValidationResult", "CertValidationContext")
            .And().ResideInNamespace("PiQApi.Abstractions.Validation")
            .Should().Exist()
            .Because("Cert-prefixed validation types should be used to avoid naming collisions");

        AssertArchRule(rule2);
    }
}