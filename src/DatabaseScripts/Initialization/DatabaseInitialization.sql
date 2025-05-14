create user mr_watchdog password 'Password01';
create database mr_watchdog;
GRANT ALL PRIVILEGES ON DATABASE mr_watchdog TO mr_watchdog;

create user mr_watchdog_tests password 'Password01Tests';
create database mr_watchdog_tests;
GRANT ALL PRIVILEGES ON DATABASE mr_watchdog_tests TO mr_watchdog_tests;

-- run on DB mr_watchdog as superadmin
GRANT ALL PRIVILEGES ON SCHEMA public TO mr_watchdog;

-- run on DB mr_watchdog_tests as superadmin
GRANT ALL PRIVILEGES ON SCHEMA public TO mr_watchdog_tests;
