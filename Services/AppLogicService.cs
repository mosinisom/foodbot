using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

public class AppLogicService
{


    DbService _db;
    ILogger<AppLogicService> _log;

    public AppLogicService(DbService db, ILogger<AppLogicService> log)
    {
        _db = db;
        _log = log;
    }

    public async Task SendOneComponentMessage(ITelegramBotClient botClient, DbService dbService, Message message, string component_name)
    {
        try
        {
            InlineKeyboardMarkup inlineKeyboardMarkup = new(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Ещё", "/sndrndcmpt"),
                }
            });

            var component = await dbService.GetComponent(component_name);
            if (component == null)
            {
                Console.WriteLine(component_name);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Такого компонента нет в базе данных, либо Саша где-то накосячил",
                    replyMarkup: inlineKeyboardMarkup
                );
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: component.name + "\n" + component.title + "\n" + component.description,
                    replyMarkup: inlineKeyboardMarkup
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(component_name);
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Саша где-то накосячил",
                replyMarkup: new ReplyKeyboardRemove()
            );
        }
    }

    public async void SendRandomComponentMessage(ITelegramBotClient botClient, DbService dbService, long chat_id)
    {
        try
        {
            InlineKeyboardMarkup inlineKeyboardMarkup = new(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Ещё", "/sndrndcmpt"),

                }
            });

            var component = await dbService.GetOneRandomComponent();
            if (component == null)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chat_id,
                    text: "Такого компонента нет в базе данных, либо Саша где-то накосячил",
                    replyMarkup: inlineKeyboardMarkup
                );
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: chat_id,
                    text: component.name + "\n" + component.title + "\n" + component.description,
                    replyMarkup: inlineKeyboardMarkup
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await botClient.SendTextMessageAsync(
                chatId: chat_id,
                text: "Саша где-то накосячил",
                replyMarkup: new ReplyKeyboardRemove()
            );
        }
    }

    public async Task HandleCommand(ITelegramBotClient botClient, DbService dbService, Message message, string command, string[] args)
    {
        switch (command)
        {

            case "/start":
                InlineKeyboardMarkup inlineKeyboardMarkup = new(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Узнать что-то новое", "/sndrndcmpt"),
                    }
                });
                await dbService.AddUser(new User { chat_id = message.Chat.Id, username = message.Chat.Username });
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Привет, я бот, который поможет тебе узнать из чего состоит еда.",
                    replyMarkup: inlineKeyboardMarkup
                );
                break;
            case "/help":
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Список доступных команд:\n" +
                    "/start - начать работу с ботом\n" +
                    "/help - список доступных команд\n" +
                    "/getcomponent - получить информацию о компоненте\n" +
                    "/getallcomponents - получить список всех компонентов\n" +
                    "/addcomponent - добавить компонент в базу данных\n" +
                    "/removecomponent - удалить компонент из базы данных\n" +
                    "/updatecomponent - обновить информацию о компоненте в базе данных\n \n \n" +

                    "а еще ни одна из этих команд не работает, кроме /start и /help",
                    replyMarkup: new ReplyKeyboardRemove()
                );
                break;
            case "/sndrndcmpt":
                SendRandomComponentMessage(botClient, dbService, message.Chat.Id);
                break;
            default:
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Неизвестная команда",
                    replyMarkup: new ReplyKeyboardRemove()
                );
                break;
        }
    }

}