using FluentValidation;
using FluentValidation.Results;
using Products.Application.Behaviors;

namespace Products.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    private sealed record TestRequest(string? Value);

    private sealed class EmptyValueValidator : AbstractValidator<TestRequest>
    {
        public EmptyValueValidator()
        {
            RuleFor(request => request.Value).NotEmpty();
        }
    }

    private sealed class AlwaysValidValidator : AbstractValidator<TestRequest>
    {
        public AlwaysValidValidator()
        {
            RuleFor(request => request.Value).NotNull();
        }
    }

    [Fact]
    public async Task Handle_WithFailingValidator_ThrowsValidationExceptionAndSkipsHandler()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new EmptyValueValidator()]);
        var handlerWasCalled = false;

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(
                new TestRequest("   "),
                _ =>
                {
                    handlerWasCalled = true;
                    return Task.FromResult("result");
                },
                CancellationToken.None));

        Assert.False(handlerWasCalled);
        Assert.NotEmpty(exception.Errors);
        Assert.Contains(exception.Errors, error => error.PropertyName == nameof(TestRequest.Value));
    }

    [Fact]
    public async Task Handle_WithPassingValidator_InvokesHandler()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new EmptyValueValidator()]);
        var handlerWasCalled = false;

        var result = await behavior.Handle(
            new TestRequest("valid"),
            _ =>
            {
                handlerWasCalled = true;
                return Task.FromResult("result");
            },
            CancellationToken.None);

        Assert.True(handlerWasCalled);
        Assert.Equal("result", result);
    }

    [Fact]
    public async Task Handle_WithoutRegisteredValidators_InvokesHandler()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);
        var handlerWasCalled = false;

        var result = await behavior.Handle(
            new TestRequest(null),
            _ =>
            {
                handlerWasCalled = true;
                return Task.FromResult("result");
            },
            CancellationToken.None);

        Assert.True(handlerWasCalled);
        Assert.Equal("result", result);
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_AggregatesAllFailures()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(
            [new EmptyValueValidator(), new AlwaysValidValidator()]);
        var handlerWasCalled = false;

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(
                new TestRequest(null),
                _ =>
                {
                    handlerWasCalled = true;
                    return Task.FromResult("result");
                },
                CancellationToken.None));

        Assert.False(handlerWasCalled);
        Assert.True(exception.Errors.Count() >= 1);
    }
}
