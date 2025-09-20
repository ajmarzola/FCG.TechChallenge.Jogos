using FluentValidation;

namespace FCG.TechChallenge.Jogos.Application.Commands.Jogos.UpdateJogo
{
    public sealed class UpdateJogoValidator : AbstractValidator<UpdateJogoCommand>
    {
        public UpdateJogoValidator()
        {
            RuleFor(x => x.JogoId).NotEmpty();
            RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Descricao).NotEmpty().MaximumLength(2000);
            RuleFor(x => x.Preco).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Categoria).NotEmpty().MaximumLength(100);
        }
    }
}
