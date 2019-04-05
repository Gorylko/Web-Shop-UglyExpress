﻿using System;
using Shop.Shared.Entities;
using Shop.Data.Repositories;
using Shop.Data.DataContext.Realization.MsSql;
using Shop.Shared.Entities.Authorize;
using System.Web.Security;
using System.Web;
using Newtonsoft.Json;

namespace Shop.Business.Services
{
    public class LoginService
    {
        UserRepository _userRepository = new UserRepository(new UserContext());
        private const int VERSION = 1;
        public User Login(string login, string password)
        {
            User user = _userRepository.GetAuthorizedUser(login, password);
            if(user == null)
            {
                return null;
            }
            SendCookies(new MembershipUser
            {
                UserId = user.Id,
                Name = user.Login,
                Role = user.Role
            });
            return user;
        }

        public MembershipUser Register(string login, string password, string email, string phone)
        {
            User user = _userRepository.RegisterUser(login, password, email, phone);
            return new MembershipUser
            {
                UserId = user.Id,
                Name = user.Login,
                Role = user.Role
            };
        }

        public void Logout()
        {
            FormsAuthentication.SignOut();
        }

        private void SendCookies(MembershipUser user)
        {
            var userData = JsonConvert.SerializeObject(user);
            var ticket = new FormsAuthenticationTicket(VERSION, user.Name, DateTime.Now, DateTime.Now.AddMinutes(15), false, userData);
            var encryptTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptTicket);
            System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
        }
    }
}
