#!/usr/bin/env bash

echo $(bash --version 2>&1 | head -n 1)

set -eo pipefail
SCRIPT_DIR=$(cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd)

###########################################################################
# CONFIGURATION
###########################################################################

BUILD_PROJECT_FILE="$SCRIPT_DIR/../build/_build.csproj"
TEMP_DIRECTORY="$SCRIPT_DIR/../.tmp"

DOTNET_GLOBAL_FILE="$SCRIPT_DIR/../global.json"
DOTNET_INSTALL_URL="https://raw.githubusercontent.com/dotnet/cli/master/scripts/obtain/dotnet-install.sh"
DOTNET_CHANNEL="Current"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

###########################################################################
# EXECUTION
###########################################################################

function FirstJsonValue {
    perl -nle 'print $1 if m{"'$1'": "([^"]+)",?}' <<< ${@:2}
}

# If global.json exists, load expected version
if [[ -f "$DOTNET_GLOBAL_FILE" ]]; then
    DOTNET_VERSION=$(FirstJsonValue "version" $(cat "$DOTNET_GLOBAL_FILE"))
    if [[ "$DOTNET_VERSION" == ""  ]]; then
        unset DOTNET_VERSION
    fi
fi

# If dotnet is installed locally, and expected version is not set or installation matches the expected version
if [[ -x "$(command -v dotnet)" && (-z ${DOTNET_VERSION+x} || $(dotnet --version 2>&1) == "$DOTNET_VERSION") ]]; then
    export DOTNET_EXE="$(command -v dotnet)"
else
    echo "Please install dotnet"
fi

echo "Microsoft (R) .NET Core SDK version $("$DOTNET_EXE" --version)"

"$DOTNET_EXE" build "$BUILD_PROJECT_FILE" /nodeReuse:false -nologo -clp:NoSummary --verbosity quiet
"$DOTNET_EXE" run --project "$BUILD_PROJECT_FILE" --no-build -- "$@"
