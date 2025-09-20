using FCG.TechChallenge.Jogos.Application.Commands.Jogos.UpdateJogo;
using FluentValidation;

namespace FCG.TechChallenge.Jogos.Application.Commands.Jogos.DeleteJogo
{
    public class DeleteJogoValidator : AbstractValidator<UpdateJogoCommand>
    {
        public DeleteJogoValidator()
        {
            RuleFor(x => x.JogoId).NotEmpty();
        }
    }
}