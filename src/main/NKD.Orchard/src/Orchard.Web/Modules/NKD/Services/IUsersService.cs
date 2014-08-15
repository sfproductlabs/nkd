using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Orchard;
using NKD.Module.BusinessObjects;
using NKD.Models;
using System.ServiceModel;
using NKD.ViewModels;
using Orchard.Security;

namespace NKD.Services
{
     [ServiceContract]
    public interface IUsersService : IDependency 
    {
         [OperationContract]
         void SyncUsers();

         [OperationContract]
         IEnumerable<Contact> GetContacts();

         [OperationContract]
         Dictionary<Guid, string> GetCompanies();

         [OperationContract]
         Dictionary<Guid, string> GetRoles();

         [OperationContract]
         Guid? GetContactID(string username);

         [OperationContract]
         Guid? GetDefaultCompanyID(Guid? contactID);

         [OperationContract]
         Contact GetContact(string username);

         [OperationContract]
         ContactViewModel GetMyInfo();

         [OperationContract]
         Guid? GetEmailContactID(string email, bool validated = true);

         [OperationContract]
         void GetCreator(Guid? contact, Guid? company, out Guid? creatorContact, out Guid? creatorCompany);

         [OperationContract]
         string[] GetUserEmails(Guid[] users);

         [OperationContract]
         bool UpdateUserEmail(string email);

         [OperationContract]
         void EmailUsers(string[] recipients, string subject, string body, bool retry = false, bool forwardToSupport = false, string from = null, string fromName = null, bool hideRecipients = false);

         [OperationContract]
         void EmailUsersAsync(string[] recipients, string subject, string body, bool retry = false, bool forwardToSupport = false, string from = null, string fromName = null, bool hideRecipients = false);

         [OperationContract]
         void UpdateSecurity(ISecured security);

         [OperationContract]
         void WarnAdmins(IEnumerable<string> warnings);


         [OperationContract]
         void DeleteSecurity(ISecured security);

         string ApplicationConnectionString
         {
             [OperationContract]
             get;
         }

         List<SecurityWhitelist> AuthorisedList
         {
             [OperationContract]
             get;
         }

         bool CheckPermission(ISecured secured, ActionPermission permission);

         bool CheckOwnership(ISecured secured, ActionPermission permission);

         string Username
         {
             [OperationContract]
             get;
         }
         
         string Email
         {
             [OperationContract]
             get;
         }

         Contact Contact
         {
             [OperationContract]
             get;
         }

         Guid? ContactID
         {
             [OperationContract]
             get;
         }

         Guid ApplicationID
         {
             [OperationContract]
             get;
         }

         Guid ApplicationCompanyID
         {
             [OperationContract]
             get;
         }

         Guid DefaultContactCompanyID
         {
             [OperationContract]
             get;
         }

         bool DefaultPublicDomain
         {
             [OperationContract]
             get;
         }

         Guid?[] ContactCompanies
         {
             [OperationContract]
             get;
         }


         Guid ServerID
         {
             [OperationContract]
             get;
         }

         bool HasPrivateCompanyID
         {
             [OperationContract]
             get;
         }

         IUser Create(string email, string username, string password);

         bool VerifyUserUnicity(string userName, string email);

    }
}