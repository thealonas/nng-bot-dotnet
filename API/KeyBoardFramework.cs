using Newtonsoft.Json;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Keyboard;

namespace nng_bot.API;

public static class PayloadTemplates
{
    public const string StartDialog = "{\"command\":\"start\"}";
    public const string GiveEditorPayload = "{\"command\":\"GiveEditor\"}";
    public const string ForceGiveEditorPayload = "{\"command\":\"ForceGiveEditor\"}";
    public const string MyProfile = "{\"command\":\"ProfileCommand\"}";
    public const string ReturnBack = "{\"command\":\"GoToMenuCommand\"}";
    public const string IveJoined = "{\"command\":\"IveJoined\"}";
    public const string IveJoinedInFiftySubs = "{\"command\":\"IveJoinedInFiftySubs\"}";
    public const string UnbanMe = "{\"command\":\"UnbanMe\"}";
    public const string UnbanMeRequest = "{\"command\":\"UnbanMeRequest\"}";
}

public static class PayloadAdminActions
{
    public const string AdminPanel = "{\"command\":\"AdminPanel\"}";
    public const string ClearCache = "{\"command\":\"ClearCache\"}";
    public const string BanUser = "{\"command\":\"BanUser\"}";
    public const string MakeUserAdmin = "{\"command\":\"MakeUserAdmin\"}";
    public const string MakeUserLimitless = "{\"command\":\"MakeUserLimitless\"}";
    public const string EditEditorRestriction = "{\"command\":\"EditEditorRestriction\"}";
    public const string ShowOtherUserProfile = "{\"command\":\"ShowOtherUserProfile\"}";
    public const string GroupsStatistics = "{\"command\":\"GroupsStatistics\"}";
    public const string LimitPanel = "{\"command\":\"Limits\"}";
    public const string UnbanRequests = "{\"command\":\"UnbanRequests\"}";
    public const string UnbanRequestMoveForward = "{\"command\":\"UnbanRequestMoveForward\"}";
    public const string UnbanRequestMoveBack = "{\"command\":\"UnbanRequestMoveBack\"}";
    public const string UnbanRequestAccept = "{\"command\":\"UnbanRequestAccept\"}";
    public const string UnbanRequestDeny = "{\"command\":\"UnbanRequestDeny\"}";
    public const string UnbanRequestDelete = "{\"command\":\"UnbanRequestDelete\"}";

    public static bool IsAdminPayload(string payload)
    {
        var payloads = new List<string>
        {
            AdminPanel, ClearCache, BanUser,
            MakeUserAdmin, MakeUserLimitless, EditEditorRestriction, ShowOtherUserProfile, GroupsStatistics,
            LimitPanel, UnbanRequests, UnbanRequestMoveForward, UnbanRequestMoveBack, UnbanRequestAccept,
            UnbanRequestDeny,
            UnbanRequestDelete
        };
        return payloads.Any(x => payload == x);
    }
}

public static class KeyBoardFramework
{
    private const string CommandButtonType = "command";

