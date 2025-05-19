using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public interface IInitializableEvent
{
    void InitializeGameObjects(JObject jsonData);
}
