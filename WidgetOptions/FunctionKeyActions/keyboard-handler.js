/**
 * Keyboard event handler for WebView2 widgets
 * Captures keyboard events and sends messages back to the C# application
 */

const shouldTriggerFor = ['F1'];

document.addEventListener('keydown', function(event) {
    if (event.repeat) {
        return;
    }

    if (shouldTriggerFor.includes(event.key)) {
        window.chrome.webview.postMessage(JSON.stringify({
            type: 'keydown',
            key: event.key,
            code: event.code,
            timestamp: Date.now()
        }));
    }
});