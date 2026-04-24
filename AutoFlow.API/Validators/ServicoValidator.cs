using FluentValidation;
using AutoFlow.API.DTO;

namespace AutoFlow.API.Validators
{
    public class ServicoValidator : AbstractValidator<ServicoDTO>
    {
        public ServicoValidator()
        {
            RuleFor(x => x.Cliente)
                .NotEmpty().WithMessage("Cliente é obrigatório");

            RuleFor(x => x.NomeServico)
                .NotEmpty().WithMessage("Serviço é obrigatório");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("Valor deve ser maior que zero");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status é obrigatório");
        }
    }
}
