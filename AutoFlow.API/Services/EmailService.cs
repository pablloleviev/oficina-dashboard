using Resend;

namespace AutoFlow.API.Services
{
    public class EmailService
    {
        private readonly IResend _resend;

        public EmailService(IResend resend)
        {
            _resend = resend;
        }

        public async Task EnviarBoasVindas(string emailDestino, string nomeOficina, string senhaTemporaria)
        {
            var message = new EmailMessage();
            message.From = "AutoFlow <onboarding@resend.dev>";
            message.To.Add(emailDestino);
            message.Subject = $"Bem-vindo ao AutoFlow — {nomeOficina}!";
            message.HtmlBody = $@"
                <div style='font-family: sans-serif; max-width: 500px; margin: auto;'>
                    <h2 style='color: #3b82f6;'>🚗 Bem-vindo ao AutoFlow!</h2>
                    <p>Olá! Sua oficina <strong>{nomeOficina}</strong> foi cadastrada com sucesso.</p>
                    <p>Seu período de <strong>trial gratuito de 14 dias</strong> já começou!</p>
                    <hr/>
                    <h3>Seus dados de acesso:</h3>
                    <p><strong>Email:</strong> {emailDestino}</p>
                    <p><strong>Senha temporária:</strong> <code style='background:#f1f5f9;padding:4px 8px;border-radius:4px;'>{senhaTemporaria}</code></p>
                    <a href='https://autoflow-gestao.vercel.app' style='display:inline-block;margin-top:16px;padding:12px 24px;background:#3b82f6;color:white;border-radius:8px;text-decoration:none;'>
                        Acessar o AutoFlow →
                    </a>
                    <p style='margin-top:24px;color:#94a3b8;font-size:12px;'>Por segurança, troque sua senha após o primeiro login.</p>
                </div>";

            await _resend.EmailSendAsync(message);
        }
    }
}
