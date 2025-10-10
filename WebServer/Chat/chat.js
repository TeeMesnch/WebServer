let username = '';

const setUsernameButton = document.getElementById('setUsernameButton');
const usernameInput = document.getElementById('usernameInput');
const sendButton = document.getElementById('sendButton');
const messageInput = document.getElementById('messageInput');
const messageList = document.getElementById('messageList');

function displayMessage({ username, message }) {
    const messageElement = document.createElement('div');
    messageElement.classList.add('message');
    messageElement.innerHTML = `<strong>${username}:</strong> ${message}`;
    messageList.appendChild(messageElement);
    messageList.scrollTop = messageList.scrollHeight; // Auto-scroll to bottom
}

setUsernameButton.addEventListener('click', () => {
    const name = usernameInput.value.trim();
    if (name) {
        username = name;
        usernameInput.disabled = true;
        setUsernameButton.disabled = true;
    } else {
        alert('Please enter a username.');
    }
});

sendButton.addEventListener('click', () => {
    const message = messageInput.value.trim();

    if (!username) {
        alert('Please set a username first.');
        return;
    }

    if (message) {
        fetch('https://localhost:4200/messages', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ username, message })
        }).then(response => {
            if (!response.ok) {
                throw new Error('Server error');
            }
            displayMessage({ username, message });
            messageInput.value = '';
        }).catch(error => {
            console.error('Send failed:', error);
        });
    }
});

messageInput.addEventListener('keydown', (e) => {
    if (e.key === 'Enter') {
        sendButton.click();
    }
});

