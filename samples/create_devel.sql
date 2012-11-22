 
create database if not exists devel;

GRANT ALL PRIVILEGES  ON devel.* TO 'devel'@'localhost' IDENTIFIED BY 'devel' WITH GRANT OPTION;

use mysql;
select user, host from user where user = 'devel';
