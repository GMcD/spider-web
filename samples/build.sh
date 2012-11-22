#!/bin/bash

mysql < create_devel.sql

xbuild People.sln

nunit-console4.exe People/People.Test/bin/People.Test.dll
