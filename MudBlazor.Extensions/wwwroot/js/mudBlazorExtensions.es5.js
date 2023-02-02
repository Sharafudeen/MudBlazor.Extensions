﻿'use strict';

var _createClass = (function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ('value' in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; })();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError('Cannot call a class as a function'); } }

var MudBlazorExtensionHelper = (function () {
    function MudBlazorExtensionHelper(options, dotNet, onDone) {
        _classCallCheck(this, MudBlazorExtensionHelper);

        this.dotnet = dotNet;
        this.onDone = onDone;
        this.options = options;
        this.mudDialogSelector = options.mudDialogSelector || '.mud-dialog:not([data-mud-extended=true])';
        this.mudDialogHeaderSelector = options.mudDialogHeaderSelector || '.mud-dialog-title';
        this.dialog = document.querySelector(this.mudDialogSelector);
        this.dialogHeader = this.dialog.querySelector(this.mudDialogHeaderSelector);
    }

    _createClass(MudBlazorExtensionHelper, [{
        key: 'findDialog',
        value: function findDialog() {
            return Array.from(document.querySelectorAll(this.mudDialogSelector)).filter(function (d) {
                return !d.__extended;
            })[0];
        }
    }, {
        key: 'init',
        value: function init() {
            var _this = this;

            this.dialog = this.dialog || this.findDialog();
            if (!this.dialog) {
                this.observer = new MutationObserver(function (e) {
                    var res = e.filter(function (x) {
                        return x.addedNodes && x.addedNodes[0] === _this.findDialog();
                    });
                    if (res.length) {
                        _this.dialog = res[0].addedNodes[0];
                        _this.observer.disconnect();
                        _this.handleExtensions();
                    }
                });
                this.observer.observe(document, { characterData: true, childList: true, subtree: true });
            } else {
                this.handleExtensions();
            }
        }
    }, {
        key: 'handleExtensions',
        value: function handleExtensions() {
            var _this2 = this;

            this.dialog.__extended = true;
            this.dialog.setAttribute('data-mud-extended', true);
            if (this.options.showAtCursor) {
                this.moveElementToMousePosition(this.dialog);
            }
            setTimeout(function () {
                return _this2.dialog.classList.remove('mud-ex-dialog-initial');
            }, 50);
            // For animations
            if (this.options.animations != null && Array.isArray(this.options.animations) && this.options.animations.length) {
                this.animate(this.options.animationDescriptions);
            }

            if (this.options.disablePositionMargin) {
                this.dialog.classList.add('mud-dialog-position-fixed');
            }

            // Full height ext
            if (this.options.fullHeight) {
                var cls = this.options.disableSizeMarginY ? 'mud-dialog-height-full-no-margin' : 'mud-dialog-height-full';
                this.dialog.classList.add(cls);
                var actions = this.dialog.querySelector('.mud-dialog-actions');
                if (actions) {
                    actions.classList.add('mud-dialog-actions-fixed-bottom');
                }
            }
            if (this.options.fullWidth && this.options.disableSizeMarginX) {
                this.dialog.classList.remove('mud-dialog-width-full');
                this.dialog.classList.add('mud-dialog-width-full-no-margin');
                if (this.dialog.classList.contains('mud-dialog-width-false')) {
                    this.dialog.classList.remove('mud-dialog-width-false');
                }
            }

            // Inject buttons
            if (this.options.buttons && this.options.buttons.length) {
                var dialogButtonWrapper = document.createElement('div');
                dialogButtonWrapper.classList.add('mud-ex-dialog-header-actions');
                if (!this.options.closeButton) {
                    dialogButtonWrapper.style.right = '8px'; // No close button, so we need to move the buttons to the right (48 - button width of 40)
                }

                if (this.dialogHeader) {
                    dialogButtonWrapper = this.dialogHeader.insertAdjacentElement('beforeend', dialogButtonWrapper);
                }
                this.options.buttons.reverse().forEach(function (b) {
                    if (dialogButtonWrapper) {
                        dialogButtonWrapper.insertAdjacentHTML('beforeend', b.html);
                        var btnEl = _this2.dialogHeader.querySelector('#' + b.id);
                        btnEl.onclick = function () {
                            if (b.id.indexOf('mud-button-maximize') >= 0) {
                                _this2.maximize();
                            }
                            if (b.id.indexOf('mud-button-minimize') >= 0) {
                                _this2.minimize();
                            } else {
                                b.callBackReference.invokeMethodAsync(b.callbackName);
                            }
                        };
                    }
                });
            }

            if (this.options.modal === false) {
                this.dialog.parentElement.querySelector('.mud-overlay').style.display = 'none';
                setTimeout(function () {
                    _this2.makeDialogAbsolute();
                    _this2.dialog.parentElement.style.height = '0';
                }, this.options.animationDurationInMs + 50);
            }

            // Handle drag
            if (this.options.dragMode !== 0 && this.dialog) {
                this.dragElement(this.dialog, this.dialogHeader, document.body, this.options.dragMode === 2);
                this.done();
            }

            // Resize
            this.checkResizeable();
        }
    }, {
        key: 'minimize',
        value: function minimize() {
            var targetElement = document.querySelector('.mud-ex-task-bar-item-for-' + this.dialog.id);
            this.moveToElement(targetElement);
        }
    }, {
        key: 'moveToElement',
        value: function moveToElement(targetElement) {
            var rect = targetElement.getBoundingClientRect();
            this.dialog.style.height = targetElement.style.height;
            this.dialog.style.width = targetElement.style.width;
            this.dialog.style.left = rect.left + "px";
            this.dialog.style.top = rect.top + "px";
        }
    }, {
        key: 'makeDialogAbsolute',
        value: function makeDialogAbsolute() {
            this.dialog.style.position = 'absolute';
            var rect = this.dialog.getBoundingClientRect();
            this.dialog.style.left = rect.left + "px";
            this.dialog.style.top = rect.top + "px";
        }
    }, {
        key: 'moveElementToMousePosition',
        value: function moveElementToMousePosition(element) {
            var e = MudBlazorExtensions.getCurrentMousePosition();
            var x = e.clientX;
            var y = e.clientY;
            var origin = this.options.cursorPositionOriginName.split('-');

            var maxWidthFalseOrLargest = this.options.maxWidth === 6 || this.options.maxWidth === 4; // 4=xxl 6=false
            if (!this.options.fullWidth || !maxWidthFalseOrLargest) {
                if (origin[1] === 'left') {
                    element.style.left = x + 'px';
                } else if (origin[1] === 'right') {
                    element.style.left = x - element.offsetWidth + 'px';
                } else if (origin[1] === 'center') {
                    element.style.left = x - element.offsetWidth / 2 + 'px';
                }
            }
            if (!this.options.fullHeight) {
                if (origin[0] === 'top') {
                    element.style.top = y + 'px';
                } else if (origin[0] === 'bottom') {
                    element.style.top = y - element.offsetHeight + 'px';
                } else if (origin[0] === 'center') {
                    element.style.top = y - element.offsetHeight / 2 + 'px';
                }
            }
            this.ensureElementIsInScreenBounds(element);
        }
    }, {
        key: 'ensureElementIsInScreenBounds',
        value: function ensureElementIsInScreenBounds(element) {
            var _this3 = this;

            var rect = element.getBoundingClientRect();
            var rectIsEmpty = rect.width === 0 && rect.height === 0;
            if (rectIsEmpty) {
                var _ret = (function () {
                    var ro = new ResizeObserver(function (entries) {
                        ro.disconnect();
                        _this3.ensureElementIsInScreenBounds(element);
                    });

                    ro.observe(element);
                    return {
                        v: undefined
                    };
                })();

                if (typeof _ret === 'object') return _ret.v;
            }

            var animationIsRunning = !!element.getAnimations().length;
            if (animationIsRunning) {
                element.addEventListener('animationend', function (e) {
                    return _this3.ensureElementIsInScreenBounds(element);
                }, { once: true });
                return;
            }
            if (rect.left < 0) {
                element.style.left = '0px';
            }
            if (rect.top < 0) {
                element.style.top = '0px';
            }
            if (rect.right > window.innerWidth) {
                element.style.left = window.innerWidth - element.offsetWidth + 'px';
            }
            if (rect.bottom > window.innerHeight) {
                element.style.top = window.innerHeight - element.offsetHeight + 'px';
            }
        }
    }, {
        key: 'checkResizeable',
        value: function checkResizeable() {
            this.dialog.style.position = 'absolute';
            if (this.options.resizeable) {
                //this.dialog.style.position = 'absolute';
                this.dialog.style['resize'] = 'both';
                this.dialog.style['overflow'] = 'auto';
                this.dialog.style.maxWidth = window.innerWidth + 'px';
                this.dialog.style.maxHeight = window.innerHeight + 'px';
                this.dialog.style.minWidth = '100px';
                this.dialog.style.minHeight = '100px';
            }
        }
    }, {
        key: 'maximize',
        value: function maximize() {
            if (this._oldStyle) {
                this.dialog.style = this._oldStyle;
                delete this._oldStyle;
            } else {
                this._oldStyle = this.dialog.style;
                this.dialog.style.position = 'absolute';
                this.dialog.style.left = "0";
                this.dialog.style.top = "0";

                this.dialog.style.maxWidth = this.dialog.style.width = window.innerWidth + 'px';
                this.dialog.style.maxHeight = this.dialog.style.height = window.innerHeight + 'px';
            }
            this.checkResizeable();
        }
    }, {
        key: 'done',
        value: function done() {
            if (this.observer) this.observer.disconnect();
            if (this.onDone) this.onDone();
        }
    }, {
        key: 'dragElement',
        value: function dragElement(dialogEl, headerEl, container, disableBoundCheck) {
            var pos1 = 0,
                pos2 = 0,
                pos3 = 0,
                pos4 = 0;
            container = container || document.body;
            if (headerEl) {
                // if present, the header is where you move the DIV from:
                headerEl.style.cursor = 'move';
                headerEl.onmousedown = dragMouseDown;
            } else {
                // otherwise, move the DIV from anywhere inside the DIV:
                dialogEl.onmousedown = dragMouseDown;
            }

            function dragMouseDown(e) {
                e = e || window.event;
                e.preventDefault();
                // get the mouse cursor position at startup:
                pos3 = e.clientX;
                pos4 = e.clientY;
                document.onmouseup = closeDragElement;
                // call a function whenever the cursor moves:
                document.onmousemove = elementDrag;
            }

            function elementDrag(e) {
                e = e || window.event;
                e.preventDefault();
                // calculate the new cursor position:

                pos1 = pos3 - e.clientX;
                pos2 = pos4 - e.clientY;
                pos3 = e.clientX;
                pos4 = e.clientY;
                var bounds = {
                    x: container.offsetWidth - dialogEl.offsetWidth,
                    y: (container === document.body ? window.innerHeight : container.offsetHeight) - dialogEl.offsetHeight
                };

                var aY = dialogEl.offsetTop - pos2;
                var aX = dialogEl.offsetLeft - pos1;
                dialogEl.style.position = 'absolute';

                if (disableBoundCheck || aX > 0 && aX < bounds.x) {
                    dialogEl.style.left = aX + "px";
                }
                if (disableBoundCheck || aY > 0 && aY < bounds.y) {
                    dialogEl.style.top = aY + "px";
                }

                pos1 = pos3 - e.clientX;
                pos2 = pos4 - e.clientY;
                pos3 = e.clientX;
                pos4 = e.clientY;
            }

            function closeDragElement() {
                // stop moving when mouse button is released:
                document.onmouseup = null;
                document.onmousemove = null;
            }
        }
    }, {
        key: 'animate',
        value: function animate(types) {
            //var names = types.map(type => this.options.dialogPositionNames.map(n => `kf-mud-dialog-${type}-${n} ${this.options.animationDurationInMs}ms ${this.options.animationTimingFunctionString} 1 alternate`));
            //this.dialog.style.animation = `${names.join(',')}`;
            this.dialog.style.animation = '' + this.options.animationStyle;
        }
    }]);

    return MudBlazorExtensionHelper;
})();

