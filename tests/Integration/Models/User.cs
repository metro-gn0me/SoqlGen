
using SoqlGen;

namespace IntegrationTest.Models;

[SoqlObject("User", "MyQuery")]
public partial class User 
{
    [SoqlField("Username", "MyQuery")]
    public string Username { get; set; } = "";
}
