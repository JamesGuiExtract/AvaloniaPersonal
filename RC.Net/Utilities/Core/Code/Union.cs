using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
