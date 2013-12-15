using System;
using System.Collections.Generic;
namespace LightningDB
{
    public class CursorGetByOperation
    {
        private readonly LightningCursor _cur;
        private readonly KeyValuePair<byte[], byte[]>? _pair;

        public CursorGetByOperation(LightningCursor cur, KeyValuePair<byte[], byte[]>? pair)
        {
            _cur = cur;
            _pair = pair;
        }

        public bool PairExists { get { return _pair.HasValue; } }

        private void EnsurePairExists()
        {
            if (!this.PairExists)
                throw new InvalidOperationException("Pair doesn't exist");
        }

        public TKey Key<TKey>()
        {
            this.EnsurePairExists();

            return _cur.FromBytes<TKey>(_pair.Value.Key);
        }

        public TValue Value<TValue>()
        {
            this.EnsurePairExists();

            return _cur.FromBytes<TValue>(_pair.Value.Value);
        }

        public KeyValuePair<TKey, TValue> Pair<TKey, TValue>()
        {
            return new KeyValuePair<TKey, TValue>(this.Key<TKey>(), this.Value<TValue>());
        }
    }
}