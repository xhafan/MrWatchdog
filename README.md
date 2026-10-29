# Mr Watchdog

A watchdog for any publicly accessible web page.

Keep track of changes and get notified by email the moment a page updates or your search term appears.

View public scrapers or create your own scraper, and create a watchdog.

Check out the live production version: [mrwatchdog.com](https://mrwatchdog.com)

## Running Locally
To run the solution locally, you need to setup user secrets first. See <a href="src/user_secrets_init.bat.example">user_secrets_init.bat.example</a>.

## Hosting
To host the app, see [Hosting-server-installation](src/Install/Hosting-server-installation.md).

## Deployment
To deploy the app to the hosting server, see [Deployment](src/Install/Deployment.md).

## Tests
Run `./run-tests.sh --dirty` from `src` under WSL. It builds the test image,
starts fresh PostgreSQL and RabbitMQ services, runs the full .NET suite and
TypeScript/Jest verification, then removes the test containers and volumes.

All five NUnit test assemblies link the configuration in
`src/Libraries/CoreBackend/CoreBackend.TestsShared/TestConfig.cs`: six workers,
parallel test cases and a separate fixture instance for each case. Keep mutable
fixture state and database transactions local to each case. Set
`MRWATCHDOG_VERBOSE_TESTS=1` when running the script to enable verbose E2E host
logging; warnings and errors are enabled by default.

Browser scraping tests retain their real external-site requests. They use the
same browser manager as production: a lazily started Chromium process with a
fresh, disposable browser context for every scrape. The application owns the
manager; queue containers borrow it. A disconnected browser is replaced on the
next scrape. `PlaywrightScraper:MaximumConcurrentScrapes` (environment variable
`PlaywrightScraper__MaximumConcurrentScrapes`) defaults to four concurrent
contexts, independently of the NUnit worker count. Shutdown drains active
contexts before closing the browser and Playwright driver.
