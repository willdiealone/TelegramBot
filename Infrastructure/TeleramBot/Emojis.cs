using Application.Interfaces;

namespace Infrastructure.TeleramBot;

public class Emojis : IEmojis
{
    public readonly Dictionary<string,string> Dictionary = new()
    {
        { "ясно", "\u2600\ufe0f ясно"},
        { "облачно", "\u2601\ufe0f облачно" },
        { "туман", "\ud83c\udf2b\ufe0f туман" },
        { "дождь", "\ud83c\udf27\ufe0f дождь" },
        { "сильный ветер", "\ud83d\udca8 сильный ветер" },
        { "ураган", "\ud83c\udf2a\ufe0f ураган" },
        { "торнадо", "\ud83c\udf2a\ufe0f торнадо" },
        { "небольшая облачность", "\ud83c\udf24\ufe0f небольшая облачность" },
        { "небольшой дождь", "\u2614\ufe0f небольшой дождь" },
        { "проливной дождь", "\ud83c\udf27\ufe0f проливной дождь" },
        { "легкий дождь", "\u2614\ufe0f небольшой дождь" },
        { "переменная облачность", "\u2601\ufe0f переменная облачность" },
        { "пасмурно", "\u2601\ufe0f пасмурно" },
        { "облачно с прояснениями", "\u26c5\ufe0f облачно с прояснениями" },
        { "гроза", "\u26c8\ufe0f гроза" },
        { "снег", "\ud83c\udf28\ufe0f снег" },
        { "небольшой снег", "\ud83c\udf28\ufe0f небольшой снег" },
        { "легкий снег", "\u2603\ufe0f легкий снег" },
        { "снегопад", "\ud83c\udf28\ufe0f снегопад" },
        { "гололед", "\u2744\ufe0f гололед" }
    };

    /// <summary>
    /// Возвращает значение по ключу
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public string GetEmoji(string str)
    {
        if (Dictionary.TryGetValue(str, out var emoji))
        {
            return emoji;
        }

        return new string("Посмотришь в окно, я не понимаю, что там за погода...");
    }
}