using System;
using System.Collections.Generic;
using System.Linq;
using HeyDo.Models;

namespace HeyDo.Data
{
    public class TestData
    {
        public static User Contests = new User
        {
            Id = "1",
            FirstName = "Test",
            LastName = "Tests",
            email = "",
            Phone = "",
            ContactPreference = ContactType.Phone
        };

        public static User Emailtests = new User
        {
            Id = "2",
            FirstName = "Email",
            LastName = "Tests",
            email = "",
            Phone = "",
            ContactPreference = ContactType.Email
        };

        public static List<User> TestUsers = new List<User>()
        {
            Emailtests,
            Contests
        };

        public static TaskItem Insta = new TaskItem
        {
            Id = "1",
            UserId = null,
            Title = "InstaPic",
            TaskDetails = "Upload a picture to instagram"
        };

        public static TaskItem Facepic = new TaskItem
        {
            Id = "2",
            UserId = null,
            Title = "FacePic",
            TaskDetails = "Upload a picture to facebook"
        };

        public static TaskItem ForConTests = new TaskItem
        {
            Id = "3",
            UserId = "1",
            Title = "Drum Video",
            TaskDetails = "Upload a drumming video clip to instagram"
        };

        public static List<TaskItem> TestTasks = new List<TaskItem>()
        {
            Insta,
            Facepic,
            ForConTests
        };

        public static MessageData TestEmail = new MessageData
        {
            tags = new string[] { "Test" },
            sender = new SimpleUser { email = Emailtests.email, name = Emailtests.FirstName },
            to = new SimpleUser[]{ new SimpleUser { name = Contests.FirstName, email = Contests.email } },
            htmlContent = ForConTests.TaskDetails,
            textContent = ForConTests.TaskDetails,
            subject = ForConTests.Title,
            replyTo = new SimpleUser { name = Emailtests.FirstName, email = Emailtests.email }
        };

        public static MessageData TestSms = new MessageData
        {
            to = new SimpleUser[] { new SimpleUser { name = Emailtests.FirstName, email = Emailtests.Phone } },
            textContent = Insta.Title+": "+ Insta.TaskDetails,
        };


    }
}
