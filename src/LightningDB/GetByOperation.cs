namespace LightningDB
{
    public class GetByOperation
    {
        private readonly LightningDatabase _db;
        private readonly byte[] _rawValue;

        public GetByOperation(LightningDatabase db, byte[] rawValue)
        {
            _db = db;
            _rawValue = rawValue;
        }

        public TValue Value<TValue>()
        {
            return _db.FromBytes<TValue>(_rawValue);
        }
    }
}