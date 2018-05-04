#!/usr/bin/env node
/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */
const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

const args = process.argv.slice(2);
if (args.length < 3) {
    console.error("Usage: npm start LUIS_AUTHORING_KEY LUIS_REGION QNAMAKER_SUBSCRIPTION");
    console.error("This will setup services for this example");
    console.error("It produces LUIS applications weather and homeautomation, a QnA Maker KB 'faq' and a LUIS language dispatch model 'dispatchSample' for use in this sample.");
    process.exit(-1);
}

const luisKey = args[0];
const luisRegion = args[1];
const luisEndpoint = "https://" + luisRegion + ".api.cognitive.microsoft.com/luis/api/v2.0";
const qnakey = args[2];

const homeid = luisImport('homeautomation.json');
const weatherid = luisImport('weather.json');

var qnakbid = qnaKbId('faq');
if (qnakbid) {
    console.log("Using existing QnA Maker KB faq");
} else {
    console.log("Importing QnA Maker KB faq");
    qnaCall('create kb --in faq.json -q');
    qnakbid = qnaKbId('faq');
    qnaCall('publish kb --kbId ' + qnakbid);
}

var dispatchid = luisID('dispatchSample');
if (dispatchid) {
    console.log('Using existing LUIS dispatchSample');
} else {
    console.log("Creating LUIS dispatchSample over homeautomation, weather and faq");
    callDispatch('dispatch init -name dispatchSample -luisAuthoringKey ' + luisKey + ' -luisAuthoringRegion ' + luisRegion);
    callDispatch('dispatch add -type luis -name homeautomation');
    callDispatch('dispatch add -type luis -name weather');
    callDispatch('dispatch add -type qna -name faq -key ' + qnakey + ' -id ' + qnakbid);
    callDispatch('dispatch create');
    dispatchid = luisID('dispatchSample');
}

console.log('Adding settings to appsettings.json');
// Needed because of BOM
var settings = JSON.parse(fs.readFileSync('../appsettings.json', {encoding:'utf8'}).toString('utf8').replace(/^\uFEFF/, ''));
settings["Luis-SubscriptionId"] = luisKey;
settings["Luis-Url"] = "https://" + luisRegion + ".api.cognitive.microsoft.com/luis/v2.0/apps/";
settings["Luis-ModelId-Dispatcher"] = dispatchid;
settings["Luis-ModelId-HomeAutomation"] = homeid;
settings["Luis-ModelId-Weather"] = weatherid;
settings["QnAMaker-SubscriptionKy"] = qnakey;
settings["QnAMaker-KnowledgeBaseId"] = qnakbid;
fs.writeFileSync("../appsettings.json", JSON.stringify(settings, null, 2), {encoding:'utf8'});

console.log("Dispatch analysis is in generated/summary.html");
console.log("All done, you should be able to run the sample now.");

function call(cmd) {
    var output;
    try {
        return execSync(cmd);
    }
    catch (err) {
        console.log(cmd);
        console.log(err);
    }
}

function callDispatch(cmd) {
    return call(cmd + " -dataFolder generated");
}

function callJSON(cmd) {
    var output;
    try {
        output = execSync(cmd);
        return output.length > 0 ? JSON.parse(output) : undefined;
    }
    catch (err) {
        console.log(cmd);
        console.log(err);
        console.log(output.toString('utf8'));
    }
}

function qnaCall(cmd) {
    const fullCmd = 'qnamaker ' + cmd + " --subscriptionKey " + qnakey;
    return callJSON(fullCmd);
}

function qnaKbId(name) {
    var id;
    var kbs = qnaCall('list kbs');
    for (var i = 0; i < kbs.knowledgebases.length; ++i) {
        var kb = kbs.knowledgebases[i];
        if (kb.name === name) {
            id = kb.id;
            break;
        }
    }
    return id;
}

function luisCall(cmd, appId) {
    const fullCmd = 'luis ' + cmd + ' --authoringKey ' + luisKey + ' --endpointBasePath ' + luisEndpoint;
    if (appId) {
        fullCmd += " --appId " + appId;
    }
    return callJSON(fullCmd);
}

function luisID(name) {
    var id;
    var apps = luisCall("list applications");
    for (i = 0; i < apps.length; ++i) {
        var app = apps[i];
        if (app.name === name) {
            id = app.id;
            break;
        }
    }
    return id;
}

function luisImport(file) {
    var name = path.basename(file, '.json');
    var id = luisID(name);
    if (!id) {
        console.log("Importing LUIS app " + name);
        id = luisCall("import application --in " + file).id;
        luisCall("train version --versionId 0.1 --wait", id);
        luisCall("publish version --versionId 0.1", id);
    }
    else {
        console.log("Using existing LUIS app " + name);
    }
    return id;
}
/*
const luisProcess = exec('luis list applications --authoringKey 0f43266ab91447ec8d705897381478c5 --endpointBastPath https://westus.api.cognitive.microsoft.com/luis/api/v2.0', { stdio: ['pipe', 'pipe', process.stderr] });
luisProcess.stdout.on('data', data => {
    var json = JSON.parse(data);
    console.log(data);
});
*/