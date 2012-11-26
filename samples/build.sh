#!/bin/bash

mysql < create_devel.sql

xbuild People.sln

rm -rf /var/www/html/people/*
cp -R People/* /var/www/html/people/

apachectl restart

nunit-console4 ./People.Test/bin/Debug/People.Test.dll
