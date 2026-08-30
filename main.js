const startButton = document.querySelector('#start-button');
const status = document.querySelector('#status');

startButton.addEventListener('click', () => {
  status.textContent = 'ダンジョンの入口は、次の制作で開きます。';
});

