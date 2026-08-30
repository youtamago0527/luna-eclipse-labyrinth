import { DUNGEON_ASSET_MANIFEST } from './js/dungeon/asset-manifest.js';
const lines=['……月が、迷宮への道を照らしてる。','準備ができたら、いつでも声をかけて。','あの門の先は、入るたびに姿を変えるみたい。'];
let index=0,timer;const text=document.querySelector('#dialogue-text'),toast=document.querySelector('#toast'),ready=document.querySelector('#ready');
function note(message){toast.textContent=message;toast.classList.add('show');clearTimeout(timer);timer=setTimeout(()=>toast.classList.remove('show'),1800)}
document.querySelector('#dialogue').addEventListener('click',()=>{index=(index+1)%lines.length;text.textContent=lines[index]});
document.querySelectorAll('[data-note]').forEach(button=>button.addEventListener('click',()=>note(button.dataset.note)));
document.querySelector('#launch').addEventListener('click',()=>{ready.classList.add('open');ready.setAttribute('aria-hidden','false')});
document.querySelector('#close').addEventListener('click',()=>{ready.classList.remove('open');ready.setAttribute('aria-hidden','true')});
window.LUNA_DEV=Object.freeze({dungeonAssets:DUNGEON_ASSET_MANIFEST});

