
using System.Collections.Generic;
using SoqlGen;

namespace IntegrationTest.Models;

[SoqlObject("Account", "MyQuery")]
public partial class Account
{
    [SoqlField("Name", "MyQuery")]
    public string Name { get; set; } = "";

    [SoqlField("AnnualRevenue", "MyQuery")]
    public decimal? AnnualRevenue { get; set; }

    [SoqlField("Contacts", "MyQuery")]
    public List<Contact>? Contacts { get; set; }

    [SoqlField("Owner", "MyQuery")]
    public User? Owner { get; set; }
}
