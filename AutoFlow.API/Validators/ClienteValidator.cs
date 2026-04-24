using FluentValidation;
using AutoFlow.API.DTO.Clientes;

namespace AutoFlow.API.Validators
{
    public class ClienteValidator : AbstractValidator<ClienteInputDTO>
    {
        public ClienteValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório")
                .MaximumLength(150).WithMessage("Nome pode ter no máximo 150 caracteres");

            RuleFor(x => x.Telefone)
                .NotEmpty().WithMessage("Telefone é obrigatório")
                .MaximumLength(20).WithMessage("Telefone pode ter no máximo 20 caracteres");

            // Email: validação apenas se informado
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("E-mail inválido")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            // Documento (CPF = 11 dígitos, CNPJ = 14 dígitos): validação básica de tamanho
            RuleFor(x => x.Documento)
                .MinimumLength(11).WithMessage("Documento deve ter no mínimo 11 caracteres (CPF)")
                .MaximumLength(18).WithMessage("Documento deve ter no máximo 18 caracteres (CNPJ formatado)")
                .When(x => !string.IsNullOrWhiteSpace(x.Documento));
        }
    }
}