window.MudBlazorExtensions = {
    helper: null,
    currentMouseArgs: null,

    __bindEvents: function __bindEvents() {
        var onMouseUpdate = function onMouseUpdate(e) {
            window.MudBlazorExtensions.currentMouseArgs = e;
        };
        document.addEventListener('mousemove', onMouseUpdate, false);
        document.addEventListener('mouseenter', onMouseUpdate, false);
    },

    getCurrentMousePosition: function getCurrentMousePosition() {
        return window.MudBlazorExtensions.currentMouseArgs;
    },

    setNextDialogOptions: function setNextDialogOptions(options, dotNet) {
        new MudBlazorExtensionHelper(options, dotNet, function () {
            MudBlazorExtensions.helper = null;
            delete MudBlazorExtensions.helper;
        }).init();
    },

    addCss: function addCss(cssContent) {
        var css = cssContent,
            head = document.head || document.getElementsByTagName('head')[0],
            style = document.createElement('style');

        head.appendChild(style);

        style.type = 'text/css';
        if (style.styleSheet) {
            // This is required for IE8 and below.
            style.styleSheet.cssText = css;
        } else {
            style.appendChild(document.createTextNode(css));
        }
    },

    downloadFile: function downloadFile(options) {
        var fileUrl = options.url || "data:" + options.mimeType + ";base64," + options.base64String;
        fetch(fileUrl).then(function (response) {
            return response.blob();
        }).then(function (blob) {
            var link = window.document.createElement("a");
            //link.href = window.URL.createObjectURL(blob, { type: options.mimeType });
            link.href = window.URL.createObjectURL(blob);
            link.download = options.fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        });
    },

    attachDialog: function attachDialog(dialogId) {
        if (dialogId) {
            var dialog = document.getElementById(dialogId);
            if (dialog) {
                var titleCmp = dialog.querySelector('.mud-dialog-title');
                var iconCmp = titleCmp ? titleCmp.querySelector('svg') : null;
                var res = {
                    title: titleCmp ? titleCmp.innerText : 'Unnamed window',
                    icon: iconCmp ? iconCmp.innerHTML : ''
                };
                return res;
            }
        }
        return null;
    }

};

window.MudBlazorExtensions.__bindEvents();
