using System.Text.Unicode;
using Discord;

namespace Stella_OpenAI;
using System.Net.Sockets;

public static class Palworld
{
    private const string HostName = "palworld.magical-project.com";
    private const int Port = 8211;
    private const string ServerName = "Stella-Pal-Server";
    private const string ThumbNaileUrl = "https://stella-bucket.sgp1.cdn.digitaloceanspaces.com/DypaJmjI_400x400.jpg";
    public static async Task<Embed> CreateServerStatusEmbedAsync(CancellationToken cancellationToken = default)
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

    public static async Task<bool> ServerConnectionCheckAsync(string hostname, int port, CancellationToken cancellationToken = default)
    {
        using var tcpClient = new TcpClient();
        tcpClient.SendTimeout = 1000;
        tcpClient.ReceiveTimeout = 1000;
        // ReSharper disable once MethodHasAsyncOverload
        try
        {
            await tcpClient.ConnectAsync(hostname, port, cancellationToken: cancellationToken);
            var ns = tcpClient.GetStream();
            ns.ReadTimeout = 10000;
            ns.WriteTimeout = 10000;
            var enc = System.Text.Encoding.UTF8;
            var ms = new MemoryStream();
            var resBytes = new byte[256];
            var resSize = 0;
            do
            {
                resSize = await ns.ReadAsync(resBytes.AsMemory(0, resSize), cancellationToken);
                if (resSize == 0)
                {
                    Console.WriteLine("Disconnect");
                    break;
                }
                await ms.WriteAsync(resBytes.AsMemory(0, resSize), cancellationToken);

            }while (ns.DataAvailable || resBytes[resSize - 1] != '\n') ;
            var resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            ms.Close();
            resMsg = resMsg.TrimEnd('\n');
            Console.WriteLine(resMsg);
            return tcpClient.Connected;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

    }
}