#!/bin/bash

xbuild People.sln

nunit-console4.exe People/People.Test/bin/People.Test.dll
