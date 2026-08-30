import { DUNGEON_ASSET_MANIFEST } from './js/dungeon/asset-manifest.js';
const lines=['……今日は、\nもう少し奥まで\n行けそう。','準備ができたら、\nいつでも声をかけて。','あの門の先は、\n入るたびに姿を変えるみたい。'];
let index=0,timer;const text=document.querySelector('#dialogue-text'),toast=document.querySelector('#toast'),ready=document.querySelector('#ready');
function note(message){toast.textContent=message;toast.classList.add('show');clearTimeout(timer);timer=setTimeout(()=>toast.classList.remove('show'),1800)}
document.querySelector('#dialogue').addEventListener('click',()=>{index=(index+1)%lines.length;text.textContent=lines[index]});
document.querySelectorAll('[data-note]').forEach(button=>button.addEventListener('click',()=>note(button.dataset.note)));
document.querySelector('#launch').addEventListener('click',()=>{window.location.href='dungeon.html'});
document.querySelector('#close').addEventListener('click',()=>{ready.classList.remove('open');ready.setAttribute('aria-hidden','true')});
window.LUNA_DEV=Object.freeze({dungeonAssets:DUNGEON_ASSET_MANIFEST});
