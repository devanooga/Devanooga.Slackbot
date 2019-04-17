namespace Devanooga.Slackbot.Slack
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using SlackAPI;

    public class SlackBotClient
    {
        protected SlackTaskClient SlackTaskClient { get; }
        
        protected SlackSocketClient SlackSocketClient { get; }
    
        protected ILogger<SlackBotClient> Logger { get; }

        protected SlackBotOptions SlackBotOptions { get; }
        
        public SlackBotClient(
            IOptions<SlackBotOptions> options,
            ILogger<SlackBotClient> logger)
        {
            SlackBotOptions = options.Value;
            SlackTaskClient = new SlackTaskClient(SlackBotOptions.Token);
            SlackSocketClient = new SlackSocketClient(SlackBotOptions.Token);
            Logger = logger;
        }

        public async Task Connect()
        {
            SlackSocketClient.Connect((loginResponse =>
            {
                if (loginResponse.ok)
                {
                    Logger.LogInformation("Socket client responded ok");
                }
                else
                {
                    Logger.LogError($"SlackSocketClient Error: {loginResponse.error}");
                }
            }), (() =>
                Logger.LogInformation("Socket connected")));
            SlackSocketClient.OnMessageReceived += message =>
            {
                Logger.LogInformation(JsonConvert.SerializeObject(message));
            };
            SlackSocketClient.OnReactionAdded += reaction =>
            {
                Logger.LogInformation(JsonConvert.SerializeObject(reaction));
            };
            await SlackTaskClient.ConnectAsync();
            var response = await SlackTaskClient.PostMessageAsync("#sandbox", "Hi", "I'm a bot");
            Logger.LogInformation("Connected");
        }
    }
}