/**
 * Hide scrollbar script for WebView2 widgets
 * Injects CSS to hide webkit scrollbars
 */

const style = document.createElement('style');
style.innerHTML = `
    ::-webkit-scrollbar { width: 0 !important; height: 0 !important; }
`;
document.head.appendChild(style);