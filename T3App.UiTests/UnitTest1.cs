using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using Xunit;

namespace T3App.UiTests
{
    public class UnitTest1 : IDisposable
    {
        private readonly IWebDriver driver;
        private readonly string appURL;
        public UnitTest1()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            driver = new ChromeDriver(options);
            appURL = Environment.GetEnvironmentVariable("TestUrl");
            if (string.IsNullOrEmpty(appURL)) appURL = "https://localhost:44376";
            Console.WriteLine($"appURL is: {appURL}");
        }

        [Fact]
        public void ShouldDisplayHelloWorld()
        {
            driver.Navigate().GoToUrl(appURL + "/");
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var webElement = wait.Until(c => c.FindElement(By.CssSelector("h1")));
            Assert.Equal("Hello, world!", driver.FindElements(By.CssSelector("h1"))[0].Text);
            driver.Quit();
        }

        [Fact]
        public void ShouldIncrementCount()
        {
            driver.Navigate().GoToUrl(appURL + "/counter");
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.ElementExists(By.CssSelector(".btn")));
            Assert.Equal("Current count: 0", driver.FindElements(By.CssSelector("p"))[0].Text);
            driver.FindElement(By.CssSelector(".btn")).Click();
            Assert.Equal("Current count: 1", driver.FindElements(By.CssSelector("p"))[0].Text);
            driver.Quit();
        }

        // Not always working correctly
        [Fact]
        public void ValidCredentialsShouldLogin()
        {
            driver.Navigate().GoToUrl(appURL + "/login");

            //var loginText = driver.FindElement(By.CssSelector("h1")).Text;

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            var webElement = wait.Until(c => c.FindElement(By.CssSelector("h1")));
            Assert.Equal("Login", driver.FindElements(By.CssSelector("h1"))[0].Text);

            wait.Until(c => c.FindElement(By.Id("email"))).SendKeys("admin");
            wait.Until(c => c.FindElement(By.Id("password"))).SendKeys("SecurePassword");
            //Assert.Equal("Current count: 0", driver.FindElements(By.CssSelector("p"))[0].Text);
            driver.FindElement(By.CssSelector(".btn")).Click();
            //Assert.Equal("Current count: 1", driver.FindElements(By.CssSelector("p"))[0].Text);

            Assert.Equal("T3App.Blazor", driver.Title);
            Assert.Contains("Hello, world!", driver.PageSource);

            driver.Quit();
        }

        // Not always working correctly
        [Fact]
        public void InvalidCredentialsShouldNotLogin()
        {
            driver.Navigate().GoToUrl(appURL + "/login");

            //var loginText = driver.FindElement(By.CssSelector("h1")).Text;

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            var webElement = wait.Until(c => c.FindElement(By.CssSelector("h1")));
            Assert.Equal("Login", driver.FindElements(By.CssSelector("h1"))[0].Text);

            wait.Until(c => c.FindElement(By.Id("email"))).SendKeys("admin");
            wait.Until(c => c.FindElement(By.Id("password"))).SendKeys("admin");
            //Assert.Equal("Current count: 0", driver.FindElements(By.CssSelector("p"))[0].Text);
            driver.FindElement(By.CssSelector(".btn")).Click();
            //Assert.Equal("Current count: 1", driver.FindElements(By.CssSelector("p"))[0].Text);

            Assert.Equal("T3App.Blazor", driver.Title);
            Assert.Contains("Error logging you in", driver.PageSource);

            driver.Quit();
        }


        public void Dispose()
        {
            driver.Quit();
            driver.Dispose();
        }
    }
}
