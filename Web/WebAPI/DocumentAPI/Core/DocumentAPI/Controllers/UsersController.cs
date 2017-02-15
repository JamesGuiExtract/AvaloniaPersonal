using DocumentAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using static DocumentAPI.Utils;

namespace DocumentAPI.Controllers
{
    /// <summary>
    /// Users (controller) class
    /// </summary>
    [Route("api/[controller]")]
    [BindRequired]
    public class UsersController : Controller
    {
        static private ConcurrentDictionary<string, User> _userList = new ConcurrentDictionary<string, User>();

        /// <summary>
        /// Add a mock user...
        /// TODO - remove
        /// </summary>
        /// <param name="user"></param>
        static public void AddMockUser(User user)
        {
            user.LoggedIn = false;
            _userList.TryAdd(user.Username, user);
        }

        /// <summary>
        /// login
        /// </summary>
        /// <param name="user">A User object (name, password, optional claim)</param>
        // POST api/User/Login
        [HttpPost("Login")]
        public IActionResult Login([FromBody] User user)
        {
            if (!ModelState.IsValid || 
                user == null || 
                String.IsNullOrEmpty(user.Username) || 
                String.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Name or Password is empty");
            }

            User foundUser;
            var found = _userList.TryGetValue(user.Username, out foundUser);
            if (!found)
            {
                return BadRequest(Inv($"Error: There is no known user named: {user.Username}"));
            }

            bool match = String.Compare(foundUser.Password, user.Password, ignoreCase: false, culture: CultureInfo.InvariantCulture) == 0;
            if (!match)
            {
                return BadRequest(Inv($"Error: password does not match for user: {user.Username}"));
            }

            foundUser.LoggedIn = true;

            return Ok("Logged in");
        }

        /// <summary>
        /// logout
        /// </summary>
        /// <param name="user"></param>
        // DELETE api/User/logout
        [HttpDelete("Logout")]
        public IActionResult Logout([FromBody] User user)
        {
            User theUser;
            var foundUser = _userList.TryGetValue(user.Username, out theUser);
            if (!foundUser)
            {
                return new NotFoundResult();
            }

            bool passwordMatches = String.Compare(user.Password, theUser.Password, ignoreCase: false, culture: CultureInfo.InvariantCulture) == 0;
            if (!passwordMatches)
            {
                return BadRequest("Error: Password doesn't match");
            }

            theUser.LoggedIn = false;
            return Ok("Logged out");
        }

        // TODO - not sure if this is necessary/useful
        /// <summary>
        /// Get user claims
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet("/GetUserClaims")]
        public List<Claim> GetUserClaims([FromQuery] string username)
        {
            User theUser;
            var foundUser = _userList.TryGetValue(username, out theUser);
            if (!foundUser)
            {
                Claim claim = new Claim
                {
                    Error = new ErrorInfo
                    {
                        ErrorOccurred = true,
                        Message = Inv($"Error: specified user: {username}, not found"),
                        Code = -1,
                    },
                    Name = "",
                    Value = ""
                };

                List<Claim> cl = new List<Claim>();
                cl.Add(claim);

                return cl;
            }

            return theUser.Claims;
        }

        /* Future stuff - 
        /// <summary>
        /// Management only - GET handler, returns names of all users
        /// </summary>
        /// <returns></returns>
        // GET: api/users
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return _userList.Select(kvp => kvp.Key).ToList<String>();
        }

        /// <summary>
        /// Management only - Get a user by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Nth user name</returns>
        // GET api/users/5
        [HttpGet("{id}", Name ="GetUserById")]
        public string Get(int id)
        {
            if (_userList.Count() < id)
            {
                return NotFound().ToString();
            }

            var name = _userList.Where(kvp => kvp.Value.Id == id).First().Value.Username;
            return name;
        }
        */
    }
}
