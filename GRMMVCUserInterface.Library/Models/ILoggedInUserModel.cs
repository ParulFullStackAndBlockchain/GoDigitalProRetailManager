using System;

namespace GRMMVCUserInterface.Library.Models
{
    public interface ILoggedInUserModel
    {
        DateTime CeatedDate { get; set; }
        string EmailAddress { get; set; }
        string FirstName { get; set; }
        string Id { get; set; }
        string LastName { get; set; }
        string Token { get; set; }
    }
}