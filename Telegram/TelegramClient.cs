
namespace Telegram;

public static class TelegramNotifier
{
    
    private static readonly HttpClient _client = new HttpClient();
    private static readonly string _botToken = "8959404615:AAFh9K7HqP86TK06HBwgWwdudetyye6iXR0";
    private static readonly string _chatId = "6432887024";

    public static async Task SendMessageAsync(string message)
    {
        
        try
        {
            
            string url = $"https://api.telegram.org/bot{_botToken}/sendMessage";

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("chat_id", _chatId),
                new KeyValuePair<string, string>("text", message)
            });

            var response = _client.PostAsync(url, content);

        }
        catch(Exception ex)
        {
            Console.WriteLine($"[Telegram] Send failure: {ex.Message}");
        }

    }

}