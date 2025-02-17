using Cassandra.Mapping;
using Cassandra.Mapping.Attributes;

namespace AsM.Models;

[Table("email_verification")]
[PrimaryKey("email")]
public class Verification
{
    [Column("email")]
    public string? Email { get; set; }

    [Column("id")]
    public Guid Id { get; set; }
}