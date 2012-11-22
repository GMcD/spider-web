#!/bin/bash

mysql < create_devel.sql

xbuild People.sln

echo "Add run tests..."
