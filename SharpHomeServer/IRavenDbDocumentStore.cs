using Raven.Client.Documents;

namespace SharpHomeServer
{
    public interface IRavenDbDocumentStore
    {
        IDocumentStore Store { get; }
    }
}