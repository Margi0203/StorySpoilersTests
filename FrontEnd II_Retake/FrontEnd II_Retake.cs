using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Text;

namespace FrontEnd_II_Retake
{
    public class Tests
    {
        private readonly static string BaseUrl = "https://d24hkho2ozf732.cloudfront.net";
        private WebDriver driver;
        public Actions actions;         

        [OneTimeSetUp]
        public void Setup()
        {
            //driver = new ChromeDriver();
            driver = new FirefoxDriver();
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            driver.Navigate().GoToUrl(BaseUrl);
            var loginButton = driver.FindElement(By.XPath("//a[contains(.,'Log In')]"));
            loginButton.Click();

            driver.FindElement(By.Id("username")).SendKeys("MargiG");
            driver.FindElement(By.Id("password")).SendKeys("0123456");
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            driver.Close();
            driver.Dispose();
        }

        [Test, Order(1)]
        public void CreateStorySpoilerWithInvalidDataTest()
        {
            driver.Navigate().GoToUrl(BaseUrl);
            var storySpoiler = driver.FindElement(By.XPath("//a[@class='nav-link'][contains(.,'Create Spoiler')]"));
            storySpoiler.Click();

            var storyTitleInput = driver.FindElement(By.XPath("//input[@id='title']"));
            storyTitleInput.Click();
            storyTitleInput.SendKeys(" ");

            var storyDescriptionInput = driver.FindElement(By.XPath("//input[@id='description']"));
            storyDescriptionInput.Click();
            storyDescriptionInput.SendKeys(" ");

            var storyPictureInput = driver.FindElement(By.XPath("//input[@id='url']"));
            storyPictureInput.Click();
            storyPictureInput.SendKeys(" ");

            var storyCreateBtn = driver.FindElement(By.XPath("//button[@type='submit']"));
            storyCreateBtn.Click();

            var storyErrorMessage = driver.FindElement(By.XPath("//div[@class='text-info validation-summary-errors']//li"));
            var titleErrorMessage = driver.FindElement(By.XPath("//span[@class='text-info field-validation-error'][1]"));
            var descriptionErrorMessage = driver.FindElement(By.XPath("//span[@data-valmsg-for='Description']"));

            var currentUrl = driver.Url;
            Assert.That(currentUrl, Is.EqualTo($"{BaseUrl}/Story/Add"), "User should remain on Create Story Spoiler page with the same URL");

            Assert.That(storyErrorMessage.Text, Is.EqualTo("Unable to add this spoiler!"), "Error message for invalid created story is not there");
            Assert.That(titleErrorMessage.Text, Is.EqualTo("The Title field is required."), "Error message for invalid created title is not there");
            Assert.That(descriptionErrorMessage.Text, Is.EqualTo("The Description field is required."), "Error message for invalid created description is not there");

        }

        [Test, Order(2)]
        public void CreateRandomStorySpoilerTest()
        {
            driver.Navigate().GoToUrl(BaseUrl);
            var storySpoiler = driver.FindElement(By.XPath("//a[@class='nav-link'][contains(.,'Create Spoiler')]"));
            storySpoiler.Click();

            var storyTitleInput = driver.FindElement(By.XPath("//input[@id='title']"));
            storyTitleInput.Clear();
            var lastCreatedStoryTitle = GenerateRandomString(5);
            storyTitleInput.SendKeys(lastCreatedStoryTitle);

            var storyDescription = driver.FindElement(By.XPath("//input[@id='description']"));
            storyDescription.Clear();
            var lastCreatedStoryDescription = GenerateRandomString(10);
            storyDescription.SendKeys(lastCreatedStoryDescription);

            var storyCreateBtn = driver.FindElement(By.XPath("//button[@type='submit']"));
            storyCreateBtn.Click();

            var currentUrl = driver.Url;
            Assert.That(currentUrl, Is.EqualTo($"{BaseUrl}/"), "User should remain on Create Story Spoiler page with the same URL");

            var storySpoilers = driver.FindElements(By.XPath("//div[@class='p-5']"));
            var lastStoryTitle = storySpoilers.Last().FindElement(By.CssSelector("h2.display-4")).Text;
            var lastStoryDescription = storySpoilers.Last().FindElement(By.CssSelector("p.flex-lg-wrap")).Text;


            Assert.That(lastStoryTitle, Is.EqualTo(lastCreatedStoryTitle), "The last Story Title is not present on the screen.");
            Assert.That(lastStoryDescription, Is.EqualTo(lastCreatedStoryDescription), "The last Story Description is not present on the screen.");
        }

