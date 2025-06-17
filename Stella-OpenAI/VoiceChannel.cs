using System.Diagnostics;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;

namespace Stella_OpenAI;

public class VoiceChannel
{
    private struct ChannelInfo
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
    }

    // ReSharper disable once CollectionNeverQueried.Local
    private readonly Dictionary<ChannelInfo, IAudioClient> _audioClients = new();

    public async Task JoinChannel(SocketSlashCommand command)
    {
        var channel = (command.User as IGuildUser)?.VoiceChannel;
        if (channel == null)
        {
            await command.RespondAsync("You must be in a voice channel to use this command.");
            return;
        }
        var audioClient = await channel.ConnectAsync();
#pragma warning disable CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
        command.RespondAsync("接続！");
#pragma warning restore CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
        _audioClients.Add(new ChannelInfo { GuildId = channel.GuildId, ChannelId = channel.Id }, audioClient);
        await Task.Delay(TimeSpan.FromSeconds(10));
        Console.WriteLine("再生開始");
#pragma warning disable CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
        SendAsync(audioClient, "test.mp3");
#pragma warning restore CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
    }

    private Process? CreateStream(string path)
    {
        try
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }

    private async Task SendAsync(IAudioClient client, string path)
    {
        using (var ffmpeg = CreateStream(path))
        using (var output = ffmpeg?.StandardOutput.BaseStream)
        using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
        {
            try
            {
                await client.SetSpeakingAsync(true);//just try to fix
#pragma warning disable CS8602 // null 参照の可能性があるものの逆参照です。
                await output?.CopyToAsync(discord);//_token to stop sending audio
#pragma warning restore CS8602 // null 参照の可能性があるものの逆参照です。
            }
            catch (OperationCanceledException)
            {
                return;
            }
            finally
            {
                await discord.FlushAsync();
                await client.SetSpeakingAsync(false);//just try to fix
            }
        }
    }
}