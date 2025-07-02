namespace CoreApp.Application.Common.Settings;

public class AISettings
{
    public string Provider { get; set; } = "OpenRouter";

    public string Model { get; set; } = "mistralai/mistral-7b-instruct";
}
