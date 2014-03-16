using System;
using System.Collections.Generic;
namespace LightningDB
{
    /// <summary>
    /// Converter class for pairs obtained via cursor
    /// </summary>
    public class CursorGetByOperation
    {
        private readonly LightningCursor _cur;
        private readonly KeyValuePair<byte[], byte[]>? _pair;

        /// <summary>
        /// Creates new instance of CursorGetByOperation.
        /// </summary>
        /// <param name="cur">Cursor.</param>
        /// <param name="pair">Key-value byte arrays pair</param>
        public CursorGetByOperation(LightningCursor cur, KeyValuePair<byte[], byte[]>? pair)
        {
            _cur = cur;
            _pair = pair;
        }

        /// <summary>
        /// Does key-value pair exist in database.
        /// </summary>
        public bool PairExists { get { return _pair.HasValue; } }

        private void EnsurePairExists()
        {
            if (!this.PairExists)
                throw new InvalidOperationException("Pair doesn't exist");
        }

        /// <summary>
        /// Converts database key to a concrete type
        /// </summary>
        /// <typeparam name="TKey">Type to convert key to</typeparam>
        /// <returns>Converted key value.</returns>
        public TKey Key<TKey>()
        {
            this.EnsurePairExists();

            return _cur.FromBytes<TKey>(_pair.Value.Key);
        }

        /// <summary>
        /// Converts database value to a concrete type.
        /// </summary>
        /// <typeparam name="TValue">Type to convert value to.</typeparam>
        /// <returns>Converted value.</returns>
        public TValue Value<TValue>()
        {
            this.EnsurePairExists();

            return _cur.FromBytes<TValue>(_pair.Value.Value);
        }

        /// <summary>
        /// Convert key-value pair to concrete types
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <returns>Pair of converted key and value.</returns>
        public KeyValuePair<TKey, TValue> Pair<TKey, TValue>()
        {
            return new KeyValuePair<TKey, TValue>(this.Key<TKey>(), this.Value<TValue>());
        }
    }
}