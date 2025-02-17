using Cassandra.Mapping;
using Cassandra.Mapping.Attributes;

namespace AsM.Models;

[Table("credentials")]
[PrimaryKey("id")]
public class Credentials
{
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("hash")]
    public string? Hash { get; set; }
}