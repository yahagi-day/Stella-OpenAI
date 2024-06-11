using Discord;
using Discord.Interactions;

namespace Stella_OpenAI.Discord;

public class SlashCommandModule : InteractionModuleBase
{

    public override Task BeforeExecuteAsync(ICommandInfo command)
    {
        return base.BeforeExecuteAsync(command);
    }

    public override Task AfterExecuteAsync(ICommandInfo command)
    {
        return base.AfterExecuteAsync(command);
    }

    public override void OnModuleBuilding(InteractionService commandService, ModuleInfo module)
    {
        base.OnModuleBuilding(commandService, module);
    }

    [SlashCommand("ping", "Get a pong")]
    public async Task PongCommand()
        => await RespondAsync("Pong!");
}