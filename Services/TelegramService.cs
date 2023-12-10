using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

class TelegramService
{
    IServiceScopeFactory _scopeFactory;
    IConfiguration _cfg;
    ILogger<TelegramService> _log;
    TelegramBotClient _bot;
    CancellationTokenSource _cts;
    AppLogicService _appLogic;

    public TelegramService(IServiceScopeFactory scopeFactory, IConfiguration cfg, ILogger<TelegramService> log, AppLogicService appLogic)
    {
        _scopeFactory = scopeFactory;
        _cfg = cfg;
        _log = log;
        _bot = new(_cfg.GetValue<string>("BotSecret"));
        _cts = new();
        _appLogic = appLogic;
    }

    public void Start()
    {
        _log.LogInformation("TelegramService started");
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _bot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: _cts.Token
        );
    }


    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbService = scope.ServiceProvider.GetRequiredService<DbService>();


            if (update.Type == UpdateType.CallbackQuery)
            {
                var callbackQuery = update.CallbackQuery;
                var chatId = callbackQuery.Message.Chat.Id;
                var messageId = callbackQuery.Message.MessageId;
                var callbackData = callbackQuery.Data;

                if (callbackData.StartsWith("/"))
                {
                    var command = callbackData.Split(" ")[0];
                    var args = callbackData.Split(" ")[1..];
                    await _appLogic.HandleCommand(botClient, dbService, callbackQuery.Message, command, args);
                }
                try
                {
                    await botClient.EditMessageReplyMarkupAsync(chatId, messageId, replyMarkup: null);
                }
                catch (ApiRequestException ex) when (ex.ErrorCode == 400 && ex.Message.Contains("message is not modified"))
                {
                    _log.LogInformation("Message is not modified");
                }
            }
            else if (update.Message != null)
            {
                var message = update.Message;
                if (message.Text is { } messageText)
                {
                    messageText = messageText.ToLower();


                    var chatId = message.Chat.Id;

                    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

                    if (messageText.StartsWith("/"))
                    {
                        var command = messageText.Split(" ")[0];
                        var args = messageText.Split(" ")[1..];
                        await _appLogic.HandleCommand(botClient, dbService, message, command, args);
                    }
                    else if (messageText.StartsWith("e"))
                    {
                        string component_name = messageText.Split(" ")[0];
                        await _appLogic.SendOneComponentMessage(botClient, dbService, message, component_name);
                    }
                    else if (messageText.StartsWith("е"))
                    {
                        string component_name = messageText.Split(" ")[0].Substring(1);
                        component_name = "e" + component_name;
                        await _appLogic.SendOneComponentMessage(botClient, dbService, message, component_name);
                    }
                }
            }



        }
        catch (Telegram.Bot.Exceptions.ApiRequestException ex)
        {
            Console.WriteLine($"Bot was blocked by the user. Exception message: {ex.Message}");
        }
    }


    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

}