using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _2GisTaskProject
{
    public struct DoubleKey<TKey1, TKey2>
    {
        private readonly TKey1 key1;
        private readonly TKey2 key2;
        public DoubleKey(TKey1 key1, TKey2 key2)
        {
            this.key1 = key1;
            this.key2 = key2;
        }
        public TKey1 Key1
        {
            get { return key1; }
        }
        public TKey2 Key2
        {
            get { return key2; }
        }

        public override bool Equals(object obj)
        {
            if (obj is DoubleKey<TKey1, TKey2> other)
            {
                return key1.Equals(other.key1) && key2.Equals(other.key2);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return key1.GetHashCode() * key2.GetHashCode();
        }
        public override string ToString()
        {

            return $"{key1} {key2}";
        }

        public static bool operator ==(DoubleKey<TKey1, TKey2> key1, DoubleKey<TKey1, TKey2> key2)
        {
            return key1.Equals(key2);
        }
        public static bool operator !=(DoubleKey<TKey1, TKey2> key1, DoubleKey<TKey1, TKey2> key2)
        {
            return !(key1 == key2);
        }
    }
    public sealed class DoubleKeyDictionary<TKey1, TKey2, TValue> : IEnumerable<TValue>
    {
        private readonly object locker = new object();

        private readonly Dictionary<DoubleKey<TKey1, TKey2>, TValue> _dictionary = new Dictionary<DoubleKey<TKey1, TKey2>, TValue>();

        public int Count
        {
            get 
            {
                lock(locker)
                {
                    return  _dictionary.Count;
                }                
            }
        }

        public bool Add(DoubleKey<TKey1, TKey2> key, TValue value)
        {
            lock (locker)
            {
                if (_dictionary.ContainsKey(key))
                    return false;
                _dictionary.Add(key, value);
                return true;
            }
        }
        public bool Add(TKey1 key1, TKey2 key2, TValue value)
        {
            return this.Add(new DoubleKey<TKey1, TKey2>(key1, key2), value);
        }

        public bool Remove(DoubleKey<TKey1, TKey2> key)
        {
            lock (locker)
            {
                return _dictionary.Remove(key);
            }
        }
        public bool Remove(TKey1 key1, TKey2 key2)
        {
            return this.Remove(new DoubleKey<TKey1, TKey2>(key1, key2));
        }
        public int RemoveByKey1(TKey1 key1)
        {
            int ret = 0;
            lock (locker)
            {
                var keys = _dictionary
                     .Where(item => item.Key.Key1.Equals(key1))
                     .Select(k => k.Key).ToArray();

                foreach (var key in keys)
                {
                    _dictionary.Remove(key);
                    ret++;
                }
            }
            return ret;
        }
        public int RemoveByKey2(TKey2 key2)
        {
            int ret = 0;
            lock (locker)
            {
                var keys = _dictionary
                     .Where(item => item.Key.Key2.Equals(key2))
                     .Select(k => k.Key).ToArray();

                foreach (var key in keys)
                {
                    _dictionary.Remove(key);
                    ret++;
                }
            }
            return ret;
        }

        public TValue this[TKey1 key1, TKey2 key2]
        {
            get 
            {
                return this[new DoubleKey<TKey1, TKey2>(key1, key2)];
            }
            set
            {
                this[new DoubleKey<TKey1, TKey2>(key1, key2)] = value;
            }
        }
        public TValue this[DoubleKey<TKey1, TKey2> key]
        {
            get
            {
                lock (locker)
                {
                    return _dictionary[key];
                }
            }
            set
            {
                lock (locker)
                {
                    if (_dictionary.ContainsKey(key))
                        _dictionary[key] = value;
                    else
                        _dictionary.Add(key, value);
                }
            }
        }

        public IEnumerable<KeyValuePair<DoubleKey<TKey1, TKey2>, TValue>> GetItemsByKey1(TKey1 key1)
        {
            lock (locker)
            {
                return _dictionary
                    .Where(item => item.Key.Key1.Equals(key1))
                    .ToList();
            }
        }
        public IEnumerable<KeyValuePair<DoubleKey<TKey1, TKey2>, TValue>> GetItemsByKey2(TKey2 key2)
        {
            lock (locker)
            {
                return _dictionary
                .Where(item => item.Key.Key2.Equals(key2))
                .ToList();
            }
        }
       
        public TValue[] GetValuesByKey1(TKey1 key1)
        {
            lock (locker)
            {
                return _dictionary
                .Where(item => item.Key.Key1.Equals(key1))
                .Select(x => x.Value)
                .ToArray();
            }
        }
        public TValue[] GetValuesByKey2(TKey2 key2)
        {
            lock (locker)
            {
                return _dictionary
                .Where(item => item.Key.Key2.Equals(key2))
                .Select(x => x.Value)
                .ToArray();
            }
        }
        public TValue GetValue(TKey1 key1, TKey2 key2)
        {
            lock (locker)
            {
                return _dictionary.FirstOrDefault(item => item.Key.Key1.Equals(key1)
                && item.Key.Key2.Equals(key2)).Value;
            }
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            lock (locker)
            {
                return _dictionary.Values.GetEnumerator();
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (locker)
            {
                var e = (_dictionary as IEnumerable).GetEnumerator();
                return e;
            }
        }

        public override string ToString()
        {
            lock (locker)
            {
                var items = _dictionary
                .Select(x => $"{x.Key.Key1}, {x.Key.Key2}, {x.Value}").ToArray();
                return string.Join(Environment.NewLine, items);
            }
        }
    }
}
