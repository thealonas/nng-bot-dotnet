using nng.Enums;

namespace nng_bot.Enums;

public static class PriorityPhrases
{
    public static string GetName(this BanPriority priority)
    {
        return priority switch
        {
            BanPriority.White => "доступна разблокировка",
            BanPriority.Green => "взаимодействие со стеной/историями или публикация рекламы в статусе",
            BanPriority.Orange => "взаимодействие с обложкой или с фото группы",
            BanPriority.Red => "взаимодействие с чёрным списком/списком участников или создание сайта из сообщества",
            BanPriority.Teal => "спам на страницах/группах или оскорбления/травля",
            BanPriority.Local => "в боте",
            _ => "🤷‍♂️"
        };
    }
}
