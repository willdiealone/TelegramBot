using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;


var botClient = new TelegramBotClient("<Insert the token you've got from BotFather here>");

using var cts = new CancellationTokenSource();

var receiverOptions = new ReceiverOptions
{ 
    AllowedUpdates = { }
};

botClient.StartReceiving(
    HandleUpdatesAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Начал прослушку @{me.Username}");

Console.ReadLine();

cts.Cancel();

async Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{ 
    if (update.Type == UpdateType.Message) 
    {
            
        var smsText = update.Message.Text;
        var message = update.Message;
        var type = update.Type;
        var chatId = update.Message.Chat.Id;
        var userId = update.Message.From.Id;
        var userFolder = $"Downloads\\{userId}";
        Directory.CreateDirectory(userFolder);
        var filesNumber = Directory.GetFiles($"{userFolder}", "*", SearchOption.TopDirectoryOnly).Length;

        if (update.Message.Type == MessageType.Document)
        {
            var document = update.Message.Document; 
            if (document.FileName.EndsWith(".gif.mp4")) 
                document.FileName = $"Animation{filesNumber + 1}";

            DownloadFileAsync(document.FileId, userFolder, botClient, chatId, document.FileName);
            return;
        }

        if (update.Message.Type == MessageType.Photo)
        {
            var fileName = $"Photo{filesNumber + 1}.jpeg";
            DownloadFileAsync(message.Photo.Last().FileId, userFolder, botClient, chatId, fileName);
            return;
        }

        if (update.Message.Type == MessageType.Audio)
        { 
            var fileName = $"Voice{filesNumber + 1}.mp3";
            return;
        }

        if (smsText == "/start")
        {
            await botClient.SendChatActionAsync(chatId, ChatAction.Typing);
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, 
                text: "Hellow, my name is Noob Saibot \nand i have commands > /file /photo\n/voice",
                cancellationToken: cts.Token);

            await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                text: "You can delete everything downloaded >> /delete",
                cancellationToken: cts.Token);

            await botClient.SendTextMessageAsync(update.Message.Chat.Id, 
                "Try sending me a file >>> ", cancellationToken: cts.Token);
            return;
        }

        if (smsText == "/delete")
        {
            if(filesNumber == 0)
            {
                await botClient.SendChatActionAsync(chatId, ChatAction.Typing);
                await botClient.SendTextMessageAsync(chatId,"There are not files to delete");
                return;
            }
            var yesButton = InlineKeyboardButton.WithCallbackData("Yes");
            var noButton = InlineKeyboardButton.WithCallbackData("No");

            var yesButtonRow = new InlineKeyboardButton[] { yesButton };
            var noButtonRow = new InlineKeyboardButton[] { noButton };

            var buttonArray = new InlineKeyboardButton[][] { yesButtonRow, noButtonRow };

            var ikButtons = new InlineKeyboardMarkup(buttonArray);

            await botClient.SendChatActionAsync(chatId, ChatAction.Typing);
            await botClient.SendTextMessageAsync(chatId, "Are you sure you want to remove all the files?",
                replyMarkup: ikButtons);
            return;
        }

        if (smsText == "/file")
        { 
            await ShowFileAsync(botClient, userFolder, chatId);
            return;
        }

        await botClient.SendTextMessageAsync(update.Message.Chat.Id, text: $"You say: \n{update.Message.Text}");
    }

    if (update.Type == UpdateType.CallbackQuery)
    {
        var cbQuery = update.CallbackQuery;
        var userId = cbQuery.From.Id;
        var userFolder = $"Downloads\\{userId}";
        var chatId = cbQuery.Message.Chat.Id;
        var filesArray = Directory.GetFiles($"{userFolder}", "*", SearchOption.TopDirectoryOnly);

        if (cbQuery.Data == "Yes")
        {
            if (filesArray.Length == 0) 
            { 
                await botClient.SendChatActionAsync(chatId, ChatAction.Typing);
                await botClient.SendTextMessageAsync(chatId, " this folder is already empty");
                return;

            }

            var dir = new DirectoryInfo(userFolder);
            foreach (FileInfo file in dir.EnumerateFiles())
            { 
                file.Delete();
            }

            await botClient.SendChatActionAsync(chatId, ChatAction.Typing);
            await botClient.SendTextMessageAsync(chatId, "All the files have been removed");
            return;
        }

        if (cbQuery.Data == "No")
        {
            if (filesArray.Length == 0)
            {
                await botClient.SendChatActionAsync(chatId, ChatAction.Typing);
                await botClient.SendTextMessageAsync(chatId, "You have cancelled files deletion");
                return;
            }
        }

        if (filesArray.Length == 0)
        {
            await botClient.SendChatActionAsync(chatId, ChatAction.Typing);
            await botClient.SendTextMessageAsync(chatId, "The file has not be found");
            return;
        }

        for (int i = 0; i < filesArray.Length; i++)
        {
            if (Path.GetFileNameWithoutExtension(filesArray[i]) == cbQuery.Data)
            { 
                await botClient.SendChatActionAsync(chatId, ChatAction.Typing); 
                await botClient.SendTextMessageAsync(chatId, "Downloading...");
               using Stream stream = System.IO.File.OpenRead(filesArray[i]);

                await botClient.SendDocumentAsync(chatId, new InputOnlineFile(stream, 
                    Path.GetFileName(filesArray[i])));
               
                return;
            }
        }
        return;
    }
            
    async void DownloadFileAsync(string fileId, string path, ITelegramBotClient bot, long chatId, string fileName)
    {
        var file = await botClient.GetFileAsync(fileId);
        using FileStream fileStream = System.IO.File.OpenWrite(path + @$"\{fileName}");
        await botClient.DownloadFileAsync(file.FilePath, fileStream);
        await bot.SendTextMessageAsync(chatId, text: $"The file is saved as \n{fileName}");
        
    }

    async Task ShowFileAsync(ITelegramBotClient botClient, string path, long chatId)
    {
        var fileArray = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
        var ibuttomsArrayList = new List<InlineKeyboardButton[]>();
        if (fileArray.Length == 0)
        {
            await botClient.SendTextMessageAsync(chatId, "You folder is empty");
            return;
        }
        for (int i = 0; i < fileArray.Length; i++)
        {
            var ibuttomsList = new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(Path.GetFileNameWithoutExtension(fileArray[i]))
            };
           ibuttomsArrayList.Add(ibuttomsList.ToArray());
        }
        InlineKeyboardMarkup markup = ibuttomsArrayList.ToArray();
        await botClient.SendTextMessageAsync(chatId, "Uploaded files:", replyMarkup: markup);
    }
}
Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Ошибка телеграм АПИ:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
        _ => exception.ToString()
    };
    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}