let username = '';

const setUsernameButton = document.getElementById('setUsernameButton');
const usernameInput = document.getElementById('usernameInput');
const sendButton = document.getElementById('sendButton');
const messageInput = document.getElementById('messageInput');
const messageList = document.getElementById('messageList');

setUsernameButton.addEventListener('click', () => {
    const name = usernameInput.value.trim();
    if (name) {
        username = name;
        usernameInput.disabled = true;
        setUsernameButton.disabled = true;
    }
});

sendButton.addEventListener('click', () => {
    const message = messageInput.value.trim();
    if (!username) {
        alert('Please set a username first.');
        return;
    }
    if (message) {
        const messageElement = document.createElement('div');
        messageElement.className = 'message';
        messageElement.innerText = `${username}: ${message}`;
        messageList.appendChild(messageElement);
        messageInput.value = '';
        messageList.scrollTop = messageList.scrollHeight;
    }
});

messageInput.addEventListener('keydown', (e) => {
    if (e.key === 'Enter') {
        sendButton.click();
    }
});