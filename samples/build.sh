#!/bin/bash

mysql < create_devel.sql

xbuild People.sln

nunit-console4 ./People.Test/bin/Debug/People.Test.dll
