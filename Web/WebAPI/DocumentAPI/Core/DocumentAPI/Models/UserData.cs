using System;
using System.Collections.Concurrent;

namespace DocumentAPI.Models
{
    /// <summary>
    /// data model for User(Controller)
    /// </summary>
    static public class UserData
    {
        static private ConcurrentDictionary<string, User> _userList = 
            new ConcurrentDictionary<string, User>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// check for match between user and known user
        /// </summary>
        /// <param name="user"></param>
        /// <returns>true if matches</returns>
        static public bool MatchUser(User user)
        {
            var found = _userList.TryGetValue(user.Username, out User foundUser);
            if (!found)
            {
                return false;
            }

            return user.Password.IsEquivalent(foundUser.Password, ignoreCase: false);
        }

        /// <summary>
        /// Add a mock user...
        /// TODO - remove
        /// </summary>
        /// <param name="user"></param>
        static public void AddMockUser(User user)
        {
            _userList.TryAdd(user.Username, user);
        }
    }
}
