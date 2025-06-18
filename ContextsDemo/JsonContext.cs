using System.Threading.Tasks;

public class JsonContext
{

}

public class JsonFile
{
  public string FileName { get; private set; }

  public JsonFile(string fileName)
  {
    // Initialize the JsonFile with the given file name
  }

  public async Task Save(object data)
  {
    // Logic to save the JSON file
    var serializedData = System.Text.Json.JsonSerializer.Serialize(data);
    using (var writer = new System.IO.StreamWriter(FileName))
    {
      await writer.WriteAsync(serializedData);
    }
  }
}
