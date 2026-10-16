using System.Text.Json;
namespace Armelle.Services;
public class PublicPhotoStore
{
    private readonly string _file;
    private readonly object _lock = new();
    public PublicPhotoStore(IWebHostEnvironment env)
    {
        _file = Path.Combine(env.ContentRootPath, "App_Data", "public_photo_slots.json");
    }
    private Dictionary<string,string> Read()
    {
        if (!File.Exists(_file)) return new();
        try { return JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(_file)) ?? new(); }
        catch (JsonException) { return new(); }
    }
    public string Resolve(string key, string fallback)
    {
        lock (_lock)
        {
            var data=Read();
            return data.TryGetValue(key,out var path) && path.StartsWith("/uploads/public-content/",StringComparison.Ordinal) ? path : fallback;
        }
    }
    public void Set(string key,string path)
    {
        lock (_lock)
        {
            var data=Read(); data[key]=path;
            Directory.CreateDirectory(Path.GetDirectoryName(_file)!);
            var temp=_file+".tmp";
            File.WriteAllText(temp,JsonSerializer.Serialize(data,new JsonSerializerOptions{WriteIndented=true}));
            File.Move(temp,_file,true);
        }
    }
}
