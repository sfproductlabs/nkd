using System;
using Orchard.Gallery.Interfaces;
using Orchard.Users.Events;
using Orchard.Security;

namespace Orchard.Gallery.Handlers {
    public class UserEventHandler : IUserEventHandler {
        private readonly IUserkeyService _userkeyService;

        public UserEventHandler(IUserkeyService userkeyService) {
            _userkeyService = userkeyService;
        }

        public void Creating(UserContext context) {
            //Not handled
        }

        public void Created(UserContext context) {
            _userkeyService.SaveKeyForUser(context.User.Id, Guid.NewGuid());
        }

        //TODO:Check
        public void LoggedIn(IUser user) { }

        public void LoggedOut(IUser user) { }

        public void AccessDenied(IUser user) { }

        public void ChangedPassword(IUser user) { }

        public void SentChallengeEmail(IUser user) { }

        public void ConfirmedEmail(IUser user) { }

        public void Approved(IUser user) { }
    }
}