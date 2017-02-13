#!/usr/bin/env bash
set -eu

readonly DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

mono "$DIR/.paket/paket.exe" restore
mono "$DIR/packages/FAKE/tools/FAKE.exe" $@ "$DIR/build.fsx cwd=$DIR"
