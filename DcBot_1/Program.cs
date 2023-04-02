using Discord.Audio;
using Discord.WebSocket;
using Discord;
using System;
using System.Threading.Tasks;
using YoutubeExplode;
using Discord.Commands;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;
using VisioForge.MediaFramework.FFMPEGEXE;
using NReco.VideoConverter;
using System.Linq;

namespace DcBot_1
{
    public class Program
    {
        private DiscordSocketClient _client;
        private readonly string _botToken = "MTA4NTI5ODk1MTQyNDU4OTk4NA.GsYO81.EV5r5_2WZFbRIy1Mn_tNTgWDI3JNaw8eNlMyuY";

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

            var command = String.Concat(userMessage.Content.ToLower().Substring(argPos + 1));
            var channel = userMessage.Channel as SocketVoiceChannel;

            if (command.StartsWith("play"))
            {
                var query = String.Concat(userMessage.Content.ToLower().Substring(argPos + 6));
                await PlayMusicAsync(query, channel);
            }
        }

        private async Task PlayMusicAsync(string query, SocketVoiceChannel channel)
        {
            var youtube = new YoutubeClient();

            var video = await youtube.Search.GetVideosAsync(query).FirstOrDefaultAsync();

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var audioStreamInfo = streamManifest.GetAudioOnly().WithHighestBitrate();

            var audioStream = await youtube.Videos.Streams.GetAsync(audioStreamInfo);
            var audioStreamUrl = audioStreamInfo.Url;

            var audioClient = await channel.ConnectAsync();
            var ffmpegStream = audioClient.CreatePCMStream(AudioApplication.Music);

            await youtube.Videos.Streams.CopyToAsync(audioStreamInfo, ffmpegStream);

            await audioClient.StopAsync();
        }



        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
