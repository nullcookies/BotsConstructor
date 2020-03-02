namespace LogicalCore
{
    public interface ITranslatable
    {
        string ToString(ITranslator session);
    }
}
