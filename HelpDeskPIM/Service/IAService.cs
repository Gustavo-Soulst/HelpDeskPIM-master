using OpenAI;
using OpenAI.Chat;

public class IAService
{
    private readonly ChatClient _client;

    public IAService(IConfiguration config)
    {
        _client = new ChatClient(
            model: "gpt-4o-mini",
            apiKey: config["OpenAI:ApiKey"]
        );
    }

    public async Task<string> PerguntarAsync(string pergunta)
    {
        var mensagens = new ChatMessage[]
        {
            new SystemChatMessage("Você é um técnico de Help Desk experiente. Sempre forneça respostas claras e objetivas."),
            new UserChatMessage(
                new ChatMessageContent(
                    ChatMessageContentPart.CreateTextPart(pergunta)
                )
            )
        };

        // CHAMANDO A IA
        var result = await _client.CompleteChatAsync(mensagens);

        // PEGA O RESULTADO REAL
        var completion = result.Value;

        // LÊ PARTES DO RETORNO
        var partes = completion.Content;

        // Junta somente texto
        string texto = string.Join("\n",
            partes.Where(p => p.Kind == ChatMessageContentPartKind.Text)
                  .Select(p => p.Text)
        );

        return texto;
    }
}
