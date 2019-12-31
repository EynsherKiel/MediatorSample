using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorSample
{
    public class GetIntFromDBHandler : IRequestHandler<GetIntFromDBRequest, int>
    {
        public GetIntFromDBHandler()
        {
        }

        public Task<int> Handle(GetIntFromDBRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(412411);
        }
    }

    public class GetIntFromDBRequest : IRequest<int>
    {
        public string Name { get; set; }
    }

    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var failures = _validators
                .Select(v => v.Validate(request))
                .SelectMany(result => result.Errors)
                .Where(error => error != null)
                .ToList();

            if (failures.Any())
            {
                throw new ValidationException($"Command Validation Errors for type {typeof(TRequest).Name}", failures);
            }

            return await next();
        }
    }

    public class GetPortfoliosYieldRequestValidator : AbstractValidator<GetIntFromDBRequest>
    {
        public GetPortfoliosYieldRequestValidator()
        {
            RuleFor(request => request.Name).NotEmpty();
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            var assembly = typeof(Program).GetTypeInfo().Assembly;

            services.AddMediatR(assembly);

            //// порядок важен
            //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>)); // логирование
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)); // валидация

            services.AddValidatorsFromAssembly(assembly);

            var provider = services.BuildServiceProvider();

            var mediator = provider.GetRequiredService<IMediator>();

            var @int = await mediator.Send(new GetIntFromDBRequest { Name = "ds"  });

            Console.WriteLine(@int);

            var a = 5;

            var x = new IntPtr(a);

            System.Runtime.InteropServices.Marshal.SizeOf(a);
        }
    }
}
