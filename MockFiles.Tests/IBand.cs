using System.Collections.Generic;

namespace MockFiles.Tests
{
    public interface IBand
    {
        List<Member> GetMembers();
        List<Member> GetMembersByStatus(bool isActive);
    }
}