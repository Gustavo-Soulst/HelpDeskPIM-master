using OpenAI;
using OpenAI.Chat;

public class IAService
{
    private readonly ChatClient _client;

    public IAService(IConfiguration config)
    {
        _client = new ChatClient(
            apiKey: config["OpenAI:ApiKey"],
            model: "gpt-4o-mini"
        );
    }

    public async Task<string> PerguntarAsync(string pergunta)
    {
        // ENVIO CORRETO: MENSAGENS ESTRUTURADAS
        var resposta = await _client.CompleteChatAsync(
            new ChatMessage[]
            {
                new SystemChatMessage(
                    "Você é um técnico de Help Desk. " +
                    "Sempre responda com uma solução técnica clara, objetiva e detalhada."
                ),
                new UserChatMessage(pergunta)
            }
        );

        var completion = resposta.Value;

        // Extrai todas as partes de texto
        var partes = completion.Content
            .Where(p => p.Kind == ChatMessageContentPartKind.Text)
            .Select(p => p.Text);

        string textoFinal = string.Join("", partes);

        return string.IsNullOrWhiteSpace(textoFinal)
            ? "Não foi possível gerar uma solução."
            : textoFinal;
    }
}
