namespace AutoFlow.API.Exceptions
{
    /// <summary>
    /// Exceção de regra de negócio. A mensagem é intencional e segura para exibir ao usuário
    /// (ex.: "Valor deve ser maior que zero"). Erros técnicos (banco, rede) NÃO devem usar este tipo —
    /// eles sobem para o handler global e retornam mensagem genérica, sem vazar detalhe (A02/A10).
    /// </summary>
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
    }
}
