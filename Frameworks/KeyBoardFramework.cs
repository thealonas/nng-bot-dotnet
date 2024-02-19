using Newtonsoft.Json;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Keyboard;

namespace nng_bot.Frameworks;

public static class PayloadTemplates
{
    public const string StartDialog = "{\"command\":\"start\"}";
    public const string GiveEditorPayload = "{\"command\":\"GiveEditor\"}";
    public const string ForceGiveEditorPayload = "{\"command\":\"ForceGiveEditor\"}";
    public const string MyProfile = "{\"command\":\"ProfileCommand\"}";
    public const string ReturnBack = "{\"command\":\"GoToMenuCommand\"}";
    public const string IveJoined = "{\"command\":\"IveJoined\"}";
    public const string IveJoinedInFiftySubs = "{\"command\":\"IveJoinedInFiftySubs\"}";
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
