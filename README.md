Baseclass.Contrib.SpecFlow.Selenium.NUnit
===============================

Test class generator to drive automated web ui tests with Selenium and SpecFlow.

Configures SpecFlow to be able to easily use Selenium for WebTesting.

Creating automated web tests to test an application in addition to testing the application with unit tests is a good practice. SpecFlow supports behavior driven development and acceptance tests driven development.

This project was created to be able to use Selenium with SpecFlow as easy as possible, and at the same time to be able to use it in a Continuos Integration Environment.

###Features:

 
- Sets SpecFlow tests up to run and teardown a Selenium WebDriver 
    - The Browser is a tool which doesn't belong to your application, why write code to instantiate it?
    
- Use App.config to configure WebDriver
    -  Use different configuration depending the environment where you run the tests on. Example: Use OpenQA.Selenium.IE.InternetExplorerDriver on developer machine but RemoteWebDriver on CI Environment-
    
- Annotate scenarios with scenario supporting Browsers
    -  Does every scenario describing the acceptance tests for every browser? Mark the scenario with the supported Browser.

- Adds the browser name as TestCategory
    - Just run the tests with the categories for the browser you actually have on the environment. Example: Don't run Android browser test as I don't have an android device attached to my machine.


Get it from Nuget.org:

https://www.nuget.org/packages/Baseclass.Contrib.SpecFlow.Selenium.NUnit/

###Before

``Scenario Outline:`` Add Two Numbers<br />
    >``Given`` I navigated to **/** using ``<browser>``<br />
    ``And`` I have entered ``<summandOne>`` into **summandOne** calculator<br />
	``And`` I have entered ``<summandTwo>`` into **summandTwo** calculator<br />
	``When`` I press add<br />
	``Then`` the result should be ``<result>`` on the screen<br />
	Scenarios: <br />
		| ``browser``| ``summandOne``| ``summandTwo``|``result``|<br />
		| Chrome   | 10   | 20   | 30   |<br />
		| Firefox  | 10  | 20  | 30  |<br />
		| IE   | 10   | 20   | 30   |<br />
		| Chrome       | 3       | 4       | 7       |<br />
		| Firefox       | 3       | 4       | 7       |<br />
		| IE       | 3       | 4       | 7       |<br />

###After

``@Browser:IE``<br />
``@Browser:Chrome``<br />
``@Browser:Firefox``<br />
``Scenario Outline:`` Add Two Numbers<br />
    >``Given`` I navigated to **/** using<br />
    ``And`` I have entered ``<summandOne>`` into **summandOne** calculator<br />
	``And`` I have entered ``<summandTwo>`` into **summandTwo** calculator<br />
	``When`` I press add<br />
	``Then`` the result should be ``<result>`` on the screen<br />
	Scenarios: <br />
		| ``summandOne``| ``summandTwo``|``result``|<br />
		| 10   | 20   | 30   |<br />
		| 3       | 4       | 7       |<br />
_______________

Blogpost: http://www.baseclass.ch/blog/Lists/Beitraege/Post.aspx?ID=4&mobile=0
_______________
Using SpecFlow with Selenium without Baseclass.Contrib.SpecFlow.Selenium.NUnit
----
As I've started with the first test scenario using Selenium i've end up with something like this:

``Scenario:`` Add Two Numbers<br />
    >``Given`` I navigated to **/**<br />
	``And`` I have entered **10** into **summandOne** calculator<br />
	``And`` I have entered **20** into **summandTwo** calculator<br />
	``When`` I press add<br />
	``Then`` the result should be **30** on the screen<br />

Pretty easy to test a simple web page containing a calculator.

Then I decided to take it further and specifiy on what browser I want to support this scenario:

``Scenario:`` Add Two Numbers<br />
    >``Given`` I navigated to **/** using **Chrome**<br />
    ``And`` I have entered **10** into **summandOne** calculator<br />
	``And`` I have entered **20** into **summandTwo** calculator<br />
	``When`` I press add<br />
	``Then`` the result should be **30** on the screen<br />

The first test step is easy to write. Use an IOC-Container to resolve the configured IWebDriver with the specified browser name.

``this.driver = this.container.ResolveNamed<OpenQA.Selenium.IWebDriver>(browser)``

So now I want the same test to run in InternetExplorer, instead of multiplying the scenario I use gherkin's Scenario Outline feature:

``Scenario Outline:`` Add Two Numbers
    >``Given`` I navigated to **/** using ``<browser>``<br />
	``And`` I have entered **10** into **summandOne** calculator<br />
	``And`` I have entered **20** into **summandTwo** calculator<br />
	``When`` I press add<br />
	``Then`` the result should be **30** on the screen<br />
	Scenarios: <br />
	| ``*browser*``| <br />
	| Chrome  | <br />
	| IE       | <br />
	| Firefox  | <br />

Do you see the combinatory explosion coming? What if I actually want to use the scenario outline for what it was supposed to? Like specifying the summands it the scenarios:

``Scenario Outline:`` Add Two Numbers<br />
    >``Given`` I navigated to **/** using ``<browser>``<br />
	``And`` I have entered ``<summandOne>`` into **summandOne** calculator<br />
	``And`` I have entered ``<summandTwo>`` into **summandTwo** calculator<br />
	``When`` I press add<br />
	``Then`` the result should be ``<result>`` on the screen<br />
	Scenarios: <br />
		| ``browser``| ``summandOne``| ``summandTwo``|``result``|<br />
		| Chrome   | 10   | 20   | 30   |<br />
		| Firefox  | 10  | 20  | 30  |<br />
		| IE   | 10   | 20   | 30   |<br />
		| Chrome       | 3       | 4       | 7       |<br />
		| Firefox       | 3       | 4       | 7       |<br />
		| IE       | 3       | 4       | 7       |<br />

and so on.

Wouldn't it be nice to write the scenario like that:

``@Browser:IE``<br />
``@Browser:Chrome``<br />
``@Browser:Firefox``<br />
``Scenario Outline:`` Add Two Numbers<br />
    >``Given`` I navigated to **/** using<br />
	``And`` I have entered ``<summandOne>`` into **summandOne** calculator<br />
	``And`` I have entered ``<summandTwo>`` into **summandTwo** calculator<br />
	``When`` I press add<br />
	``Then`` the result should be ``<result>`` on the screen<br />
	Scenarios: <br />
		| ``summandOne``| ``summandTwo``|``result``|<br />
		| 10   | 20   | 30   |<br />
		| 3       | 4       | 7       |<br />
		
____________________

###Bindings

Until now I've included one binding:

Regex: I navigated to (.*)<br />
Example Usage for navigating to Root:<br />
Given I navigated to /<br />
<br />
Logic:<br />
<br />
Looks for the seleniumBaseUrl in the App settings and navigates with to the concatonated url of {seleniumBaseUrl}{passedUrl}<br />

```xml
<appSettings>
    <add key="seleniumBaseUrl" value="http://localhost:58909" />
</appSettings>
```