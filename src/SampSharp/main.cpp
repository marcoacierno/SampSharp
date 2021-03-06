// SampSharp
// Copyright 2015 Tim Potze
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#include "main.h"
#include <fstream>
#include <sampgdk/sampgdk.h>
#include "Config.h"
#include "MonoRuntime.h"
#include "unicode.h"
#include "monohelper.h"
#include "GameMode.h"
#include <iostream>

extern void *pAMXFunctions;

using std::string;
using std::stringstream;
using sampgdk::logprintf;

bool filterscript_loaded = false;

void convertSymbols() {
    string symbols = Config::GetSymbolFiles();

    if (symbols.length() > 0) {
        logprintf("Symbol file generation");
        logprintf("----------------------");

        stringstream symbols_stream(symbols);
        string file;
        int successes = 0;
        while (std::getline(symbols_stream, file, ' ')) {
            logprintf("Converting: %s", file.c_str());

            std::string path = PathUtil::GetGameModeDirectory().append(file);

            std::ifstream ifile(path.c_str());
            if (file.empty() || !ifile) {
                logprintf("  Failed.");
                continue;
            }
            mono_convert_symbols(path.c_str());

            successes++;
            logprintf("  Converted.");
        }
        logprintf(" Converted %d files.", successes);
        logprintf("");
    }
}

void loadGamemode() {
    if (GameMode::IsLoaded()) return;

    /* Load empty filterscript */
    if (!filterscript_loaded) {
        SendRconCommand("loadfs empty");
        filterscript_loaded = true;
    }

    /* Load mono */
    if (!MonoRuntime::IsLoaded()) {
        MonoRuntime::Load(Config::GetMonoAssemblyDir(),
            Config::GetMonoConfigDir(), Config::GetTraceLevel(),
            PathUtil::GetPathInBin("gamemode/")
            .append(Config::GetGameModeNameSpace()).append(".dll"));
    }

    /* Set initial codepage to the configured one. */
    int codepage = Config::GetCodepage();
    set_codepage(codepage);

    convertSymbols();

    /* Load game mode */
    string namespaceName = Config::GetGameModeNameSpace();
    string className = Config::GetGameModeClass();

    logprintf("Gamemode");
    logprintf("---------------");
    logprintf("Loading gamemode: %s:%s", namespaceName.c_str(),
        className.c_str());

    if (GameMode::Load(Config::GetGameModeNameSpace(),
        Config::GetGameModeClass()))
        logprintf("  Loaded.");
    else
        logprintf("  Failed.");

    logprintf("");
}

void unloadGamemode() {
    if (!GameMode::IsLoaded()) return;

    string namespase = Config::GetGameModeNameSpace();
    string klass = Config::GetGameModeClass();

    logprintf("");
    logprintf("---------------");
    logprintf("Unloading gamemode: %s:%s", namespase.c_str(), klass.c_str());

    GameMode::Unload();

    logprintf("  Unloaded.");
    logprintf("");
}

PLUGIN_EXPORT unsigned int PLUGIN_CALL Supports() {
    return sampgdk::Supports() | SUPPORTS_PROCESS_TICK;
}

PLUGIN_EXPORT bool PLUGIN_CALL Load(void **ppData) {
    if (!sampgdk::Load(ppData)) return false;

    pAMXFunctions = ppData[PLUGIN_DATA_AMX_EXPORTS];

    logprintf("");
    logprintf("SampSharp Plugin");
    logprintf("----------------");
    logprintf("v%s, (C)2014-2015 Tim Potze", PLUGIN_VERSION);
    logprintf("");

    Config::Read();
    return true;
}

PLUGIN_EXPORT void PLUGIN_CALL Unload() {
    GameMode::Unload();
    MonoRuntime::Unload();
    sampgdk::Unload();
}

PLUGIN_EXPORT void PLUGIN_CALL ProcessTick() {
    GameMode::ProcessTick();
    sampgdk::ProcessTick();
}

PLUGIN_EXPORT bool PLUGIN_CALL OnPublicCall(AMX *amx, const char *name,
    cell *params, cell *retval) {
    if (!strcmp(name, "OnGameModeInit")) {
        loadGamemode();
    }
    else if (GameMode::IsLoaded() && !strcmp(name, "OnGameModeExit")) {
        /* Process call before actually unloading the gamemode. */
        GameMode::ProcessPublicCall(amx, name, params, retval);

        unloadGamemode();
    }

    if (GameMode::IsLoaded())
        GameMode::ProcessPublicCall(amx, name, params, retval);

    return true;
}
