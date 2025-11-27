create user mr_watchdog password 'Password01';
create database mr_watchdog;
GRANT ALL PRIVILEGES ON DATABASE mr_watchdog TO mr_watchdog;

create user mr_watchdog_test password 'mr_watchdog_test_password';
create database mr_watchdog_test;
GRANT ALL PRIVILEGES ON DATABASE mr_watchdog_test TO mr_watchdog_test;

-- run on DB mr_watchdog as superadmin
GRANT ALL PRIVILEGES ON SCHEMA public TO mr_watchdog;

-- run on DB mr_watchdog_test as superadmin
GRANT ALL PRIVILEGES ON SCHEMA public TO mr_watchdog_test;
