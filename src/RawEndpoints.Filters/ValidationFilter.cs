using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace RawEndpoints.Filters
{
    public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            if (validator is not null
                && context.Arguments.FirstOrDefault(x => x?.GetType() == typeof(T)) is T model
                && await validator.ValidateAsync(model) is var validation
                && !validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            return await next(context);
        }
    }
}
