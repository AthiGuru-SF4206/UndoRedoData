var extended = {
	modules: {},
    tempCallBacks: [],
    dataSourceProperties: ["dataSource", "dataSource_custom", "shapeData"],
    enableRtl: function (status) {
        sf.base.enableRtl(status);
    },
    loadCldr: function (cultureData) {
        sf.base.loadCldr(JSON.parse(cultureData));
    },
    setCulture: function (cultureName, cultureFormats) {
        sf.base.setCulture(cultureName);
        sf.base.extend(sf.base.blazorCultureFormats, cultureFormats);
    },
    convertOptions: function (options, isInitialRendering) {
        var output = {};
        for (var prop in options) {
            var props = prop.trim().split('.'), nestedObj = output;
            for (var i = 0; i < props.length - 1; i++) {
                var isArray = props[i].match(/^(\w+)\[(\d+)\]$/); // is array 
                if (isArray) {
                    var arr = nestedObj[isArray[1]];
                    if (!arr) arr = nestedObj[isArray[1]] = [];
                    nestedObj = arr[isArray[2]] = arr[isArray[2]] || {};
                } else {
                    nestedObj = nestedObj[props[i]] = nestedObj[props[i]] || {};
                }
            }
            nestedObj[props[i]] = options[prop];
            if (isInitialRendering) delete options[prop];
        }
        return output;
    },
    convertInitialOptions: function (options) {
        options = sfBlazor.convertOptions(JSON.parse(options), true);
        return JSON.stringify(options);
    },
    initialize: function (id, options, events, namespace, dotnet, bindableProps, htmlAttributes, templateRefs, interopAdaptor) {
        try {
            if (namespace == null) { return; }
            if (namespace == 'sf.popups.Spinner') { return; }
            sf.base.enableBlazorMode();
            if (!window.BlazorAdaptor && sf.data && typeof sf.data === 'object') {
                sfBlazor.initBlazorAdaptor();
            }
            options = sfBlazor.convertInitialOptions(options);
            var dotnetObj = {
                dotnetInstance: dotnet,
                templateRefs: templateRefs
            }
            options = JSON.parse(options, sfBlazor.parseRevive.bind(dotnetObj));
            options["elementID"] = "_ejs" + id;
            var type = sf.base.getValue(namespace, window);
            sfBlazor.bindEvents(options, events, dotnet, namespace);
            if (type) {
                var comp = new type(options);
            }
            if (comp) {
                comp.isServerRendered = true;
                comp._dotnetInstance = dotnet;
                comp.templateDotnetInstance = templateRefs;
                comp.interopAdaptor = interopAdaptor;
                // var change = comp.saveChanges;
                // comp._saveChanges = change.bind(comp);
                comp.bindableProps = bindableProps = JSON.parse(bindableProps);
                // comp.saveChanges = sfBlazor.updateModel.bind(comp);
                if (htmlAttributes) {
                    var element = document.getElementById(id);
                    for (var attr in htmlAttributes) {
                        element.setAttribute(attr, htmlAttributes[attr]);
                    }
                }
                if (namespace == 'sf.pdfviewer.PdfViewer') {
                    if (comp.serviceUrl == null) {
                        sf.pdfviewer.AjaxHandler.prototype.send = sfBlazor.requestHandler.bind(comp);
                    }
                }
                if (namespace == 'sf.documenteditor.DocumentEditorContainer' || namespace == 'sf.documenteditor.DocumentEditor') {
                    if (comp.serviceUrl == null || comp.serviceUrl == "" || comp.serviceUrl == undefined) {
                        sf.documenteditor.XmlHttpRequestHandler.prototype.send = sfBlazor.docEditRequestHandler.bind(comp);
                     }
                }
                comp.appendTo("#" + id);
            }
        }
        catch (e) {
            sfBlazor.throwError(e, namespace);
        }
    },
    renderComplete: function (element, blazorElement) {
        if (element != null) {
            if (window[element] && (window[element.id] instanceof element || window[element.id] instanceof HTMLElement)) {
                delete window[element.id];
            }
            element.classList.remove("e-blazor-hidden");
        }
    },
    isDataSourceProperty: function (key) {
        return sfBlazor.dataSourceProperties.indexOf(key) !== -1;
    },
    setModel: function (id, options, namespace) {
        try {
            var compElement = document.getElementById(id);
            if (!compElement) {
                return;
            }
            var comp = compElement.ej2_instances[0];
            var componentObject = {};
            comp.preventUpdate = true;
            sfBlazor.updateOldProperties(comp, options);
            var dotnetObj = { dotnetInstance: comp._dotnetInstance };
            componentObject = sfBlazor.getCompObject(comp, options, dotnetObj, namespace);
            comp.setProperties(componentObject);
            comp.dataBind();
            comp.preventUpdate = false;
        }
        catch (e) {
            window.sfBlazor.throwError(e, comp);
        }
    },
    getCompObject: function (comp, options, dotnetObj, namespace) {
        var compObject = sfBlazor.getModelObject(comp, options, dotnetObj, false);
        if (!compObject) {
            return;
        }
        var modelKeys = Object.keys(compObject);
        for (var i = 0; i < modelKeys.length; i++) {
            var key = modelKeys[i];
            var modelValue = comp[key];
            var newValue = compObject[key];
            var isParentArray = modelValue && modelValue.isComplexArray;
            var currentIndex = newValue && typeof newValue === 'object' && Object.keys(newValue)[0];
            if ((!(modelValue instanceof sf.base.ChildProperty) && !isParentArray) || isNaN(currentIndex)) {
                continue;
            }
            var childValue;
            var childModel = newValue[currentIndex];
            if (!Array.isArray(newValue) && childModel && childModel.isNewComponent) {
                var childNameType = key.charAt(0).toUpperCase() + key.substring(1, key.length);
                var childName = namespace.split('.').splice(0, 2).join('.') + "." + childNameType;
                var childType = sf.base.getValue(childName, window);
                if (childType) {
                    childValue = sf.base.createInstance(childType, [comp, key, childModel, isParentArray])
                }
            }
            if (childValue) {
                compObject[key][currentIndex] = childValue;
                if (comp[key] && Array.isArray(comp[key])) {
                    comp[key].filter(function (item, index) {
                        if (currentIndex !== index) {
                            compObject[key][index] = item;
                        }
                    });
                }
            }
        }
        return compObject;
    },
    getModelObject: function (comp, props, dotnetObj, isOldProp) {
        var modelObject = {};
        var propKeys = props.match(/"(.*?)":/g);
        for (var i = 0; i < propKeys.length; i++) {
            var key = propKeys[i].replace(":", "").replace(/"/g, "");
            var convertedProperties = sfBlazor.convertInitialOptions(props);
            var value = isOldProp ? sf.base.getValue(key, comp) : sf.base.getValue(key, JSON.parse(convertedProperties, sfBlazor.parseRevive.bind(dotnetObj)));
            if (value !== undefined) {
                key = key.split(/\[(\d+)\]/).join(".").replace(/\.\./g, ".");
                var propObj = sfBlazor.getNestedObject(comp, key, value);
                sf.base.extend(modelObject, {}, propObj.model, true);
                if (!isOldProp) {
                    comp["deepMerge"] = comp["deepMerge"] ? Array.from(new Set(comp.deepMerge.concat(propObj.deepMerge))) : propObj.deepMerge;
                }
            }
        }
        return modelObject;
    },
    updateOldProperties: function (comp, props) {
        var oldProps = sfBlazor.getModelObject(comp, props, null, true);
        sf.base.extend(comp.oldProperties, {}, oldProps, true);
    },
    getNestedObject: function (actualParent, key, value, parent) {
        var model = parent ? parent : {};
        var deepMerge = [];
        if (key.indexOf(".") !== -1) {
            var splitKeys = key.split(".");
            if (deepMerge.indexOf(splitKeys[0]) === -1) {
                deepMerge.push(splitKeys[0]);
            }
            for (var i = 0; i < splitKeys.length; i++) {
                var nestedValue = i === splitKeys.length - 1 ? value : {};
                var parentObj = i === 0 ? model : parentObj[splitKeys[i - 1]];
                actualParent = i === 0 ? actualParent : actualParent[splitKeys[i - 1]];
                parentObj = sfBlazor.getNestedObject(actualParent, splitKeys[i], nestedValue, parentObj, splitKeys[i + 1]).model;
            }
        }
        else {
            model[key] = value;
        }
        return { model: model, deepMerge: deepMerge };
    },
    updateModel: async function (comp) {
        try {
            if (!comp.isRendered || comp.preventUpdate || !comp._dotnetInstance) {
                return;
            }
            var bulkChanges = sfBlazor.copyWithoutCircularReferences([comp.bulkChanges], comp.bulkChanges);
            var updatedModel = sfBlazor.getUpdatedValue(comp, bulkChanges);
            await comp._dotnetInstance.invokeMethodAsync('UpdateModel', updatedModel);
            comp.preventUpdate = false;
        }
        catch (e) {
            window.sfBlazor.throwError(e, comp);
        }
    },
    getUpdatedValue(comp, changedProperties) {
        var updatedModel = {};
        var ignoreKeys = ["query", "formatter"];
        var changedPropertyKeys = Object.keys(changedProperties);
        for (var i = 0; i < changedPropertyKeys.length; i++) {
            var key = changedPropertyKeys[i];
            var modelValue = changedProperties[key];
            if (ignoreKeys.indexOf(key) !== -1) {
                continue;
            }
            if (sfBlazor.isChildProperty(comp, key)) {
                updatedModel[key] = sfBlazor.getUpdatedValue(comp, modelValue);
            }
            else if (typeof modelValue === "object" && modelValue !== null && !(modelValue instanceof Date) && !(Array.isArray(modelValue) && (modelValue[0]) instanceof Date)) {
                modelValue = JSON.stringify(modelValue);
                updatedModel[key] = modelValue;
            }
            else {
                updatedModel[key] = modelValue;
            }
        }
        return updatedModel;
    },

    isChildProperty(comp, key) {
        return comp.properties && comp.properties[key] && comp.properties[key].parentObj !== undefined;
    },
    // start region diagram methods
    updateBlazorProperties: function (object, component) {
        component._dotnetInstance.invokeMethodAsync('UpdateBlazorDiagramObjects', object);
    },
    updateBlazorDiagramEvents: async function (object, component) {
        var data;
        try {
            data = await component._dotnetInstance.invokeMethodAsync('UpdateBlazorDiagramEvents', object);
            if (data && typeof data === "string") {
                data = JSON.parse(data);
            }
        }
        catch (e) {
            window.sfBlazor.throwError(e, this);
        }
        return data;
    },

    callDiagramMethod: async function (args, compID) {
        var comp = document.getElementById(compID) && document.getElementById(compID).ej2_instances[0];
        if (args.methodName === "updateDiagramObjects") {
            args.obj = JSON.parse(args.obj, sfBlazor.parseRevive);
        }
        return comp.callFromServer(args);
    },
    // Update templates for diagram
    diagramSetTemplateInstance: function (namespace, dotnetInstance, guid) {
        if (!sfBlazor.templateDotnetInstance) {
            sfBlazor.templateDotnetInstance = [];
        }
        sfBlazor.templateDotnetInstance[guid || namespace] = dotnetInstance;
    },
    diagramSetTemplate: function (templateId, name) {
        setTimeout(function () {
            if (templateId != null) {
                // Consider more templates in two or more diagram in a same page
                var templates = document.getElementsByClassName("blazor-template");
                for (var z = 0; z < templates.length; z++) {
                    var template = templates[z];
                    var innerTemplates = template.getElementsByClassName("blazor-inner-template");
                    for (var i = 0; i < innerTemplates.length; i++) {
                        var tempElements = document.getElementsByClassName("e-blazor-template");

                        for (var k = 0; k < tempElements.length; k++) {
                            var tempElement = tempElements[k];
                            if (tempElement && innerTemplates[i].children.length) {
                                if (tempElement.children.length == 0 && k === i) {
                                        tempElement.appendChild(innerTemplates[i].children[0].cloneNode(true));
                                }
                            } else if (tempElement) {
                                tempElement.innerHTML = innerTemplates[i].innerHTML;
                            }
                        }
                    }

                    if (window.sfBlazor.tempCallBacks.length) {
                        for (var p = 0; p < window.sfBlazor.tempCallBacks.length; p++) {
                            if (window.sfBlazor.tempCallBacks[p].Id == templateId) {
                                window.sfBlazor.tempCallBacks[p].Promise(window.sfBlazor.tempCallBacks[p].Id);
                                window.sfBlazor.tempCallBacks.splice(p, 1);
                            }
                        }
                    }
                }
            }
        }, 100);
    },
    // end region diagram methods

    updateTemplate: function (name, templateData, templateId, comp, promise) {
        if (comp === undefined) {
            comp = {};
        }
        if (promise) {
            window.sfBlazor.tempCallBacks.push({ Id: templateId, Promise: promise });
        }
        var cloneTemplateData = []; var blazIds = []; var innerTemplates = [];
        if (!sf.base.isNullOrUndefined(templateData) && (templateData.length && !sf.base.isNullOrUndefined(templateData[0].BlazId))) {
            for (var i = 0; i < templateData.length; i++) {
                blazIds.push(templateData[i].BlazId);
                innerTemplates.push(templateData[i].BlazorTemplateId);
            }
        } else if (!sf.base.isNullOrUndefined(templateData)) {
            for (var i = 0; i < templateData.length; i++) {
                innerTemplates.push(templateData[i].BlazorTemplateId);
                // var innerTemplate = document.getElementById(innerTemplates[i]);
                cloneTemplateData.push(JSON.parse(window.sfBlazor.cleanStringify(templateData[i])));
                delete cloneTemplateData[i].BlazorTemplateId;
            }
        }
        var intervalId = setInterval(function () {
            var templateInstance = comp.templateDotnetInstance ? comp.templateDotnetInstance[name] || window.sfBlazor.templateDotnetInstance[comp.guid || name] : window.sfBlazor.templateDotnetInstance ? window.sfBlazor.templateDotnetInstance[comp.guid || name] : null;
            if (!templateInstance) {
                if ((comp.templateDotnetInstance) && (!window.sfBlazor.templateDotnetInstance[comp.guid || name])) {
                    return;
                }
                else if (comp.parentObj && comp.parentObj.templateDotnetInstance && comp.parentObj.templateDotnetInstance[name]) {
                    templateInstance = comp.parentObj.templateDotnetInstance[name];
                } else {
                    return;
                }
            }
            if (templateInstance) {
                templateInstance.invokeMethodAsync("UpdateTemplate", name, JSON.stringify(cloneTemplateData), templateId, innerTemplates, blazIds);
                clearInterval(intervalId);
            }
        }, 10);
    },

    setTemplateInstance: function (namespace, dotnetInstance, guid) {
        if (!sfBlazor.templateDotnetInstance) {
            sfBlazor.templateDotnetInstance = [];
        }
        sfBlazor.templateDotnetInstance[guid || namespace] = dotnetInstance;
    },

    setTemplate: function (templateId, name) {
        setTimeout(function () {
            if (templateId != null) {
                var template = document.getElementById(templateId);
                var innerTemplates = template.getElementsByClassName("blazor-inner-template");
                for (var i = 0; i < innerTemplates.length; i++) {

                    var tempId = innerTemplates[i].getAttribute("data-templateid");
                    var tempElement = document.getElementById(tempId);
                    if (tempElement && innerTemplates[i].childNodes) {
                        var length = innerTemplates[i].childNodes.length;
                        for (var j = 0; j < length; j++) {
                            tempElement.appendChild(innerTemplates[i].childNodes[0]);
                        }
                    } else if (tempElement) {
                        tempElement.innerHTML = innerTemplates[i].innerHTML;
                    }
                }
                if (window.sfBlazor.tempCallBacks.length) {
                    for (var p = 0; p < window.sfBlazor.tempCallBacks.length; p++) {
                        if (window.sfBlazor.tempCallBacks[p].Id == templateId) {
                            window.sfBlazor.tempCallBacks[p].Promise(window.sfBlazor.tempCallBacks[p].Id);
                            window.sfBlazor.tempCallBacks.splice(p, 1);
                        }
                    }
                }
            }
        }, 100);
    },
    invokeMethod: async function (elementId, methodName, moduleName, args, element) {
        try {
            var returnValue = null;
            args = JSON.parse(args, sfBlazor.parseRevive);
            var comp = document.getElementById(elementId) && document.getElementById(elementId).ej2_instances && document.getElementById(elementId).ej2_instances[0];
            if (methodName === "destroy" && comp) {
                comp._dotnetInstance = null;
            }
            if (element) {
                args.push(element);
            }
            if (comp) {
                if (moduleName === null) {
                    returnValue = comp[methodName].apply(comp, args);
                }
                else {
                    comp = sfBlazor.getDocEditor(comp, moduleName);
                    comp = window.sfBlazor.getChildModule(comp, moduleName);
                    returnValue = comp[methodName].apply(comp, args);
                    returnValue = await sfBlazor.promiseHandler(returnValue);
                }
                if (returnValue && typeof returnValue === "object" && !(returnValue instanceof Promise)) {
                    returnValue = sfBlazor.cleanStringify(returnValue);
                }
                if (comp.getModuleName() == 'PdfViewer' && methodName == 'saveAsBlob' && returnValue instanceof Promise) {
                    returnValue = await sfBlazor.promiseHandler(returnValue);
                }
                return returnValue;
            }
        }
        catch (e) {
            return window.sfBlazor.throwError(e, comp);
        }
    },
    promiseHandler: async function (returnValue) {
        if (returnValue instanceof Promise) {
            await returnValue.then(async function (data) {
                if (data instanceof Blob) {
                    await window.sfBlazor.docEditFileReader(data).then(function (dataUrl) {
                        returnValue = JSON.stringify({ "data": dataUrl.substring(dataUrl.indexOf(',') + 1) });
                    });
                } else {
                    returnValue = data;
                }
            });
        }
        return returnValue;
    },
    getDocEditor: function (comp, moduleName) {
        return (comp.getModuleName() === "DocumentEditorContainer" && moduleName !== "documentEditor") ? comp.documentEditor : comp;
    },
    getChildModule: function (comp, moduleName) {
        try {
            var path = moduleName.split(',');
            for (var i = 0; i < path.length; i++) {
                comp = comp[path[i]];
            }
            return comp;
        } catch (e) {
            window.sfBlazor.throwError(e, comp);
        }
    },
    getMethodCall: function (elementId, moduleName, methodName) {
        try {
            var comp = document.getElementById(elementId) && document.getElementById(elementId).ej2_instances[0];
            comp = sfBlazor.getDocEditor(comp);
            if (moduleName == null || moduleName == "null") {
                return comp[methodName];
            }
            else {
                comp = window.sfBlazor.getChildModule(comp, moduleName);
                return comp[methodName];
            }
        }
        catch (e) {
            window.sfBlazor.throwError(e, comp);
        }
    },
    setMethodCall: function (elementId, moduleName, methodName, args) {
        try {
            var comp = document.getElementById(elementId) && document.getElementById(elementId).ej2_instances[0];
            comp = sfBlazor.getDocEditor(comp);
            comp = window.sfBlazor.getChildModule(comp, moduleName);
            comp[methodName] = args[0];
        }
        catch (e) {
            window.sfBlazor.throwError(e, comp);
        }
    },
    methodCall: function (elementId, moduleName, methodName, args) {
        try {

            var comp = document.getElementById(elementId) && document.getElementById(elementId).ej2_instances[0];
            comp = window.sfBlazor.getChildModule(comp, moduleName);
            comp[methodName].apply(comp, [args]);
        }
        catch (e) {
            window.sfBlazor.throwError(e, comp);
        }
    },
    parseRevive: function (key, value) {
        var dateRegex = new RegExp(/(\d{4})-(\d{2})-(\d{2})T(\d{2})\:(\d{2})\:(\d{2}).*/);
        var arrayRegex = new RegExp(/^\[.*?\]$/);
        var objectRegex = new RegExp(/^\{.*?\}$/);
        if (key === "" && value && value.dataSource_custom) {
            value.dataSource_custom.key = value.dataSource.key;
            value.dataSource = value.dataSource_custom;
        }
        if (value && typeof value === "string" && (value.indexOf("sf.data.Query()") !== -1 || key === "formatter")) {
            try {
                return value;
            }
            catch (e) {
                var funValue = sfBlazor.getEvaluatedFunc(value);
                return funValue;
            }

        }
        else if (sfBlazor.isDataSourceProperty(key)) {
            if (value === null) return;
            value = typeof value === "string" ? JSON.parse(value) : value;
            if (!value.adaptor || value instanceof sf.data.DataManager) {
                return value;
            }
            var isOffline = value.offline;
            value.offline = false;
            value.adaptor = sfBlazor.getAdaptor(value.adaptor);
            var dataManager = new sf.data.DataManager(value);
            dataManager["offline"] = isOffline;
            if (this.dotnetInstance) {
                dataManager["dotnetInstance"] = this.dotnetInstance;
                dataManager["key"] = value.key;
                if (dataManager.adaptor instanceof BlazorAdaptor) {
                    dataManager["adaptorName"] = value["adaptorName"];
                    dataManager.dataSource.offline = false;
                }
            }
            if (this[value.guid] || (this.templateRefs && this.templateRefs[value.guid])) {
                dataManager["baseAdaptorInstance"] = this[value.guid] || this.templateRefs[value.guid];
                dataManager["adaptorName"] = "CustomAdaptor";
            }
            return dataManager;
        }
        else if (typeof value === "string" && dateRegex.test(value)) {
            if (!arrayRegex.test(value)) {
                return !isNaN(Date.parse(value)) ? new Date(value) : value;
            } else {
                var values = JSON.parse(value);
                var val = []
                for (i = 0; i < values.length; i++) {
                    val.push(new Date(values[i]));
                }
                return val;
            }
        }
        else if (typeof value === "string" && (arrayRegex.test(value) || objectRegex.test(value)) && sfBlazor.isJson(value)) {
            return JSON.parse(value);
        }
        return value;
    },
    getEvaluatedFunc: function (args) {
        var reg = new RegExp(/\(+[^\)]+\)+/g);
        var spltData = args.split('new sf.data.Query().')[1];
        var rteFormatOptions = (args.trim()).split('new sf.richtexteditor.MarkdownFormatter')[1];
        if (spltData) {
            var splt = spltData.split(reg)[0];
            var match = spltData.match(reg)[0];
            var regx = new RegExp(/(\([a-z])+[^\)]+\)+/g);
            var inData = args.match(regx);
            if (inData !== null) {
                var inData1 = inData[0];
                var mtchinData = inData1.match(/\{+[^\)]+\}/g)[0];
                var spltinData = inData1.split(/\{+[^\)]+\}/g)[0];
            }
            if (match.indexOf('sf') !== -1) {
                var matchData = spltinData.match(/[^(]+/g)[0];
                var fData = matchData.split('.');
                var finalDatas = new sf.data.Query()[splt](sf[fData[1]][fData[2]][fData[3]](JSON.parse(mtchinData)));
            }
            else if (spltData.split('.').length > 1) {
                var reg = new RegExp(/([^\"]([A-Z]|[a-z])+[\"\]])/g);
                if (spltData.match(reg) !== null) {
                    var regx = new RegExp(/([^\"]([A-Z]|[a-z])+[\"\]])|[0-9]/g);
                    var matchg = spltData.match(regx);
                    var spltDt = spltData.split('.');
                    var chck = [];
                    for (var i = 0; i < spltDt.length; i++) {
                        var d = spltDt[i].match(/([A-z])+/g)[0];
                        chck.push(d);
                    }
                    var finalDatas = new sf.data.Query()[chck[0]]()[chck[1]](matchg[0].slice(0, -1))[chck[2]](Number(matchg[1]));
                }
                else {
                    var regh = new RegExp(/([^\']+([A-Z]|[a-z])+[\'\)])|[0-9]/g);
                    var matchg = spltData.match(regh);
                    var spltDt = spltData.split('.');
                    var chck = [];
                    for (var i = 0; i < spltDt.length; i++) {
                        var d = spltDt[i].match(/([A-z])+/g)[0];
                        chck.push(d);
                    }
                    var finalDatas = new sf.data.Query()[chck[0]](matchg[0].slice(0, -1))[chck[1]](matchg[1].slice(0, -1))[chck[2]](Number(matchg[2]));
                }
            }
            else {
                var matchData = match.match(/([A-z]+|[0-9]+)+/g);
                var finalDatas = new sf.data.Query()[splt](...matchData);
            }
        }
        else if (rteFormatOptions) {
            var trimVal = rteFormatOptions.trim();
            var optionsString = trimVal.substring(1, trimVal.length - 1).trim();
            if (optionsString.length > 0) {
                var jsonString = optionsString.replace(/'/g, "\"");
                var options = JSON.parse(jsonString);
                var finalDatas = new sf.richtexteditor.MarkdownFormatter(options);
            }
            else {
                var finalDatas = new sf.richtexteditor.MarkdownFormatter({});
            }
        }
        else {
            var finalDatas = new sf.data.Query();
        }
        return finalDatas;
    },
    triggerEJEvents: async function (arg) {
        var data;
        try {
            if (arg) {
                arg["elementID"] = this["elementID"];
                data = await this.dotnet.invokeMethodAsync("Trigger", this.eventName, window.sfBlazor.cleanStringify(arg));
            } else {
                data = await this.dotnet.invokeMethodAsync("Trigger", this.eventName, '');
            }
        }
        catch (e) {
            window.sfBlazor.throwError(e, this);
        }
        return data;
    },
    copyWithoutCircularReferences: function (references, object) {
        try {
            var isArray = object && Array.isArray(object);
            var cleanObject = isArray ? [] : {};
            var keys = isArray ? object : (object instanceof Node ? [object] : sfBlazor.getObjectKeys(object));
            keys.forEach(async function (key) {
                var childObject = object.hasOwnProperty('parentObj') && object.properties ? object.properties : object;
                var value = isArray ? key : childObject[key];
                if (isArray && (typeof value === "string" || typeof value === "number")) {
                    cleanObject.push(value);
                }
                else if (value instanceof Node) {
                    var domObject = sfBlazor.getDomObject(key, value, object);
                    if (isArray) {
                        cleanObject.push(domObject);
                    }
                    else {
                        cleanObject[key] = domObject;
                    }
                }

                else if (value && Array.isArray(value)) {
                    if (value.length > 0) {
                        for (var i = 0; i < value.length; i++) {
                            if (!cleanObject[key]) cleanObject[key] = [];
                            if (key !== 'ej2_instances') {
                                if (value[i] && typeof value[i] === 'object' && !(value[i] instanceof Date)) {
                                    cleanObject[key].push(window.sfBlazor.copyWithoutCircularReferences(references, value[i]));
                                }
                                else {
                                    cleanObject[key].push(value[i]);
                                }
                            }
                        }
                    } else {
                        cleanObject[key] = [];
                    }
                }
                else if (value && window.sfBlazor.isJson(value) && (new RegExp(/^\[.*?\]$/).test(value))) {
                    var arrValues = JSON.parse(value);
                    if (!cleanObject[key]) cleanObject[key] = [];
                    for (var ij = 0; ij < arrValues.length; ij++) {
                        cleanObject[key].push(arrValues[ij]);
                    }
                }
                else if (value && value instanceof Event) {
                    var eventObj = {};
                    for (var propKey in value) {
                        var eventValue = value[propKey];
                        if (!(eventValue instanceof Node || eventValue instanceof Window) && propKey !== "path") {
                            eventObj[propKey] = eventValue;
                        }
                    }
                    cleanObject[key] = eventObj;
                }
                else if (value && typeof value === 'object') {
                    if (value instanceof File) {
                        cleanObject[key] = value;
                    } else if (references.indexOf(value) < 0) {
                        references.push(value);
                        if (value && value instanceof Date) {
                            cleanObject[key] = value;
                        } else if (sfBlazor.isJsonStringfy(value) && !sfBlazor.doesHaveFileObject(value)) {
                            isArray ? cleanObject.push(value) : cleanObject[key] = value;
                        }
                        else {
                            if (!sfBlazor.isIgnoreProperty(key.toString())) {
                                cleanObject[key] = window.sfBlazor.copyWithoutCircularReferences(references, value);
                            }
                            else {
                                cleanObject[key] = '###_Circular_###';
                            }
                        }
                        references.pop();
                    } else {
                        cleanObject[key] = '###_Circular_###';
                    }
                }
                else if (typeof value !== 'function') {
                    cleanObject[key] = value;
                }
            });
            return cleanObject;
        }
        catch (e) {
            console.log(e);
            return {};
        }
    },
    doesHaveFileObject: function (obj) {
        var keys = Object.keys(obj);
        for (var m = 0; m < keys.length; m++) {
            if (obj[keys[m]] instanceof File) {
                return true;
            }
        }
        return false;
    },
    isJsonStringfy: function (args) {
        try {
            return JSON.stringify(args) && true;
        } catch (e) {
            return false;
        }
    },
    getObjectKeys: function (obj) {
        var objectKeys = [];
        if (obj instanceof Event) {
            objectKeys = Object.keys(obj);
        } else {
            for (var key in obj) {
                objectKeys.push(key);
            }
        }
        return objectKeys;
    },
    isIgnoreProperty: function (key) {
        return ['parentObj', 'controlParent', 'modelObserver', 'localObserver', 'moduleLoader'].indexOf(key) >= 0;
    },
    isJson: function (value) {
        try {
            return JSON.parse(value);
        } catch (e) {
            return false;
        }
    },
    cleanStringify: function (object) {
        try {
            if (object && typeof object === 'object') {
                object = window.sfBlazor.copyWithoutCircularReferences([object], object);
            }
            return JSON.stringify(object);
        }
        catch (e) {
            console.log(e);
            return '';
        }
    },
    bindEvents: function (modelObj, events, dotnet, namespace) {
        if (events) {
            for (var i = 0; i < events.length; i = i + 1) {
                var curEvent = events[i];
                var scope = { dotnet: dotnet, eventName: curEvent, elementID: modelObj["elementID"], namespace: namespace };
                if (curEvent.indexOf('.') > 0) {
                    var items = curEvent.split('.');
                    var currentObject = modelObj;
                    for (var j = 0; j < items.length - 1; j++) {
                        var arrayIndex = new RegExp(/\[.*?\]/);
                        if (arrayIndex.test(items[j])) {
                            var index = items[j].match(arrayIndex)[0];
                            var prop = items[j].replace(index, "");
                            index = index.match(/\[(.*?)\]/)[1];
                            j += 1;
                            currentObject = currentObject[prop][index];
                        } else {
                            currentObject = currentObject[items[j]];
                        }
                    }
                    currentObject[items[items.length - 1]] = window.sfBlazor.triggerEJEvents.bind(scope);
                } else {
                    modelObj[curEvent] = window.sfBlazor.triggerEJEvents.bind(scope);
                }
            }
        }
    },
    tryParseInt: function (val) {
        var numRegex = /^-?\d+\.?\d*$/;
        return numRegex.test(val);
    },
    throwError: function (e, comp) {
        // comp._dotnetInstance.invokeMethodAsync("ErrorHandling", e.message, e.stack);
        console.error(e.message + "\n" + e.stack);
    },
    getAdaptor: function (adaptor) {
        var adaptorObject;
        switch (adaptor) {
            case "ODataAdaptor":
                adaptorObject = new sf.data.ODataAdaptor();
                break;
            case "ODataV4Adaptor":
                adaptorObject = new sf.data.ODataV4Adaptor();
                break;
            case "UrlAdaptor":
                adaptorObject = new sf.data.UrlAdaptor();
                break;
            case "WebApiAdaptor":
                adaptorObject = new sf.data.WebApiAdaptor();
                break;
            case "JsonAdaptor":
                adaptorObject = new sf.data.JsonAdaptor();
                break;
            case "RemoteSaveAdaptor":
                adaptorObject = new sf.data.RemoteSaveAdaptor();
                break;
            case "CustomAdaptor":
                sfBlazor.initCustomAdaptor();
                adaptorObject = new window.CustomAdaptor();
                break;
            default:
                adaptorObject = new window.BlazorAdaptor();
                break;
        }
        return adaptorObject;
    },
    spinnerUtility: function (action, options, target, type) {
        try {
            sf.popups.Spinner(action, options, target, type);
        }
        catch (e) {
            return window.sfBlazor.throwError(e);
        }
    },
    initBlazorAdaptor: function () {
        var BaseClass = sf.data ? sf.data.UrlAdaptor : function () { };
        window.BlazorAdaptor = class BlazorAdaptor extends BaseClass {
            processQuery(dm, query, hierarchyFilters) {
                var request = sf.data.UrlAdaptor.prototype.processQuery.apply(this, arguments);
                request.dotnetInstance = dm.dotnetInstance;
                request.key = dm.key;
                return request;
            }
            makeRequest(request, deffered, args, query) {
                var fnFail = function(e) {
                    args.error = e;
                    deffered.reject(args);
                };
                var process = function (data, aggregates, virtualSelectRecords) {
                    var args = {};
                    args.count = data.count ? parseInt(data.count.toString(), 10) : 0;
                    args.result = data.result ? data.result : data;
                    args.aggregates = data.aggregates;
                    args.virtualSelectRecords = virtualSelectRecords;
                    deffered.resolve(args);
                };
                var dm = JSON.parse(request.data);
                dm.serverSideGroup = false;
                var proxy = this;
                request.dotnetInstance.invokeMethodAsync("DataProcess", JSON.stringify(dm), request.key).then(function(data) {
                    data = sf.data.DataUtil.parse.parseJson(data);
                    if (data === null) {
                        data = [];
                    } else if (data.result === null) {
                        data.result = [];
                    }
                    var pResult = proxy.processResponse(data, {}, query, null, request);
                    process(pResult);
                    return;
                }).catch(function(e) { fnFail(e) });
            }
            insert(dm, data, tableName, query, position) {
                var args = {};
                args.dm = dm;
                args.data = data;
                args.tableName = tableName;
                args.query = query;
                args.requestType = "insert";
                args.position = position;
                return args;
            }
            remove(dm, keyField, value, tableName, query) {
                var args = {};
                args.dm = dm;
                args.data = value;
                args.keyField = keyField;
                args.tableName = tableName;
                args.query = query;
                args.requestType = "remove";
                return args;
            }
            update(dm, keyField, value, tableName, query) {
                var args = {};
                args.dm = dm;
                args.data = value;
                args.keyField = keyField;
                args.tableName = tableName;
                args.query = query;
                args.requestType = "update";
                return args;
            }
            batchRequest(dm, changes, e, query, original) {
                var args = {};
                args.dm = dm;
                args.changed = changes.changedRecords;
                args.added = changes.addedRecords;
                args.deleted = changes.deletedRecords;
                args.requestType = "batchsave";
                args.keyField = e.key;
                args.dropIndex = !sf.base.isNullOrUndefined(query) ? query.dragDropDestinationIndex : null;
                args.query = query;
                return args;
            }
            doAjaxRequest(args) {
                var defer = new sf.data.Deferred();
                var dm = args.dm;
                var query = sf.data.UrlAdaptor.prototype.processQuery.apply(this, [dm, args.query, false]);
                var fnFail = function(e) {
                    args.error = e;
                    defer.reject(args);
                };
                if (args.requestType === "insert") {
                    dm.dotnetInstance.invokeMethodAsync('Insert', JSON.stringify(args.data), dm.key, args.position ? args.position : 0, query.data).then(function(data) {
                        defer.resolve(data);
                    }).catch(function(e) { fnFail(e) });
                }
                if (args.requestType === "remove") {
                    var dataKey = (typeof args.data == "string") ? args.data : JSON.stringify(args.data);
                    if (args.data instanceof Date) {
                        dataKey = args.data.toJSON();
                    }
                    dm.dotnetInstance.invokeMethodAsync('Remove', dataKey, args.keyField, dm.key, query.data).then(function(data) {
                        defer.resolve();
                    }).catch(function(e) { fnFail(e) });
                }
                if (args.requestType === "update") {
                    dm.dotnetInstance.invokeMethodAsync('Update', JSON.stringify(args.data), args.keyField, dm.key, query.data).then(function(data) {
                        var record = sf.data.DataUtil.parse.parseJson(data);
                        defer.resolve(record);
                    }).catch(function(e) { fnFail(e) });
                }
                if (args.requestType === "batchsave") {
                    dm.dotnetInstance.invokeMethodAsync('BatchUpdate', JSON.stringify(args.changed), JSON.stringify(args.added), JSON.stringify(args.deleted), args.keyField, dm.key, args.dropIndex, query.data).then(function(data) {
                        var record = sf.data.DataUtil.parse.parseJson(data);
                        defer.resolve(record);
                    }).catch(function(e) { fnFail(e) });
                }
                return defer.promise;
            }
        };
    },
    initCustomAdaptor: function () {
        window.CustomAdaptor = class CustomAdaptor extends window.BlazorAdaptor {
            processQuery(dm, query, hierarchyFilters) {
                var request = sf.data.UrlAdaptor.prototype.processQuery.apply(this, arguments);
                request.dotnetInstance = dm.dotnetInstance;
                request.baseAdaptorInstance = dm.baseAdaptorInstance;
                request.key = dm.key;
                return request;
            }
            makeRequest(request, deffered, args, query) {
                var fnFail = function(e) {
                    args.error = e;
                    deffered.reject(args);
                };
                var process = function (data, aggregates, virtualSelectRecords) {
                    var args = {};
                    args.count = data.count ? parseInt(data.count.toString(), 10) : 0;
                    args.result = data.result ? data.result : data;
                    args.aggregates = aggregates;
                    args.virtualSelectRecords = virtualSelectRecords;
                    deffered.resolve(args);
                };
                var dm = JSON.parse(request.data);
                dm.serverSideGroup = false;
                var proxy = this;
                request["baseAdaptorInstance"].invokeMethodAsync("BaseRead", JSON.stringify(dm), request.key).then(function(data) {
                    data = sf.data.DataUtil.parse.parseJson(data);
                    if (data.result === null) {
                        data.result = [];
                    }
                    var pResult = proxy.processResponse(data, {}, query, null, request);
                    process(pResult);
                    return;
                }).catch(function(e) { fnFail(e) });
            }
            doAjaxRequest(args) {
                var defer = new sf.data.Deferred();
                var dm = args.dm;
                var fnFail = function(e) {
                    args.error = e;
                    defer.reject(args);
                };
                if (args.requestType === "insert") {
                    dm.baseAdaptorInstance.invokeMethodAsync('BaseInsert', JSON.stringify(args.data), dm.key).then(function(data) {
                        data = sf.data.DataUtil.parse.parseJson(data);
                        defer.resolve(data);
                    }).catch(function(e) { fnFail(e) });
                }
                if (args.requestType === "remove") {
                    dm.baseAdaptorInstance.invokeMethodAsync('BaseRemove', JSON.stringify(args.data), args.keyField, dm.key).then(function(data) {
                        data = sf.data.DataUtil.parse.parseJson(data);
                        defer.resolve(data);
                    }).catch(function(e) { fnFail(e) });
                }
                if (args.requestType === "update") {
                    dm.baseAdaptorInstance.invokeMethodAsync('BaseUpdate', JSON.stringify(args.data), args.keyField, dm.key).then(function(data) {
                        data = sf.data.DataUtil.parse.parseJson(data);
                        defer.resolve(data);
                    }).catch(function(e) { fnFail(e) });
                }
                if (args.requestType === "batchsave") {
                    dm.baseAdaptorInstance.invokeMethodAsync('BaseBatchUpdate', JSON.stringify(args.changed), JSON.stringify(args.added), JSON.stringify(args.deleted), args.keyField, dm.key, args.dropIndex).then(function(data) {
                        data = sf.data.DataUtil.parse.parseJson(data);
                        defer.resolve(data);
                    }).catch(function(e) { fnFail(e) });
                }
                return defer.promise;
            }
        };
    },
    requestHandler: function (jsonObject) {
        try {
            var currentElement = document.getElementById(jsonObject.elementId);
            if(currentElement != null)
            {
                if (currentElement.ej2_instances.length>0)
                {
                    var comp = currentElement.ej2_instances[0];
                    comp._dotnetInstance.invokeMethodAsync('GetPDFInfo', jsonObject);
                } else {
                    if (jsonObject.action == 'Unload') {
                        DotNet.invokeMethodAsync('Syncfusion.Blazor.PdfViewerServer','Unload',jsonObject);
                    }
                }
            }
        }
        catch (e) {
            window.sfBlazor.throwError(e, this);
        }
    },
    ioSuccessHandler: function (id, namespace, action, jsonResult) {
        try {
            var element = document.getElementById(id);
            if (element) {
                var comp = element.ej2_instances[0];
                var result = { data: jsonResult };
                if (namespace == 'sf.pdfviewer.PdfViewer') {
                    switch (action) {
                        case 'Load':
                            comp.viewerBase.loadRequestHandler.successHandler(result);
                            break;
                        case "RenderPdfPages":
                            comp.viewerBase.pageRequestHandler.successHandler(result);
                            break;
                        case "VirtualLoad":
                            comp.viewerBase.virtualLoadRequestHandler.successHandler(result);
                            break;
                        case "Download":
                            comp.viewerBase.dowonloadRequestHandler.successHandler(result);
                            break;
                        case "PrintImages":
                            comp.printModule.printRequestHandler.successHandler(result);
                            break;
                        case "Search":
                            comp.textSearchModule.searchRequestHandler.successHandler(result);
                            break;
                        case "Bookmarks":
                            comp.bookmarkViewModule.bookmarkRequestHandler.successHandler(result);
                            break;
                        case "RenderThumbnailImages":
                            comp.thumbnailViewModule.thumbnailRequestHandler.successHandler(result);
                            break;
                        case "RenderAnnotationComments":
                            comp.annotationModule.stickyNotesAnnotationModule.commentsRequestHandler.successHandler(result);
                            break;
						case "RenderPdfTexts":
                            comp.textSearchModule.searchRequestHandler.successHandler(result);
                            break;	
                        case "ImportAnnotations":
                            comp.viewerBase.importAnnotationRequestHandler.successHandler(result);
                            break;
                        case "ExportAnnotations":
                            comp.viewerBase.exportAnnotationRequestHandler.successHandler(result);
                            break;
                        case "ExportFormFields":
                            comp.viewerBase.exportFormFieldsRequestHandler.successHandler(result);
                            break;
                        case "ImportFormFields":
                            comp.viewerBase.importFormFieldsRequestHandler.successHandler(result);
                            break;
                    }
                }
                if (namespace == 'sf.documenteditor.DocumentEditorContainer' || namespace == 'sf.documenteditor.DocumentEditor') {
                    var docEditorComp = sfBlazor.getDocEditor(comp);
                    switch (action) {
                        case 'SystemClipboard':
                            result.data = JSON.parse(result.data);
                            docEditorComp.editor.pasteRequestHandler.successHandler(result);
                            break;
                        case 'Import':
                            comp.toolbarModule.importHandler.successHandler(result);
                            break;
                        case 'EnforceProtection':
                            docEditorComp.editor.enforceProtectionInternal(result);
                            break;
                        case 'UnprotectDocument':
                            docEditorComp.editor.onUnProtectionSuccess(result);
                            break;
                    }
                }
            }
        }
        catch (e) {
            window.sfBlazor.throwError(e, this);
        }
    },
    docEditRequestHandler: async function (jsonObject) {
        try {
            if (jsonObject instanceof FormData) {
                var file = jsonObject.get('files');
                var dataUrl = "";
                await window.sfBlazor.docEditFileReader(file).then(function (data) {
                    dataUrl = data;
                });
                var fileInfo = {
                    "documentData": dataUrl.substring(dataUrl.indexOf(',') + 1),
                    "fileName": file.name,
                    "action": 'Import'
                };
                this._dotnetInstance.invokeMethodAsync('GetDocumentInfo', fileInfo);
            } else {
                if (jsonObject.hasOwnProperty('saltBase64')) {
                    jsonObject['action'] = jsonObject.saltBase64 === '' ? 'EnforceProtection' : 'UnprotectDocument';
                } else if (jsonObject.hasOwnProperty('type') && jsonObject.hasOwnProperty('content')) {
                    jsonObject['action'] = 'SystemClipboard';
                }
                this._dotnetInstance.invokeMethodAsync('GetDocumentInfo', jsonObject);
            }
        }
        catch (e) {
            window.sfBlazor.throwError(e, this);
        }
    },
    docEditFileReader: function (file) {
        try {
            return new Promise(function (resolve, reject) {
                var fileReader = new FileReader();
                fileReader.onload = function () {
                    resolve(fileReader.result)
                };
                fileReader.readAsDataURL(file);
            });
        } catch (e) {
            window.sfBlazor.throwError(e, this);
        }
    },
};

(function () {
    sf.base.enableBlazorMode();
})();

window.sfBlazor = window.sfBlazor || {};
Object.assign(window.sfBlazor, extended);