    public static string StartButtons
    {
        get
        {
            var keyboard = new KeyboardBuilder();
            keyboard.SetOneTime();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Primary,
                Extra = "GiveEditor",
                Type = CommandButtonType,
                Label = "Выдать редактора"
            });
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Default,
                Extra = "ProfileCommand",
                Type = CommandButtonType,
                Label = "Мой профиль"
            });
            return JsonConvert.SerializeObject(keyboard.Build());
        }
    }

    public static string AgreeWithRulesButtons
    {
        get
        {
            var keyboard = new KeyboardBuilder();
            keyboard.SetOneTime();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Positive,
                Extra = "ForceGiveEditor",
                Type = CommandButtonType,
                Label = "Я согласен"
            });
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Negative,
                Extra = "GoToMenuCommand",
                Type = CommandButtonType,
                Label = "Вернуться назад"
            });
            return JsonConvert.SerializeObject(keyboard.Build());
        }
    }

    public static string RestrictedStartButtons
    {
        get
        {
            var keyboard = new KeyboardBuilder();
            keyboard.SetOneTime();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Default,
                Extra = "ProfileCommand",
                Type = CommandButtonType,
                Label = "Мой профиль"
            });
            return JsonConvert.SerializeObject(keyboard.Build());
        }
    }

    public static string UnbanRequestConfirm
    {
        get
        {
            var keyboard = new KeyboardBuilder();
            keyboard.SetOneTime();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Positive,
                Extra = "UnbanMe",
                Type = CommandButtonType,
                Label = "Отправить"
            });
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Negative,
                Extra = "GoToMenuCommand",
                Type = CommandButtonType,
                Label = "Вернуться назад"
            });
            return JsonConvert.SerializeObject(keyboard.Build());
        }
    }

    public static string RestrictedStartButtonsWithUnbanRequest
    {
        get
        {
            var keyboard = new KeyboardBuilder();
            keyboard.SetOneTime();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Default,
                Extra = "UnbanMeRequest",
                Type = CommandButtonType,
                Label = "Запросить разблокировку"
            });
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Default,
                Extra = "ProfileCommand",
                Type = CommandButtonType,
                Label = "Мой профиль"
            });
            return JsonConvert.SerializeObject(keyboard.Build());
        }
    }

    public static string GoToAdminUnbanRequests
    {
        get
        {
            var keyboard = new KeyboardBuilder();
            keyboard.SetOneTime();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Negative,
                Extra = "UnbanRequests",
                Type = CommandButtonType,
                Label = "Вернуться назад"
            });
            return JsonConvert.SerializeObject(keyboard.Build());
        }
    }

    public static string AdminStartButtons
    {
        get
        {
            var keyboard = new KeyboardBuilder();
            keyboard.SetOneTime();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Primary,
                Extra = "GiveEditor",
                Type = CommandButtonType,
                Label = "Редактор"
            });
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Default,
                Extra = "ProfileCommand",
                Type = CommandButtonType,
                Label = "Мой профиль"
            });
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Negative,
                Extra = "AdminPanel",
                Type = CommandButtonType,
                Label = "Админка"
            });
            return JsonConvert.SerializeObject(keyboard.Build());
        }
    }

    public static string AdminLimitsButtons
    {
        get
        {
            var keyboard = new KeyboardBuilder();
            keyboard.SetOneTime();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Default,
                Extra = "EditEditorRestriction",
                Type = CommandButtonType,
                Label = "Группы"
            });
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Default,
                Extra = "MakeUserLimitless",
                Type = CommandButtonType,
                Label = "Юзеры"
            });
            keyboard.AddLine();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Primary,
                Extra = "AdminPanel",
                Type = CommandButtonType,
                Label = "Вернуться назад"
            });
            return JsonConvert.SerializeObject(keyboard.Build());
        }
    }

    public static string AdminPanelButtons
    {
        get
        {
            var keyboard = new KeyboardBuilder();
            keyboard.SetOneTime();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Primary,
                Extra = "ClearCache",
                Type = CommandButtonType,
                Label = "Очистить кэш"
            });
            keyboard.AddLine();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Default,
                Extra = "BanUser",
                Type = CommandButtonType,
                Label = "Забаненные"
            });
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Default,
                Extra = "MakeUserAdmin",
                Type = CommandButtonType,
                Label = "Админы"
            });
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Default,
                Extra = "Limits",
                Type = CommandButtonType,
                Label = "Лимиты"
            });
            keyboard.AddLine();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Default,
                Extra = "GroupsStatistics",
                Type = CommandButtonType,
                Label = "Статистика"
            });
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Default,
                Extra = "UnbanRequests",
                Type = CommandButtonType,
                Label = "Запросы"
            });
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Default,
                Extra = "ShowOtherUserProfile",
                Type = CommandButtonType,
                Label = "Профиль"
            });
            keyboard.AddLine();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Negative,
                Extra = "GoToMenuCommand",
                Type = CommandButtonType,
                Label = "Вернуться назад"
            });
            return JsonConvert.SerializeObject(keyboard.Build());
        }
    }

    public static string GoToAdminLimitPanel
    {
        get
        {
            var keyboard = new KeyboardBuilder();
            keyboard.SetOneTime();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Negative,
                Extra = "Limits",
                Type = CommandButtonType,
                Label = "Вернуться назад"
            });
            return JsonConvert.SerializeObject(keyboard.Build());
        }
    }

    public static string GoToAdminPanel
    {
        get
        {
            var keyboard = new KeyboardBuilder();
            keyboard.SetOneTime();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Negative,
                Extra = "AdminPanel",
                Type = CommandButtonType,
                Label = "Вернуться назад"
            });
            return JsonConvert.SerializeObject(keyboard.Build());
        }
    }

    public static string GoToMenuButtons
    {
        get
        {
            var keyboard = new KeyboardBuilder();
            keyboard.SetOneTime();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Negative,
                Extra = "GoToMenuCommand",
                Type = CommandButtonType,
                Label = "Вернуться назад"
            });
            return JsonConvert.SerializeObject(keyboard.Build());
        }
    }

    public static string IveJoinedButtonsLessThanFiftySubs
    {
        get
        {
            var keyboard = new KeyboardBuilder();
            keyboard.SetOneTime();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Positive,
                Extra = "IveJoinedInFiftySubs",
                Type = CommandButtonType,
                Label = "Я вступил"
            });
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Negative,
                Extra = "GoToMenuCommand",
                Type = CommandButtonType,
                Label = "Вернуться назад"
            });
            return JsonConvert.SerializeObject(keyboard.Build());
        }
    }

    public static string IveJoinedButtons
    {
        get
        {
            var keyboard = new KeyboardBuilder();
            keyboard.SetOneTime();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Positive,
                Extra = "IveJoined",
                Type = CommandButtonType,
                Label = "Я вступил"
            });
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Negative,
                Extra = "GoToMenuCommand",
                Type = CommandButtonType,
                Label = "Вернуться назад"
            });
            return JsonConvert.SerializeObject(keyboard.Build());
        }
    }
}
