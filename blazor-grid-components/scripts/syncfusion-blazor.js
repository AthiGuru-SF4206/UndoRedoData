var sfBlazorBase = {
    instances: [],
    getElementByXpath: function (xPath) {
        return document.evaluate(xPath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
    },
    getElement: function (elementID, id, xPath) {
        var dom = (elementID != null && window[elementID] != null) ? window[elementID][id] : null;
        return (dom != null ? dom : window.sfBlazor.getElementByXpath(xPath));
    },
    getAttribute: function (elementID, dom, xPath, propName) {
        var element = window.sfBlazor.getElement(elementID, dom, xPath);
        if (element != null)
            return element.getAttribute(propName);
    },
    setAttribute: function (elementID, dom, xPath, propName, value) {
        (window.sfBlazor.getElement(elementID, dom, xPath)).setAttribute(propName, value);
    },
    addClass: function (elementID, dom, xPath, classList) {
        sf.base.addClass([window.sfBlazor.getElement(elementID, dom, xPath)], classList);
    },
    removeClass: function (elementID, dom, xPath, classList) {
        sf.base.removeClass([window.sfBlazor.getElement(elementID, dom, xPath)], classList);
    },
    getClassList: function (elementID, dom, xPath) {
        return Array.prototype.slice.call((window.sfBlazor.getElement(elementID, dom, xPath)).classList);
    },
    enableRipple: function (isRipple) {
        sf.base.enableRipple(isRipple);
    },
    isDevice: function (isRtl) {
        if (isRtl) {
           this.enableRtl(isRtl);
        }
        return {
            IsDevice: sf.base.Browser.isDevice,
            IsIos: sf.base.Browser.isIos,
            IsMacOs: sf.base.Browser.isMacOs
            };
    },
    animate: function (elementRef, animationSettings) {
        var animationObj = new sf.base.Animation(animationSettings);
        animationObj.animate(elementRef);
    },
    setGlobalAnimationValue: function (value) {
        sf.base.setGlobalAnimation(value);
    },
    callRipple: function (elementRef, rippleSettings) {
        sf.base.rippleEffect(elementRef, rippleSettings);
    },
    createXPathFromElement: function (elm) {
        var allNodes = document.getElementsByTagName('*');
        for (var segs = []; elm && elm.nodeType === 1; elm = elm.parentNode) {
            if (elm.hasAttribute('id')) {
                var uniqueIdCount = 0;
                for (var n = 0; n < allNodes.length; n++) {
                    if (allNodes[n].hasAttribute('id') && allNodes[n].id === elm.id) uniqueIdCount++;
                    if (uniqueIdCount > 1) break;
                };
                if (uniqueIdCount === 1) {
                    segs.unshift('id("' + elm.getAttribute('id') + '")');
                    return segs.join('/');
                } else {
                    segs.unshift(elm.localName.toLowerCase() + '[@id="' + elm.getAttribute('id') + '"]');
                }
            } else {
                for (var i = 1, sib = elm.previousSibling; sib; sib = sib.previousSibling) {
                    if (sib.localName === elm.localName) i++;
                }
                segs.unshift(elm.localName.toLowerCase() + '[' + i + ']');
            }
        }
        return segs.length ? '/' + segs.join('/') : null;
    },
    getDomObject: function (key, node, object) {
        var uuid = key + sf.base.getUniqueID(key);
        var domObject = {
            id: node.id,
            class: node.className,
            xPath: window.sfBlazor.createXPathFromElement(node),
            domUUID: uuid
        };
        var elementID = object && object["elementID"];
        if (elementID) {
            window[elementID] = sf.base.isNullOrUndefined(window[elementID]) ? {} : window[elementID];
            window[elementID][uuid] = node;
            domObject["elementID"] = elementID;
        }
        return domObject;
    },
    focusButton: function (element) {
        element.focus();
    },
    // Function for store/set the component instances into window object
    setCompInstance: function (obj) {
        window.sfBlazor.instances[obj.dataId] = obj;
    },
    // Function for retrive/get the component instances from window object which is stored by 'setCompInstance' method
    getCompInstance: function (id) {
        return window.sfBlazor.instances[id];
    },
    // Function for delete/remove the component instances from window object which is stored by 'setCompInstance' method
    disposeWindowsInstance: function (id) {
        if (id) {
            delete window.sfBlazor.instances[id];
        }
    },
    //sf-progressbutton interop start
    setProgress: function (progressElem, contElem, spinnerElem, percent, enableProgress, isVertical) {
        if (spinnerElem) {
            spinnerElem = spinnerElem.querySelector('.e-spinner');
            return window.requestAnimationFrame(function () {
                if (enableProgress) {
                    progressElem.style[isVertical ? 'height' : 'width'] = percent + '%';
                }
                contElem.parentElement.setAttribute('aria-valuenow', percent.toString());
                if (percent === 100) {
                    contElem.classList.remove('e-cont-animate', 'e-animate-end');
                    spinnerElem.style.width = 'auto';
                    spinnerElem.style.height = 'auto';
                }
            });
        }
    },
    setAnimation: function (contElem, spinnerElem, effect, duration, easing, isCenter) {
		spinnerElem = spinnerElem.querySelector('.e-spinner');
        new sf.base.Animation({}).animate(contElem, {
            duration: duration,
            name: 'Progress' + effect,
            timingFunction: easing,
            begin: function () {
                if (isCenter) {
                    spinnerElem.style.width = Math.max(spinnerElem.offsetWidth, contElem.offsetWidth) + 'px';
                    spinnerElem.style.height = Math.max(spinnerElem.offsetHeight, contElem.offsetHeight) + 'px';
                    contElem.classList.add('e-cont-animate');
                }
            },
            end: function () {
                contElem.classList.add('e-animate-end');
            }
        });
    },
    cancelAnimation: function(timerId) {
        window.cancelAnimationFrame(timerId);
    },
    //sf-progressbutton interop end
    //sf-spinner interop start
    getSpinnerTheme: function(element) {
        var theme = element ? window.getComputedStyle(element, ':after').getPropertyValue('content') : 'Material';
        return theme && theme.replace(/['"]+/g, '');
    },
    //sf-spinner interop end
    //sf-chip interop start
    chipKeydownHandler: function(element) {
        if (element) {
            element.addEventListener("keydown", function (e) {
                if (e.target && e.target.classList.contains("e-chip") && e.key == ' ') {
                    e.preventDefault();
                }
            });
        }
    },

    MediaQuery: {
        initialize: function (options) {
            if (options.dataId) {
                sf.base.extend(options, this, options);
                window.sfBlazor.setCompInstance(options);
                options.activeBreakpoint = "";
                options.isMediaChanged = false;
                options.initializeMediaQueries(options);
                options.updateActiveBreakpoint(options, true);
                return options.activeBreakpoint;
            }
            return null;
        },
        initializeMediaQueries: function (mediaQueryObj) {
            for (var i = 0; i < mediaQueryObj.mediaBreakpoints.length; i++) {
                var mq = window.matchMedia(mediaQueryObj.mediaBreakpoints[i].mediaQuery);
                mediaQueryObj.mediaBreakpoints[i].mq = mq;
                sf.base.EventHandler.add(mq, 'change', mediaQueryObj.mediaQueryChangeHandler, mediaQueryObj);
            }
        },
        updateActiveBreakpoint: function (mediaQueryObj, isInitialRender) {
            var isCurrentActiveMediaChanged = false;
            for (var i = 0; i < mediaQueryObj.mediaBreakpoints.length; i++) {
                if (mediaQueryObj.mediaBreakpoints[i].mq.matches) {
                    if ((!mediaQueryObj.isMediaChanged || isCurrentActiveMediaChanged) && !isInitialRender) mediaQueryObj.isMediaChanged = mediaQueryObj.activeBreakpoint != mediaQueryObj.mediaBreakpoints[i].breakpoint;
                    mediaQueryObj.activeBreakpoint = mediaQueryObj.mediaBreakpoints[i].breakpoint;
                    break;
                } else {
                    if (mediaQueryObj.activeBreakpoint == mediaQueryObj.mediaBreakpoints[i].breakpoint) {
                        isCurrentActiveMediaChanged = mediaQueryObj.isMediaChanged = true;
                        mediaQueryObj.activeBreakpoint = "";
                    }
                }
            }
        },
        mediaQueryChangeHandler: function () {
            this.updateActiveBreakpoint(this);
            if (this.isMediaChanged) {
                this.dotNetRef.invokeMethodAsync("UpdateActiveBreakpoint", this.activeBreakpoint);
                this.isMediaChanged = false;
            }
        },
        destroyComponent: function () {
            for (var i = 0; i < this.mediaBreakpoints.length; i++) {
                sf.base.EventHandler.remove(this.mediaBreakpoints[i].mq, 'change', this.mediaQueryChangeHandler);
            }
        },
        destroy: function (dataId) {
            if (dataId) {
                window.sfBlazor.getCompInstance(dataId).destroyComponent();
            }
        }
    },

    // Function to append the license banner alert in application body tag for Syncfusion Blazor components.
    validateBlazorLicense: function (licenseContent, claimLicenseKeyURL, trailComp) {
        if (trailComp) {
            const LicenseBanner = sf.base.createElement('div', {
                innerHTML: `<div style="position: fixed;
                width: 100%;
                height: 100%;
                top: 0;
                left: 0;
                right: 0;
                bottom: 0;
                background-color: rgba(0, 0, 0, 0.5);
                z-index: 99999;">
                    <div style="background: #FFFFFF;
                    height: 490px;
                    width: 840px;
                    font-family: Helvetica Neue, Helvetica, Arial;
                    color: #000000;
                    box-shadow: 0px 4.8px 14.4px rgb(0 0 0 / 18%), 0px 25.6px 57.6px rgb(0 0 0 / 22%);
                    display: block;
                    margin: 6% auto;
                    border-radius: 20px;">
                        <div style="
                        position: absolute;
                        width: 838px;
                        height: 80px;
                        background-color: #F9F9F9;
                        border: 1px solid #EEEEEE;
                        border-top-left-radius: 20px;
                        border-top-right-radius: 20px;">
                <img src="data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTQ2IiBoZWlnaHQ9IjMyIiB2aWV3Qm94PSIwIDAgMTQ2IDMyIiBmaWxsPSJub25lIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPgo8cGF0aCBkPSJNNDAuNTk2NSAxNS4wMDc4SDMyLjQyNUMzMS41NTU3IDE1LjAwNzggMzAuOTAzNyAxNS4xODEyIDMwLjUxMjUgMTUuNDg0NkMzMC4xMjEzIDE1LjgzMTQgMjkuOTA0IDE2LjMwODIgMjkuOTA0IDE3LjA0NTFDMjkuOTA0IDE3LjYwODYgMzAuMDc3OCAxOC4wNDIxIDMwLjQyNTYgMTguMzAyMkMzMC43NzMzIDE4LjYwNTYgMzEuMjk0OSAxOC43MzU2IDMxLjk5MDMgMTguNzM1NkgzNi4zMzY5QzM4LjExODkgMTguNzM1NiAzOS40MjI5IDE5LjA4MjQgNDAuMTYxOCAxOS43MzI2QzQwLjk0NDIgMjAuNDI2MiA0MS4yOTE5IDIxLjU1MzIgNDEuMjkxOSAyMy4xMTM3QzQxLjI5MTkgMjQuNzE3NiA0MC44NTcyIDI1Ljg4OCAzOS45ODc5IDI2LjY2ODJDMzkuMTE4NiAyNy40MDUxIDM3LjcyNzcgMjcuNzk1MyAzNS44NTg3IDI3Ljc5NTNIMjcuMDc4N1YyNS4wMjFIMzUuMzM3MkMzNi4yOTM0IDI1LjAyMSAzNi45NDU0IDI0Ljg5MSAzNy4zMzY2IDI0LjYzMDlDMzcuNzI3NyAyNC4zNzA4IDM3LjkwMTYgMjMuODk0IDM3LjkwMTYgMjMuMjg3MUMzNy45MDE2IDIyLjYzNjkgMzcuNzI3NyAyMi4xNjAxIDM3LjM4IDIxLjlDMzcuMDMyMyAyMS42Mzk5IDM2LjQyMzggMjEuNDY2NSAzNS41NTQ1IDIxLjQ2NjVIMzEuNjQyNkMyOS44NjA1IDIxLjQ2NjUgMjguNTEzMSAyMS4xMTk4IDI3LjY4NzMgMjAuMzgyOEMyNi44NjE0IDE5LjY0NTkgMjYuNDI2OCAxOC41MTg5IDI2LjQyNjggMTcuMDAxN0MyNi40MjY4IDE1LjM1NDUgMjYuODYxNCAxNC4xNDA4IDI3LjczMDcgMTMuMzYwNkMyOC42IDEyLjU4MDMgMjkuOTkwOSAxMi4yMzM1IDMxLjkwMzQgMTIuMjMzNUg0MC41OTY1VjE1LjAwNzhaIiBmaWxsPSIjMzU0M0E4Ii8+CjxwYXRoIGQ9Ik00OC4wNzI3IDI1LjI4MTFINTAuNTA2OFYxNi4zOTQ5SDUzLjU0OTNWMjcuNTM1MkM1My41NDkzIDI5LjA1MjQgNTMuMjAxNiAzMC4xNzk0IDUyLjUwNjIgMzAuOTE2M0M1MS44MTA3IDMxLjY1MzIgNTAuNzI0MSAzMiA0OS4yNDYzIDMySDQzLjMzNVYyOS42NTkySDQ4LjcyNDdDNDkuMjg5NyAyOS42NTkyIDQ5Ljc2NzkgMjkuNTI5MiA1MC4wNzIxIDI5LjIyNThDNTAuMzc2NCAyOC45NjU3IDUwLjU1MDIgMjguNTMyMiA1MC41NTAyIDI4LjAxMlYyNy44Mzg2SDQ3Ljg5ODlDNDYuMjAzNyAyNy44Mzg2IDQ0Ljk0MzIgMjcuNDkxOSA0NC4yNDc4IDI2Ljg0MTZDNDMuNTA4OSAyNi4xNDgxIDQzLjE2MTEgMjUuMDY0NCA0My4xNjExIDIzLjQ2MDVWMTYuMzk0OUg0Ni4xNjAyVjIzLjIwMDVDNDYuMTYwMiAyNC4wNjc0IDQ2LjI5MDYgMjQuNjMwOSA0Ni41NTE0IDI0Ljg5MUM0Ni43MjUzIDI1LjE1MTEgNDcuMjQ2OSAyNS4yODExIDQ4LjA3MjcgMjUuMjgxMVoiIGZpbGw9IiMzNTQzQTgiLz4KPHBhdGggZD0iTTU1Ljg5NjUgMTYuMzk0OUg2MS41OTA0QzYzLjMyOTEgMTYuMzk0OSA2NC41NDYxIDE2LjY5ODMgNjUuMjg1IDE3LjM0ODVDNjYuMDIzOSAxNy45OTg4IDY2LjM3MTYgMTkuMDgyNCA2Ni4zNzE2IDIwLjU1NjNWMjcuNzk1M0g2My4zMjkxVjIwLjk0NjRDNjMuMzI5MSAyMC4wNzk0IDYzLjE5ODcgMTkuNTE1OSA2Mi45Mzc5IDE5LjI5OTJDNjIuNjc3MSAxOS4wMzkxIDYyLjE1NTUgMTguOTA5MSA2MS4zMjk3IDE4LjkwOTFINTguODk1NlYyNy44Mzg2SDU1Ljg1M1YxNi4zOTQ5SDU1Ljg5NjVaIiBmaWxsPSIjMzU0M0E4Ii8+CjxwYXRoIGQ9Ik03NC45MzQyIDI1LjM2NzhINzguMTUwNlYyNy43OTUySDc0LjAyMTRDNzIuOTc4MiAyNy43OTUyIDcyLjEwODkgMjcuNjY1MiA3MS40NTcgMjcuNDkxOEM3MC44MDUgMjcuMjc1IDcwLjE5NjUgMjYuOTI4MyA2OS43MTgzIDI2LjQ1MTRDNjkuMTk2OCAyNS45MzEzIDY4Ljc2MjEgMjUuMjgxMSA2OC40NTc4IDI0LjU0NDJDNjguMTUzNiAyMy44MDcyIDY4LjAyMzIgMjIuOTgzNiA2OC4wMjMyIDIyLjE2QzY4LjAyMzIgMjEuMjkzMSA2OC4xNTM2IDIwLjQ2OTUgNjguNDU3OCAxOS42ODkyQzY4Ljc2MjEgMTguOTA5IDY5LjE1MzMgMTguMzAyMSA2OS43MTgzIDE3Ljc4MTlDNzAuMjM5OSAxNy4zMDUxIDcwLjgwNSAxNi45NTgzIDcxLjUwMDQgMTYuNzQxNkM3Mi4xOTU5IDE2LjUyNDkgNzMuMDIxNyAxNi40MzgyIDc0LjA2NDkgMTYuNDM4Mkg3OC4xOTQxVjE4LjkwOUg3NC45MzQyQzczLjQ5OTggMTguOTA5IDcyLjU0MzYgMTkuMTY5MSA3MS45Nzg1IDE5LjY0NTlDNzEuNDU2OSAyMC4xMjI3IDcxLjE1MjcgMjAuOTg5NyA3MS4xNTI3IDIyLjIwMzRDNzEuMTUyNyAyMi44OTY5IDcxLjI4MzEgMjMuNDYwNSA3MS41MDA0IDIzLjkzNzNDNzEuNzE3NyAyNC40MTQxIDcyLjA2NTUgMjQuNzYwOSA3Mi41MDAxIDI1LjA2NDNDNzIuNzE3NCAyNS4xOTQ0IDcyLjk3ODIgMjUuMjgxMSA3My4yODI1IDI1LjM2NzhDNzMuNjMwMiAyNS4zMjQ0IDc0LjE1MTggMjUuMzY3OCA3NC45MzQyIDI1LjM2NzhaIiBmaWxsPSIjMzU0M0E4Ii8+CjxwYXRoIGQ9Ik04MC44NDU2IDE4LjY0ODlINzguNjcyNFYxNi4zNTE1SDgwLjg0NTZWMTUuMTgxMUM4MC44NDU2IDE0LjAxMDggODEuMDYzIDEzLjIzMDUgODEuNDk3NiAxMi44NDA0QzgxLjkzMjMgMTIuNDUwMyA4Mi43NTgxIDEyLjIzMzUgODMuOTc1MSAxMi4yMzM1SDg2Ljg0MzhWMTQuNDAwOUg4NS40MDk1Qzg0Ljg4NzkgMTQuNDAwOSA4NC41NDAyIDE0LjQ4NzYgODQuMzIyOSAxNC42NjFDODQuMTA1NSAxNC44MzQ0IDgzLjk3NTEgMTUuMDk0NSA4My45NzUxIDE1LjQ0MTJWMTYuMzUxNUg4Ni44NDM4VjE4LjY0ODlIODMuOTc1MVYyNy43OTUzSDgwLjg0NTZWMTguNjQ4OVoiIGZpbGw9IiMzNTQzQTgiLz4KPHBhdGggZD0iTTk4LjQwNTYgMjcuNzk1M0g5Mi43MTE2QzkxLjAxNjUgMjcuNzk1MyA4OS44NDI5IDI3LjQ0ODUgODkuMDYwNSAyNi43OTgzQzg4LjMyMTYgMjYuMTQ4MSA4Ny45MzA0IDI1LjA2NDQgODcuOTMwNCAyMy41OTA2VjE2LjM5NDlIOTAuOTI5NVYyMy40MTcyQzkwLjkyOTUgMjQuMTk3NCA5MS4wNTk5IDI0LjY3NDMgOTEuMzIwNyAyNC45MzQ0QzkxLjU4MTUgMjUuMTk0NCA5Mi4xMDMxIDI1LjMyNDUgOTIuOTI4OSAyNS4zMjQ1SDk1LjM2M1YxNi4zOTQ5SDk4LjQwNTZWMjcuNzk1M1oiIGZpbGw9IiMzNTQzQTgiLz4KPHBhdGggZD0iTTEwMC42MjIgMjUuNDExMkgxMDcuMDExQzEwNy41NzcgMjUuNDExMiAxMDguMDExIDI1LjMyNDUgMTA4LjI3MiAyNS4xNTExQzEwOC41MzMgMjQuOTc3NyAxMDguNjYzIDI0LjY3NDMgMTA4LjY2MyAyNC4zMjc1QzEwOC42NjMgMjMuOTM3NCAxMDguNTMzIDIzLjY3NzMgMTA4LjI3MiAyMy40NjA1QzEwOC4wMTEgMjMuMjg3MSAxMDcuNTc3IDIzLjIwMDUgMTA3LjA1NSAyMy4yMDA1SDEwNC40NDdDMTAyLjg4MiAyMy4yMDA1IDEwMS44MzkgMjIuOTgzNyAxMDEuMzE4IDIyLjUwNjlDMTAwLjc1MiAyMi4wMzAxIDEwMC40OTIgMjEuMjA2NSAxMDAuNDkyIDE5Ljk5MjdDMTAwLjQ5MiAxOC43NzkgMTAwLjgzOSAxNy44Njg3IDEwMS40OTEgMTcuMjYxOEMxMDIuMTQzIDE2LjY5ODMgMTAzLjE4NyAxNi4zOTQ5IDEwNC41MzQgMTYuMzk0OUgxMTEuMDU0VjE4Ljc3OUgxMDUuNzA4QzEwNC44MzggMTguNzc5IDEwNC4yNzMgMTguODY1NyAxMDQuMDEyIDE4Ljk5NTdDMTAzLjc1MiAxOS4xNjkxIDEwMy42MjEgMTkuNDI5MiAxMDMuNjIxIDE5LjgxOTRDMTAzLjYyMSAyMC4xNjYxIDEwMy43NTIgMjAuNDI2MiAxMDMuOTY5IDIwLjU5OTZDMTA0LjE4NiAyMC43NzMgMTA0LjU3NyAyMC44NTk3IDEwNS4wNTYgMjAuODU5N0gxMDcuNzk0QzEwOS4wNTQgMjAuODU5NyAxMTAuMDExIDIxLjE2MzEgMTEwLjY2MyAyMS43MjY2QzExMS4zMTUgMjIuMjkwMiAxMTEuNjYyIDIzLjE1NzEgMTExLjY2MiAyNC4yNDA4QzExMS42NjIgMjUuMjgxMSAxMTEuMzU4IDI2LjE0ODEgMTEwLjc5MyAyNi43OTgzQzExMC4yMjggMjcuNDQ4NSAxMDkuNDQ2IDI3Ljc5NTMgMTA4LjUzMyAyNy43OTUzSDEwMC43MDlWMjUuNDExMkgxMDAuNjIyWiIgZmlsbD0iIzM1NDNBOCIvPgo8cGF0aCBkPSJNMTE2LjU3NCAxNS4wOTQ0SDExMy40MDFWMTIuMjc2OUgxMTYuNTc0VjE1LjA5NDRaTTExNi41NzQgMjcuNzk1M0gxMTMuNDAxVjE2LjM5NDlIMTE2LjU3NFYyNy43OTUzWiIgZmlsbD0iIzM1NDNBOCIvPgo8cGF0aCBkPSJNMTMwLjMwOSAyMi4xMTY3QzEzMC4zMDkgMjMuODkzOSAxMjkuNzQ0IDI1LjMyNDQgMTI4LjY1NyAyNi40MDgxQzEyNy41NzEgMjcuNDkxOCAxMjYuMDkzIDI4LjAxMiAxMjQuMjI0IDI4LjAxMkMxMjIuMzU1IDI4LjAxMiAxMjAuODc3IDI3LjQ5MTggMTE5Ljc5IDI2LjQwODFDMTE4LjcwNCAyNS4zMjQ0IDExOC4xMzkgMjMuODkzOSAxMTguMTM5IDIyLjExNjdDMTE4LjEzOSAyMC4zMzk0IDExOC43MDQgMTguOTA5IDExOS43OSAxNy44MjUzQzEyMC44NzcgMTYuNzQxNiAxMjIuMzk4IDE2LjIyMTQgMTI0LjIyNCAxNi4yMjE0QzEyNi4wNDkgMTYuMjIxNCAxMjcuNTI3IDE2Ljc0MTYgMTI4LjY1NyAxNy44MjUzQzEyOS43NDQgMTguODY1NiAxMzAuMzA5IDIwLjI5NjEgMTMwLjMwOSAyMi4xMTY3Wk0xMjEuMjY4IDIyLjExNjdDMTIxLjI2OCAyMy4yMDA0IDEyMS41MjkgMjQuMDY3MyAxMjIuMDUxIDI0LjY3NDJDMTIyLjU3MiAyNS4yODExIDEyMy4yNjggMjUuNTg0NSAxMjQuMTggMjUuNTg0NUMxMjUuMDkzIDI1LjU4NDUgMTI1Ljc4OSAyNS4yODExIDEyNi4zMSAyNC42NzQyQzEyNi44MzIgMjQuMDY3MyAxMjcuMDkzIDIzLjIwMDQgMTI3LjA5MyAyMi4xMTY3QzEyNy4wOTMgMjEuMDMzIDEyNi44MzIgMjAuMTY2MSAxMjYuMzEgMTkuNjAyNUMxMjUuNzg5IDE4Ljk5NTcgMTI1LjA5MyAxOC42OTIyIDEyNC4xMzcgMTguNjkyMkMxMjMuMjI0IDE4LjY5MjIgMTIyLjUyOSAxOC45OTU3IDEyMi4wMDcgMTkuNjAyNUMxMjEuNTI5IDIwLjE2NjEgMTIxLjI2OCAyMS4wMzMgMTIxLjI2OCAyMi4xMTY3WiIgZmlsbD0iIzM1NDNBOCIvPgo8cGF0aCBkPSJNMTMxLjc4NyAxNi4zOTQ5SDEzNy40ODFDMTM5LjIxOSAxNi4zOTQ5IDE0MC40MzYgMTYuNjk4MyAxNDEuMTc1IDE3LjM0ODVDMTQxLjkxNCAxNy45OTg4IDE0Mi4yNjIgMTkuMDgyNCAxNDIuMjYyIDIwLjU1NjNWMjcuNzk1M0gxMzkuMjE5VjIwLjk0NjRDMTM5LjIxOSAyMC4wNzk0IDEzOS4wODkgMTkuNTE1OSAxMzguODI4IDE5LjI5OTJDMTM4LjU2NyAxOS4wMzkxIDEzOC4wNDYgMTguOTA5MSAxMzcuMjIgMTguOTA5MUgxMzQuNzg2VjI3LjgzODZIMTMxLjc0M1YxNi4zOTQ5SDEzMS43ODdaIiBmaWxsPSIjMzU0M0E4Ii8+CjxwYXRoIGQ9Ik03LjEyODMxIDMuNzM3NDNIMFYxMC44NDY0SDcuMTI4MzFWMy43Mzc0M1oiIGZpbGw9IiMzNTQzQTgiLz4KPHBhdGggZD0iTTIzLjI1MTMgLTIuMTU3MjVlLTA1TDE4LjU1MTMgNS41MTY4NUwyNC4wODMxIDEwLjIwNDFMMjguNzgzMSA0LjY4NzI1TDIzLjI1MTMgLTIuMTU3MjVlLTA1WiIgZmlsbD0iI0ZGODYwMCIvPgo8cGF0aCBkPSJNMTUuNjA0MSAzLjczNzQzSDguNDc1ODNWMTAuODQ2NEgxNS42MDQxVjMuNzM3NDNaIiBmaWxsPSIjMzU0M0E4Ii8+CjxwYXRoIGQ9Ik03LjEyODMxIDEyLjE5MDJIMFYxOS4yOTkySDcuMTI4MzFWMTIuMTkwMloiIGZpbGw9IiMzNTQzQTgiLz4KPHBhdGggZD0iTTE1LjYwNDEgMTIuMTkwMkg4LjQ3NTgzVjE5LjI5OTJIMTUuNjA0MVYxMi4xOTAyWiIgZmlsbD0iIzM1NDNBOCIvPgo8cGF0aCBkPSJNMjQuMDc5NyAxMi4xOTAySDE2Ljk1MTRWMTkuMjk5MkgyNC4wNzk3VjEyLjE5MDJaIiBmaWxsPSIjRkY4NjAwIi8+CjxwYXRoIGQ9Ik03LjEyODMxIDIwLjY4NjNIMFYyNy43OTUzSDcuMTI4MzFWMjAuNjg2M1oiIGZpbGw9IiMzNTQzQTgiLz4KPHBhdGggZD0iTTE1LjYwNDEgMjAuNjg2M0g4LjQ3NTgzVjI3Ljc5NTNIMTUuNjA0MVYyMC42ODYzWiIgZmlsbD0iIzM1NDNBOCIvPgo8cGF0aCBkPSJNMjQuMTIzMiAyMC42ODYzSDE2Ljk5NDlWMjcuNzk1M0gyNC4xMjMyVjIwLjY4NjNaIiBmaWxsPSIjMzU0M0E4Ii8+CjxwYXRoIGQ9Ik0xNDYgMTUuODMxM0MxNDYgMTYuODcxNyAxNDUuMTc0IDE3LjY5NTMgMTQ0LjEzMSAxNy42OTUzQzE0My4wODggMTcuNjk1MyAxNDIuMjYyIDE2Ljg3MTcgMTQyLjI2MiAxNS44MzEzQzE0Mi4yNjIgMTQuNzkxIDE0My4wODggMTQuMDEwNyAxNDQuMTMxIDE0LjAxMDdDMTQ1LjEzMSAxMy45Njc0IDE0NiAxNC43OTEgMTQ2IDE1LjgzMTNaTTE0Mi45NTcgMTQuNzkxQzE0Mi42OTcgMTUuMDUxMSAxNDIuNTY2IDE1LjQ0MTIgMTQyLjU2NiAxNS44MzEzQzE0Mi41NjYgMTYuNjk4MyAxNDMuMjYyIDE3LjM5MTggMTQ0LjEzMSAxNy4zOTE4QzE0NSAxNy4zOTE4IDE0NS42OTYgMTYuNjk4MyAxNDUuNjk2IDE1LjgzMTNDMTQ1LjY5NiAxNS4wMDc3IDE0NSAxNC4yNzA4IDE0NC4xNzQgMTQuMjcwOEMxNDMuNjUzIDE0LjI3MDggMTQzLjI2MiAxNC40NDQyIDE0Mi45NTcgMTQuNzkxWk0xNDQuODcgMTYuOTE1SDE0NC40NzlMMTQzLjkxNCAxNi4wOTE0VjE2LjkxNUgxNDMuNjA5VjE0Ljc0NzZIMTQzLjk1N0MxNDQuNDM1IDE0Ljc0NzYgMTQ0LjY1MyAxNC45NjQ0IDE0NC42NTMgMTUuMzU0NUMxNDQuNjUzIDE1LjY1NzkgMTQ0LjQ3OSAxNS44NzQ3IDE0NC4xNzQgMTUuOTYxNEwxNDQuODcgMTYuOTE1Wk0xNDQuMDQ0IDE1LjY1NzlDMTQ0LjI2MSAxNS42NTc5IDE0NC4zOTIgMTUuNTI3OSAxNDQuMzkyIDE1LjM1NDVDMTQ0LjM5MiAxNS4xMzc4IDE0NC4yNjEgMTUuMDUxMSAxNDQuMDAxIDE1LjA1MTFIMTQzLjkxNFYxNS42NTc5SDE0NC4wNDRaIiBmaWxsPSIjMzU0M0E4Ii8+Cjwvc3ZnPgo=" style="
                text-align: left;
                width: 146px;
                position: absolute;
                top: 24px;
                left: 40px;"></div>
                <div style="position: relative;
                top: 104px;
                left: 32px;
                font-size: 20px;
                text-align: left;
                font-weight: 700;
                letter-spacing: 0.02em;
                font-style: normal;
                line-height: 125%;">Claim your FREE account and get a key in less than a minute</div>
                        <ul style="font-size: 15px;
                        margin-top: 15px;
                        font-weight: 400;
                        color: #333333;
                        letter-spacing: 0.01em;
                        position: relative;
                        left: 40px;
                        top: 103px;
                        line-height: 180%;">
                            <li><span>Access to a 30-day free trial of any of our products.</span></li>
                            <li><span>Access to 24x5 support by developers via the <a
                                        href="https://support.syncfusion.com/create"
                                        style="text-decoration: none;
                            color: #0D6EFD;
                            font-weight: 500;">support tickets</a>, <a
                                        href="https://www.syncfusion.com/forums"
                                        style="text-decoration: none;
                            color: #0D6EFD;
                            font-weight: 500;">forum</a>, <a
                                        href="https://www.syncfusion.com/feedback"
                                        style="text-decoration: none;
                            color: #0D6EFD;
                            font-weight: 500;">feature & feedback page</a> and chat.</span></li>
                            <li><span>200+ <a
                                        href="https://www.syncfusion.com/succinctly-free-ebooks"
                                        style="text-decoration: none;
                            color: #0D6EFD;
                            font-weight: 500;">ebooks</a> on the latest technologies, industry trends, and research topics.</span>
                            </li>
                            <li><span>Largest collection of over 7,500 flat and wireframe icons for free with Syncfusion <a
                                        href="https://www.syncfusion.com/downloads/metrostudio"
                                        style="text-decoration: none;
                            color: #0D6EFD;
                            font-weight: 500;">Metro Studio</a>.</span></li>
                            <li><span>Free and unlimited access to Syncfusion technical <a
                                        href="https://www.syncfusion.com/blogs/"
                                        style="text-decoration: none;
                            color: #0D6EFD;
                            font-weight: 500;">blogs</a> and <a
                                        href="https://www.syncfusion.com/resources/techportal/whitepapers"
                                        style="text-decoration: none;
                            color: #0D6EFD;
                            font-weight: 500;">white papers.</a></span></li>
                        </ul>
                        <div style="font-size: 18px;
                        font-weight: 700;
                        position: relative;
                        line-height: 125%;
                        letter-spacing: 0.02em;
                        top: 113px;
                        left: 32px;">Trusted by the world's leading companies</div>
                <img src='data:image/png;base64,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'style="width: 742.5px;
                    position: relative;
                    top: 123px;
                    left: 32px;"/>
                    <a href="${claimLicenseKeyURL}" style="float: left;
                    border-radius: 56px;
                    background: #0D6EFD;
                    padding-top: 12px;
                    width: 280px;
                    height: 45px;
                    text-align: center;
                    position: relative;
                    top: 141px;
                    left: 270px;
                    font-size: 17px;
                    color: white;
                    text-decoration: none;
                    line-height: 125%;
                    letter-spacing: 0.02em;">Claim your free account</a>
                        <div style="
                        font-size: 14px;
                        position: relative;
                        top: 197px;
                        left: 15px;
                        letter-spacing: 0.02em;
                        font-weight: 500;
                        line-height: 125%;">have a Syncfusion account? <a
                href="https://www.syncfusion.com/account/login?ReturnUrl=/account/login"
                style="text-decoration: none;
                color: #0D6EFD;
                font-weight: 500;">Sign In</a></div>
                    </div>
                </div>` });
            document.body.appendChild(LicenseBanner);
        } else {
			if (this.LicenseBanner){
				this.LicenseBanner.remove();
			}
            if(!document.body.contains(this.LicenseBanner)){
                this.LicenseBanner = sf.base.createElement('div', {
                    innerHTML: `<img src='data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjQiIGhlaWdodD0iMjQiIHZpZXdCb3g9IjAgMCAyNCAyNCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPGcgY2xpcC1wYXRoPSJ1cmwoI2NsaXAwXzE5OV80KSI+CjxwYXRoIGQ9Ik0xMiAyMUMxNi45NzA2IDIxIDIxIDE2Ljk3MDYgMjEgMTJDMjEgNy4wMjk0NCAxNi45NzA2IDMgMTIgM0M3LjAyOTQ0IDMgMyA3LjAyOTQ0IDMgMTJDMyAxNi45NzA2IDcuMDI5NDQgMjEgMTIgMjFaIiBzdHJva2U9IiM3MzczNzMiIHN0cm9rZS13aWR0aD0iMiIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIi8+CjxwYXRoIGQ9Ik0xMS4yNSAxMS4yNUgxMlYxNi41SDEyLjc1IiBmaWxsPSIjNjE2MDYzIi8+CjxwYXRoIGQ9Ik0xMS4yNSAxMS4yNUgxMlYxNi41SDEyLjc1IiBzdHJva2U9IiM3MzczNzMiIHN0cm9rZS13aWR0aD0iMiIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIi8+CjxwYXRoIGQ9Ik0xMS44MTI1IDlDMTIuNDMzOCA5IDEyLjkzNzUgOC40OTYzMiAxMi45Mzc1IDcuODc1QzEyLjkzNzUgNy4yNTM2OCAxMi40MzM4IDYuNzUgMTEuODEyNSA2Ljc1QzExLjE5MTIgNi43NSAxMC42ODc1IDcuMjUzNjggMTAuNjg3NSA3Ljg3NUMxMC42ODc1IDguNDk2MzIgMTEuMTkxMiA5IDExLjgxMjUgOVoiIGZpbGw9IiM3MzczNzMiLz4KPC9nPgo8ZGVmcz4KPGNsaXBQYXRoIGlkPSJjbGlwMF8xOTlfNCI+CjxyZWN0IHdpZHRoPSIyNCIgaGVpZ2h0PSIyNCIgZmlsbD0id2hpdGUiLz4KPC9jbGlwUGF0aD4KPC9kZWZzPgo8L3N2Zz4K' style="
                position: absolute;
                left: 16px;
                height: 24px;"/><span>${licenseContent} <a style="text-decoration: none;color: #0D6EFD;font-weight: 500;" href="${claimLicenseKeyURL}">Claim your free account</a>.</span>`
                });
                this.LicenseBanner.setAttribute('style', `position: fixed;
                top: 10px;
                left: 10px;
                right: 10px;
                font-size: 14px;
                background: #EEF2FF;
                color: #222222;
                z-index: 999999999;
                text-align: left;
                border: 1px solid #EEEEEE;
                padding: 10px 11px 10px 50px;
                border-radius: 8px;
                font-family: Helvetica Neue, Helvetica, Arial;`);
                document.body.appendChild(this.LicenseBanner);
            }
        }
    }
};

(function () {
    sf.base.enableBlazorMode();
})();

(function () {
    // Prevent multiple initializations if the script is accidentally loaded more than once
    if (window.SfStyleWatcher) return;

    /**
     * Minimal microtask scheduler to batch multiple style writes into a single batch.
     * queueMicrotask finishes before the browser does things like rendering the page Ensures no flicker.
     */
    const queue = [];
    let queued = false; // flag
    /**
     * Schedule a function to run in the next microtask batch.
     * Multiple calls before the batch executes will queue multiple functions.
     */
    function schedule(fn) {
        queue.push(fn);
        if (!queued) {
            queued = true;
            queueMicrotask(() => {
                try {
                    // Execute all queued style updates in order
                    for (let i = 0; i < queue.length; i++) {
                        queue[i]()
                    }
                } finally {
                    // Always reset the queue and flag
                    queue.length = 0;
                    queued = false;
                }
            });
        }
    }

    /**
     * Parse a CSS style text into a property map.
     * Handles !important priority and trims whitespace.
     *
     * @param {string} styleText - Raw style text
     * @returns {Object<string, { value: string, priority: string }>} - Parsed styles
     */
    function parseStyleText(styleText) {
        const map = {};
        if (!styleText) return map;
        const parts = styleText.split(';');
        for (let i = 0; i < parts.length; i++) {
            const part = parts[i].trim();
            if (!part) continue;
            const colonIndex = part.indexOf(':');
            if (colonIndex === -1) continue;
            const prop = part.slice(0, colonIndex).trim();
            let value = part.slice(colonIndex + 1).trim();
            let priority = '';
            if (/!important$/i.test(value)) {
                value = value.replace(/!important$/i, '').trim();
                priority = 'important';
            }
            if (prop) {
                map[prop] = { value, priority };
            }
        }
        return map;
    }

    /**
     * Apply inline styles from the data-sf-style attribute without overriding
     * styles set by component scripts. Only properties present in data-sf-style
     * are updated, and previously applied properties are removed when absent.
     *
     * Uses cached values (sfAppliedStyleText/sfAppliedStyleMap) to skip redundant writes.
     * All actual DOM writes are deferred to the microtask scheduler to avoid flicker.
     *
     * @param {Element} element - The DOM element to style
     * @param {string|null} value - The raw value of the data-sf-style attribute
     */
    function applySfStyle(element, styleValue) {
        const newCss = styleValue || '';
        const prevCss = element.sfAppliedStyleText || '';

        // Skip if the style string hasn't changed
        if (newCss === prevCss) return;

        const newMap = parseStyleText(newCss);
        const prevMap = element.sfAppliedStyleMap || {};

        // Apply in microtask to coalesce rapid mutations and avoid flicker
        schedule(() => {
            // Remove properties that were previously applied but no longer present
            Object.keys(prevMap).forEach(prop => {
                if (!newMap.hasOwnProperty(prop)) {
                    element.style.removeProperty(prop);
                }
            });

            // Set/update current properties from data-sf-style
            Object.keys(newMap).forEach(prop => {
                const entry = newMap[prop];
                element.style.setProperty(prop, entry.value, entry.priority);
            });

            // Update cache so future comparisons know what we last applied
            element.sfAppliedStyleText = newCss;
            element.sfAppliedStyleMap = newMap;

            if (element.hasAttribute('data-sf-style')) {
                // Mark intentional removal expando flag
                element.isDataAttrRemoved = true;
                element.removeAttribute('data-sf-style');
                // Remove flag from the element so we don't accidentally suppress future real removals
                queueMicrotask(() => { delete element.isDataAttrRemoved; });
            }
        });
    }

    /**
       * Clear inline styles that were previously applied by this script.
       * Important: Only clears if we previously set something (checks cache).
       * This prevents accidentally wiping user- or framework-defined inline styles.
       * 
       * @param {Element} element - The DOM element to potentially clear
       */
    function clearSfStyle(element) {
        // Only act if this script previously applied a style to this element
        if (element.sfAppliedStyleMap !== undefined || element.sfAppliedStyleText !== undefined) {
            schedule(() => {
                const prevMap = element.sfAppliedStyleMap || {};
                Object.keys(prevMap).forEach(prop => {
                    element.style.removeProperty(prop);
                });
                // Remove cache entries to indicate we no longer manage this element's styles
                delete element.sfAppliedStyleText;
                delete element.sfAppliedStyleMap;
            });
        }
    }

    /**
     * Process a single element if it has data-sf-style.
     */
    function processElement(element) {
        if (!element || element.nodeType !== 1) return;
        if (element.hasAttribute('data-sf-style')) {
            applySfStyle(element, element.getAttribute('data-sf-style'));
        }
    }

    /**
     * Recursively process a subtree for elements with data-sf-style attributes.
     * 
     * Used both on initial load and when new nodes are added to the DOM.
     * 
     * @param {Node} root - The root node of the subtree (usually an Element)
     */
    function processTree(root) {
        if (!root || root.nodeType !== 1) return;
        // Process root itself
        processElement(root);
        // Process descendants
        if (root.querySelectorAll) {
            root.querySelectorAll('[data-sf-style]').forEach(processElement);
        }
    }

    /**
       * MutationObserver callback - reacts to DOM changes.
       * 
       * Handles:
       * New nodes being added (childList mutations)
       * Changes to data-sf-style attribute
       */
    const observer = new MutationObserver(mutationsList => {
        for (const mutation of mutationsList) {
            if (mutation.type === 'childList') {
                // New nodes added
                mutation.addedNodes.forEach(node => processTree(node));
            } else if (mutation.type === 'attributes') {
                const element = mutation.target;
                if (mutation.attributeName === 'data-sf-style') {
                    const styleValue = element.getAttribute('data-sf-style');
                    // Attribute removed
                    if (styleValue === null) {
                        // To ensure if its not called from removal of data attr we intentionally did
                        if (!element.isDataAttrRemoved)
                            clearSfStyle(element);
                    } else {
                        // Attribute changed/added
                        applySfStyle(element, styleValue);
                    }
                } else if (mutation.attributeName === 'style') {
                    // If data-sf-style still exists, re-apply our styles to prevent visible flicker.
                    if (element.hasAttribute('data-sf-style')) {
                        applySfStyle(element, element.getAttribute('data-sf-style'));
                    }
                }
            }
        }
    });

    // Observe the entire document for subtree changes, new nodes, and specific attributes
    observer.observe(document.documentElement, {
        subtree: true,
        childList: true,
        attributes: true,
        // Limited attribute monitoring to only what we need
        attributeFilter: ['data-sf-style', 'style'] // narrow filter for performance
    });

    // Process the existing DOM once at startup
    processTree(document.documentElement);

    /**
       * Public API exposed on window.SfStyleWatcher
       * 
       * Useful for manual refresh
       */
    window.SfStyleWatcher = {
        refresh(root = document.documentElement) {
            processTree(root);
        },
        // Stop observing mutations
        disconnect() {
            observer.disconnect();
        }
    };
})();

window.sfBlazor = window.sfBlazor || {};
Object.assign(window.sfBlazor, sfBlazorBase);
