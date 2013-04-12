Baseclass.Contrib.SpecFlow.Selenium.NUnit
===============================

Test class generator to drive automated web ui tests with Selenium and SpecFlow.

Configures SpecFlow to be able to easily use Selenium for WebTesting.

Creating automated web tests to test an application in addition to testing the application with unit tests is a good practice. SpecFlow supports behavior driven development and acceptance tests driven development.

This project was created to be able to use Selenium with SpecFlow as easy as possible, and at the same time to be able to use it in an Continuos Integration Environment.

###Features:

 
- Sets SpecFlow tests up to run and teardown a Selenium WebDriver 
    - The Browser is a tool which doesn't belong to your application, why write code to instantiate it?
    
- Use App.config to configure WebDriver
    -  Use different configuration depending the environment where you run the tests on. Example: Use OpenQA.Selenium.IE.InternetExplorerDriver on developer machine but RemoteWebDriver on CI Environment-
    
- Annotate scenarios with scenario supporting Browsers
    -  Does every scenario describing the acceptance tests for every browser? Mark the scenario with the supported Browser.

- Adds the browser name as TestCategory
    - Just run the tests with the categories for the browser you actually have on the environment. Example: Don't run Android browser test as I don't have an android device attached on my machine.


###Before

``Scenario Outline:`` Add Two Numbers
    >``Given`` I navigated to **/** using ``<browser>``
    ``And`` I have entered ``<summandOne>`` into **summandOne** calculator
	``And`` I have entered ``<summandTwo>`` into **summandTwo** calculator
	``When`` I press add
	``Then`` the result should be ``<result>`` on the screen
	Scenarios: 
		| ``browser``| ``summandOne``| ``summandTwo``|``result``|
		| Chrome   | 10   | 20   | 30   |
		| Firefox  | 10  | 20  | 30  |
		| IE   | 10   | 20   | 30   |
		| Chrome       | 3       | 4       | 7       |
		| Firefox       | 3       | 4       | 7       |
		| IE       | 3       | 4       | 7       |

###After

``@Browser:IE``
``@Browser:Chrome``
``@Browser:Firefox``
``Scenario Outline:`` Add Two Numbers
    >``Given`` I navigated to **/** using
    ``And`` I have entered ``<summandOne>`` into **summandOne** calculator
	``And`` I have entered ``<summandTwo>`` into **summandTwo** calculator
	``When`` I press add
	``Then`` the result should be ``<result>`` on the screen
	Scenarios: 
		| ``summandOne``| ``summandTwo``|``result``|
		| 10   | 20   | 30   |
		| 3       | 4       | 7       |

_______________
Using SpecFlow with Selenium without Baseclass.Contrib.SpecFlow.Selenium.NUnit
----
As I've started with the first test scenario using Selenium i've end up with something like this:

``Scenario:`` Add Two Numbers
    >``Given`` I navigated to **/**
	``And`` I have entered **10** into **summandOne** calculator
	``And`` I have entered **20** into **summandTwo** calculator
	``When`` I press add
	``Then`` the result should be **30** on the screen

Pretty easy to test a simple web page containing a calculator.

Then I decided to take it further and specifiy on what browser I want to support this scenario:

``Scenario:`` Add Two Numbers
    >``Given`` I navigated to **/** using **Chrome**
    ``And`` I have entered **10** into **summandOne** calculator
	``And`` I have entered **20** into **summandTwo** calculator
	``When`` I press add
	``Then`` the result should be **30** on the screen

The first test step is easy to write. Use an IOC-Container to resolve the configured IWebDriver with the specified browser name.

``this.driver = this.container.ResolveNamed<OpenQA.Selenium.IWebDriver>(browser)``

So now I want the same test to run in InternetExplorer, instead of multiplying the scenario I use gherkin's Scenario Outline feature:

``Scenario Outline:`` Add Two Numbers
    >``Given`` I navigated to **/** using ``<browser>``
	``And`` I have entered **10** into **summandOne** calculator
	``And`` I have entered **20** into **summandTwo** calculator
	``When`` I press add
	``Then`` the result should be **30** on the screen
	Scenarios: 
	| ``*browser*``| 
	| Chrome  | 
	| IE       | 
	| Firefox  | 

Do you see the combinatory explosion coming? What if I actually want to use the scenario outline for what it was supposed to? Like specifying the summands it the scenarios:

``Scenario Outline:`` Add Two Numbers
    >``Given`` I navigated to **/** using ``<browser>``
	``And`` I have entered ``<summandOne>`` into **summandOne** calculator
	``And`` I have entered ``<summandTwo>`` into **summandTwo** calculator
	``When`` I press add
	``Then`` the result should be ``<result>`` on the screen
	Scenarios: 
		| ``browser``| ``summandOne``| ``summandTwo``|``result``|
		| Chrome   | 10   | 20   | 30   |
		| Firefox  | 10  | 20  | 30  |
		| IE   | 10   | 20   | 30   |
		| Chrome       | 3       | 4       | 7       |
		| Firefox       | 3       | 4       | 7       |
		| IE       | 3       | 4       | 7       |

and so on.

Wouldn't it be nice to write the scenario like that:

``@Browser:IE``
``@Browser:Chrome``
``@Browser:Firefox``
``Scenario Outline:`` Add Two Numbers
    >``Given`` I navigated to **/** using
	``And`` I have entered ``<summandOne>`` into **summandOne** calculator
	``And`` I have entered ``<summandTwo>`` into **summandTwo** calculator
	``When`` I press add
	``Then`` the result should be ``<result>`` on the screen
	Scenarios: 
		| ``summandOne``| ``summandTwo``|``result``|
		| 10   | 20   | 30   |
		| 3       | 4       | 7       |