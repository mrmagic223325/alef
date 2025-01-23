using System.ComponentModel.DataAnnotations;
using Cassandra;
using Cassandra.Mapping;
using Cassandra.Mapping.Attributes;

namespace AsM.Models;

[Table("users")]
[PrimaryKey("id")]
public class User
{
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("dob")]
    public LocalDate Dob { get; set; }
    
    [Column("email")] 
    public string Email { get; set; }
    
    [Column("username")]
    public string Username { get; set; }
}