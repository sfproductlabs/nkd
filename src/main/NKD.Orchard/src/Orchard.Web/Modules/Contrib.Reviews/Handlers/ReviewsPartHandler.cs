using Contrib.Reviews.Models;
using Contrib.Reviews.Settings;
using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;

namespace Contrib.Reviews.Handlers
{
    [UsedImplicitly]
    public class ReviewsPartHandler : ContentHandler {
        public ReviewsPartHandler() {
            OnInitializing<ReviewsPart>((context, part) => {
                part.ShowStars = part.Settings.GetModel<ReviewTypePartSettings>().ShowStars;
            });
        }
    }
}