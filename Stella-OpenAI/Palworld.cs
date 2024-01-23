using System.Net;
using System.Text;
using Discord;
using Discord.WebSocket;

namespace Stella_OpenAI;
using System.Net.Sockets;

public static class Palworld
{
    private const string HostName = "palworld.magical-project.com";
    private const int Port = 8211;
    private const string ServerName = "Stella-Pal-Server";
    private const string ThumbNaileUrl = "https://stella-bucket.sgp1.cdn.digitaloceanspaces.com/DypaJmjI_400x400.jpg";

    public static async Task SendPalworldEmbedAsync(SocketSlashCommand command)
    {
        var embed = await CreateServerStatusEmbedAsync();
        await command.FollowupAsync(embed: embed);
    }
    private static async Task<Embed> CreateServerStatusEmbedAsync(CancellationToken cancellationToken = default)
    {
        //サーバーのステータス確認
        var status = await ServerConnectionCheckAsync(HostName, Port, cancellationToken: cancellationToken);
        var embed = new EmbedBuilder();
        embed.WithTitle(ServerName);
        embed.AddField("サーバーアドレス", $"{HostName}:{Port}");
        embed.WithThumbnailUrl(ThumbNaileUrl);
        embed.WithTimestamp(DateTime.Now);
        if (status)
        {
            embed.WithColor(Color.Green);
            embed.AddField("ServerStatus","200:接続できました！");
        }
        else
        {
            embed.WithColor(Color.Red);
            embed.AddField("ServerStatus","503:接続に失敗しました");
        }

        return embed.Build();
    }

    private static async Task<bool> ServerConnectionCheckAsync(string hostname, int port, CancellationToken cancellationToken = default)
    {
        try
        {
            using var udpClient = new UdpClient();
            udpClient.Connect(hostname, port);
            var sendData = "Hello Palworld"u8.ToArray();
            await udpClient.SendAsync(sendData, sendData.Length);
            await udpClient.ReceiveAsync(cancellationToken);
            return true;
        }
        catch(Exception)
        {
            return false;
        }

    }
}