using System.Net;
using System.Net.Mail;

namespace AutoFlow.API.Services
{
    public class EmailService
    {
        private readonly string _gmailUser;
        private readonly string _gmailPass;

        public EmailService()
        {
            _gmailUser = Environment.GetEnvironmentVariable("GMAIL_USER") ?? "";
            _gmailPass = Environment.GetEnvironmentVariable("GMAIL_PASS") ?? "";
        }

        public async Task EnviarBoasVindas(string emailDestino, string nomeOficina, string senhaTemporaria)
        {
            if (string.IsNullOrEmpty(_gmailUser) || string.IsNullOrEmpty(_gmailPass))
            {
                Console.WriteLine("⚠️ EMAIL: GMAIL_USER ou GMAIL_PASS não configurados.");
                return;
            }

            try
            {
                // HTML-encode de todo dado dinâmico antes de interpolar no corpo HTML (A05 — injeção de HTML).
                // Um nome de oficina com <script> ou tags passa a aparecer como texto literal.
                var nomeOficinaSafe = System.Net.WebUtility.HtmlEncode(nomeOficina);
                var emailDestinoSafe = System.Net.WebUtility.HtmlEncode(emailDestino);
                var senhaTemporariaSafe = System.Net.WebUtility.HtmlEncode(senhaTemporaria);

                var smtp = new SmtpClient("smtp.gmail.com", 465)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_gmailUser, _gmailPass),
                    EnableSsl = true
                };

                var msg = new MailMessage
                {
                    From = new MailAddress(_gmailUser, "AutoFlow"),
                    Subject = $"Bem-vindo ao AutoFlow — {nomeOficinaSafe}!",
                    IsBodyHtml = true,
                    Body = $@"
                    <div style='font-family: sans-serif; max-width: 500px; margin: auto;'>
                        <h2 style='color: #3b82f6;'>🚗 Bem-vindo ao AutoFlow!</h2>
                        <p>Olá! Sua oficina <strong>{nomeOficinaSafe}</strong> foi cadastrada com sucesso.</p>
                        <p>Seu período de <strong>trial gratuito de 14 dias</strong> já começou!</p>
                        <hr/>
                        <h3>Seus dados de acesso:</h3>
                        <p><strong>Email:</strong> {emailDestinoSafe}</p>
                        <p><strong>Senha temporária:</strong> <code style='background:#f1f5f9;padding:4px 8px;border-radius:4px;font-size:16px;'>{senhaTemporariaSafe}</code></p>
                        <a href='https://autoflow-gestao.vercel.app' style='display:inline-block;margin-top:16px;padding:12px 24px;background:#3b82f6;color:white;border-radius:8px;text-decoration:none;font-weight:bold;'>
                            Acessar o AutoFlow →
                        </a>
                        <p style='margin-top:24px;color:#94a3b8;font-size:12px;'>Por segurança, troque sua senha após o primeiro login.</p>
                    </div>"
                };

                msg.To.Add(emailDestino);
                await smtp.SendMailAsync(msg);
                Console.WriteLine($"✅ EMAIL: Boas-vindas enviado para {emailDestino}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EMAIL: Erro ao enviar para {emailDestino}: {ex.Message}");
            }
        }
    }
}
