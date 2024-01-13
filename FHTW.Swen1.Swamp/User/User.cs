using System;
using System.Collections.Generic;

namespace FHTW.Swen1.Swamp
{
    public class User
    {

        public long Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public List<Card> Cards { get; set; } = new List<Card>();
        public List<Card> Deck { get; set; } = new List<Card>();
        public List<Package> Packages { get; set; } = new List<Package>();
        public int Coins { get; set; } = 20;
        public string AccessToken { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
        public int Elo { get; set; } = 100;
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WL { get; set; }
        

        public User()
        {
        }
        public User(long id, string username, string password)
        {
            Id = id;
            Username = username;
            Password = password;
        }

    }
}
