using StatisGoat.WebApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StatisGoat.Data
{
    public class Games_DB_Initializer
    {
        public static void Initialize(Games_DB context)
        {
            // Ensures the database has been created
            context.Database.EnsureCreated();

            // Look for any games.
            if (context.Games.Any())
            {
                    return;   // DB has been seeded
            }
            
            // If we want to seed the database with fake games, this is where that will go.
            // Some code is taken from the Web Software Arch. class to demonstrate how to make/add games to the database
            
            //// Seeds applications list with many different applicants
            //var applications = new Application[]
            //{
            //new Application{FirstName="u0",LastName="u0",uID="u0000000",PhoneNumber="996-315-5632",Address="123 Joy St. Salt Lake City, Utah 812345",
            //CurrentDegree="Computer Science",CurrentProgram="BS", GPA=4.0, NumberHours=12,PersonalStatement="I'm u0000000.",
            //fluency = Application.EnglishFluency.Native, SemestersCompleted=4, LinkedInURL="www.linkedin.com", ResumeFile="blank",
            //CreationDate=DateTime.Parse("2005-09-01"), ModificationDate=DateTime.Parse("2021-09-20"), UserID="21467292d-f32d-4abc-8359-cbbd7e31236e"},

            //new Application{FirstName="u1",LastName="u1",uID="u0000001",PhoneNumber="996-315-5633",Address="123 Joy St. Salt Lake City, Utah 812345",
            //CurrentDegree="Computer Science",CurrentProgram="BS", GPA=4.0, NumberHours=12,PersonalStatement="Hey there.",
            //fluency = Application.EnglishFluency.Native, SemestersCompleted=4, LinkedInURL="www.linkedin.com", ResumeFile="blank",
            //CreationDate=DateTime.Parse("2005-09-01"), ModificationDate=DateTime.Parse("2021-09-20"), UserID="6736292d-f32d-4abc-8359-cbbd7e31236e"},

            //new Application{FirstName="u2",LastName="u2",uID="u0000002",PhoneNumber="996-315-5633",Address="123 Joy St. Salt Lake City, Utah 812345",
            //CurrentDegree="Computer Science",CurrentProgram="BS", GPA=4.0, NumberHours=12,PersonalStatement="Hey there.",
            //fluency = Application.EnglishFluency.Native, SemestersCompleted=4, LinkedInURL="www.linkedin.com", ResumeFile="blank",
            //CreationDate=DateTime.Parse("2005-09-01"), ModificationDate=DateTime.Parse("2021-09-20"), UserID="a01dea9c-4c5b-40e4-8928-36e017ade511"},

            //// Add each seeded applicant to the database table
            //foreach (Application s in applications)
            //{
            //    context.Applications.Add(s);
            //}

            context.SaveChanges();
        }
    }
}
