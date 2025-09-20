using FluentValidation;

namespace FCG.TechChallenge.Jogos.Application.Commands.Jogos.UpdateJogoPreco
{
    public sealed class UpdateJogoPrecoValidator : AbstractValidator<UpdateJogoPrecoCommand>
    {
        public UpdateJogoPrecoValidator()
        {
            RuleFor(x => x.JogoId).NotEmpty();
            RuleFor(x => x.NovoPreco).GreaterThanOrEqualTo(0);
        }
    }
}
