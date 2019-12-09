using System;
using Xunit;
using M32COM_CW.Models;

namespace M32COM_CW_TESTS
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            ApplicationUser applicationUser = new ApplicationUser();
            Member hi = new Member(applicationUser, "Calvin", "Lam");

            Assert.Equal("Calvin", hi.Forename);
        }
        [Fact]
        public void Test2()
        {
            Venue venue = new Venue();
            venue.Town = "Coventry";
            venue.Id = 2;
            Assert.Equal("Coventry", venue.Town);
            Assert.Equal(2, venue.Id);
        }
        [Fact]
        public void Test3()
        {
            Team team = new Team();
            team.Name = "Alpha";
            Assert.Equal("Alpha", team.Name);
        }
        [Fact]
        public void Test4()
        {
            Event unit = new Event();
            unit.Name = "Big thing";
            Assert.Equal("Big thing", unit.Name);
        }
        [Fact]
        public void Test5()
        {
            Boat boat = new Boat();
            boat.Name = "Black Pearl";
            Assert.Equal("Black Pearl", boat.Name);
        }
        [Fact]
        public void Test6()
        {
            Entry entry = new Entry();
            entry.ID = 2;
            Assert.Equal(2, entry.ID);
        }
    }
}
