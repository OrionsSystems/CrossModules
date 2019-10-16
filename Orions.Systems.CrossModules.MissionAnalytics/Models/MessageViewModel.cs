namespace Orions.Systems.CrossModules.MissionAnalytics
{
    public enum MessageType
    {
        Success,
        Warning,
        Info,
        Error
    }

    public class MessageViewModel
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public MessageType Type { get; set; }

        public MessageViewModel(string text, string title, MessageType type)
        {
            Text = text;
            Type = type;
            Title = title;
        }
    }
}