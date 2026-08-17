'use strict';

var fs = global.fs = global.fs || require('fs');
var path = global.path = global.path || require('path');

// Get component configuration from config.json
var componentConfig;
function getComponentConfig() {
    if (!componentConfig) {
        var localConfig = JSON.parse(fs.readFileSync('./config.json', 'utf8'));
        localConfig.themes = ['material', 'fabric', 'bootstrap',
            'highcontrast', 'material-dark', 'fabric-dark', 'bootstrap-dark',
            'highcontrast-light', 'bootstrap4', 'bootstrap5-dark', 'tailwind', 'tailwind-dark',
            'fluent', 'fluent-dark', 'material3', 'material3-dark', 'fluent2', 'bootstrap5.3', 'tailwind3', 'bds'];
        componentConfig = localConfig;
    }
    return componentConfig;
}

// Initialize extended array if modelGenerator is present
var extArray;
var config = getComponentConfig();
if (config.modelGenerator) {
    extArray = config.modelGenerator.extended;
}

module.exports = function () {
    var through = global.through || require('through2');
    return through.obj(function (file, encoding, callback) {
        if (file.isNull()) { return callback(null, file); }

        //Required Variable
        var propertyDetectionRegx = new RegExp('@Property|@Complex|@Collection|@ComplexFactory|@CollectionFactory');
        var componentPath = '';
        var eventDetectionRegx = new RegExp('@Event');
        var classDetectionRegx = new RegExp('export class|export abstract class');
        var chartSeriesDetectionRegx = new RegExp('(public addPoint|public removePoint|public setData)\\(');
        var interFaceDetectionRegx = /export interface/g;
        var enumDetectionRegx = /export enum/g;
        var aliasDetectionRegx = /export type /g;
        var commentStartRegx = new RegExp('/\\*\\*');
        var refRegex = /\/\/\/ *<reference/;
        var importRegx = new RegExp('import ');
        var lastMarkedCommentStart, hasProperty, exportedInterfaces = [];
        var classDetails = {};
        var curClassName, moduleName, exImported = '',
            extModColl = [],
            importColl = [],
            refCollection = [];
        var componentExtend = false;
        var moduleExtend = false;
        //File data variables
        var data = fs.readFileSync(file.path);
        var lines = data.toString().split('\n');
        moduleName = path.basename(file.path).replace('.ts', '');
        processLines();
        createFile();

        function getPropertyDetails(fromLine, toLine) {
            var propertyDetails = {};
            var prop = lines[toLine];
            var acc_name = prop.substr(0, prop.indexOf(':')).trim().split(' ');
            var type = prop.substr(prop.indexOf(':') + 1);
            //var len = dProp.length;
            propertyDetails.access = acc_name[0];
            propertyDetails.name = acc_name[1];
            propertyDetails.type = type;
            propertyDetails.comments = '   /**\n';
            for (var i = fromLine + 1; i < toLine - 1; i++) {
                propertyDetails.comments = propertyDetails.comments + lines[i] + '\n';
            }
            return propertyDetails;
        }

        function getFunctionDetails(fromLine, toLine) {
            var functionDetails = {};
            var funcProp = lines[toLine];
            var func_name = funcProp.substr(0, funcProp.indexOf('):')).trim() + ')';
            func_name = func_name.split(/ (.+)/);
            var funcType = funcProp.substr(funcProp.indexOf(')')).split(' ')[1];
            functionDetails.access = func_name[0];
            functionDetails.name = func_name[1];
            functionDetails.type = funcType;
            functionDetails.isChartSeriesModel = true;
            functionDetails.comments = '   /**\n';
            for (var i = fromLine + 1; i < toLine; i++) {
                functionDetails.comments = functionDetails.comments + lines[i] + '\n';
            }
            return functionDetails;
        }


        function getCurClass(line) {
            var obj = {};
            if (line.indexOf('extends') > -1) {
                if (line.indexOf('extends Component') > -1) {
                    componentExtend = true;
                } else if (line.indexOf('extends Base') === -1 && line.indexOf('extends ChildProperty') === -1) {
                    moduleExtend = true;
                }
            }

            var classN = line.split(' ');
            obj.class = classN[classN.indexOf('class') + 1];
            if (obj.class && obj.class.indexOf('3D') === -1) {
                obj.class = classN[classN.indexOf('class') + 1].split(/[^A-Za-z]/)[0];
            }
            obj.extendClass = classN[classN.indexOf('extends') + 1].split(/[^A-Za-z]/)[0];
            return obj;
        }

        function getInterface(line) {
            return line.split('interface')[1].trim().split(' ')[0];
        }

        function getEnum(line) {
            return line.split('enum')[1].trim().split(' ')[0];
        }

        function getAlias(line) {
            return line.split('type')[1].trim().split(' ')[0];
        }

        function processReference(line) {
            var curPath = line.split('=')[1].split('\/>')[0];
            curPath = curPath.trim().replace(/\'/g, '');
            if (curPath.indexOf('-model') > -1) {
                refCollection.push(curPath.split('-model')[0]);
            }

        }

        function processLines() {
            for (var i = 0; i < lines.length; i++) {
                var curLine = lines[i];
                //MarkCommentLine
                if (commentStartRegx.test(curLine)) {
                    lastMarkedCommentStart = i;
                }
                if (refRegex.test(curLine)) {
                    processReference(curLine);
                }
                //Detect class decorator
                if (classDetectionRegx.test(curLine) && curLine.trim()[0] !== '*') {
                    var classReturn = getCurClass(lines[i]);
                    curClassName = classReturn.class;
                    classDetails[curClassName] = {};
                    classDetails[curClassName].properties = [];
                    classDetails[curClassName].componentExtend = componentExtend;
                    classDetails[curClassName].typeDoc = getTypeDoc(lastMarkedCommentStart, i);
                    classDetails[curClassName].isPrivateDoc = isPrivateComment(classDetails[curClassName].typeDoc);
                    if (moduleExtend) {
                        var extended = classReturn.extendClass;
                        classDetails[curClassName].extModuleName = classReturn.extendClass;
                        classDetails[curClassName].moduleExtend = moduleExtend;
                        if (extModColl.indexOf(extended) === -1) {
                            extModColl.push(classReturn.extendClass);
                        }
                    }
                    componentExtend = false;
                    moduleExtend = false;
                }

                //Detect propert decorator
                if (propertyDetectionRegx.test(curLine) && curClassName) {
                    hasProperty = true;
                    classDetails[curClassName].properties.push(getPropertyDetails(lastMarkedCommentStart, i + 1));
                }
                // Detect chart series function decorator
                if (chartSeriesDetectionRegx.test(curLine) && (file.path.includes('chart-series.ts') || (file.path.includes('acc-base.ts'))) && curClassName) {
                    classDetails[curClassName].properties.push(getFunctionDetails(lastMarkedCommentStart, i));
                }
                // Detect event decorator
                if (eventDetectionRegx.test(curLine) && curClassName) {
                    hasProperty = true;
                    classDetails[curClassName].properties.push(getPropertyDetails(lastMarkedCommentStart, i + 1));
                }
                //Collect eported interfaces
                if (interFaceDetectionRegx.test(curLine)) {
                    exportedInterfaces.push(getInterface(curLine));
                }

                //Collect eported enums
                if (enumDetectionRegx.test(curLine)) {
                    exportedInterfaces.push(getEnum(curLine));
                }

                // Collect alias types
                if (aliasDetectionRegx.test(curLine)) {
                    exportedInterfaces.push(getAlias(curLine));
                }

                //imported lines
                if (importRegx.test(curLine) && !curLine.match('./' + moduleName + '-model') && !curLine.match('./' + moduleName + '\'')) {
                    if (curLine.indexOf('Component') !== -1) {
                        if (curLine.indexOf('@syncfusion/ej2-base') === -1) {
                            var compSplit = curLine.split('from')[1].replace(/\'|;/g, '').trim();
                            componentPath = compSplit + '-model\';';
                        }
                    }
                    exImported = exImported + curLine;
                    importColl.push(curLine);
                }
            }
        }

        function getTypeDoc(startIndex, endIndex) {
            return lines.slice(startIndex, endIndex).join('\n');
        }

        function isPrivateComment(str) {
            return /@private/.test(str);
        }

        function createFile() {
            if (hasProperty === true) {
                var tab = '    ',
                    newLine = '\n';
                var content = '',
                    bodyCont = [];
                var importInterface = [];
                var frontImport = '',
                    frontDoc = '',
                    frontContent = '',
                    rearContent = '\n}',
                    compExt = false,
                    modExt = false;
                var modleImport = '';
                var expModelColl = [];
                var classKeys = Object.keys(classDetails);
                var modexLen = extModColl.length;
                if (modexLen) {
                    for (var m = 0; m < modexLen; m++) {
                        var curExp = extModColl[m];
                        for (var l = 0; l < importColl.length; l++) {
                            var curText = importColl[l];
                            var indx = curText.indexOf(curExp);
                            var curLen = indx + curExp.length;
                            if (indx !== -1 && curText.substr(curLen, curLen + 1).match(/([A-Z])/i)) {
                                var endRef = curText.split('from')[1].split(';')[0].replace(/\'/g, '').trim();
                                if (refCollection.indexOf(endRef) > -1) {
                                    expModelColl.push(curExp);
                                    modleImport += 'import {' + curExp + 'Model} from ' + '\"' + endRef + '-model\";' + newLine;
                                } else if (extArray) {
                                    for (var x in extArray) {
                                        if (curExp === extArray[x]) {
                                            modleImport += 'import { ' + curExp + 'Model } from ' + '\"' + endRef + '\";' + newLine;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                for (var k = 0; k < classKeys.length; k++) {
                    var props = classDetails[classKeys[k]];
                    var extModule = props.extModuleName;
                    var addTypeDocProperties = props.isPrivateDoc ? ' * @private' + newLine : '';
                    frontDoc = '/**' + newLine + ' * Interface for a class ' + classKeys[k] + newLine + addTypeDocProperties + ' */';
                    if (props.componentExtend) {
                        compExt = true;
                        frontContent = frontDoc + '\n' + 'export interface ' + classKeys[k] + 'Model extends ComponentModel{' + newLine;
                    } else if (props.moduleExtend && (classKeys.indexOf(extModule) > -1 || expModelColl.indexOf(extModule) > -1)) {
                        modExt = true;
                        frontContent = frontDoc + '\n' + 'export interface ' + classKeys[k] +
                            'Model extends ' + props.extModuleName + 'Model{' + newLine;
                    } else {
                        frontContent = frontDoc + '\n' + 'export interface ' + classKeys[k] + 'Model {' + newLine;
                    }
                    content = '';
                    for (var i = 0; i < props.properties.length; i++) {
                        var singleProp = props.properties[i];
                        var curType = singleProp.type.trim().split(';')[0];
                        if (importInterface.indexOf(curType) === -1) {
                            if (exportedInterfaces.indexOf(curType) !== -1) {
                                importInterface.push(curType);
                            } else {
                                for (var j = 0; j < exportedInterfaces.length; j++) {
                                    var IFace = exportedInterfaces[j];
                                    if (RegExp(IFace).test(curType) && importInterface.indexOf(IFace) === -1) {
                                        importInterface.push(IFace);
                                    }
                                }
                            }
                        }
                        if (singleProp.isChartSeriesModel) {
                            content = content + newLine + ' ' + singleProp.comments + tab + singleProp.name.replace('(', '?(') + ' : ' + singleProp.type + newLine;
                        } else {
                            content = content + newLine + ' ' + singleProp.comments + tab + singleProp.name + '?:' + singleProp.type + newLine;
                        }
                    }
                    bodyCont.push(frontContent + content + rearContent);
                }
                if (importInterface.length) {
                    frontImport = 'import {' + importInterface.join(',') + '} from "./' + moduleName + '";' + newLine;
                }
                if (compExt) {
                    frontImport = frontImport + 'import {ComponentModel} from \'' + (componentPath.replace(' /*externalscript*/-model', '').trim() || '@syncfusion/ej2-base\';') + newLine;
                }

                try {
                    fs.writeFileSync(file.path.replace('.ts', '') + '-model.d.' + 'ts', exImported + '\n' + frontImport +
                        modleImport + '\n' + bodyCont.join('\n\n'), 'utf8');
                } catch (er) {

                }
            }
        }

        callback(null, file);
    });
};