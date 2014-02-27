Orchard.Gallery is an add-on module for the Orchard Project (http://orchard.codeplex.com); it is not a stand-alone application. Orchard must be installed in order to use Orchard.Gallery.

Orchard
==========================
Download the source or clone the repository from https://hg01.codeplex.com/orchard.
Run ClickToBuild.cmd in the root folder.
Orchard.Gallery does not currently work under medium trust. The web.config in Orchard\src\Orchard.Web needs to be edited to run under full trust. 
	<trust level="Full" originUrl="" />

The Orchard.Gallery module is currently being developed against the 1.x branch of the Orchard repository, changeset number: 4406 (7caba1cd1dcd).


Orchard.Gallery
==========================
Download the source or clone the repository from https://hg01.codeplex.com/orchardgallery to Orchard\src\Orchard.Web\Modules\Orchard.Gallery.
Orchard.Gallery has a dependency on Contrib.Profile, Contrib.Taxonomies, Contrib.Voting and Contrib.Reviews.
Download the source or clone these repositories:
	https://hg01.codeplex.com/orchardprofile to Orchard\src\Orchard.Web\Modules\Contrib.Profile.
	https://hg01.codeplex.com/orchardtaxonomies to Orchard\src\Orchard.Web\Modules\Contrib.Taxonomies.
	https://hg01/codeplex.com/orchardvoting to Orchard\src\Orchard.Web\Modules\Contrib.Voting
	https://bitbucket.org/nimblepros/orchard-reviews to Orchard\src\Orchard.Web\Modules\Contrib.Reviews

Add the Orchard.Gallery and four Contrib projects to the Modules folder in the Orchard solution.
Alternatively, these four modules can all be installed directly from the Gallery in the Orchard admin dashboard.

Run Orchard. 
On the Get Started page, enter a site name and set the admin password.
Navigate to the Dashboard and click on Features in the sidebar of the Dashboard. 
Under Packaging, enable the Orchard.Gallery feature (this will also enable its dependencies).

To enable email functionality: Make sure the Email feature is enabled. Go to Configuration/Settings, configure SMTP, and turn on "Users must verify their email address" under "Users registration".


Orchard.Gallery Settings
==========================
There are several settings you can configure for your instance of the gallery. Got to the Configuration/Settings page in the Orchard admin dashboard. Under Gallery Settings there are two URL's to configure for your instance of Gallery Server (see http://galleryserver.codeplex.com for more information). You can also configure the settings related to package ID expiration if you want to limit the number and length of time that users can hold onto pre-registered package IDs without using them.


Orchard Gallery Theme
==========================
To add the OrchardGallery theme, clone http://bitbucket.org/kevinkuebler/orchard-gallery-theme to Orchard.Web/Themes/OrchardGallery (the folder name of "OrchardGallery" is important). Navigate to the Dashboard and click Themes in the left-hand menu. Under Available Themes you should see Orchard Gallery. Click the Set Current button under the Orchard Gallery theme to enable it.

The Orchard Gallery theme can also be added as an existing project (under the Themes solution folder) to the Orchard solution in Visual Studio if you wish to view or modify the code.


Add Package to Gallery
==========================
In your Orchard site, click on the Contribute tab.
You can either upload a package file from your local computer or submit a package using a URL to an externally hosted package. Choose one option and specify the package file.
After submitting the package you will be taken to the Package edit screen. The details of the package can be modified if desired. 
Note: At this point the package has not been published. The Update button must be clicked to publish the package to the public feed.
After publishing the package, you will be taken to the tab for the published package type. A message will indicate that the package was updated successfully, but that it will take a moment before the package appears in Orchard. After a minute, refresh the page and the package should be 
visible.
