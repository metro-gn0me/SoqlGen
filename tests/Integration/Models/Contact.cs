
using System;
using SoqlGen;

namespace IntegrationTest.Models;

[SoqlObject("Contact", "MyQuery")]
public partial class Contact
{
    [SoqlField("LastName", "MyQuery")]
    public string LastName { get; set; } = "";
    
    [SoqlField("CreatedDate", "MyQuery")]
    public DateTime CreatedDate { get; set; }
}
