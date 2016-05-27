# webcomic-dl
A collection of C# tools for batch downloading pages off various webcomic content management systems and hosts.

## Supported CMS
- Webcomic https://<i></i>wordpress.org/plugins/webcomic/
- ComicPress https://<i></i>wordpress.org/themes/comicpress/
- ComicFury http://<i></i>comicfury.com
- ComicControl
- Inkblok https://<i></i>wordpress.org/themes/inkblot/
- SmackJeeves http://<i></i>smackjeeves.com
- TheDuckWebcomics http://<i></i>theduckwebcomics.com/
- Tapastic https://<i></i>tapastic.com/
- Plus a custom scraper that the user can supply an XPath to.

## Compiling information
The application has been developed using VS2015 with a target .NET framework of 4.5.2.
The project uses HTMLAglitiyPack for HTML parsing, make sure you have installed it via NuGet
