require.config({ paths: { 'vs': 'lib/monaco-editor/min/vs' } });

let _dotNetInstance;

const throttleLastTimeFuncNameMappings = {};

function isScrollAtBottom(containerOrId) {
    if (typeof containerOrId === 'string' || containerOrId instanceof String) {
        containerOrId = document.querySelector(containerOrId);
    }
    
    return containerOrId.scrollHeight - containerOrId.scrollTop === containerOrId.clientHeight;
}

function registerLangugageProvider(language) {
    monaco.languages.registerCompletionItemProvider(language, {
        provideCompletionItems: async function (model, position) {
            var textUntilPosition = model.getValueInRange({
                startLineNumber: 1,
                startColumn: 1,
                endLineNumber: position.lineNumber,
                endColumn: position.column,
            });

            if(language == 'razor')
            {
                if ((textUntilPosition.match(/{/g) || []).length !== (textUntilPosition.match(/}/g) || []).length) {
                    var data = await fetch("editor/snippets/csharp.json").then((response) => response.json());
                } else {
                    //var data = await fetch("editor/snippets/mudblazor.json").then((response) => response.json());
                    var data = await fetch("api/snippets/mudex.json").then((response) => response.json());
                }
            }else {
                var data = await fetch("editor/snippets/csharp.json").then((response) => response.json());
            }
            
            var word = model.getWordUntilPosition(position);
            var range = {
                startLineNumber: position.lineNumber,
                endLineNumber: position.lineNumber,
                startColumn: word.startColumn,
                endColumn: word.endColumn,
            };
            
            var response = Object.keys(data).map(key => {
                return {
                    label: data[key].prefix,
                    detail : data[key].description,
                    documentation : data[key].body.join('\n'),
                    insertText: data[key].body.join('\n'),
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    range: range
                }
            });
            return {
                suggestions: response,
            };
        },
    });
}

function onKeyDown(e) {
    if (e.ctrlKey && e.keyCode === 83) {
        e.preventDefault();

        if (_dotNetInstance && _dotNetInstance.invokeMethodAsync) {
            throttle(() => _dotNetInstance.invokeMethodAsync('TriggerCompileAsync'), 1000, 'compile');
        }
    }
}

function throttle(func, timeFrame, id) {
    const now = new Date();
    if (now - throttleLastTimeFuncNameMappings[id] >= timeFrame) {
        func();

        throttleLastTimeFuncNameMappings[id] = now;
    }
}

window.Try = {

    initialize: function (dotNetInstance) {
        _dotNetInstance = dotNetInstance;
        throttleLastTimeFuncNameMappings['compile'] = new Date();
        
        window.addEventListener('keydown', onKeyDown);
    },
    changeDisplayUrl: function (url) {
        if (!url) {return; }
        window.history.pushState(null, null, url);
    },
    reloadIframe: function (id, newSrc) {
        const iFrame = document.getElementById(id);
        if (!iFrame) { return; }

        if (!newSrc) {
            iFrame.contentWindow.location.reload();
        } else if (iFrame.src !== `${window.location.origin}${newSrc}`) {
            iFrame.src = newSrc;
        } else {
            // There needs to be some change so the iFrame is actually reloaded
            iFrame.src = '';
            setTimeout(() => iFrame.src = newSrc);
        }
    },
    dispose: function () {
        _dotNetInstance = null;
        window.removeEventListener('keydown', onKeyDown);
    }
}

window.Try.__providerRegistered = false;
window.Try.Editor = window.Try.Editor || (function () {
    let _editor;
    let _overrideValue;

    return {
        create: function (id, value, language, readOnly, theme) {
            if (!id) { return; }
            
            require(['vs/editor/editor.main'], () => {
                _editor = monaco.editor.create(document.getElementById(id), {
                    value: _overrideValue || value || '',
                    language: language || 'razor',
                    theme: theme,
                    readOnly: readOnly,
                    automaticLayout: true,
                    mouseWheelZoom: true,
                    bracketPairColorization: {
                        enabled: true
                    },
                    minimap: {
                        enabled: false
                    }
                });

                _overrideValue = null;

                monaco.languages.html.razorDefaults.setModeConfiguration({
                    completionItems: true,
                    diagnostics:  true,
                    documentFormattingEdits: true,
                    documentHighlights: true,
                    documentRangeFormattingEdits: true,
                });

                if (!window.Try.__providerRegistered) {
                    registerLangugageProvider('razor');
                    registerLangugageProvider('csharp');
                    window.Try.__providerRegistered = true;
                }
            })
        },
        getValue: function () {
            return _editor.getValue();
        },
        setValue: function (value) {
            if(_editor) {
                _editor.setValue(value);
            } else {
                _overrideValue = value;
            }
        },
        setReadOnly: function (readOnly) {
            if (_editor) {
                _editor.updateOptions({ readOnly: readOnly });
            }
        },
        focus: function () {
            return _editor.focus();
        },
        setLanguage: function (language) {
            if(_editor) {
                monaco.editor.setModelLanguage(_editor.getModel(), language);
            }
        },
        setPosition: function (line, column) {
            if (_editor) {
                _editor.setPosition({ lineNumber: line, column: column });
            }
        },
        setSelection: function (startLineNumber, startColumn, endLineNumber, endColumn) {
            if (_editor) {
                _editor.setSelection({
                    startLineNumber: startLineNumber,
                    startColumn: startColumn || 0,
                    endLineNumber: endLineNumber || startLineNumber,
                    endColumn: endColumn || _editor.getModel().getLineMaxColumn(endLineNumber || startLineNumber)
                });
            }
        },
        setTheme: function (theme) {
            if (_editor) {
                monaco.editor.setTheme(theme);
            }
        },
        dispose: function () {
            _editor = null;
        }
    }
}());

window.Try.CodeExecution = window.Try.CodeExecution || (function () {
    const UNEXPECTED_ERROR_MESSAGE = 'An unexpected error has occurred. Please try again later or contact the team.';

    function putInCacheStorage(cache, fileName, fileBytes, contentType) {
        const cachedResponse = new Response(
            new Blob([fileBytes]),
            {
                headers: {
                    'Content-Type': contentType || 'application/octet-stream',
                    'Content-Length': fileBytes.length.toString()
                }
            });

        return cache.put(fileName, cachedResponse);
    }

    function convertBase64StringToBytes(base64String) {
        const binaryString = window.atob(base64String);

        const bytesCount = binaryString.length;
        const bytes = new Uint8Array(bytesCount);
        for (let i = 0; i < bytesCount; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }

        return bytes;
    }

    return {
        updateUserComponentsDll: async function (fileContent) {
            if (!fileContent) {
                return;
            }

            //const cache = await caches.open('blazor-resources-/');
            const cache = await caches.open('dotnet-resources-/');

            const cacheKeys = await cache.keys();
            const userComponentsDllCacheKey = cacheKeys.find(x => x.url.indexOf('Try.UserComponents.dll') > -1);
            if (!userComponentsDllCacheKey || !userComponentsDllCacheKey.url) {
                alert(UNEXPECTED_ERROR_MESSAGE);
                return;
            }

            const dllPath = userComponentsDllCacheKey.url.substr(window.location.origin.length);
            fileContent = typeof fileContent === 'number' ? BINDING.conv_string(fileContent) : fileContent // tranfering raw pointer to the memory of the mono string
            const dllBytes = typeof fileContent === 'string' ? convertBase64StringToBytes(fileContent) : fileContent;

            await putInCacheStorage(cache, dllPath, dllBytes);
        }
    };
}());
