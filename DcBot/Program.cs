using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using YoutubeExplode;

private DiscordSocketClient _client;
private readonly string _botToken = "YOUR_BOT_TOKEN_HERE";

static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

public async Task MainAsync()
{
    _client = new DiscordSocketClient();

    _client.Log += Log;

    await _client.LoginAsync(TokenType.Bot, _botToken);
    await _client.StartAsync();

    _client.MessageReceived += HandleCommandAsync;

    await Task.Delay(-1);
}

private async Task HandleCommandAsync(SocketMessage message)
{
    var userMessage = message as SocketUserMessage;
    if (userMessage == null) return;

    var argPos = 0;
    if (!userMessage.HasCharPrefix('!', ref argPos)) return;

    var command = userMessage.Substring(argPos).ToLower();
    var channel = userMessage.Channel as SocketVoiceChannel;

    if (command.StartsWith("play"))
    {
        var query = userMessage.Substring(argPos + 5);
        await PlayMusicAsync(query, channel);
    }
}

private async Task PlayMusicAsync(string query, SocketVoiceChannel channel)
{
    var youtube = new YoutubeClient();

    var searchResult = await youtube.Search.GetVideosAsync(query);
    var video = searchResult[0];

    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
    var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

    var audioStream = await youtube.Videos.Streams.GetAsync(audioStreamInfo);
    var audioStreamUrl = audioStream.Url;

    var audioClient = await channel.ConnectAsync();
    var ffmpegStream = audioClient.CreatePCMStream(AudioApplication.Music);

    var ffmpeg = new NReco.VideoConverter.FFMpegConverter();
    var task = ffmpeg.ConvertLiveMedia(audioStreamUrl, null, ffmpegStream, "mp3", new ConvertSettings() { Seek = TimeSpan.FromSeconds(0) });

    await task;
    await audioClient.StopAsync();
}

private Task Log(LogMessage msg)
{
    Console.WriteLine(msg.ToString());
    return Task.CompletedTask;
}