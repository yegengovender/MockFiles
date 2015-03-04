using System;
using System.Collections.Generic;
using System.Linq;

namespace MockFiles.Tests
{
    public class Band: IBand
    {
        private readonly List<Member> _students = new List<Member>
        {
            new Member {Id = 1, FirstName = "John", LastName = "Lennon", IsActive = true},
            new Member {Id = 1, FirstName = "Paul", LastName = "McCartney", IsActive = true},
            new Member {Id = 1, FirstName = "George", LastName = "Harrison", IsActive = true},
            new Member {Id = 1, FirstName = "Ringo", LastName = "Star", IsActive = true},
            new Member {Id = 1, FirstName = "Pete", LastName = "Best", IsActive = false}
        };
        public List<Member> GetMembers()
        {
            return _students;
        }

        public List<Member> GetMembersByStatus(bool isActive)
        {
            return _students.Where(s => s.IsActive == isActive).ToList();
        }
    }
}