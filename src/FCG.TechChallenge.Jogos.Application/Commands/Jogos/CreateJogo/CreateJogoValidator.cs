using FluentValidation;

namespace FCG.TechChallenge.Jogos.Application.Commands.Jogos.CreateJogo
{
    public sealed class CreateJogoValidator : AbstractValidator<CreateJogoCommand>
    {
        public CreateJogoValidator()
        {
            RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Descricao).NotEmpty().MaximumLength(2000);
            RuleFor(x => x.Preco).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Categoria).NotEmpty().MaximumLength(100);
        }
    }
}