using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Behaviors;
using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;
using Xunit;

namespace ModularMonolith.Tests
{
    public class MediatorTests
    {
        public record PingCommand(string Message) : IRequest<Result<string>>;

        public class PingCommandHandler : IRequestHandler<PingCommand, Result<string>>
        {
            public Task<Result<string>> Handle(PingCommand request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Result<string>.Success($"Pong: {request.Message}"));
            }
        }

        public class PingCommandValidator : AbstractValidator<PingCommand>
        {
            public PingCommandValidator()
            {
                RuleFor(x => x.Message).NotEmpty().WithMessage("Message cannot be empty.");
            }
        }

        [Fact]
        public async Task Sender_ShouldExecuteHandler_WhenNoBehaviors()
        {
            var services = new ServiceCollection();
            services.AddScoped<ISender, Sender>();
            services.AddScoped<IRequestHandler<PingCommand, Result<string>>, PingCommandHandler>();

            var sp = services.BuildServiceProvider();
            var sender = sp.GetRequiredService<ISender>();

            var response = await sender.Send(new PingCommand("Hello"));

            response.IsSuccess.Should().BeTrue();
            response.Data.Should().Be("Pong: Hello");
        }

        [Fact]
        public async Task Sender_ShouldExecuteValidationBehavior_AndFailWhenInputIsInvalid()
        {
            var services = new ServiceCollection();
            services.AddScoped<ISender, Sender>();
            services.AddScoped<IRequestHandler<PingCommand, Result<string>>, PingCommandHandler>();
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddScoped<IValidator<PingCommand>, PingCommandValidator>();

            var sp = services.BuildServiceProvider();
            var sender = sp.GetRequiredService<ISender>();

            var response = await sender.Send(new PingCommand(""));

            response.IsSuccess.Should().BeFalse();
            response.ErrorType.Should().Be(ErrorType.Validation);
            response.Errors.Should().Contain(e => e.Contains("Message: Message cannot be empty."));
        }

        [Fact]
        public async Task Sender_ShouldPassValidationBehavior_WhenInputIsValid()
        {
            var services = new ServiceCollection();
            services.AddScoped<ISender, Sender>();
            services.AddScoped<IRequestHandler<PingCommand, Result<string>>, PingCommandHandler>();
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddScoped<IValidator<PingCommand>, PingCommandValidator>();

            var sp = services.BuildServiceProvider();
            var sender = sp.GetRequiredService<ISender>();

            var response = await sender.Send(new PingCommand("Valid"));

            response.IsSuccess.Should().BeTrue();
            response.Data.Should().Be("Pong: Valid");
        }
    }
}
