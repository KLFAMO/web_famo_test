const divTerminal = document.getElementById('divTerminal');

export function Terminal_ScrollToBottom() {
    divTerminal.scrollTop = divTerminal.scrollHeight;
}