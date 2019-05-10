﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Models
{
    public enum ContactType
    {
        Phone,
        Email
    }
    public class User:SimpleUser
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
 
        public ContactType ContactPreference { get; set; }

    }
}