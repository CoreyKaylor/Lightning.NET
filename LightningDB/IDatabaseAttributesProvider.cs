using System.Text;

namespace LightningDB
{
    //TODO Is this interface buying us anything?
    public interface IDatabaseAttributesProvider
    {
        DatabaseOpenFlags OpenFlags { get; }

        string Name { get; }

        Encoding Encoding { get; }
    }
}
