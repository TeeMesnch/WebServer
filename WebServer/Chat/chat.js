let username = '';

const setUsernameButton = document.getElementById('setUsernameButton');
const usernameInput = document.getElementById('usernameInput');
const sendButton = document.getElementById('sendButton');
const messageInput = document.getElementById('messageInput');
const messageList = document.getElementById('messageList');
const chatBox = document.getElementById('chatBox');
const eventSource = new EventSource("https://localhost:4200/messages");

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
        
        fetch('https://localhost:4200/messages', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                username: username,
                message: message
            })
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Server responded with an error.');
                }
                return response.text();
            })
            .then(data => {
                console.log('Server response:', data);
            })
            .catch(error => {
                console.error('Fetch error:', error);
            });
    }
});

eventSource.onmessage = event => {
    const div = document.createElement("div");
    div.textContent = event.data;
    chatBox.appendChild(div);
}

messageInput.addEventListener('keydown', (e) => {
    if (e.key === 'Enter') {
        sendButton.click();
    }
});

