using System;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Utilities
{
    /// <summary>
    /// Discriminated union for two cases
    /// </summary>
    /// <typeparam name="TCase1">The type of case 1</typeparam>
    /// <typeparam name="TCase2">The type of case 2</typeparam>
    public sealed class Union<TCase1, TCase2>
    {
        readonly TCase1 Item1;
        readonly TCase2 Item2;
        int tag;

        /// <summary>
        /// Initializes a new instance of case 1
        /// </summary>
        /// <param name="item">The item that this instance will contain</param>
        public Union(TCase1 item)
        {
            Item1 = item;
            tag = 0;
        }

        /// <summary>
        /// Initializes a new instance of case 2
        /// </summary>
        /// <param name="item">The item that this instance will contain</param>
        public Union(TCase2 item)
        {
            Item2 = item;
            tag = 1;
        }

        /// <summary>
        /// Matches the appropriate function to this instance
        /// </summary>
        /// <typeparam name="TResult">The return value of the functions</typeparam>
        /// <param name="case1Function">The function to use if this instance represents case 1</param>
        /// <param name="case2Function">The function to use if this instance represents case 2</param>
        /// <returns>The result of either <see paramref="case1Function"/> or <see paramref="case2Function"/></returns>
        public TResult Match<TResult>(Func<TCase1, TResult> case1Function, Func<TCase2, TResult> case2Function)
        {
            try
            {
                switch (tag)
                {
                    case 0:
                        return case1Function(Item1);
                    case 1:
                        return case2Function(Item2);
                    default:
                        return default(TResult);
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40058");
            }
        }

        /// <summary>
        /// Matches the appropriate action to this instance
        /// </summary>
        /// <param name="case1Action">The action to use if this instance represents case 1</param>
        /// <param name="case2Action">The action to use if this instance represents case 2</param>
        public void Match(Action<TCase1> case1Action, Action<TCase2> case2Action)
        {
            try
            {
                switch (tag)
                {
                    case 0:
                        case1Action(Item1);
                        break;
                    case 1:
                        case2Action(Item2);
                        break;
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40059");
            }
        }

        /// <summary>
        /// Calls ToString() on the underlying case
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Match(o => o.ToString(), o => o.ToString());
        }

        /// <summary>
        /// Creates an instance or returns null if <see paramref="item"/> is null
        /// </summary>
        /// <param name="item">The item to wrap in the union instance</param>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static Union<TCase1, TCase2> Maybe(TCase1 item)
        {
            if (item == null)
            {
                return null;
            }
            return new Union<TCase1, TCase2>(item);
        }

        /// <summary>
        /// Creates an instance or returns null if <see paramref="item"/> is null
        /// </summary>
        /// <param name="item">The item to wrap in the union instance</param>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static Union<TCase1, TCase2> Maybe(TCase2 item)
        {
            if (item == null)
            {
                return null;
            }
            return new Union<TCase1, TCase2>(item);
        }

        /// <summary>
        /// Compares this to <see paramref="obj"/> using value semantics
        /// </summary>
        /// <param name="obj">The other object to compare</param>
        /// <returns><c>true</c> if the other object has the same type as this and if the underlying value equals this one</returns>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as Union<TCase1, TCase2>;

            if (other == null
                || other.tag != tag
                || other.Match(
                    x => !x.Equals(Item1),
                    x => !x.Equals(Item2)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the hash code of the underlying value
        /// </summary>
        public override int GetHashCode()
        {
            return Match(
                x => x.GetHashCode(),
                x => x.GetHashCode());
        }
    }

    /// <summary>
    /// Discriminated union for three cases
    /// </summary>
    /// <typeparam name="TCase1">The type of case 1</typeparam>
    /// <typeparam name="TCase2">The type of case 2</typeparam>
    /// <typeparam name="TCase3">The type of case 3</typeparam>
    [SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public sealed class Union<TCase1, TCase2, TCase3>
    {
        readonly TCase1 Item1;
        readonly TCase2 Item2;
        readonly TCase3 Item3;
        int tag;

        /// <summary>
        /// Initializes a new instance of case 1
        /// </summary>
        /// <param name="item">The item that this instance will contain</param>
        public Union(TCase1 item)
        {
            Item1 = item;
            tag = 0;
        }

        /// <summary>
        /// Initializes a new instance of case 2
        /// </summary>
        /// <param name="item">The item that this instance will contain</param>
        public Union(TCase2 item)
        {
            Item2 = item;
            tag = 1;
        }

        /// <summary>
        /// Initializes a new instance of case 3
        /// </summary>
        /// <param name="item">The item that this instance will contain</param>
        public Union(TCase3 item)
        {
            Item3 = item;
            tag = 2;
        }

        /// <summary>
        /// Matches the appropriate function to this instance
        /// </summary>
        /// <typeparam name="TResult">The return value of the functions</typeparam>
        /// <param name="case1Function">The function to use if this instance represents case 1</param>
        /// <param name="case2Function">The function to use if this instance represents case 2</param>
        /// <param name="case3Function">The function to use if this instance represents case 3</param>
        /// <returns>The result of either <see paramref="case1Function"/>, <see paramref="case2Function"/>
        /// or <see paramref="case3Function"/></returns>
        public TResult Match<TResult>(Func<TCase1, TResult> case1Function,
            Func<TCase2, TResult> case2Function,
            Func<TCase3, TResult> case3Function)
        {
            try
            {
                switch (tag)
                {
                    case 0:
                        return case1Function(Item1);
                    case 1:
                        return case2Function(Item2);
                    case 2:
                        return case3Function(Item3);
                    default:
                        return default(TResult);
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI41844");
            }
        }

        /// <summary>
        /// Matches the appropriate action to this instance
        /// </summary>
        /// <param name="case1Action">The action to use if this instance represents case 1</param>
        /// <param name="case2Action">The action to use if this instance represents case 2</param>
        /// <param name="case3Action">The action to use if this instance represents case 3</param>
        public void Match(Action<TCase1> case1Action, Action<TCase2> case2Action, Action<TCase3> case3Action)
        {
            try
            {
                switch (tag)
                {
                    case 0:
                        case1Action(Item1);
                        break;
                    case 1:
                        case2Action(Item2);
                        break;
                    case 2:
                        case3Action(Item3);
                        break;
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI41845");
            }
        }

        /// <summary>
        /// Creates an instance or returns null if <see paramref="item"/> is null
        /// </summary>
        /// <param name="item">The item to wrap in the union instance</param>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static Union<TCase1, TCase2, TCase3> Maybe(TCase1 item)
        {
            if (item == null)
            {
                return null;
            }
            return new Union<TCase1, TCase2, TCase3>(item);
        }

        /// <summary>
        /// Creates an instance or returns null if <see paramref="item"/> is null
        /// </summary>
        /// <param name="item">The item to wrap in the union instance</param>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static Union<TCase1, TCase2, TCase3> Maybe(TCase2 item)
        {
            if (item == null)
            {
                return null;
            }
            return new Union<TCase1, TCase2, TCase3>(item);
        }

        /// <summary>
        /// Creates an instance or returns null if <see paramref="item"/> is null
        /// </summary>
        /// <param name="item">The item to wrap in the union instance</param>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static Union<TCase1, TCase2, TCase3> Maybe(TCase3 item)
        {
            if (item == null)
            {
                return null;
            }
            return new Union<TCase1, TCase2, TCase3>(item);
        }

        /// <summary>
        /// Calls ToString() on the underlying case
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Match(o => o.ToString(), o => o.ToString(), o => o.ToString());
        }

        /// <summary>
        /// Compares this to <see paramref="obj"/> using value semantics
        /// </summary>
        /// <param name="obj">The other object to compare</param>
        /// <returns><c>true</c> if the other object has the same type as this and if the underlying value equals this one</returns>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as Union<TCase1, TCase2, TCase3>;

            if (other == null
                || other.tag != tag
                || other.Match(
                    x => !x.Equals(Item1),
                    x => !x.Equals(Item2),
                    x => !x.Equals(Item3)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the hash code of the underlying value
        /// </summary>
        public override int GetHashCode()
        {
            return Match(
                x => x.GetHashCode(),
                x => x.GetHashCode(),
                x => x.GetHashCode());
        }
    }

    /// <summary>
    /// Discriminated union for four cases
    /// </summary>
    /// <typeparam name="TCase1">The type of case 1</typeparam>
    /// <typeparam name="TCase2">The type of case 2</typeparam>
    /// <typeparam name="TCase3">The type of case 3</typeparam>
    /// <typeparam name="TCase4">The type of case 4</typeparam>
    [SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public sealed class Union<TCase1, TCase2, TCase3, TCase4>
    {
        readonly TCase1 Item1;
        readonly TCase2 Item2;
        readonly TCase3 Item3;
        readonly TCase4 Item4;
        int tag;

        /// <summary>
        /// Initializes a new instance of case 1
        /// </summary>
        /// <param name="item">The item that this instance will contain</param>
        public Union(TCase1 item)
        {
            Item1 = item;
            tag = 0;
        }

        /// <summary>
        /// Initializes a new instance of case 2
        /// </summary>
        /// <param name="item">The item that this instance will contain</param>
        public Union(TCase2 item)
        {
            Item2 = item;
            tag = 1;
        }

        /// <summary>
        /// Initializes a new instance of case 3
        /// </summary>
        /// <param name="item">The item that this instance will contain</param>
        public Union(TCase3 item)
        {
            Item3 = item;
            tag = 2;
        }

        /// <summary>
        /// Initializes a new instance of case 4
        /// </summary>
        /// <param name="item">The item that this instance will contain</param>
        public Union(TCase4 item)
        {
            Item4 = item;
            tag = 3;
        }

        /// <summary>
        /// Matches the appropriate function to this instance
        /// </summary>
        /// <typeparam name="TResult">The return value of the functions</typeparam>
        /// <param name="case1Function">The function to use if this instance represents case 1</param>
        /// <param name="case2Function">The function to use if this instance represents case 2</param>
        /// <param name="case3Function">The function to use if this instance represents case 3</param>
        /// <param name="case4Function">The function to use if this instance represents case 4</param>
        /// <returns>The result of either <see paramref="case1Function"/>, <see paramref="case2Function"/>,
        /// <see paramref="case3Function"/> or <see paramref="case4Function"/> </returns>
        public TResult Match<TResult>(Func<TCase1, TResult> case1Function,
            Func<TCase2, TResult> case2Function,
            Func<TCase3, TResult> case3Function,
            Func<TCase4, TResult> case4Function)
        {
            try
            {
                switch (tag)
                {
                    case 0:
                        return case1Function(Item1);
                    case 1:
                        return case2Function(Item2);
                    case 2:
                        return case3Function(Item3);
                    case 3:
                        return case4Function(Item4);
                    default:
                        return default(TResult);
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI46225");
            }
        }

        /// <summary>
        /// Matches the appropriate action to this instance
        /// </summary>
        /// <param name="case1Action">The action to use if this instance represents case 1</param>
        /// <param name="case2Action">The action to use if this instance represents case 2</param>
        /// <param name="case3Action">The action to use if this instance represents case 3</param>
        /// <param name="case4Action">The action to use if this instance represents case 4</param>
        public void Match(Action<TCase1> case1Action, Action<TCase2> case2Action, Action<TCase3> case3Action,
            Action<TCase4> case4Action)
        {
            try
            {
                switch (tag)
                {
                    case 0:
                        case1Action(Item1);
                        break;
                    case 1:
                        case2Action(Item2);
                        break;
                    case 2:
                        case3Action(Item3);
                        break;
                    case 3:
                        case4Action(Item4);
                        break;
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI46226");
            }
        }

        /// <summary>
        /// Calls ToString() on the underlying case
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Match(o => o.ToString(), o => o.ToString(), o => o.ToString(), o => o.ToString());
        }

        /// <summary>
        /// Compares this to <see paramref="obj"/> using value semantics
        /// </summary>
        /// <param name="obj">The other object to compare</param>
        /// <returns><c>true</c> if the other object has the same type as this and if the underlying value equals this one</returns>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as Union<TCase1, TCase2, TCase3, TCase4>;

            if (other == null
                || other.tag != tag
                || other.Match(
                    x => !x.Equals(Item1),
                    x => !x.Equals(Item2),
                    x => !x.Equals(Item3),
                    x => !x.Equals(Item4)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the hash code of the underlying value
        /// </summary>
        public override int GetHashCode()
        {
            return Match(
                x => x.GetHashCode(),
                x => x.GetHashCode(),
                x => x.GetHashCode(),
                x => x.GetHashCode());
        }
    }

    /// <summary>
    /// Discriminated union for five cases
    /// </summary>
    /// <typeparam name="TCase1">The type of case 1</typeparam>
    /// <typeparam name="TCase2">The type of case 2</typeparam>
    /// <typeparam name="TCase3">The type of case 3</typeparam>
    /// <typeparam name="TCase4">The type of case 4</typeparam>
    /// <typeparam name="TCase5">The type of case 5</typeparam>
    [SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public sealed class Union<TCase1, TCase2, TCase3, TCase4, TCase5>
    {
        readonly TCase1 Item1;
        readonly TCase2 Item2;
        readonly TCase3 Item3;
        readonly TCase4 Item4;
        readonly TCase5 Item5;
        int tag;

        /// <summary>
        /// Initializes a new instance of case 1
        /// </summary>
        /// <param name="item">The item that this instance will contain</param>
        public Union(TCase1 item)
        {
            Item1 = item;
            tag = 0;
        }

        /// <summary>
        /// Initializes a new instance of case 2
        /// </summary>
        /// <param name="item">The item that this instance will contain</param>
        public Union(TCase2 item)
        {
            Item2 = item;
            tag = 1;
        }

        /// <summary>
        /// Initializes a new instance of case 3
        /// </summary>
        /// <param name="item">The item that this instance will contain</param>
        public Union(TCase3 item)
        {
            Item3 = item;
            tag = 2;
        }

        /// <summary>
        /// Initializes a new instance of case 4
        /// </summary>
        /// <param name="item">The item that this instance will contain</param>
        public Union(TCase4 item)
        {
            Item4 = item;
            tag = 3;
        }

        /// <summary>
        /// Initializes a new instance of case 5
        /// </summary>
        /// <param name="item">The item that this instance will contain</param>
        public Union(TCase5 item)
        {
            Item5 = item;
            tag = 4;
        }

        /// <summary>
        /// Matches the appropriate function to this instance
        /// </summary>
        /// <typeparam name="TResult">The return value of the functions</typeparam>
        /// <param name="case1Function">The function to use if this instance represents case 1</param>
        /// <param name="case2Function">The function to use if this instance represents case 2</param>
        /// <param name="case3Function">The function to use if this instance represents case 3</param>
        /// <param name="case4Function">The function to use if this instance represents case 4</param>
        /// <param name="case5Function">The function to use if this instance represents case 5</param>
        /// <returns>The result of either <see paramref="case1Function"/>, <see paramref="case2Function"/>,
        /// <see paramref="case3Function"/>, <see paramref="case3Function"/> or <see paramref="case5Function"/> </returns>
        public TResult Match<TResult>(Func<TCase1, TResult> case1Function,
            Func<TCase2, TResult> case2Function,
            Func<TCase3, TResult> case3Function,
            Func<TCase4, TResult> case4Function,
            Func<TCase5, TResult> case5Function)
        {
            try
            {
                switch (tag)
                {
                    case 0:
                        return case1Function(Item1);
                    case 1:
                        return case2Function(Item2);
                    case 2:
                        return case3Function(Item3);
                    case 3:
                        return case4Function(Item4);
                    case 4:
                        return case5Function(Item5);
                    default:
                        return default(TResult);
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI46227");
            }
        }

        /// <summary>
        /// Matches the appropriate action to this instance
        /// </summary>
        /// <param name="case1Action">The action to use if this instance represents case 1</param>
        /// <param name="case2Action">The action to use if this instance represents case 2</param>
        /// <param name="case3Action">The action to use if this instance represents case 3</param>
        /// <param name="case4Action">The action to use if this instance represents case 4</param>
        /// <param name="case5Action">The action to use if this instance represents case 5</param>
        public void Match(Action<TCase1> case1Action, Action<TCase2> case2Action, Action<TCase3> case3Action,
            Action<TCase4> case4Action, Action<TCase5> case5Action)
        {
            try
            {
                switch (tag)
                {
                    case 0:
                        case1Action(Item1);
                        break;
                    case 1:
                        case2Action(Item2);
                        break;
                    case 2:
                        case3Action(Item3);
                        break;
                    case 3:
                        case4Action(Item4);
                        break;
                    case 4:
                        case5Action(Item5);
                        break;
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI46228");
            }
        }

        /// <summary>
        /// Calls ToString() on the underlying case
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Match(o => o.ToString(), o => o.ToString(), o => o.ToString(), o => o.ToString(), o => o.ToString());
        }

        /// <summary>
        /// Compares this to <see paramref="obj"/> using value semantics
        /// </summary>
        /// <param name="obj">The other object to compare</param>
        /// <returns><c>true</c> if the other object has the same type as this and if the underlying value equals this one</returns>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as Union<TCase1, TCase2, TCase3, TCase4, TCase5>;

            if (other == null
                || other.tag != tag
                || other.Match(
                    x => !x.Equals(Item1),
                    x => !x.Equals(Item2),
                    x => !x.Equals(Item3),
                    x => !x.Equals(Item4),
                    x => !x.Equals(Item5)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the hash code of the underlying value
        /// </summary>
        public override int GetHashCode()
        {
            return Match(
                x => x.GetHashCode(),
                x => x.GetHashCode(),
                x => x.GetHashCode(),
                x => x.GetHashCode(),
                x => x.GetHashCode());
        }
    }
}
