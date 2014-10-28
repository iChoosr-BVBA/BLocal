BLocal
======

A better way of Internationalization/Globalization/Localization for .NET

# Installing the BLocal manager locally

1. checkout this git repository
2. make sure you have installed the rigth tooling
	* ruby
	* rake
	* ruby gems
	* git hooks (https://github.com/icefox/git-hooks)
	* mvc 3: http://www.asp.net/mvc/mvc3
3. run "git hooks init"
4. set up the manager
	* create a folder for the website to be hosted in
	* make sure that the user executing git hooks (usually your current user) has read/write access to this folder
	* configure the website in IIS
		* make sure ASP.NET MVC 3 is installed
		* run in .NET v4.x
5. configure the manager
	* find the web.config.template in the BLocal.Web.Manager.MVC3 directory
	* copy as web.config to the folder where the website is hosted, and customize it
6. configure the build
	* find the config.yml.template in the root
	* copy as config.yml, and fill in	
7. build it
	* open a bash shell in the root folder of the repository
	* rake -f build.rake manager:build
