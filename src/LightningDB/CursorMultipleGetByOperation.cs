using System;
using System.Collections.Generic;
namespace LightningDB
{
    /// <summary>
    /// Converter class for pairs obtained via cursor
    /// </summary>
    public class CursorMultipleGetByOperation
    {
        private readonly LightningCursor _cur;
        private readonly KeyValuePair<GetByOperation, MultipleGetByOperation>? _pair;

        /// <summary>
        /// Creates new instance of CursorGetByOperation.
        /// </summary>
        /// <param name="cur">Cursor.</param>
        /// <param name="pair">Key-value byte arrays pair</param>
        public CursorMultipleGetByOperation(LightningCursor cur, KeyValuePair<byte[], byte[]>? pair)
        {
            _cur = cur;

            if (pair.HasValue)
            {
                _pair = new KeyValuePair<GetByOperation, MultipleGetByOperation>(
                    new GetByOperation(cur.Database, pair.Value.Key),
                    new MultipleGetByOperation(cur.Database, pair.Value.Value));
            }
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

            return _pair.Value.Key.Value<TKey>();
        }

        /// <summary>
        /// Converts database value to a concrete type.
        /// </summary>
        /// <typeparam name="TValue">Type to convert value to.</typeparam>
        /// <returns>Converted value.</returns>
        public TValue[] Values<TValue>()
        {
            this.EnsurePairExists();

            return _pair.Value.Value.Values<TValue>();
        }

        /// <summary>
        /// Convert key-value pair to concrete types
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <returns>Pair of converted key and value.</returns>
        public KeyValuePair<TKey, TValue[]> Pair<TKey, TValue>()
        {
            return new KeyValuePair<TKey, TValue[]>(this.Key<TKey>(), this.Values<TValue>());
        }
    }
}