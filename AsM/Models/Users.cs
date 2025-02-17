using Cassandra;
using Cassandra.Mapping;
using Cassandra.Mapping.Attributes;

namespace AsM.Models;

[Table("users")]
[PrimaryKey("id")]
public class User
{
    [Column("id")] public Guid? Id { get; set; }

    [Column("dob")] public LocalDate? Dob { get; set; }

    [Column("email")]
    [SecondaryIndex]
    public string? Email { get; set; }

    [Column("username")] public string? Username { get; set; } = default;

    [Column("displayname")] public string? Displayname { get; set; }
}