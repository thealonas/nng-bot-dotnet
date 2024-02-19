namespace nng_bot.Frameworks;

public partial class PhraseFramework
{
    public const string AgreeWithRules =
        "📃 Правила 📃\nПрежде чем выдать тебе редактора, ты должен ознакомиться с нашими правилами. Найти их можешь здесь — https://vk.com/@mralonas-nng-rules\nУ нас так же есть ответы на другие вопросы, что тоже может быть тебе полезным — https://vk.com/@mralonas-nng-faq";

    public const string CommandNotFound =
        "Упс. Команда не найдена 😥\nЕсли что, всегда используй кнопки при использовании бота 🤗";

    public const string EditorAfterFiftySubs = "👀 Отлично!\nТы зарезервировал себе место в последней группе.";

    public const string LimitReached =
        "К сожалению, мы не можем выдать тебе редактора, так как ты достиг лимита 😬\nВ основном мы снимаем лимиты за [https://vk.com/app5748831_-147811741#subscribe/1006976|прохождение опросов] или [https://vk.me/mralonas|олдам].";

    public const string MainMenu =
        "Привет 👋\nЗдесь ты можешь выдать себе редактора в одной из групп nng или посмотреть свой профиль.\nЕсли у тебя возникнут вопросы, напиши в [mralonas|основную группу] 🙃";

    public const string NoAvailableSlots =
        "К сожалению, сейчас нет доступных групп 😞\nСкорее всего, ты уже зарезервировал места во всех доступных тебе группах.";

    public const string YouAreOnCoolDown = "Не стоит выдавать себе редактора так часто 🙄";

    private const string YouHaveNoEditor = "❌";

    public const string YouHaveNotJoinedClub = "Похоже, ты не вступил в группу 🤔";

    public const string YourAccountIsTooYoung =
        "🦄 К сожалению твой аккаунт создан недавно, поэтому мы не можем выдать тебе редактора";

    public const string YourRequestNoLongerValid =
        "Так как прошло много времени с того момента как ты не вошёл в группу, мы отменили твой запрос. Запроси редактора ещё раз.";

    private const string EditorSuccessTemplate = "🎉 Готово! 🎉\nМы выдали тебе редактора в группе {ID}";

    private const string ErrorTemplate =
        "Произошла ошибка 😳\nХоть мы уже и получили уведомление о ней, всё равно рекомендуем тебе переслать это сообщение в [mralonas|основную группу] 👐\n{LOG}";

    private const string LessThanFiftySubsTemplate =
        "К сожалению, сейчас нет доступных групп, но недавно появилась новая группа @{ID}, где ещё нет 50 подписчиков.\nТы можешь вступить в неё и как только там наберётся 50 подписчиков, тебе будет автоматически выдан редактор.";

    private const string PleaseJoinGroupTemplate =
        "Круто! Мы подобрали тебе группу. Теперь вступи в неё: @{ID}\nКогда вступишь, нажми на кнопку «Я вступил» и не выходи из группы пока выдача не закончится 🥸";

    private const string ProfileTemplate =
        "👤 Имя: {Name}\n👁 Айди: @id{ID}\n{Date}{DateNewLine}{BanStatus}\n✏️ Редактор в группах{EditorCounter}: {Editor}{BAN}";

    public static string EditorSuccess(string id)
    {
        return EditorSuccessTemplate.SetId(id);
    }

    public static string Error(string log)
    {
        return ErrorTemplate.SetLog(log);
    }

    public static string LessThanFiftySubs(string group)
    {
        return LessThanFiftySubsTemplate.SetId(group);
    }

    public static string PleaseJoinGroup(string group)
    {
        return PleaseJoinGroupTemplate.SetId(group);
    }

    private static string Profile(string name, long id, string date, string dateNewLine, string ban, string banStatus,
        string editor, string editorCount)
    {
        return ProfileTemplate.SetName(name).SetId(id).SetDate(date).SetDateNewLine(dateNewLine).SetBan(ban)
            .SetEditorCounter(editorCount).SetEditor(editor).SetBanStatus(banStatus);
    }
}
