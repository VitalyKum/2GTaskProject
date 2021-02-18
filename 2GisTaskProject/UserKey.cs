using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2GisTaskProject
{
    class UserKey<T>
    {
        T key;
        public UserKey(T key)
        {
            this.key = key;
        }
        public T Key
        {
            get { return key; }
        }

        public override bool Equals(object obj)
        {
            var other = obj as UserKey<T>;

            if (Object.ReferenceEquals(other, null))
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (this.GetType() != other.GetType())
            {
                return false;
            }
            return key.Equals(other.Key) && key.GetHashCode() == other.key.GetHashCode();
        }
 
        public override int GetHashCode()
        {
            return key.GetHashCode();
        }

        public static bool operator ==(UserKey<T> key1, UserKey<T> key2)
        {
            return key1.Equals(key2);
        }
        public static bool operator !=(UserKey<T> key1, UserKey<T> key2)
        {
            return !(key1 == key2);
        }
        public override string ToString()
        {
            return key.ToString();
        }
        public string Todo()
        {
            return string.Empty;
        }
    }
}
