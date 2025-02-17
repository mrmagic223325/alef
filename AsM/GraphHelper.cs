using Neo4j.Driver;

namespace AsM;

public class GraphService(IConfiguration configuration)
{
    public IDriver Driver { get; set; } =
        GraphDatabase.Driver(configuration["Neo4j:LocalUrl"], AuthTokens.Basic(configuration["Neo4j:Username"], configuration["Neo4j:Password"]));
}