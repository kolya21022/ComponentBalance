using System.Collections.Generic;
using System.Linq;
using ComponentBalanceSqlv2.Data.Moves;

namespace ComponentBalanceSqlv2.Model
{
    //internal class MultipartKey<T>
    //{
    //    private readonly HashSet<T> _items;
    //    private readonly int _hashCode;
    //    public MultipartKey(IEnumerable<T> items)
    //    {
    //        _items = new HashSet<T>(items);
    //        _hashCode = _items.Where(i => i != null)
    //            .Aggregate(0, (p, v) => p * 31 + v.GetHashCode());
    //    }
    //    public override int GetHashCode()
    //    {
    //        return _hashCode;
    //    }
    //    public override bool Equals(object obj)
    //    {
    //        if (obj == this) return true;
    //        var other = obj as MultipartKey<T>;
    //        if (other == null) return false;
    //        return _items.SetEquals(other._items);
    //    }
    //}

    // Методом тыка что то вышло...
    internal class KeyMoves
    {
        private readonly HashSet<Move> _items;
        private readonly int _hashCode;

        public KeyMoves(IEnumerable<Move> items)
        {
            var moveComparer = new MoveComparer();

            _items = new HashSet<Move>(items);
            _hashCode = _items.Where(i => i != null)
                .Aggregate(0, (hashCode, v) => hashCode * 397 ^ moveComparer.GetHashCode(v));
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            var other = (KeyMoves)obj;
            if (other == null) return false;

            return new MovesHashSetEqualityComparer().Equals(_items, other._items);
        }
    }

    class MoveComparer : EqualityComparer<Move>
    {
        public override bool Equals(Move x, Move y)
        {
            if (x == null) return y == null;
            if (y == null) return false;
            if (ReferenceEquals(x, y)) return true;
            if (x.GetType() != y.GetType()) return false;

            return x.BalanceId == y.BalanceId
                   && x.Count == y.Count
                   && x.Cost == y.Cost
                   && x.Month == y.Month
                   && x.Year == y.Year
                   && x.IsSupply == y.IsSupply
                   && x.IsUserCanDelete == y.IsUserCanDelete;
        }

        public override int GetHashCode(Move x)
        {
            unchecked
            {
                var hashCode = x.BalanceId.GetHashCode();
                hashCode = (hashCode * 397) ^ x.Count.GetHashCode();
                hashCode = (hashCode * 397) ^ x.Cost.GetHashCode();
                hashCode = (hashCode * 397) ^ x.Month;
                hashCode = (hashCode * 397) ^ x.Year;
                hashCode = (hashCode * 397) ^ x.IsSupply.GetHashCode();
                hashCode = (hashCode * 397) ^ x.IsUserCanDelete.GetHashCode();
                return hashCode;
            }
        }
    }

    class MovesHashSetEqualityComparer : IEqualityComparer<HashSet<Move>>
    {
        private readonly MoveComparer _moveComparer;

        public MovesHashSetEqualityComparer()
        {
            _moveComparer = new MoveComparer();
        }

        public bool Equals(HashSet<Move> x, HashSet<Move> y)
        {
            if (x == null)
            {
                return (y == null);
            }

            if (y == null)
            {
                return false;
            }

            if (AreEqualityComparersEqual(x, y))
            {
                if (x.Count != y.Count)
                {
                    return false;
                }
            }
            // n^2 search because items are hashed according to their respective ECs
            foreach (var set2Item in y)
            {
                var found = false;
                foreach (var set1Item in x)
                {
                    if (_moveComparer.Equals(set2Item, set1Item))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(HashSet<Move> obj)
        {
            var hashCode = 0;
            foreach (var move in obj)
            {
                hashCode = hashCode * 397 ^ _moveComparer.GetHashCode(move);
            }
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            var comparer = (MovesHashSetEqualityComparer)obj;
            if (comparer == null)
            {
                return false;
            }
            return (_moveComparer == comparer._moveComparer);
        }

        public override int GetHashCode()
        {
            return _moveComparer.GetHashCode();
        }

        private static bool AreEqualityComparersEqual(HashSet<Move> set1, HashSet<Move> set2)
        {
            return set1.Comparer.Equals(set2.Comparer);
        }
    }
}
