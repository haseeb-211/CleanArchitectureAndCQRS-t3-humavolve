using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Products.Application.Behaviors;

namespace Products.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers MediatR handlers, FluentValidation validators and the
    /// validation pipeline behavior.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(applicationAssembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
