let audio = null;
let dotNetRef = null;

export function init(dotNetHelper) {
    dotNetRef = dotNetHelper;
    audio = new Audio();
    audio.addEventListener('ended', () => {
        dotNetRef.invokeMethodAsync('OnTrackEnded');
    });
}

export function play(url, volume) {
    audio.src = url;
    audio.volume = volume / 100;
    audio.play();
}

export function resume() {
    audio.play();
}

export function pause() {
    audio.pause();
}

export function setVolume(volume) {
    if (audio) {
        audio.volume = volume / 100;
    }
}

export function dispose() {
    if (audio) {
        audio.pause();
        audio.src = '';
        audio = null;
    }
    dotNetRef = null;
}
