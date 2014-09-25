BLocal
======

A better way of Internationalization/Globalization/Localization for .NET

# Installing the BLocal manager locally

1. checkout this git repository
2. set up the manager
	* create a folder for the website to be hosted in
	* configure the website in IIS
		* make sure ASP.NET MVC 3 is installed
		* run in .NET v4.x
3. configure the manager
	* find the web.config.template in the BLocal.Web.Manager.MVC3 directory
	* copy as web.config to the folder where the website is hosted, and customize it
4. configure the build
	* find the config.yml.template in the root
	* copy as config.yml, and fill in	
5. build it
	* open a bash shell in the root folder of the repository
	* rake -f build.rake manager:build