        [Test, Order(3)]
        public void EditLastCreatedStorySpoilerTitleTest()
        {
            driver.Navigate().GoToUrl(BaseUrl);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(100));

            var storySpoilers = driver.FindElements(By.CssSelector("div.p-5"));
            var lastStorySpoiler = wait.Until(driver =>
                driver.FindElement(By.XPath("//div[contains(@class, 'p-5')]")));
            lastStorySpoiler = storySpoilers.Last();            

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView(true);", lastStorySpoiler);

            var editButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector("a.btn.btn-info.btn-xl.rounded-pill.mt-5")));
            editButton.Click();            

            var storyTitleInput = driver.FindElement(By.XPath("//input[@class='form-control active' and @placeholder='Please enter your story title']"));
            storyTitleInput.Clear();
            storyTitleInput.SendKeys(GenerateRandomString(5) + " Updated");

            driver.FindElement(By.XPath("//button[@class='btn btn-info btn-block fa-lg gradient-custom-2 mb-3']")).Click();

            var currentUrl = driver.Url;
            Assert.That(currentUrl, Is.EqualTo($"{BaseUrl}/"), "User should be redirected to the home page after editing the story.");
        }

        [Test, Order(4)]
        public void DeleteLastCreatedStorySpoilerTest()
        {
            driver.Navigate().GoToUrl(BaseUrl);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
         
            var storySpoilers = driver.FindElements(By.CssSelector("div.p-5"));
            var lastStorySpoiler = wait.Until(driver =>
                driver.FindElement(By.XPath("(//div[contains(@class, 'p-5')])[last()]")));
            lastStorySpoiler = storySpoilers.Last();
            
            var lastCreatedStory = storySpoilers.LastOrDefault();
            Assert.That(lastCreatedStory, Is.Not.Null, "Last created story is null");
            Assert.That(lastCreatedStory.Displayed, Is.True, "Last created story is not displayed");            

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView(true);", lastCreatedStory);
          
            var deleteButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("(//a[contains(.,'Delete')][last()])"))); 
            deleteButton.Click();

            var currentUrl = driver.Url;
            Assert.That(currentUrl, Is.EqualTo($"{BaseUrl}/"), "User was not redirected to My StorySpoil Page.");

            var storySpoilersResult = driver.FindElements(By.CssSelector(".row.gx-5.align-items-center"));
            Assert.That(storySpoilersResult.Count, Is.LessThan(storySpoilers.Count), "The number of Spoilers did not decrease");
        }

        [Test, Order(5)]
        public void TrytoEditNonExistentStorySpoilerTest()
        {
            driver.Navigate().GoToUrl(BaseUrl);
            actions = new Actions(driver);
     
            var searchField = driver.FindElement(By.XPath("//input[contains(@type,'search')]"));
            searchField.Click();
            //var searchField = driver.FindElement(By.Id("editing-view-port"));            
            //actions.ScrollToElement(searchField).Perform();
            searchField.SendKeys("story Id = 23955fd-20da-4319-8aaa-08dcc82be2da");            
            driver.FindElement(By.XPath("//a[@class='btn btn-info rounded-pill mt-5 col-2']")).Click();

            var currentUrl = driver.Url;
            Assert.That(currentUrl, Is.EqualTo($"{BaseUrl}/"), "User was not redirected to My StorySpoil Page.");    
        }

        [Test, Order(6)]
        public void TryToDeleteNonExistentStorySpoilerTest()
        {
            driver.Navigate().GoToUrl(BaseUrl);
            actions = new Actions(driver);

            var searchField = driver.FindElement(By.XPath("//input[contains(@type,'search')]"));
            searchField.Click();
            //var searchField = driver.FindElement(By.Id("editing-view-port"));            
            //actions.ScrollToElement(searchField).Perform();
            searchField.SendKeys("story Id = 123456789");
            driver.FindElement(By.XPath("//a[@class='btn btn-info rounded-pill mt-5 col-2']")).Click();

            var currentUrl = driver.Url;
            Assert.That(currentUrl, Is.EqualTo($"{BaseUrl}/"), "No such spoiler.");
        }

        public static string GenerateRandomString(int length)
        {
            char[] chars ="abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
            if (length <= 0)
            {
                throw new ArgumentException("Length must be greater than zero.", nameof(length));
            }

            var random = new Random();
            var result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }
            return result.ToString();
        }       
    }
}