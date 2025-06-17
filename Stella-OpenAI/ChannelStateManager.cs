using Newtonsoft.Json;

namespace Stella_OpenAI;

public class ChannelStateManager
{
    private const string StateFilePath = "channel_state.json";
    private readonly HashSet<ulong> _enabledChannels = new();
    private readonly object _lock = new();

    public async Task LoadStateAsync()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(StateFilePath))
                    {
                        var json = File.ReadAllText(StateFilePath);
                        var channelIds = JsonConvert.DeserializeObject<ulong[]>(json);
                        if (channelIds != null)
                        {
                            _enabledChannels.Clear();
                            foreach (var channelId in channelIds)
                            {
                                _enabledChannels.Add(channelId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"チャンネル状態の読み込みに失敗しました: {ex.Message}");
                }
            }
        });
    }

    public async Task SaveStateAsync()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                try
                {
                    var channelIds = _enabledChannels.ToArray();
                    var json = JsonConvert.SerializeObject(channelIds, Formatting.Indented);
                    File.WriteAllText(StateFilePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"チャンネル状態の保存に失敗しました: {ex.Message}");
                }
            }
        });
    }

    public bool IsChannelEnabled(ulong channelId)
    {
        lock (_lock)
        {
            return _enabledChannels.Contains(channelId);
        }
    }

    public async Task EnableChannelAsync(ulong channelId)
    {
        lock (_lock)
        {
            _enabledChannels.Add(channelId);
        }
        await SaveStateAsync();
    }

    public async Task DisableChannelAsync(ulong channelId)
    {
        lock (_lock)
        {
            _enabledChannels.Remove(channelId);
        }
        await SaveStateAsync();
    }

    public ulong[] GetEnabledChannels()
    {
        lock (_lock)
        {
            return _enabledChannels.ToArray();
        }
    }
}