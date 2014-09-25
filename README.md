BLocal
======

A better way of Internationalization/Globalization/Localization for .NET

# Installing the BLocal manager locally

1. checkout this git repository
2. configuration
	* find the config.yml.template in the root
	* copy as config.yml, and fill in
3. manager configuration
	* find the web.config.template in the BLocal.Web.Manager.MVC3 directory
	* copy as web.config, and fill in
4. build it
	* open a bash shell in the root folder of the repository
	* rake -f build.rake manager:build
