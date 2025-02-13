using Neo4j.Driver;

namespace AsM;

public class GraphService
{
    public IDriver Driver { get; set; } =
        GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "QUALLENDRECK1a."));
}