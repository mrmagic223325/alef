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
    public required string Hash { get; set; }
    
    [Column("salt")]
    public required string Salt { get; set; }
}