
namespace PVInfo
{
    public struct IndexedValue
    {
        public readonly string Index;
        public readonly string Value;
        public readonly string Description;

        public IndexedValue(string index, string value, string description)
        {
            Index = index;
            Value = value;
            Description = description;
        }
    }
}