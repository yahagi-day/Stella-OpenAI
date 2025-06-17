using System.Diagnostics;
using Discord;
using Discord.Interactions;

namespace Stella_OpenAI.Discord;

public class SlashCommandModule : InteractionModuleBase<SocketInteractionContext>
{
    private const string Version = "0.10.0 GPT-4 Omni";

    [SlashCommand("version", "Stella-Chanのバージョンを表示します")]
    public async Task VersionRespond()
        => await RespondAsync(Version);

    [SlashCommand("sudo", "大いなる力には責任が伴うよ！")]
    public async Task SuperUser()
    {
        //サーバーIDのチェック
        if (Context.Guild.Id != 839366588486123552) return;
        try
        {
            //管理者ロールを付与する
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "SuperUser");
            Console.WriteLine(role);
            await (Context.User as IGuildUser)?.AddRoleAsync(role)!;
            await RespondAsync("権限を付与しました");
            var task = async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(600));
                await (Context.User as IGuildUser)?.RemoveRoleAsync(role)!;
            };
            _ = task();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}