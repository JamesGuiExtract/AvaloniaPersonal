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
    }

    /// <summary>
    /// Discriminated union for three cases
    /// </summary>
    /// <typeparam name="TCase1">The type of case 1</typeparam>
    /// <typeparam name="TCase2">The type of case 2</typeparam>
    /// <typeparam name="TCase3">The type of case 2</typeparam>
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
        /// <param name="case3Action">The action to use if this instance represents case 2</param>
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
    }
}
